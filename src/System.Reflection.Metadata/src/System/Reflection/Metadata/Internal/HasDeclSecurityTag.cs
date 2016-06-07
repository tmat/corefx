// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal static class HasDeclSecurityTag
    {
        internal const int NumberOfBits = 2;
        internal const uint TagMask = 0x00000003;
        internal const uint TagToTokenTypeByteVector = (TokenTypeIds.TypeDef >> 24) | (TokenTypeIds.MethodDef >> 16) | (TokenTypeIds.Assembly >> 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EntityHandle ConvertToHandle(uint hasDeclSecurity)
        {
            uint tokenType = (TagToTokenTypeByteVector >> ((int)(hasDeclSecurity & TagMask) << 3)) << TokenTypeIds.RowIdBitCount;
            uint rowId = (hasDeclSecurity >> NumberOfBits);

            if (tokenType == 0 || (rowId & ~TokenTypeIds.RIDMask) != 0)
            {
                Throw.InvalidCodedIndex();
            }

            return new EntityHandle(tokenType | rowId);
        }
    }
}
