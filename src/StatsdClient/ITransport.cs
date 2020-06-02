using System;

namespace StatsdClient
{
    internal enum TransportType
    {
        UDS,
        UDP,
        NamedPipe,
    }

    internal interface ITransport : IDisposable
    {
        TransportType TransportType { get; }

        bool Send(byte[] buffer, int length);
    }
}