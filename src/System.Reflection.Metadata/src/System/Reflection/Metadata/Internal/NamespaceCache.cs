// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal class NamespaceCache
    {
        private readonly MetadataReader _metadataReader;
        private readonly object _namespaceTableAndListLock = new object();
        private Dictionary<NamespaceDefinitionHandle, NamespaceData> _namespaceTable;
        private NamespaceData _rootNamespace;
        private ImmutableArray<NamespaceDefinitionHandle> _namespaceList;

        internal NamespaceCache(MetadataReader reader)
        {
            Debug.Assert(reader != null);
            _metadataReader = reader;
        }

        /// <summary>
        /// Returns whether the namespaceTable has been created. If it hasn't, calling a GetXXX method
        /// on this will probably have a very high amount of overhead.
        /// </summary>
        internal bool CacheIsRealized
        {
            get { return _namespaceTable != null; }
        }

        internal string GetFullName(NamespaceDefinitionHandle handle)
        {
            Debug.Assert(!handle.HasFullName); // we should not hit the cache in this case.
            NamespaceData data = GetNamespaceData(handle);
            return data.FullName;
        }

        internal NamespaceData GetRootNamespace()
        {
            EnsureNamespaceTableIsPopulated();
            Debug.Assert(_rootNamespace != null);
            return _rootNamespace;
        }

        internal NamespaceData GetNamespaceData(NamespaceDefinitionHandle handle)
        {
            EnsureNamespaceTableIsPopulated();
            NamespaceData result;
            if (!_namespaceTable.TryGetValue(handle, out result))
            {
                ThrowInvalidHandle();
            }
            return result;
        }

        // TODO: move throw helpers to common place. 
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidHandle()
        {
            throw new BadImageFormatException(MetadataResources.InvalidHandle);
        }

        /// <summary>
        /// This will return a StringHandle for the simple name of a namespace name at the given segment index.
        /// If no segment index is passed explicitly or the "segment" index is greater than or equal to the number
        /// of segments, then the last segment is used. "Segment" in this context refers to part of a namespace
        /// name between dots.
        ///
        /// Example: Given a NamespaceDefinitionHandle to "System.Collections.Generic.Test" called 'handle':
        ///
        ///   reader.GetString(GetSimpleName(handle)) == "Test"
        ///   reader.GetString(GetSimpleName(handle, 0)) == "System"
        ///   reader.GetString(GetSimpleName(handle, 1)) == "Collections"
        ///   reader.GetString(GetSimpleName(handle, 2)) == "Generic"
        ///   reader.GetString(GetSimpleName(handle, 3)) == "Test"
        ///   reader.GetString(GetSimpleName(handle, 1000)) == "Test"
        /// </summary>
        private StringHandle GetSimpleName(NamespaceDefinitionHandle fullNamespaceHandle, int segmentIndex = Int32.MaxValue)
        {
            StringHandle handleContainingSegment = fullNamespaceHandle.GetFullName();
            Debug.Assert(!handleContainingSegment.IsVirtual);

            int lastFoundIndex = fullNamespaceHandle.Offset - 1;
            int currentSegment = 0;
            while (currentSegment < segmentIndex)
            {
                int currentIndex = _metadataReader.StringStream.IndexOfRaw(lastFoundIndex + 1, '.');
                if (currentIndex == -1)
                {
                    break;
                }
                lastFoundIndex = currentIndex;
                ++currentSegment;
            }

            Debug.Assert(lastFoundIndex >= 0 || currentSegment == 0);

            // + 1 because lastFoundIndex will either "point" to a '.', or will be -1. Either way,
            // we want the next char.
            int resultIndex = lastFoundIndex + 1;
            return StringHandle.FromOffset(resultIndex).WithDotTermination();
        }

        /// <summary>
        /// Two distinct namespace handles represent the same namespace if their full names are the same. This
        /// method merges builders corresponding to such namespace handles.
        /// </summary>
        private void PopulateNamespaceTable()
        {
            lock (_namespaceTableAndListLock)
            {
                if (_namespaceTable != null)
                {
                    return;
                }

                var namespaceBuilderTable = new Dictionary<NamespaceDefinitionHandle, NamespaceDataBuilder>();

                // Make sure to add entry for root namespace. The root namespace is special in that even
                // though it might not have types of its own it always has an equivalent representation
                // as a nil handle and we don't want to handle it below as dot-terminated synthetic namespace.
                // We use NamespaceDefinitionHandle.FromIndexOfFullName(0) instead of default(NamespaceDefinitionHandle) so
                // that we never hand back a handle to the user that doesn't have a typeid as that prevents
                // round-trip conversion to Handle and back. (We may discover other handle aliases for the
                // root namespace (any nil/empty string will do), but we need this one to always be there.
                NamespaceDefinitionHandle rootNamespace = NamespaceDefinitionHandle.FromFullNameOffset(0);
                namespaceBuilderTable.Add(
                    rootNamespace,
                    new NamespaceDataBuilder(
                        rootNamespace,
                        rootNamespace.GetFullName(),
                        String.Empty));

                PopulateTableWithTypeDefinitions(namespaceBuilderTable);
                PopulateTableWithExportedTypes(namespaceBuilderTable);

                Dictionary<string, NamespaceDataBuilder> stringTable;
                MergeDuplicateNamespaces(namespaceBuilderTable, out stringTable);

                List<NamespaceDataBuilder> syntheticNamespaces;
                ResolveParentChildRelationships(stringTable, out syntheticNamespaces);

                var namespaceTable = new Dictionary<NamespaceDefinitionHandle, NamespaceData>();

                foreach (var group in namespaceBuilderTable)
                {
                    // Freeze() caches the result, so any many-to-one relationships
                    // between keys and values will be preserved and efficiently handled.
                    namespaceTable.Add(group.Key, group.Value.Freeze());
                }

                if (syntheticNamespaces != null)
                {
                    foreach (var syntheticNamespace in syntheticNamespaces)
                    {
                        namespaceTable.Add(syntheticNamespace.Handle, syntheticNamespace.Freeze());
                    }
                }

                _namespaceTable = namespaceTable;
                _rootNamespace = namespaceTable[rootNamespace];
            }
        }

        /// <summary>
        /// This will take 'table' and merge all of the NamespaceData instances that point to the same
        /// namespace. It has to create 'stringTable' as an intermediate dictionary, so it will hand it
        /// back to the caller should the caller want to use it.
        /// </summary>
        private void MergeDuplicateNamespaces(Dictionary<NamespaceDefinitionHandle, NamespaceDataBuilder> table, out Dictionary<string, NamespaceDataBuilder> stringTable)
        {
            var namespaces = new Dictionary<string, NamespaceDataBuilder>();
            List<KeyValuePair<NamespaceDefinitionHandle, NamespaceDataBuilder>> remaps = null;
            foreach (var group in table)
            {
                NamespaceDataBuilder data = group.Value;
                NamespaceDataBuilder existingRecord;
                if (namespaces.TryGetValue(data.FullName, out existingRecord))
                {
                    // Children should not exist until the next step.
                    Debug.Assert(data.Namespaces.Count == 0);
                    data.MergeInto(existingRecord);

                    if (remaps == null)
                    {
                        remaps = new List<KeyValuePair<NamespaceDefinitionHandle, NamespaceDataBuilder>>();
                    }
                    remaps.Add(new KeyValuePair<NamespaceDefinitionHandle, NamespaceDataBuilder>(group.Key, existingRecord));
                }
                else
                {
                    namespaces.Add(data.FullName, data);
                }
            }

            // Needs to be done outside of foreach (var group in table) to avoid modifying the dictionary while foreach'ing over it.
            if (remaps != null)
            {
                foreach (var tuple in remaps)
                {
                    table[tuple.Key] = tuple.Value;
                }
            }

            stringTable = namespaces;
        }

        /// <summary>
        /// Creates a NamespaceDataBuilder instance that contains a synthesized NamespaceDefinitionHandle, 
        /// as well as the name provided.
        /// </summary>
        private NamespaceDataBuilder SynthesizeNamespaceData(string fullName, NamespaceDefinitionHandle realChild)
        {
            Debug.Assert(realChild.HasFullName);

            int numberOfSegments = 0;
            foreach (char c in fullName)
            {
                if (c == '.')
                {
                    numberOfSegments++;
                }
            }

            StringHandle simpleName = GetSimpleName(realChild, numberOfSegments);
            var namespaceHandle = NamespaceDefinitionHandle.FromSimpleNameOffset(simpleName.GetHeapOffset());
            return new NamespaceDataBuilder(namespaceHandle, simpleName, fullName);
        }

        /// <summary>
        /// Quick convenience method that handles linking together child + parent
        /// </summary>
        private void LinkChildDataToParentData(NamespaceDataBuilder child, NamespaceDataBuilder parent)
        {
            Debug.Assert(child != null && parent != null);
            Debug.Assert(!child.Handle.IsNil);
            child.Parent = parent.Handle;
            parent.Namespaces.Add(child.Handle);
        }

        /// <summary>
        /// Links a child to its parent namespace. If the parent namespace doesn't exist, this will create a
        /// synthetic one. This will automatically link any synthetic namespaces it creates up to its parents.
        /// </summary>
        private void LinkChildToParentNamespace(Dictionary<string, NamespaceDataBuilder> existingNamespaces,
            NamespaceDataBuilder realChild,
            ref List<NamespaceDataBuilder> syntheticNamespaces)
        {
            Debug.Assert(realChild.Handle.HasFullName);
            string childName = realChild.FullName;
            var child = realChild;

            // The condition for this loop is very complex -- essentially, we keep going
            // until we: 
            //   A. Encounter the root namespace as 'child'
            //   B. Find a preexisting namespace as 'parent'
            while (true)
            {
                int lastIndex = childName.LastIndexOf('.');
                string parentName;
                if (lastIndex == -1)
                {
                    if (childName.Length == 0)
                    {
                        return;
                    }
                    else
                    {
                        parentName = String.Empty;
                    }
                }
                else
                {
                    parentName = childName.Substring(0, lastIndex);
                }

                NamespaceDataBuilder parentData;
                if (existingNamespaces.TryGetValue(parentName, out parentData))
                {
                    LinkChildDataToParentData(child, parentData);
                    return;
                }

                if (syntheticNamespaces != null)
                {
                    foreach (var data in syntheticNamespaces)
                    {
                        if (data.FullName == parentName)
                        {
                            LinkChildDataToParentData(child, data);
                            return;
                        }
                    }
                }
                else
                {
                    syntheticNamespaces = new List<NamespaceDataBuilder>();
                }

                var syntheticParent = SynthesizeNamespaceData(parentName, realChild.Handle);
                LinkChildDataToParentData(child, syntheticParent);
                syntheticNamespaces.Add(syntheticParent);
                childName = syntheticParent.FullName;
                child = syntheticParent;
            }
        }

        /// <summary>
        /// This will link all parents/children in the given namespaces dictionary up to each other.
        /// 
        /// In some cases, we need to synthesize namespaces that do not have any type definitions or forwarders
        /// of their own, but do have child namespaces. These are returned via the syntheticNamespaces out
        /// parameter.
        /// </summary>
        private void ResolveParentChildRelationships(Dictionary<string, NamespaceDataBuilder> namespaces, out List<NamespaceDataBuilder> syntheticNamespaces)
        {
            syntheticNamespaces = null;
            foreach (var namespaceData in namespaces.Values)
            {
                LinkChildToParentNamespace(namespaces, namespaceData, ref syntheticNamespaces);
            }
        }

        /// <summary>
        /// Loops through all type definitions in metadata, adding them to the given table
        /// </summary>
        private void PopulateTableWithTypeDefinitions(Dictionary<NamespaceDefinitionHandle, NamespaceDataBuilder> table)
        {
            Debug.Assert(table != null);

            foreach (var typeHandle in _metadataReader.TypeDefinitions)
            {
                TypeDefinition type = _metadataReader.GetTypeDefinition(typeHandle);
                if (type.Attributes.IsNested())
                {
                    continue;
                }

                NamespaceDefinitionHandle namespaceHandle = _metadataReader.TypeDefTable.GetNamespace(typeHandle);
                NamespaceDataBuilder builder;
                if (table.TryGetValue(namespaceHandle, out builder))
                {
                    builder.TypeDefinitions.Add(typeHandle);
                }
                else
                {
                    StringHandle name = GetSimpleName(namespaceHandle);
                    string fullName = _metadataReader.GetString(namespaceHandle);
                    var newData = new NamespaceDataBuilder(namespaceHandle, name, fullName);
                    newData.TypeDefinitions.Add(typeHandle);
                    table.Add(namespaceHandle, newData);
                }
            }
        }

        /// <summary>
        /// Loops through all type forwarders in metadata, adding them to the given table
        /// </summary>
        private void PopulateTableWithExportedTypes(Dictionary<NamespaceDefinitionHandle, NamespaceDataBuilder> table)
        {
            Debug.Assert(table != null);

            foreach (var exportedTypeHandle in _metadataReader.ExportedTypes)
            {
                ExportedType exportedType = _metadataReader.GetExportedType(exportedTypeHandle);
                if (exportedType.Implementation.Kind == HandleKind.ExportedType)
                {
                    continue; // skip nested exported types.
                }

                NamespaceDefinitionHandle namespaceHandle = exportedType.NamespaceDefinition;
                NamespaceDataBuilder builder;
                if (table.TryGetValue(namespaceHandle, out builder))
                {
                    builder.ExportedTypes.Add(exportedTypeHandle);
                }
                else
                {
                    Debug.Assert(namespaceHandle.HasFullName);
                    StringHandle simpleName = GetSimpleName(namespaceHandle);
                    string fullName = _metadataReader.GetString(namespaceHandle);
                    var newData = new NamespaceDataBuilder(namespaceHandle, simpleName, fullName);
                    newData.ExportedTypes.Add(exportedTypeHandle);
                    table.Add(namespaceHandle, newData);
                }
            }
        }

        /// <summary>
        /// Populates namespaceList with distinct namespaces. No ordering is guaranteed.
        /// </summary>
        private void PopulateNamespaceList()
        {
            lock (_namespaceTableAndListLock)
            {
                if (_namespaceList != null)
                {
                    return;
                }

                Debug.Assert(_namespaceTable != null);
                var namespaceNameSet = new HashSet<string>();
                var namespaceListBuilder = ImmutableArray.CreateBuilder<NamespaceDefinitionHandle>();

                foreach (var group in _namespaceTable)
                {
                    var data = group.Value;
                    if (namespaceNameSet.Add(data.FullName))
                    {
                        namespaceListBuilder.Add(group.Key);
                    }
                }

                _namespaceList = namespaceListBuilder.ToImmutable();
            }
        }

        /// <summary>
        /// If the namespace table doesn't exist, populates it!
        /// </summary>
        private void EnsureNamespaceTableIsPopulated()
        {
            // PERF: Branch will rarely be taken; do work in PopulateNamespaceList() so this can be inlined easily.
            if (_namespaceTable == null)
            {
                PopulateNamespaceTable();
            }
            Debug.Assert(_namespaceTable != null);
        }

        /// <summary>
        /// If the namespace list doesn't exist, populates it!
        /// </summary>
        private void EnsureNamespaceListIsPopulated()
        {
            if (_namespaceList == null)
            {
                PopulateNamespaceList();
            }
            Debug.Assert(_namespaceList != null);
        }

        /// <summary>
        /// An intermediate class used to build NamespaceData instances. This was created because we wanted to
        /// use ImmutableArrays in NamespaceData, but having ArrayBuilders and ImmutableArrays that served the
        /// same purpose in NamespaceData got ugly. With the current design of how we create our Namespace
        /// dictionary, this needs to be a class because we have a many-to-one mapping between NamespaceHandles
        /// and NamespaceData. So, the pointer semantics must be preserved.
        /// 
        /// This class assumes that the builders will not be modified in any way after the first call to
        /// Freeze().
        /// </summary>
        private class NamespaceDataBuilder
        {
            public readonly NamespaceDefinitionHandle Handle;
            public readonly StringHandle Name;
            public readonly string FullName;
            public NamespaceDefinitionHandle Parent;
            public ImmutableArray<NamespaceDefinitionHandle>.Builder Namespaces;
            public ImmutableArray<TypeDefinitionHandle>.Builder TypeDefinitions;
            public ImmutableArray<ExportedTypeHandle>.Builder ExportedTypes;

            private NamespaceData _frozen;

            public NamespaceDataBuilder(NamespaceDefinitionHandle handle, StringHandle name, string fullName)
            {
                Handle = handle;
                Name = name;
                FullName = fullName;
                Namespaces = ImmutableArray.CreateBuilder<NamespaceDefinitionHandle>();
                TypeDefinitions = ImmutableArray.CreateBuilder<TypeDefinitionHandle>();
                ExportedTypes = ImmutableArray.CreateBuilder<ExportedTypeHandle>();
            }

            /// <summary>
            /// Returns a NamespaceData that represents this NamespaceDataBuilder instance. After calling
            /// this method, it is an error to use any methods or fields except Freeze() on the target 
            /// NamespaceDataBuilder.
            /// </summary>
            public NamespaceData Freeze()
            {
                // It is not an error to call this function multiple times. We cache the result
                // because it's immutable.
                if (_frozen == null)
                {
                    var namespaces = Namespaces.ToImmutable();
                    Namespaces = null;

                    var typeDefinitions = TypeDefinitions.ToImmutable();
                    TypeDefinitions = null;

                    var exportedTypes = ExportedTypes.ToImmutable();
                    ExportedTypes = null;

                    _frozen = new NamespaceData(Name, FullName, Parent, namespaces, typeDefinitions, exportedTypes);
                }

                return _frozen;
            }

            public void MergeInto(NamespaceDataBuilder other)
            {
                Parent = default(NamespaceDefinitionHandle);
                other.Namespaces.AddRange(this.Namespaces);
                other.TypeDefinitions.AddRange(this.TypeDefinitions);
                other.ExportedTypes.AddRange(this.ExportedTypes);
            }
        }
    }
}
