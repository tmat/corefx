// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal static class HasConstantTag
    {
        internal const int NumberOfBits = 2;
        internal const TableMask TablesReferenced =
          TableMask.Field
          | TableMask.Param
          | TableMask.Property;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EntityHandle ConvertToHandle(uint hasConstant)
        {
            const uint TagMask = 0x00000003;
            const uint TagToTokenTypeByteVector = TokenTypeIds.FieldDef >> 24 | TokenTypeIds.ParamDef >> 16 | TokenTypeIds.Property >> 8;

            uint tokenType = (TagToTokenTypeByteVector >> ((int)(hasConstant & TagMask) << 3)) << TokenTypeIds.RowIdBitCount;
            uint rowId = (hasConstant >> NumberOfBits);

            if (tokenType == 0 || (rowId & ~TokenTypeIds.RIDMask) != 0)
            {
                Throw.InvalidCodedIndex();
            }

            return new EntityHandle(tokenType | rowId);
        }
    }
}
