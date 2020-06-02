using System;
using System.Net;
using System.Net.Sockets;

namespace StatsdClient
{
    internal class UDPTransport : ITransport
    {
        private readonly Socket _socket;
        private readonly IPEndPoint _endPoint;

        public UDPTransport(IPEndPoint endPoint)
        {
            TransportType = StatsSenderTransportType.UDP;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                // When closing, wait 2 seconds to send data.
                _socket.LingerState = new LingerOption(true, 2);
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.ProtocolOption)
            {
                // It is not supported on Windows for Dgram with UDP.
            }

            _endPoint = endPoint;
        }

        public StatsSenderTransportType TransportType { get; }

        /// <summary>
        /// Send the buffer.
        /// Must be thread safe.
        /// </summary>
        public bool Send(byte[] buffer, int length)
        {
            _socket.SendTo(buffer, 0, length, SocketFlags.None, _endPoint);
            return true;
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}