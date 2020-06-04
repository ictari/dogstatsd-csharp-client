using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Utils
{
    internal class NamedPipeServer : AbstractServer
    {
        private readonly NamedPipeServerStream _pipeServer;

        public NamedPipeServer(string pipeName)
        {
            _pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In);
            Start(1000);
        }

        public override void Dispose()
        {
            base.Dispose();
            _pipeServer.Dispose();
        }

        protected override int? Read(byte[] buffer)
        {
            if (!_pipeServer.IsConnected)
            {
                try
                {
                    _pipeServer.WaitForConnection();
                }
                catch (IOException)
                {
                    return null;
                }
            }

            return _pipeServer.Read(buffer, 0, buffer.Length);
        }
    }
}