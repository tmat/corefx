// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    public static class CodedIndex
    {
        // Arbitrary value not equal to any of the token types in the array. This includes 0 which is TokenTypeIds.Module.
        internal const uint InvalidTokenType = uint.MaxValue;

        public static int ToHasCustomAttribute       (EntityHandle handle) => FromHandle(handle, HasCustomAttribute.BitCount, HasCustomAttribute.ToTag(handle.Kind));
        public static int ToHasConstant              (EntityHandle handle) => FromHandle(handle, (int)HasConstant.__bits, HasConstant.ToTag(handle.Kind));
        public static int ToCustomAttributeType      (EntityHandle handle) => FromHandle(handle, (int)CustomAttributeType.__bits, CustomAttributeType.ToTag(handle.Kind));
        public static int ToHasDeclSecurity          (EntityHandle handle) => FromHandle(handle, (int)HasDeclSecurity.__bits, HasDeclSecurity.ToTag(handle.Kind));
        public static int ToHasFieldMarshal          (EntityHandle handle) => FromHandle(handle, (int)HasFieldMarshal.__bits, HasFieldMarshal.ToTag(handle.Kind));
        public static int ToHasSemantics             (EntityHandle handle) => FromHandle(handle, (int)HasSemantics.__bits, HasSemantics.ToTag(handle.Kind));
        public static int ToImplementation           (EntityHandle handle) => FromHandle(handle, (int)Implementation.__bits, Implementation.ToTag(handle.Kind));
        public static int ToMemberForwarded          (EntityHandle handle) => FromHandle(handle, (int)MemberForwarded.__bits, MemberForwarded.ToTag(handle.Kind));
        public static int ToMemberRefParent          (EntityHandle handle) => FromHandle(handle, (int)MemberRefParent.__bits, MemberRefParent.ToTag(handle.Kind));
        public static int ToMethodDefOrRef           (EntityHandle handle) => FromHandle(handle, (int)MethodDefOrRef.__bits, MethodDefOrRef.ToTag(handle.Kind));
        public static int ToResolutionScope          (EntityHandle handle) => FromHandle(handle, (int)ResolutionScope.__bits, ResolutionScope.ToTag(handle.Kind));
        public static int ToTypeDefOrRef             (EntityHandle handle) => FromHandle(handle, (int)TypeDefOrRef.__bits, TypeDefOrRef.ToTag(handle.Kind));
        public static int ToTypeDefOrRefOrSpec       (EntityHandle handle) => FromHandle(handle, (int)TypeDefOrRefOrSpec.__bits, TypeDefOrRefOrSpec.ToTag(handle.Kind));
        public static int ToTypeOrMethodDef          (EntityHandle handle) => FromHandle(handle, (int)TypeOrMethodDef.__bits, TypeOrMethodDef.ToTag(handle.Kind));
        public static int ToHasCustomDebugInformation(EntityHandle handle) => FromHandle(handle, HasCustomDebugInformation.BitCount, HasCustomDebugInformation.ToTag(handle.Kind));

        private static int FromHandle(EntityHandle handle, int bitCount, int tag) => (handle.RowId << bitCount) | tag;

        private static class HasCustomAttribute
        {
            internal const int MethodDef = 0;
            internal const int Field = 1;
            internal const int TypeRef = 2;
            internal const int TypeDef = 3;
            internal const int Param = 4;
            internal const int InterfaceImpl = 5;
            internal const int MemberRef = 6;
            internal const int Module = 7;
            internal const int DeclSecurity = 8;
            internal const int Property = 9;
            internal const int Event = 10;
            internal const int StandAloneSig = 11;
            internal const int ModuleRef = 12;
            internal const int TypeSpec = 13;
            internal const int Assembly = 14;
            internal const int AssemblyRef = 15;
            internal const int File = 16;
            internal const int ExportedType = 17;
            internal const int ManifestResource = 18;
            internal const int GenericParam = 19;
            internal const int GenericParamConstraint = 20;
            internal const int MethodSpec = 21;

            internal const int BitCount = 5;

            internal const TableMask TablesReferenced =
                TableMask.MethodDef
              | TableMask.Field
              | TableMask.TypeRef
              | TableMask.TypeDef
              | TableMask.Param
              | TableMask.InterfaceImpl
              | TableMask.MemberRef
              | TableMask.Module
              | TableMask.DeclSecurity
              | TableMask.Property
              | TableMask.Event
              | TableMask.StandAloneSig
              | TableMask.ModuleRef
              | TableMask.TypeSpec
              | TableMask.Assembly
              | TableMask.AssemblyRef
              | TableMask.File
              | TableMask.ExportedType
              | TableMask.ManifestResource
              | TableMask.GenericParam
              | TableMask.GenericParamConstraint
              | TableMask.MethodSpec;

            internal static int ToTag(HandleKind kind)
            {
                switch (kind)
                {
                    case HandleKind.MethodDefinition: return HasCustomAttribute.MethodDef;
                    case HandleKind.FieldDefinition: return HasCustomAttribute.Field;
                    case HandleKind.TypeReference: return HasCustomAttribute.TypeRef;
                    case HandleKind.TypeDefinition: return HasCustomAttribute.TypeDef;
                    case HandleKind.Parameter: return HasCustomAttribute.Param;
                    case HandleKind.InterfaceImplementation: return HasCustomAttribute.InterfaceImpl;
                    case HandleKind.MemberReference: return HasCustomAttribute.MemberRef;
                    case HandleKind.ModuleDefinition: return HasCustomAttribute.Module;
                    case HandleKind.DeclarativeSecurityAttribute: return HasCustomAttribute.DeclSecurity;
                    case HandleKind.PropertyDefinition: return HasCustomAttribute.Property;
                    case HandleKind.EventDefinition: return HasCustomAttribute.Event;
                    case HandleKind.StandaloneSignature: return HasCustomAttribute.StandAloneSig;
                    case HandleKind.ModuleReference: return HasCustomAttribute.ModuleRef;
                    case HandleKind.TypeSpecification: return HasCustomAttribute.TypeSpec;
                    case HandleKind.AssemblyDefinition: return HasCustomAttribute.Assembly;
                    case HandleKind.AssemblyReference: return HasCustomAttribute.AssemblyRef;
                    case HandleKind.AssemblyFile: return HasCustomAttribute.File;
                    case HandleKind.ExportedType: return HasCustomAttribute.ExportedType;
                    case HandleKind.ManifestResource: return HasCustomAttribute.ManifestResource;
                    case HandleKind.GenericParameter: return HasCustomAttribute.GenericParam;
                    case HandleKind.GenericParameterConstraint: return HasCustomAttribute.GenericParamConstraint;
                    case HandleKind.MethodSpecification: return HasCustomAttribute.MethodSpec;

                    default:
                        throw UnexpectedHandleKind(kind);
                }
            }
        }

        private enum HasConstant
        {
            Field = 0,
            Param = 1,
            Property = 2,

            __bits = 2
        }

        internal const TableMask HasConstant_TablesReferenced =
            TableMask.Field
          | TableMask.Param
          | TableMask.Property;

        private static HasConstant ToHasConstantTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.FieldDefinition: return HasConstant.Field;
                case HandleKind.Parameter: return HasConstant.Param;
                case HandleKind.PropertyDefinition: return HasConstant.Property;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        private enum CustomAttributeType
        {
            MethodDef = 2,
            MemberRef = 3,

            __bits = 3
        }

        internal const TableMask CustomAttributeType_TablesReferenced =
            TableMask.MethodDef
          | TableMask.MemberRef;

        private static CustomAttributeType ToCustomAttributeTypeTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.MethodDefinition: return CustomAttributeType.MethodDef;
                case HandleKind.MemberReference: return CustomAttributeType.MemberRef;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        private enum HasDeclSecurity
        {
            TypeDef = 0,
            MethodDef = 1,
            Assembly = 2,

            __bits = 2
        }

        internal const TableMask HasDeclSecurity_TablesReferenced =
            TableMask.TypeDef
          | TableMask.MethodDef
          | TableMask.Assembly;

        private static HasDeclSecurity ToHasDeclSecurityTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.TypeDefinition: return HasDeclSecurity.TypeDef;
                case HandleKind.MethodDefinition: return HasDeclSecurity.MethodDef;
                case HandleKind.AssemblyDefinition: return HasDeclSecurity.Assembly;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        private enum HasFieldMarshal
        {
            Field = 0,
            Param = 1,

            __bits = 1
        }

        internal const TableMask HasFieldMarshal_TablesReferenced =
            TableMask.Field
          | TableMask.Param;

        private static HasFieldMarshal ToHasFieldMarshalTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.FieldDefinition: return HasFieldMarshal.Field;
                case HandleKind.Parameter: return HasFieldMarshal.Param;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        private enum HasSemantics
        {
            Event = 0,
            Property = 1,

            __bits = 1
        }

        internal const TableMask HasSemantics_TablesReferenced =
            TableMask.Event
          | TableMask.Property;

        private static HasSemantics ToHasSemanticsTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.EventDefinition: return HasSemantics.Event;
                case HandleKind.PropertyDefinition: return HasSemantics.Property;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        private enum Implementation
        {
            File = 0,
            AssemblyRef = 1,
            ExportedType = 2,

            __bits = 2
        }

        internal const TableMask Implementation_TablesReferenced =
            TableMask.File
          | TableMask.AssemblyRef
          | TableMask.ExportedType;

        private static Implementation ToImplementationTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.AssemblyFile: return Implementation.File;
                case HandleKind.AssemblyReference: return Implementation.AssemblyRef;
                case HandleKind.ExportedType: return Implementation.ExportedType;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        private enum MemberForwarded
        {
            Field = 0,
            MethodDef = 1,

            __bits = 1
        }

        internal const TableMask MemberForwarded_TablesReferenced =
            TableMask.Field
          | TableMask.MethodDef;

        private static MemberForwarded ToMemberForwardedTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.FieldDefinition: return MemberForwarded.Field;
                case HandleKind.MethodDefinition: return MemberForwarded.MethodDef;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        private enum MemberRefParent
        {
            TypeDef = 0,
            TypeRef = 1,
            ModuleRef = 2,
            MethodDef = 3,
            TypeSpec = 4,

            __bits = 3
        }

        internal const TableMask MemberRefParent_TablesReferenced =
            TableMask.TypeDef
          | TableMask.TypeRef
          | TableMask.ModuleRef
          | TableMask.MethodDef
          | TableMask.TypeSpec;

        private static MemberRefParent ToMemberRefParentTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.TypeDefinition: return MemberRefParent.TypeDef;
                case HandleKind.TypeReference: return MemberRefParent.TypeRef;
                case HandleKind.ModuleReference: return MemberRefParent.ModuleRef;
                case HandleKind.MethodDefinition: return MemberRefParent.MethodDef;
                case HandleKind.TypeSpecification: return MemberRefParent.TypeSpec;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        private enum MethodDefOrRef
        {
            MethodDef = 0,
            MemberRef = 1,

            __bits = 1
        }

        private static MethodDefOrRef ToMethodDefOrRefTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.MethodDefinition: return MethodDefOrRef.MethodDef;
                case HandleKind.MemberReference: return MethodDefOrRef.MemberRef;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        internal const TableMask MethodDefOrRef_TablesReferenced =
            TableMask.MethodDef
          | TableMask.MemberRef;

        private enum ResolutionScope
        {
            Module = 0,
            ModuleRef = 1,
            AssemblyRef = 2,
            TypeRef = 3,

            __bits = 2
        }

        internal const TableMask ResolutionScope_TablesReferenced =
            TableMask.Module
          | TableMask.ModuleRef
          | TableMask.AssemblyRef
          | TableMask.TypeRef;

        private static ResolutionScope ToResolutionScopeTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.ModuleDefinition: return ResolutionScope.Module;
                case HandleKind.ModuleReference: return ResolutionScope.ModuleRef;
                case HandleKind.AssemblyReference: return ResolutionScope.AssemblyRef;
                case HandleKind.TypeReference: return ResolutionScope.TypeRef;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        private enum TypeDefOrRefOrSpec
        {
            TypeDef = 0,
            TypeRef = 1,
            TypeSpec = 2,

            __bits = 2
        }

        internal const TableMask TypeDefOrRefOrSpec_TablesReferenced =
            TableMask.TypeDef
          | TableMask.TypeRef
          | TableMask.TypeSpec;

        private static TypeDefOrRefOrSpec ToTypeDefOrRefOrSpecTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.TypeDefinition: return TypeDefOrRefOrSpec.TypeDef;
                case HandleKind.TypeReference: return TypeDefOrRefOrSpec.TypeRef;
                case HandleKind.TypeSpecification: return TypeDefOrRefOrSpec.TypeSpec;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        private enum TypeDefOrRef
        {
            TypeDef = 0,
            TypeRef = 1,

            __bits = 2
        }

        private static TypeDefOrRef ToTypeDefOrRefTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.TypeDefinition: return TypeDefOrRef.TypeDef;
                case HandleKind.TypeReference: return TypeDefOrRef.TypeRef;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        private enum TypeOrMethodDef
        {
            TypeDef = 0,
            MethodDef = 1,

            __bits = 1
        }

        internal const TableMask TypeOrMethodDef_TablesReferenced =
            TableMask.TypeDef
          | TableMask.MethodDef;

        private static TypeOrMethodDef ToTypeOrMethodDefTag(HandleKind kind)
        {
            switch (kind)
            {
                case HandleKind.TypeDefinition: return TypeOrMethodDef.TypeDef;
                case HandleKind.MethodDefinition: return TypeOrMethodDef.MethodDef;

                default:
                    throw UnexpectedHandleKind(kind);
            }
        }

        internal static class HasCustomDebugInformation
        {
            internal const int
                MethodDef = 0,
                Field = 1,
                TypeRef = 2,
                TypeDef = 3,
                Param = 4,
                InterfaceImpl = 5,
                MemberRef = 6,
                Module = 7,
                DeclSecurity = 8,
                Property = 9,
                Event = 10,
                StandAloneSig = 11,
                ModuleRef = 12,
                TypeSpec = 13,
                Assembly = 14,
                AssemblyRef = 15,
                File = 16,
                ExportedType = 17,
                ManifestResource = 18,
                GenericParam = 19,
                GenericParamConstraint = 20,
                MethodSpec = 21,
                Document = 22,
                LocalScope = 23,
                LocalVariable = 24,
                LocalConstant = 25,
                ImportScope = 26;

            internal const int BitCount = 5;

            internal const TableMask TablesReferenced =
                TableMask.MethodDef
              | TableMask.Field
              | TableMask.TypeRef
              | TableMask.TypeDef
              | TableMask.Param
              | TableMask.InterfaceImpl
              | TableMask.MemberRef
              | TableMask.Module
              | TableMask.DeclSecurity
              | TableMask.Property
              | TableMask.Event
              | TableMask.StandAloneSig
              | TableMask.ModuleRef
              | TableMask.TypeSpec
              | TableMask.Assembly
              | TableMask.AssemblyRef
              | TableMask.File
              | TableMask.ExportedType
              | TableMask.ManifestResource
              | TableMask.GenericParam
              | TableMask.GenericParamConstraint
              | TableMask.MethodSpec
              | TableMask.Document
              | TableMask.LocalScope
              | TableMask.LocalVariable
              | TableMask.LocalConstant
              | TableMask.ImportScope;

            internal static int ToTag(HandleKind kind)
            {
                switch (kind)
                {
                    case HandleKind.MethodDefinition: return HasCustomDebugInformation.MethodDef;
                    case HandleKind.FieldDefinition: return HasCustomDebugInformation.Field;
                    case HandleKind.TypeReference: return HasCustomDebugInformation.TypeRef;
                    case HandleKind.TypeDefinition: return HasCustomDebugInformation.TypeDef;
                    case HandleKind.Parameter: return HasCustomDebugInformation.Param;
                    case HandleKind.InterfaceImplementation: return HasCustomDebugInformation.InterfaceImpl;
                    case HandleKind.MemberReference: return HasCustomDebugInformation.MemberRef;
                    case HandleKind.ModuleDefinition: return HasCustomDebugInformation.Module;
                    case HandleKind.DeclarativeSecurityAttribute: return HasCustomDebugInformation.DeclSecurity;
                    case HandleKind.PropertyDefinition: return HasCustomDebugInformation.Property;
                    case HandleKind.EventDefinition: return HasCustomDebugInformation.Event;
                    case HandleKind.StandaloneSignature: return HasCustomDebugInformation.StandAloneSig;
                    case HandleKind.ModuleReference: return HasCustomDebugInformation.ModuleRef;
                    case HandleKind.TypeSpecification: return HasCustomDebugInformation.TypeSpec;
                    case HandleKind.AssemblyDefinition: return HasCustomDebugInformation.Assembly;
                    case HandleKind.AssemblyReference: return HasCustomDebugInformation.AssemblyRef;
                    case HandleKind.AssemblyFile: return HasCustomDebugInformation.File;
                    case HandleKind.ExportedType: return HasCustomDebugInformation.ExportedType;
                    case HandleKind.ManifestResource: return HasCustomDebugInformation.ManifestResource;
                    case HandleKind.GenericParameter: return HasCustomDebugInformation.GenericParam;
                    case HandleKind.GenericParameterConstraint: return HasCustomDebugInformation.GenericParamConstraint;
                    case HandleKind.MethodSpecification: return HasCustomDebugInformation.MethodSpec;
                    case HandleKind.Document: return HasCustomDebugInformation.Document;
                    case HandleKind.LocalScope: return HasCustomDebugInformation.LocalScope;
                    case HandleKind.LocalVariable: return HasCustomDebugInformation.LocalVariable;
                    case HandleKind.LocalConstant: return HasCustomDebugInformation.LocalConstant;
                    case HandleKind.ImportScope: return HasCustomDebugInformation.ImportScope;

                    default:
                        throw UnexpectedHandleKind(kind);
                }
            }

            private static uint[] TagToTokenTypeArray =
            {
                TokenTypeIds.MethodDef,
                TokenTypeIds.FieldDef,
                TokenTypeIds.TypeRef,
                TokenTypeIds.TypeDef,
                TokenTypeIds.ParamDef,
                TokenTypeIds.InterfaceImpl,
                TokenTypeIds.MemberRef,
                TokenTypeIds.Module,
                TokenTypeIds.DeclSecurity,
                TokenTypeIds.Property,
                TokenTypeIds.Event,
                TokenTypeIds.Signature,
                TokenTypeIds.ModuleRef,
                TokenTypeIds.TypeSpec,
                TokenTypeIds.Assembly,
                TokenTypeIds.AssemblyRef,
                TokenTypeIds.File,
                TokenTypeIds.ExportedType,
                TokenTypeIds.ManifestResource,
                TokenTypeIds.GenericParam,
                TokenTypeIds.GenericParamConstraint,
                TokenTypeIds.MethodSpec,

                TokenTypeIds.Document,
                TokenTypeIds.LocalScope,
                TokenTypeIds.LocalVariable,
                TokenTypeIds.LocalConstant,
                TokenTypeIds.ImportScope,

                InvalidTokenType,
                InvalidTokenType,
                InvalidTokenType,
                InvalidTokenType,
                InvalidTokenType
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static EntityHandle ConvertToHandle(uint taggedReference)
            {
                const uint TagMask = 0x0000001F;

                uint tokenType = TagToTokenTypeArray[taggedReference & TagMask];
                uint rowId = (taggedReference >> BitCount);

                if (tokenType == InvalidTokenType || ((rowId & ~TokenTypeIds.RIDMask) != 0))
                {
                    Throw.InvalidCodedIndex();
                }

                return new EntityHandle(tokenType | rowId);
            }
        }

        private static Exception UnexpectedHandleKind(HandleKind kind)
        {
            return new ArgumentException(SR.Format(SR.UnexpectedHandleKind, kind));
        }
    }
}
