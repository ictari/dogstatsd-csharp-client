using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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
                // Must be outside the loop to avoid GC issue.
                var cts = new CancellationTokenSource();
                bool isConnected = false;
                while (!isConnected)
                {
                    try
                    {
                        cts.CancelAfter(100);
                        _namedPipe.WriteAsync(buffer, 0, length, cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        return false;
                    }
                    catch (SocketException)
                    {
                        // $$ improved
                        _namedPipe.Connect(1000); // $$ there is a connectAsync
                        isConnected = true;
                    }
                }
            }

            return true;
        }

        public void Dispose()
        {
            _namedPipe.Dispose();
        }
    }
}