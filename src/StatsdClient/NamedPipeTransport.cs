using System.IO.Pipes;
using System.Threading;

namespace StatsdClient
{
    internal class NamedPipeTransport : ITransport
    {
        private readonly NamedPipeClientStream _namedPipe;
        private readonly object _lock = new object();

        public NamedPipeTransport(string pipeName)
        {
            _namedPipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
        }

        public TransportType TransportType => TransportType.NamedPipe;

        public bool Send(byte[] buffer, int length)
        {
            lock (_lock)
            {
                if (!_namedPipe.IsConnected)
                {
                    _namedPipe.Connect(1000);
                }

                var cts = new CancellationTokenSource();
                cts.CancelAfter(300);
                _namedPipe.WriteAsync(buffer, 0, length, cts.Token).Wait();
            }

            return true;
        }

        public void Dispose()
        {
            if (_namedPipe.IsConnected)
            {
                _namedPipe.WaitForPipeDrain();
            }

            _namedPipe.Dispose();
        }
    }
}