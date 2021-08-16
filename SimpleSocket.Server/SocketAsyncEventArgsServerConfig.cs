using System;

namespace SimpleSocket.Server
{
    public class SocketAsyncEventArgsServerConfig
    {
        public int recvBufferSize { get; set; } = 1024;
        public int sendBufferSize { get; set; } = 1024;
        public int maxConnection { get; set; } = 30000;

        private SocketAsyncEventArgsServerConfig(int recvBufferSize, int sendBufferSize, int maxConnection)
        {
            if (0 >= recvBufferSize)
            {
                throw new ArgumentException($"{recvBufferSize} cannot be equal to or less than zero");
            }

            if (0 >= sendBufferSize)
            {
                throw new ArgumentException($"{sendBufferSize} cannot be equal to or less than zero");
            }

            if (0 >= maxConnection)
            {
                throw new ArgumentException($"{maxConnection} cannot be equal to or less than zero");
            }

            this.recvBufferSize = recvBufferSize;
            this.sendBufferSize = sendBufferSize;
            this.maxConnection = maxConnection;
        }

        public class Builder
        {
            public int recvBufferSize { get; private set; } = 1024;
            public int sendBufferSize { get; private set; } = 1024;
            public int maxConnection { get; private set; } = 30000;

            public SocketAsyncEventArgsServerConfig Build()
                => new SocketAsyncEventArgsServerConfig(recvBufferSize, sendBufferSize, maxConnection);
            
            public Builder SetRecvBufferSize(int recvBufferSize)
            {
                if (0 >= recvBufferSize)
                {
                    throw new ArgumentException($"{recvBufferSize} cannot be equal to or less than zero");
                }

                this.recvBufferSize = recvBufferSize;

                return this;
            }

            public Builder SetSendBufferSize(int sendBufferSize)
            {
                if (0 >= sendBufferSize)
                {
                    throw new ArgumentException($"{sendBufferSize} cannot be equal to or less than zero");
                }

                this.sendBufferSize = sendBufferSize;

                return this;
            }

            public Builder SetMaxConnection(int maxConnection)
            {
                if (0 >= maxConnection)
                {
                    throw new ArgumentException($"{maxConnection} cannot be equal to or less than zero");
                }

                this.maxConnection = maxConnection;

                return this;
            }
        }
    }
}