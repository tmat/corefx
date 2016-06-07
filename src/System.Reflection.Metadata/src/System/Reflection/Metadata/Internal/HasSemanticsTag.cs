// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal static class HasSemanticsTag
    {
        internal const int NumberOfBits = 1;
        internal const TableMask TablesReferenced =
          TableMask.Event
          | TableMask.Property;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EntityHandle ConvertToHandle(uint hasSemantic)
        {
            const uint TagMask = 0x00000001;
            const uint TagToTokenTypeByteVector = TokenTypeIds.Event >> 24 | TokenTypeIds.Property >> 16;

            uint tokenType = (TagToTokenTypeByteVector >> ((int)(hasSemantic & TagMask) << 3)) << TokenTypeIds.RowIdBitCount;
            uint rowId = (hasSemantic >> NumberOfBits);

            if ((rowId & ~TokenTypeIds.RIDMask) != 0)
            {
                Throw.InvalidCodedIndex();
            }

            return new EntityHandle(tokenType | rowId);
        }
    }
}
