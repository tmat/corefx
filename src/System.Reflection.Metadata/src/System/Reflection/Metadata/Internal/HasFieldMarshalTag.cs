// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal static class HasFieldMarshalTag
    {
        internal const int NumberOfBits = 1;
        internal const TableMask TablesReferenced =
          TableMask.Field
          | TableMask.Param;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EntityHandle ConvertToHandle(uint hasFieldMarshal)
        {
            const uint TagMask = 0x00000001;
            const uint TagToTokenTypeByteVector = TokenTypeIds.FieldDef >> 24 | TokenTypeIds.ParamDef >> 16;

            uint tokenType = (TagToTokenTypeByteVector >> ((int)(hasFieldMarshal & TagMask) << 3)) << TokenTypeIds.RowIdBitCount;
            uint rowId = (hasFieldMarshal >> NumberOfBits);

            if ((rowId & ~TokenTypeIds.RIDMask) != 0)
            {
                Throw.InvalidCodedIndex();
            }

            return new EntityHandle(tokenType | rowId);
        }
    }
}
