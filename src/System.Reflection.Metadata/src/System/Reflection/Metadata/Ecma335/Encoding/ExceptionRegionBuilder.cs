// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace System.Reflection.Metadata.Ecma335
{
    public sealed class ExceptionRegionBuilder
    {
        private readonly ImmutableArray<int>.Builder _regions;

        public ExceptionRegionBuilder()
        {
            _regions = ImmutableArray.CreateBuilder<int>();
        }

        internal void Try(int offset)
        {
            _regions.Add(offset);
        }
    }
}
