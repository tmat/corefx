// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal static class HasCustomAttributeTag
    {
        internal const int NumberOfBits = 5;
        internal const uint TagMask = 0x0000001F;

        // Arbitrary value not equal to any of the token types in the array. This includes 0 which is TokenTypeIds.Module.
        internal const uint InvalidTokenType = uint.MaxValue;

        internal static uint[] TagToTokenTypeArray =
        {
            TokenTypeIds.MethodDef,
            TokenTypeIds.FieldDef,
            TokenTypeIds.TypeRef,
            TokenTypeIds.TypeDef,
            TokenTypeIds.ParamDef,
            TokenTypeIds.InterfaceImpl,
            TokenTypeIds.MemberRef,
            TokenTypeIds.Module,
            TokenTypeIds.DeclSecurity,
            TokenTypeIds.Property,
            TokenTypeIds.Event,
            TokenTypeIds.Signature,
            TokenTypeIds.ModuleRef,
            TokenTypeIds.TypeSpec,
            TokenTypeIds.Assembly,
            TokenTypeIds.AssemblyRef,
            TokenTypeIds.File,
            TokenTypeIds.ExportedType,
            TokenTypeIds.ManifestResource,
            TokenTypeIds.GenericParam,
            TokenTypeIds.GenericParamConstraint,
            TokenTypeIds.MethodSpec,

            InvalidTokenType,
            InvalidTokenType,
            InvalidTokenType,
            InvalidTokenType,
            InvalidTokenType,
            InvalidTokenType,
            InvalidTokenType,
            InvalidTokenType,
            InvalidTokenType,
            InvalidTokenType
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EntityHandle ConvertToHandle(uint hasCustomAttribute)
        {
            uint tokenType = TagToTokenTypeArray[hasCustomAttribute & TagMask];
            uint rowId = (hasCustomAttribute >> NumberOfBits);

            if (tokenType == InvalidTokenType || ((rowId & ~TokenTypeIds.RIDMask) != 0))
            {
                Throw.InvalidCodedIndex();
            }

            return new EntityHandle(tokenType | rowId);
        }
    }
}
