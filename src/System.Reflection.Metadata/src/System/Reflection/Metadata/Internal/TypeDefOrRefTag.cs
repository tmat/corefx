// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal static class TypeDefOrRefTag
    {
        internal const int NumberOfBits = 2;

        // inlining improves perf of the tight loop in FindSystemObjectTypeDef by 25%
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EntityHandle ConvertToHandle(uint typeDefOrRefTag)
        {
            const uint TagMask = 0x00000003;
            const uint TagToTokenTypeByteVector = TokenTypeIds.TypeDef >> 24 | TokenTypeIds.TypeRef >> 16 | TokenTypeIds.TypeSpec >> 8;

            uint tokenType = (TagToTokenTypeByteVector >> ((int)(typeDefOrRefTag & TagMask) << 3)) << TokenTypeIds.RowIdBitCount;
            uint rowId = (typeDefOrRefTag >> NumberOfBits);

            if (tokenType == 0 || (rowId & ~TokenTypeIds.RIDMask) != 0)
            {
                Throw.InvalidCodedIndex();
            }

            return new EntityHandle(tokenType | rowId);
        }
    }
}
