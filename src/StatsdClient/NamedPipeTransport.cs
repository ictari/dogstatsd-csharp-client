namespace StatsdClient
{
    internal class NamedPipeTransport : ITransport
    {
        public NamedPipeTransport(string pipeName)
        {
        }

        public TransportType TransportType => throw new System.NotImplementedException();

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool Send(byte[] buffer, int length)
        {
            throw new System.NotImplementedException();
        }
    }
}