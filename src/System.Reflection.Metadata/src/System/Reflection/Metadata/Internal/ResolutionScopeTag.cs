// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal static class ResolutionScopeTag
    {
        internal const int NumberOfBits = 2;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        internal static EntityHandle ConvertToHandle(uint resolutionScope)
        {
            const uint TagMask = 0x00000003;
            const uint TagToTokenTypeByteVector = TokenTypeIds.Module >> 24 | TokenTypeIds.ModuleRef >> 16 | TokenTypeIds.AssemblyRef >> 8 | TokenTypeIds.TypeRef;

            uint tokenType = (TagToTokenTypeByteVector >> ((int)(resolutionScope & TagMask) << 3)) << TokenTypeIds.RowIdBitCount;
            uint rowId = (resolutionScope >> NumberOfBits);

            if ((rowId & ~TokenTypeIds.RIDMask) != 0)
            {
                Throw.InvalidCodedIndex();
            }

            return new EntityHandle(tokenType | rowId);
        }
    }
}
