namespace StatsdClient
{
    internal struct RawMetric
    {
        public RawMetric(string buffer)
        {
            Buffer = buffer;
        }

        public string Buffer { get; private set; }
    }
}