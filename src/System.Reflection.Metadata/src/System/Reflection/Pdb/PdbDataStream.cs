// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection.Internal;

namespace System.Reflection.Pdb
{
    public sealed class PdbDataStream
    {
        internal unsafe static AbstractMemoryBlock CreateDataBlock(MemoryBlockProvider pdbImage, int[] descriptor, int size, int pageSize)
        {
            if (descriptor.Length == 0)
            {
                // TODO:
                return null;
            }

            if (IsContiguousDataStream(descriptor))
            {
                return pdbImage.GetMemoryBlock(descriptor[0] * pageSize, size);
            }

            // TODO: load to memory
            throw new NotImplementedException();
        }

        internal static bool IsContiguousDataStream(int[] descriptor)
        {
            if (descriptor.Length == 0)
            {
                return true;
            }

            int firstPageIndex = descriptor[0];
            for (int i = 1; i < descriptor.Length; i++)
            {
                if (descriptor[i] != firstPageIndex + i)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
