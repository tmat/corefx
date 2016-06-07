// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal static class TypeOrMethodDefTag
    {
        internal const int NumberOfBits = 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EntityHandle ConvertToHandle(uint typeOrMethodDef)
        {
            const uint TagMask = 0x0000001;
            const uint TagToTokenTypeByteVector = TokenTypeIds.TypeDef >> 24 | TokenTypeIds.MethodDef >> 16;

            uint tokenType = (TagToTokenTypeByteVector >> ((int)(typeOrMethodDef & TagMask) << 3)) << TokenTypeIds.RowIdBitCount;
            uint rowId = (typeOrMethodDef >> NumberOfBits);

            if ((rowId & ~TokenTypeIds.RIDMask) != 0)
            {
                Throw.InvalidCodedIndex();
            }

            return new EntityHandle(tokenType | rowId);
        }
    }
}
