using System.Text;

namespace StatsdClient
{
    internal struct RawMetric
    {
        public RawMetric(StringBuilder buffer)
        {
            Buffer = buffer;
        }

        public StringBuilder Buffer { get; private set; }
    }
}