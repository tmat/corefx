// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace System.Reflection.Pdb
{
    public sealed class PdbWriter
    {
        private static int CeilingDiv(int x, int y)
        {
            return (x + y - 1) / y;
        }

        private static readonly byte[] HeaderSignature = new byte[] 
        {
           0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66,
           0x74, 0x20, 0x43, 0x2F, 0x43, 0x2B, 0x2B, 0x20,
           0x4D, 0x53, 0x46, 0x20, 0x37, 0x2E, 0x30, 0x30,
           0x0D, 0x0A, 0x1A, 0x44, 0x53, 0x00, 0x00, 0x00
        };

        // Native PDB have implementation version PDBImpvVC70 = 20000404
        private const int PDBImpvVC70 = 20000404;

        public static void WritePortablePdb(Stream stream, uint stamp, Guid pdbId, byte[] debugMetadata)
        {
            WriteStreams(stream, stamp, pdbId, 1, new[] { "<PortablePdbMetadata>" }, new[] { debugMetadata });
        }

        public static void WriteStreams(Stream stream, uint stamp, Guid pdbId, int age, string[] streamNames, byte[][] streamContents)
        {
            Debug.Assert(streamNames.Length == streamContents.Length);

            int pageSize = 512;

            List<byte[]> streams = new List<byte[]>();

            const int reservedStreamsCount = 5;

            // <StreamTable>
            streams.Add(new byte[0]);

            // <Pdb>
            streams.Add(BuildPdbDataStream(stamp, pdbId, age, reservedStreamsCount, streamNames));

            // <Tpi>
            streams.Add(new byte[0]);

            // <Dbi>
            streams.Add(new byte[0]);

            // <Ipi>
            streams.Add(new byte[0]);

            Debug.Assert(streams.Count == reservedStreamsCount);

            // custom data streams:
            streams.AddRange(streamContents);

            int directorySize = CalculateDirectoryStreamSize(streams, pageSize);
            int directoryPageCount = CeilingDiv(directorySize, pageSize);
            int directoryDescriptorSize = 4 * directoryPageCount;

            int rootSize = directoryDescriptorSize;
            int rootPageCount = CeilingDiv(rootSize, pageSize);
            int rootDescriptorSize = 4 * rootPageCount;

            int headerSize = 52 + rootDescriptorSize;
            int headerPageCount = CeilingDiv(headerSize, pageSize);

            // TODO: increase page size if the data doesn't fit:
            Debug.Assert(headerPageCount == 1);

            // Two pages reserved for Free Page Maps
            int freePageMapPageCount = 2;

            int freePageMapPageStart = headerPageCount;
            int rootPageStart = freePageMapPageStart + freePageMapPageCount;
            int directoryPageStart = rootPageStart + rootPageCount;
            int dataPageStart = directoryPageStart + directoryPageCount;
            
            int dataPageCount = CalculateDataStreamsTotalPageCount(streams, pageSize);
            int totalPageCount = dataPageStart + dataPageCount;

            //
            // Header
            //

            var writer = new BinaryWriter(stream);

            // Signature:
            writer.Write(HeaderSignature);

            // Page Size:
            writer.Write(pageSize);

            // Free Page Map:
            writer.Write(freePageMapPageStart);

            // Pages Used:
            writer.Write(totalPageCount);

            // Directory Size:
            writer.Write(directorySize);

            // Reserved:
            writer.Write(0);

            // Root Descriptor:
            WriteDescriptor(writer, rootPageStart, rootPageCount);

            //
            // Free Page Map
            // 

            writer.Seek(freePageMapPageStart * pageSize, SeekOrigin.Begin);
            WriteFreePageMap(writer, totalPageCount, pageSize);

            //
            // Root Stream
            // 

            writer.Seek(rootPageStart * pageSize, SeekOrigin.Begin);
            WriteDescriptor(writer, directoryPageStart, directoryPageCount);

            //
            // Directory Stream
            // 

            writer.Seek(directoryPageStart * pageSize, SeekOrigin.Begin);

            // Stream Count
            writer.Write(streams.Count);

            // Stream Sizes
            for (int i = 0; i < streams.Count; i++)
            {
                writer.Write(streams[i].Length);
            }

            // Stream Descriptors
            WriteStreamDescriptors(writer, streams, dataPageStart, pageSize);

            //
            // Data Streams
            //

            WriteDataStreams(writer, streams, dataPageStart, pageSize);
        }

        private static void WriteFreePageMap(BinaryWriter writer, int usedPages, int pageSize)
        {
            int zeroBytes = usedPages / 8;

            int i = 0;
            while (i < zeroBytes)
            {
                writer.Write((byte)0x00);
                i++;
            }

            if ((usedPages % 8) != 0)
            {
                writer.Write(unchecked((byte)(0xff << (usedPages % 8))));
                i++;
            }

            while (i < pageSize)
            {
                writer.Write((byte)0xff);
                i++;
            }
        }

        private static int CalculateDataStreamsTotalPageCount(List<byte[]> streams, int pageSize)
        {
            int result = 0;
            for (int i = 0; i < streams.Count; i++)
            {
                result += CeilingDiv(streams[i].Length, pageSize);
            }

            return result;
        }

        private static void WriteDataStreams(BinaryWriter writer, List<byte[]> streams, int dataPageStart, int pageSize)
        {
            int streamPageStart = dataPageStart;
            for (int i = 0; i < streams.Count; i++)
            {
                writer.Seek(streamPageStart * pageSize, SeekOrigin.Begin);
                writer.Write(streams[i]);
                streamPageStart += CeilingDiv(streams[i].Length, pageSize);
            }

            writer.Flush();

            // align the last stream to page size:
            writer.BaseStream.SetLength(streamPageStart * pageSize);
        }

        private static void WriteStreamDescriptors(BinaryWriter writer, List<byte[]> streams, int dataPageStart, int pageSize)
        {
            int streamPageStart = dataPageStart;
            for (int i = 0; i < streams.Count; i++)
            {
                int streamPageCount = CeilingDiv(streams[i].Length, pageSize);
                WriteDescriptor(writer, streamPageStart, streamPageCount);
                streamPageStart += streamPageCount;
            }
        }

        private static void WriteDescriptor(BinaryWriter writer, int firstPage, int pageCount)
        {
            for (int p = 0; p < pageCount; p++)
            {
                writer.Write(firstPage + p);
            }
        }

        private static int CalculateDirectoryStreamSize(List<byte[]> streams, int pageSize)
        {
            int result = 1 + streams.Count;

            foreach (var stream in streams)
            {
                result += CeilingDiv(stream.Length, pageSize);
            }

            return 4 * result;
        }

        private static byte[] BuildPdbDataStream(uint stamp, Guid pdbId, int age, int reservedStreamsCount, string[] streamNames)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            // Version: 
            writer.Write(PDBImpvVC70);

            // Stamp
            writer.Write(stamp);

            // Age
            writer.Write(age);

            // Guid
            writer.Write(pdbId.ToByteArray());

            // NameMapOffset (to be filled later)
            writer.Write(0U);

            // String heap:
            long stringHeapPosition = writer.BaseStream.Position;
            Debug.Assert(stringHeapPosition == 32);

            int[] nameOffsets = new int[streamNames.Length];
            for (int i = 0; i < streamNames.Length; i++)
            {
                nameOffsets[i] = (int)(writer.BaseStream.Position - stringHeapPosition);

                // null-terminated UTF-8 string
                writer.Write(Encoding.UTF8.GetBytes(streamNames[i]));
                writer.Write((byte)0);
            }

            long nameMapPosition = writer.BaseStream.Position;

            // NameCount
            writer.Write(streamNames.Length);

            // SlotCount
            writer.Write(streamNames.Length);

            // PresentBitmap: size & bits
            WriteBitmap(writer, streamNames.Length);

            // DeletedBitmap: size & bits
            WriteBitmap(writer, 0);

            // Stream name offsets and indices:
            for (int i = 0; i < streamNames.Length; i++)
            {
                writer.Write(nameOffsets[i]);
                writer.Write(reservedStreamsCount + i);
            }

            // NextStreamIndex
            writer.Write(reservedStreamsCount + streamNames.Length);

            writer.BaseStream.Position = stringHeapPosition - 4;
            writer.Write((int)(nameMapPosition - stringHeapPosition));

            writer.Flush();
            return stream.ToArray();
        }

        private static void WriteBitmap(BinaryWriter writer, int bitCount)
        {
            if (bitCount == 0)
            {
                writer.Write(0);
                return;
            }

            const int bitsPerWord = 4 * 8;

            int bitmapWordCount = CeilingDiv(bitCount, bitsPerWord);

            // size in 4B words 
            writer.Write(bitmapWordCount);

            for (int i = 0; i < bitmapWordCount - 1; i++)
            {
                writer.Write(0xffffffff);
            }

            int remainingBits = bitCount % bitsPerWord;
            if (remainingBits == 0)
            {
                // the bit count is a multiple of 32, the last word is all 1s
                writer.Write(0xffffffff);
            }
            else
            {
                writer.Write((1U << remainingBits) - 1);
            }
        }
    }
}
