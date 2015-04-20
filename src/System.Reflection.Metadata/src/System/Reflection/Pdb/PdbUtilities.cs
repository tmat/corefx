// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection.Metadata;

namespace System.Reflection.Pdb
{
    internal static class PdbUtilities
    {
        internal static int CeilingDiv(int x, int y)
        {
            return (x + y - 1) / y;
        }

        internal static int[] ReadSequenceInt32(ref BlobReader reader, int length)
        {
            var descriptor = new int[length];

            for (int i = 0; i < descriptor.Length; i++)
            {
                descriptor[i] = reader.ReadInt32();
            }

            return descriptor;
        }

        internal static Guid ReadGuid(ref BlobReader reader)
        {
            // TODO: avoid alloc
            return new Guid(reader.ReadBytes(16));
        }
    }
}
