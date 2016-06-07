// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal static class CustomAttributeTypeTag
    {
        internal const int NumberOfBits = 3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EntityHandle ConvertToHandle(uint customAttributeType)
        {
            const uint TagMask = 0x0000007;
            const ulong TagToTokenTypeByteVector = TokenTypeIds.MethodDef >> 8 | TokenTypeIds.MemberRef;

            uint tokenType = unchecked((uint)(TagToTokenTypeByteVector >> ((int)(customAttributeType & TagMask) << 3)) << TokenTypeIds.RowIdBitCount);
            uint rowId = (customAttributeType >> NumberOfBits);

            if (tokenType == 0 || (rowId & ~TokenTypeIds.RIDMask) != 0)
            {
                Throw.InvalidCodedIndex();
            }

            return new EntityHandle(tokenType | rowId);
        }
    }
}
