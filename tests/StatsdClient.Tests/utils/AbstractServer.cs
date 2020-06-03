using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Utils
{
    internal abstract class AbstractServer : IDisposable
    {
        private readonly ManualResetEventSlim _serverStop = new ManualResetEventSlim(false);
        private readonly List<string> _messagesReceived = new List<string>();
        private Task _receiver;

        private volatile bool _shutdown = false;

        public virtual void Dispose()
        {        
            Stop();
        }

        public List<string> Stop()
        {
            if (!_shutdown)
            {
                _shutdown = true;
                _serverStop.Wait();
            }

            return _messagesReceived;
        }

        protected void Start(int bufferSize)
        {
            _receiver = Task.Run(() => ReadFromServer(bufferSize));
        }

        protected abstract int Read(byte[] buffer);

        protected abstract bool IsTimeoutException(Exception e);
        
        private void ReadFromServer(int bufferSize)
        {
            var buffer = new byte[bufferSize];

            while (true)
            {
                try
                {
                    var count = Read(buffer);
                    var message = System.Text.Encoding.UTF8.GetString(buffer, 0, count);
                    _messagesReceived.AddRange(message.Split("\n", StringSplitOptions.RemoveEmptyEntries));
                }
                catch (Exception e)
                {
                    if (IsTimeoutException(e) & _shutdown)
                    {
                        _serverStop.Set();
                        return;
                    }
                }
            }
        }
    }
}