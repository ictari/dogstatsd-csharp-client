using System;
using System.Text;

namespace StatsdClient
{
    public struct Message
    {
        public ArraySegment<byte> buffer;
        //public StringBuilder buffer;
    }
}