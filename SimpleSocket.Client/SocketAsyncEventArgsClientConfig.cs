using System;

namespace SimpleSocket.Client
{
    public class SocketAsyncEventArgsClientConfig
    {
        public readonly int recvBufferSize = 0;
        public readonly int sendBufferSize = 0;

        private SocketAsyncEventArgsClientConfig(int recvBufferSize, int sendBufferSize)
        {
            if (0 > recvBufferSize)
            {
                throw new ArgumentException($"Invalid {nameof(recvBufferSize)} - recv buffer size({recvBufferSize})");
            }
            
            if (0 > sendBufferSize)
            {
                throw new ArgumentException($"Invalid {nameof(sendBufferSize)} - recv buffer size({sendBufferSize})");
            }

            this.recvBufferSize = recvBufferSize;
            this.sendBufferSize = sendBufferSize;
        }
        
        public class Builder
        {
            public int recvBufferSize { get; private set; } = 1024;
            public int sendBufferSize { get; private set; } = 1024;

            public SocketAsyncEventArgsClientConfig Build()
            {
                return new SocketAsyncEventArgsClientConfig(recvBufferSize, sendBufferSize);
            }
            
            public void SetRecvBufferSize(int recvBufferSize)
            {
                if (0 > recvBufferSize)
                {
                    throw new ArgumentException($"Invalid {nameof(recvBufferSize)} - recv buffer size({recvBufferSize})");
                }

                this.recvBufferSize = recvBufferSize;
            }

            public void SetSendBufferSize(int sendBufferSize)
            {
                if (0 > sendBufferSize)
                {
                    throw new ArgumentException($"Invalid {nameof(sendBufferSize)} - recv buffer size({sendBufferSize})");
                }

                this.sendBufferSize = sendBufferSize;
            }
        }
    }
}