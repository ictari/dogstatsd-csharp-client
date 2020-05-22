using System;
using System.Text;

namespace StatsdClient
{
    internal struct RawMetric
    {
        public readonly byte[] _buffer;

        public RawMetric(byte[] buffer, int length)
        {
            _buffer = buffer;
            BytesLength = length;
        }

        public RawMetric(string buffer) // $$$ Temporary constructor: TO REMOVE
        {
            _buffer = Encoding.UTF8.GetBytes(buffer);
            BytesLength = _buffer.Length;
        }

        public int BytesLength { get; }

        public int CopyTo(byte[] buffer, int startIndex)
        {
            Array.Copy(_buffer, 0, buffer, startIndex, BytesLength);
            return BytesLength;
        }
    }
}