using System;
using System.IO.Pipes;
using System.Threading;

namespace Tests.Utils
{
    internal class NamedPipeServer : AbstractServer
    {
        private readonly NamedPipeServerStream _pipeServer;

        public NamedPipeServer(string pipeName)
        {
            _pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.Out);
            Start(1000);
        }

        public override void Dispose()
        {
            base.Dispose();
            _pipeServer.Dispose();
        }

        protected override bool IsTimeoutException(Exception e)
        {
            return true; // $$ TODO
        }

        protected override void OnServerStarting()
        {
            _pipeServer.WaitForConnection();
        }

        protected override int Read(byte[] buffer)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            return _pipeServer.ReadAsync(buffer, 0, buffer.Length, cts.Token).Result;
        }
    }
}