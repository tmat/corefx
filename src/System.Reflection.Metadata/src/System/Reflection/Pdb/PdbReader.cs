// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Reflection.Internal;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading;

namespace System.Reflection.Pdb
{
    public sealed class PdbReader : IDisposable
    {
        // May be null in the event that the entire image is not
        // deemed necessary and we have been instructed to read
        // the image contents without being lazy.
        private MemoryBlockProvider _pdbImage;

        private PdbHeaders _lazyPdbHeaders;

        private AbstractMemoryBlock _lazyImageBlock;
        private AbstractMemoryBlock[] _lazyDataBlocks;

        public unsafe PdbReader(byte* pdbImage, int size)
        {
            if (pdbImage == null)
            {
                throw new ArgumentNullException("pdbImage");
            }

            if (size < 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            _pdbImage = new ExternalMemoryBlockProvider(pdbImage, size);
        }

        public PdbReader(Stream pdbStream)
            : this(pdbStream, PEStreamOptions.Default)
        {
        }

        public PdbReader(Stream pdbStream, PEStreamOptions options)
            : this(pdbStream, options, (int?)null)
        {
        }

        public PdbReader(Stream pdbStream, PEStreamOptions options, int size)
            : this(pdbStream, options, (int?)size)
        {
        }

        public PdbReader(ImmutableArray<byte> pdbImage)
        {
            if (pdbImage.IsDefault)
            {
                throw new ArgumentNullException("pdbImage");
            }

            _pdbImage = new ByteArrayMemoryProvider(pdbImage);
        }

        private unsafe PdbReader(Stream pdbStream, PEStreamOptions options, int? sizeOpt)
        {
            if (pdbStream == null)
            {
                throw new ArgumentNullException("pdbStream");
            }

            if (!pdbStream.CanRead || !pdbStream.CanSeek)
            {
                throw new ArgumentException(MetadataResources.StreamMustSupportReadAndSeek, "pdbStream");
            }

            if (!options.IsValid())
            {
                throw new ArgumentOutOfRangeException("options");
            }

            long start = pdbStream.Position;
            int size = PEBinaryReader.GetAndValidateSize(pdbStream, sizeOpt);

            bool closeStream = true;
            try
            {
                bool isFileStream = FileStreamReadLightUp.IsFileStream(pdbStream);

                if ((options & (PEStreamOptions.PrefetchMetadata | PEStreamOptions.PrefetchEntireImage)) == 0)
                {
                    _pdbImage = new StreamMemoryBlockProvider(pdbStream, start, size, isFileStream, (options & PEStreamOptions.LeaveOpen) != 0);
                    closeStream = false;
                }
                else
                {
                    // Read in the entire image or metadata blob:
                    if ((options & PEStreamOptions.PrefetchEntireImage) != 0)
                    {
                        var imageBlock = StreamMemoryBlockProvider.ReadMemoryBlockNoLock(pdbStream, isFileStream, 0, (int)Math.Min(pdbStream.Length, int.MaxValue));
                        _lazyImageBlock = imageBlock;
                        _pdbImage = new ExternalMemoryBlockProvider(imageBlock.Pointer, imageBlock.Size);

                        // if the caller asked for metadata initialize the PE headers (calculates metadata offset):
                        if ((options & PEStreamOptions.PrefetchMetadata) != 0)
                        {
                            _lazyPdbHeaders = new PdbHeaders(_pdbImage);
                        }
                    }
                    else
                    {
                        // TODO:

                        // The peImage is left null, but the lazyMetadataBlock is initialized up front.
                        // _lazyPdbHeaders = new PdbHeaders(pdbStream, size);
                        //_lazyMetadataBlock = StreamMemoryBlockProvider.ReadMemoryBlockNoLock(pdbStream, isFileStream, _lazyPdbHeaders.MetadataStartOffset, _lazyPEHeaders.MetadataSize);
                    }
                    // We read all we need, the stream is going to be closed.
                }
            }
            finally
            {
                if (closeStream && (options & PEStreamOptions.LeaveOpen) == 0)
                {
                    pdbStream.Dispose();
                }
            }
        }

        public void Dispose()
        {
            var image = _pdbImage;
            if (image != null)
            {
                image.Dispose();
                _pdbImage = null;
            }

            var imageBlock = _lazyImageBlock;
            if (imageBlock != null)
            {
                imageBlock.Dispose();
                _lazyImageBlock = null;
            }

            var dataBlocks = _lazyDataBlocks;
            if (dataBlocks != null)
            {
                foreach (var block in dataBlocks)
                {
                    if (block != null)
                    {
                        block.Dispose();
                    }
                }

                _lazyDataBlocks = null;
            }
        }

        public PdbHeaders Headers
        {
            get
            {
                if (_lazyPdbHeaders == null)
                {
                    InitializePdbHeaders();
                }

                return _lazyPdbHeaders;
            }
        }

        private void InitializePdbHeaders()
        {
            Debug.Assert(_pdbImage != null);
            PdbHeaders headers = new PdbHeaders(_pdbImage);
            Interlocked.CompareExchange(ref _lazyPdbHeaders, headers, null);
        }

        public PEMemoryBlock GetDataStream(string name)
        {
            // TODO: name -> index
            return GetDataStream(0);
        }

        public PEMemoryBlock GetDataStream(int index)
        {
            var descriptor = Headers.StreamDirectory.GetDescriptor(index);
            var size = Headers.StreamDirectory.GetSize(index);

            if (size == 0)
            {
                return default(PEMemoryBlock);
            }

            return new PEMemoryBlock(PdbDataStream.CreateDataBlock(_pdbImage, descriptor, size, Headers.PageSize));
        }
    }
}
