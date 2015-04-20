// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Xunit;

namespace System.Reflection.Pdb.Tests
{
    public class PDbReaderTests
    {
        [Fact]
        public void Test1()
        {
            var pdbStream = new MemoryStream(TestResources.Pdb.Minimal);
            var reader = new PdbReader(pdbStream);
            var headers = reader.Headers;

            Assert.Equal(0x01312e94, headers.ImplementationVersion);
            Assert.Equal(0U, headers.Stamp);
            Assert.Equal(1, headers.Age);

            // <StreamTable>
            Assert.Equal(0, reader.GetDataStream(0).Length);

            // <Pdb>
            Assert.Equal(
                new byte[] { 0x94, 0x2E, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                reader.GetDataStream(1).GetContent());

            // <Tpi>
            Assert.Equal(0, reader.GetDataStream(2).Length);

            // <Dbi>
            Assert.Equal(0, reader.GetDataStream(3).Length);

            // <Ipi>
            Assert.Equal(0, reader.GetDataStream(4).Length);

            Assert.Equal(0, headers.NamedStreams.Count);
        }
    }
}
