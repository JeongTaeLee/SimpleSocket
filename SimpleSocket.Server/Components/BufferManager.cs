using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace SimpleSocket.Server.Components
{
    internal class BufferManager
    {
        private readonly int _totalBufferSize = 0;
        private readonly int _stubBufferSize = 0;
        private readonly Stack<int> _freeIndexPool = new Stack<int>();
        private readonly object _syncRoot = null;

        private byte[] _buffers = null;
        private int _currentIndex = 0;


        public BufferManager(int totalBufferSize, int stubBufferSize, object syncRoot = null)
        {
            if (0 >= totalBufferSize)
            {
                throw new ArgumentException("total buffer size cannot be less than 0.");
            }

            if (0 >= stubBufferSize)
            {
                throw new ArgumentException("stub buffer size cannot be less than 0.");
            }

            _totalBufferSize = totalBufferSize;
            _stubBufferSize = stubBufferSize;
            _syncRoot = syncRoot ?? new object();
        }

        public void InitBuffer()
        {
            _buffers = new byte[_totalBufferSize];
        }

        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            lock (_syncRoot)
            {
                if (_freeIndexPool.TryPop(out var index))
                {
                    args.SetBuffer(_buffers, index, _stubBufferSize);
                }
                else
                {
                    if ((_totalBufferSize - _stubBufferSize) < _currentIndex)
                    {
                        return false;
                    }

                    args.SetBuffer(_buffers, _currentIndex, _stubBufferSize);
                    _currentIndex = _stubBufferSize;
                }

                return true;
            }
        }

        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            lock (_syncRoot)
            {
                _freeIndexPool.Push(args.Offset);
                args.SetBuffer(null, 0, 0);
            }
        }
    }
}