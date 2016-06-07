// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal static class ImplementationTag
    {
        internal const int NumberOfBits = 2;
        internal const TableMask TablesReferenced =
          TableMask.File
          | TableMask.AssemblyRef
          | TableMask.ExportedType;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EntityHandle ConvertToHandle(uint implementation)
        {
            const uint TagMask = 0x00000003;
            const uint TagToTokenTypeByteVector = TokenTypeIds.File >> 24 | TokenTypeIds.AssemblyRef >> 16 | TokenTypeIds.ExportedType >> 8;

            uint tokenType = (TagToTokenTypeByteVector >> ((int)(implementation & TagMask) << 3)) << TokenTypeIds.RowIdBitCount;
            uint rowId = (implementation >> NumberOfBits);

            if (tokenType == 0 || (rowId & ~TokenTypeIds.RIDMask) != 0)
            {
                Throw.InvalidCodedIndex();
            }

            return new EntityHandle(tokenType | rowId);
        }
    }
}
