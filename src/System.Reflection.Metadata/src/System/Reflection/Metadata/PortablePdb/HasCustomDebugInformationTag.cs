// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Reflection.Metadata.Ecma335
{
    internal static class HasCustomDebugInformationTag
    {
        internal const int NumberOfBits = 5;
        internal const int LargeRowSize = 0x00000001 << (16 - NumberOfBits);        
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

            TokenTypeIds.Document,
            TokenTypeIds.LocalScope,
            TokenTypeIds.LocalVariable,
            TokenTypeIds.LocalConstant,
            TokenTypeIds.ImportScope,

            InvalidTokenType,
            InvalidTokenType,
            InvalidTokenType,
            InvalidTokenType,
            InvalidTokenType
        };

        internal const TableMask TablesReferenced =
          TableMask.MethodDef
          | TableMask.Field
          | TableMask.TypeRef
          | TableMask.TypeDef
          | TableMask.Param
          | TableMask.InterfaceImpl
          | TableMask.MemberRef
          | TableMask.Module
          | TableMask.DeclSecurity
          | TableMask.Property
          | TableMask.Event
          | TableMask.StandAloneSig
          | TableMask.ModuleRef
          | TableMask.TypeSpec
          | TableMask.Assembly
          | TableMask.AssemblyRef
          | TableMask.File
          | TableMask.ExportedType
          | TableMask.ManifestResource
          | TableMask.GenericParam
          | TableMask.GenericParamConstraint
          | TableMask.MethodSpec
          | TableMask.Document
          | TableMask.LocalScope
          | TableMask.LocalVariable
          | TableMask.LocalConstant
          | TableMask.ImportScope;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EntityHandle ConvertToHandle(uint taggedReference)
        {
            uint tokenType = TagToTokenTypeArray[taggedReference & TagMask];
            uint rowId = (taggedReference >> NumberOfBits);

            if (tokenType == InvalidTokenType || ((rowId & ~TokenTypeIds.RIDMask) != 0))
            {
                Throw.InvalidCodedIndex();
            }

            return new EntityHandle(tokenType | rowId);
        }
    }
}
