// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Internal;
using System.Reflection.Metadata;

namespace System.Reflection.Pdb
{
    public sealed class PdbHeaders
    {
        public static readonly ImmutableArray<byte> HeaderSignature = ImmutableArray.Create<byte>(
            0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66,
            0x74, 0x20, 0x43, 0x2F, 0x43, 0x2B, 0x2B, 0x20,
            0x4D, 0x53, 0x46, 0x20, 0x37, 0x2E, 0x30, 0x30,
            0x0D, 0x0A, 0x1A, 0x44, 0x53, 0x00, 0x00, 0x00);

        private const int HeaderSize = 52;

        public readonly int ImplementationVersion;
        public readonly uint Stamp;
        public readonly int Age;
        public readonly Guid Id;
        public readonly int PageSize;

        internal readonly PdbStreamDirectory StreamDirectory;

        internal unsafe PdbHeaders(MemoryBlockProvider pdbImage)
        {
            int directorySize;
            
            using (var block = pdbImage.GetMemoryBlock(0, HeaderSize))
            {
                var reader = new BlobReader(block.GetSlice());

                ValidateHeaderSignature(ref reader);

                PageSize = reader.ReadInt32();

                // 4B free page map - ignore
                // 4B pages used - ignore
                reader.SkipBytes(8);

                directorySize = reader.ReadInt32();
            }

            int directoryDescriptorLength = PdbUtilities.CeilingDiv(directorySize, PageSize);
            int rootSize = 4 * directoryDescriptorLength;
            int rootDescriptorLength = PdbUtilities.CeilingDiv(rootSize, PageSize);
            int rootDescriptorSize = 4 * rootDescriptorLength;

            // root
            int[] rootDescriptor;
            using (var block = pdbImage.GetMemoryBlock(HeaderSize, rootDescriptorSize))
            {
                var reader = new BlobReader(block.GetSlice());
                rootDescriptor = PdbUtilities.ReadSequenceInt32(ref reader, rootDescriptorLength);
            }

            // directory
            int[] directoryDescriptor;
            using (var block = PdbDataStream.CreateDataBlock(pdbImage, rootDescriptor, rootSize, PageSize))
            {
                var reader = new BlobReader(block.GetSlice());
                directoryDescriptor = PdbUtilities.ReadSequenceInt32(ref reader, directoryDescriptorLength);
            }

            int[] streamSizes;
            int[][] streamDescriptors;
            using (var block = PdbDataStream.CreateDataBlock(pdbImage, directoryDescriptor, directorySize, PageSize))
            {
                var reader = new BlobReader(block.GetSlice());

                int streamCount = reader.ReadInt32();
                streamSizes = PdbUtilities.ReadSequenceInt32(ref reader, streamCount);

                // stream descriptors
                streamDescriptors = new int[streamCount][];
                for (int i = 0; i < streamDescriptors.Length; i++)
                {
                    streamDescriptors[i] = PdbUtilities.ReadSequenceInt32(ref reader, PdbUtilities.CeilingDiv(streamSizes[i], PageSize));
                }
            }

            var pdbStreamDescriptor = streamDescriptors[PdbStreamDirectory.PdbStreamIndex];
            var pdbStreamSize = streamSizes[PdbStreamDirectory.PdbStreamIndex];

            ImmutableDictionary<string, int>.Builder nameMapBuilder;
            using (var block = PdbDataStream.CreateDataBlock(pdbImage, pdbStreamDescriptor, pdbStreamSize, PageSize))
            {
                MemoryBlock slice = block.GetSlice();

                var reader = new BlobReader(slice);
                ImplementationVersion = reader.ReadInt32();
                Stamp = reader.ReadUInt32();
                Age = reader.ReadInt32();
                Id = PdbUtilities.ReadGuid(ref reader);

                int stringHeapSize = reader.ReadInt32();

                // UTF8 encoded names are stored here
                var stringHeap = slice.GetMemoryBlockAt(reader.Offset, stringHeapSize);

                // name map follows the heap:
                reader.SkipBytes(stringHeapSize);

                int nameCount = reader.ReadInt32();

                // slot count
                reader.SkipBytes(sizeof(int));

                // present bitmap size
                int presentBitmapSize = reader.ReadInt32();
                reader.SkipBytes(presentBitmapSize * sizeof(uint));

                // deleted bitmap size
                int deletedBitmapSize = reader.ReadInt32();
                reader.SkipBytes(deletedBitmapSize * sizeof(uint));

                nameMapBuilder = ImmutableDictionary.CreateBuilder<string, int>();
                for (int i = 0; i < nameCount; i++)
                {
                    int nameOffset = reader.ReadInt32();
                    int nameIndex = reader.ReadInt32();

                    int bytesRead;
                    string name = stringHeap.PeekUtf8NullTerminated(nameOffset, null, MetadataStringDecoder.DefaultUTF8, out bytesRead);

                    nameMapBuilder.Add(name, nameIndex);
                }
            }

            StreamDirectory = new PdbStreamDirectory(streamSizes, streamDescriptors, nameMapBuilder.ToImmutable());
        }

        public ImmutableDictionary<string, int> NamedStreams
        {
            get
            {
                return StreamDirectory.NameMap;
            }
        }

        private static void ValidateHeaderSignature(ref BlobReader reader)
        {
            for (int i = 0; i < HeaderSignature.Length; i++)
            {
                if (reader.ReadByte() != HeaderSignature[i])
                {
                    throw new BadImageFormatException("Invalid PDB signature");
                }
            }
        }
    }
}
