// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection.Metadata.Ecma335;
using Xunit;

namespace System.Reflection.Metadata.Tests
{
    public class HandleComparerTests
    {
        [Fact]
        public void CompareEntityHandles()
        {
            Assert.True(Handle.Compare(new EntityHandle(0x02000001), new EntityHandle(0x02000002)) < 0);
            Assert.True(Handle.Compare(new EntityHandle(0x02000002), new EntityHandle(0x02000001)) > 0);
            Assert.True(Handle.Compare(new EntityHandle(0x02000001), new EntityHandle(0x02000001)) == 0);

            // token type is ignored
            Assert.True(Handle.Compare(new EntityHandle(0x20000001), new EntityHandle(0x21000002)) < 0);

            // virtual tokens follow non-virtual:
            Assert.True(Handle.Compare(new EntityHandle(0x82000001), new EntityHandle(0x02000002)) > 0);
            Assert.True(Handle.Compare(new EntityHandle(0x02000002), new EntityHandle(0x82000001)) < 0);
            Assert.True(Handle.Compare(new EntityHandle(0x82000001), new EntityHandle(0x82000001)) == 0);

            // make sure we won't overflow for extreme values:
            Assert.True(Handle.Compare(new EntityHandle(0xffffffff), new EntityHandle(0x00000000)) > 0);
            Assert.True(Handle.Compare(new EntityHandle(0x00000000), new EntityHandle(0xffffffff)) < 0);
            Assert.True(Handle.Compare(new EntityHandle(0xfffffffe), new EntityHandle(0xffffffff)) < 0);
            Assert.True(Handle.Compare(new EntityHandle(0xffffffff), new EntityHandle(0xfffffffe)) > 0);
            Assert.True(Handle.Compare(new EntityHandle(0xffffffff), new EntityHandle(0xffffffff)) == 0);
        }

        [Fact]
        public void CompareHandles()
        {
            Assert.True(Handle.Compare(new Handle(0x02, 0x00000001), new Handle(0x02, 0x00000002)) < 0);
            Assert.True(Handle.Compare(new Handle(0x02, 0x00000002), new Handle(0x02, 0x00000001)) > 0);
            Assert.True(Handle.Compare(new Handle(0x02, 0x00000001), new Handle(0x02, 0x00000001)) == 0);

            // token type is ignored
            Assert.True(Handle.Compare(new Handle(0x20, 0x00000001), new Handle(0x21, 0x00000002)) < 0);

            // virtual tokens follow non-virtual:
            Assert.True(Handle.Compare(new Handle(0x82, 0x00000001), new Handle(0x02, 0x00000002)) > 0);
            Assert.True(Handle.Compare(new Handle(0x02, 0x00000002), new Handle(0x82, 0x00000001)) < 0);
            Assert.True(Handle.Compare(new Handle(0x82, 0x00000001), new Handle(0x82, 0x00000001)) == 0);

            // make sure we won't overflow for extreme values:
            Assert.True(Handle.Compare(new Handle(0xff, 0x7fffffff), new Handle(0x00, 0x00000000)) > 0);
            Assert.True(Handle.Compare(new Handle(0x00, 0x00000000), new Handle(0xff, 0x7fffffff)) < 0);
            Assert.True(Handle.Compare(new Handle(0xff, 0x7ffffffe), new Handle(0xff, 0x7fffffff)) < 0);
            Assert.True(Handle.Compare(new Handle(0xff, 0x7fffffff), new Handle(0xff, 0x7ffffffe)) > 0);
            Assert.True(Handle.Compare(new Handle(0xff, 0x7fffffff), new Handle(0xff, 0x7fffffff)) == 0);
        }
    }
}
