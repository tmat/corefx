// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Internal;

namespace System.Reflection.Pdb
{
    internal sealed class PdbStreamDirectory
    {
        internal const int PdbStreamIndex = 1;

        private readonly int[] _sizes;
        private readonly int[][] _descriptors;
        public readonly ImmutableDictionary<string, int> NameMap;

        public PdbStreamDirectory(int[] sizes, int[][] descriptors, ImmutableDictionary<string, int> nameMap)
        {
            _sizes = sizes;
            _descriptors = descriptors;
            NameMap = nameMap;
        }

        public bool TryGetIndex(string name, out int index)
        {
            return NameMap.TryGetValue(name, out index);
        }

        public int GetSize(int index)
        {
            return _sizes[index];
        }

        public int[] GetDescriptor(int index)
        {
            return _descriptors[index];
        }
    }
}
