namespace StatsdClient.Bufferize
{
    /// <summary>
    /// BufferBuilderHandler forwards metrics to ITransport and update telemetry.
    /// </summary>
    internal class BufferBuilderHandler : IBufferBuilderHandler
    {
        private readonly Telemetry _telemetry;
        private readonly ITransport _statsSender;

        public BufferBuilderHandler(
            Telemetry telemetry,
            ITransport transport)
        {
            _telemetry = telemetry;
            _statsSender = transport;
        }

        public void Handle(byte[] buffer, int length)
        {
            if (_statsSender.Send(buffer, length))
            {
                _telemetry.OnPacketSent(length);
            }
            else
            {
                _telemetry.OnPacketDropped(length);
            }
        }
    }
}