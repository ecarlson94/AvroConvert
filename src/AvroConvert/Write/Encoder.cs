﻿using System;
using System.Collections.Generic;
using System.IO;
using SolTechnology.Avro.Codec;
using SolTechnology.Avro.Constants;
using SolTechnology.Avro.Exceptions;
using SolTechnology.Avro.Helpers;

namespace SolTechnology.Avro.Write
{
    public class Encoder : IDisposable
    {
        public delegate void WriteItem(object value, IWriter encoder);

        private readonly Schema.Schema _schema;
        private readonly AbstractCodec _codec;
        private readonly Stream _stream;
        private MemoryStream _blockStream;
        private readonly IWriter _encoder;
        private IWriter _blockEncoder;
        private readonly WriteItem _writer;
        private byte[] _syncData;
        private bool _isOpen;
        private bool _headerWritten;
        private int _blockCount;
        private readonly int _syncInterval;
        private readonly Metadata _metadata;


        public Encoder(Schema.Schema schema, Stream outStream)
        {
            _codec = AbstractCodec.CreateCodec(AbstractCodec.Type.Null);
            _stream = outStream;
            _metadata = new Metadata();
            _schema = schema;
            _syncInterval = DataFileConstants.DefaultSyncInterval;

            _blockCount = 0;
            _encoder = new Writer(_stream);
            _blockStream = new MemoryStream();
            _blockEncoder = new Writer(_blockStream);

            _writer = Resolver.ResolveWriter(schema);

            _isOpen = true;
        }

        public void Append(object datum)
        {
            AssertOpen();
            EnsureHeader();

            _writer(datum, _blockEncoder);

            _blockCount++;
            WriteIfBlockFull();
        }

        private void EnsureHeader()
        {
            if (!_headerWritten)
            {
                WriteHeader();
                _headerWritten = true;
            }
        }

        public long Sync()
        {
            AssertOpen();
            WriteBlock();
            return _stream.Position;
        }

        private void WriteHeader()
        {
            _encoder.WriteFixed(DataFileConstants.AvroHeader);
            WriteMetaData();
            WriteSyncData();
        }

        private void AssertOpen()
        {
            if (!_isOpen) throw new AvroRuntimeException("Cannot complete operation: avro file/stream not open");
        }

        private void WriteMetaData()
        {
            // Add sync, code & schema to metadata
            GenerateSyncData();
            _metadata.Add(DataFileConstants.CodecMetadataKey, _codec.Name);
            _metadata.Add(DataFileConstants.SchemaMetadataKey, _schema.ToString());

            // write metadata 
            int size = _metadata.GetSize();
            _encoder.WriteInt(size);

            foreach (KeyValuePair<string, byte[]> metaPair in _metadata.GetValue())
            {
                _encoder.WriteString(metaPair.Key);
                _encoder.WriteBytes(metaPair.Value);
            }
            _encoder.WriteMapEnd();
        }

        private void WriteIfBlockFull()
        {
            if (_blockStream.Position >= _syncInterval)
                WriteBlock();
        }

        private void WriteBlock()
        {
            if (_blockCount > 0)
            {
                byte[] dataToWrite = _blockStream.ToArray();

                // write count 
                _encoder.WriteLong(_blockCount);

                // write data 
                _encoder.WriteBytes(_codec.Compress(dataToWrite));

                // write sync marker 
                _encoder.WriteFixed(_syncData);

                // reset / re-init block
                _blockCount = 0;
                _blockStream = new MemoryStream();
                _blockEncoder = new Writer(_blockStream);
            }
        }

        private void WriteSyncData()
        {
            _encoder.WriteFixed(_syncData);
        }

        private void GenerateSyncData()
        {
            _syncData = new byte[16];

            Random random = new Random();
            random.NextBytes(_syncData);
        }

        public void Dispose()
        {
            EnsureHeader();
            Sync();
            _stream.Flush();
            _stream.Dispose();
            _isOpen = false;
        }
    }
}
