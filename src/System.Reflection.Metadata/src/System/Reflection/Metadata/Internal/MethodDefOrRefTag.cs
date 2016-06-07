// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal static class MethodDefOrRefTag
    {
        internal const int NumberOfBits = 1;
        internal const TableMask TablesReferenced =
          TableMask.MethodDef
          | TableMask.MemberRef;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EntityHandle ConvertToHandle(uint methodDefOrRef)
        {
            const uint TagMask = 0x00000001;
            const uint TagToTokenTypeByteVector = TokenTypeIds.MethodDef >> 24 | TokenTypeIds.MemberRef >> 16;

            uint tokenType = (TagToTokenTypeByteVector >> ((int)(methodDefOrRef & TagMask) << 3)) << TokenTypeIds.RowIdBitCount;
            uint rowId = (methodDefOrRef >> NumberOfBits);

            if ((rowId & ~TokenTypeIds.RIDMask) != 0)
            {
                Throw.InvalidCodedIndex();
            }

            return new EntityHandle(tokenType | rowId);
        }
    }
}
