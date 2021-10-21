using System;
using System.Net.Sockets;

namespace SimpleSocket.Client
{
    public class SocketClientConfig
    {
        public const SocketType DEFAULT_SOCKET_TYPE = SocketType.Stream;
        public const ProtocolType DEFAULT_PROTOCOL_TYPE = ProtocolType.Tcp;
        public const bool DEFAULT_NO_DELAY = true;
        public const int DEFAULT_RECV_BUFFER_SIZE = 1024;
        public const int DEFAULT_SEND_BUFFER_SIZE = 1024;

        public readonly SocketType socketType;
        public readonly ProtocolType protocolType;
        public readonly string ip;
        public readonly int port;
        public readonly bool noDelay;
        public readonly int recvBufferSize;
        public readonly int sendBufferSize;


        private SocketClientConfig(SocketType socketType, ProtocolType protocolType, string ip, int port, bool noDelay, int recvBufferSize, int sendBufferSize)
        {
            if (SocketType.Unknown == socketType)
            {
                throw new ArgumentException($"Invalid {nameof(socketType)} - socket Type({socketType})");
            }

            if (ProtocolType.Unknown == protocolType)
            {
                throw new ArgumentException($"Invalid {nameof(protocolType)} - protocol Type({protocolType})");
            }

            if (string.IsNullOrEmpty(ip))
            {
                throw new ArgumentException($"Invalid {nameof(ip)} - ip({ip})");
            }

            if (0 > port)
            {
                throw new ArgumentException($"Invalid {nameof(port)} - port({port})");
            }

            if (0 >= recvBufferSize)
            {
                throw new ArgumentException($"{nameof(recvBufferSize)} cannot be equal to or less than zero");
            }
            
            if (0 >= sendBufferSize)
            {
                throw new ArgumentException($"{nameof(sendBufferSize)} cannot be equal to or less than zero");
            }

            this.socketType = socketType;
            this.protocolType = protocolType;
            this.ip = ip;
            this.port = port;
            this.noDelay = noDelay;
            this.recvBufferSize = recvBufferSize;
            this.sendBufferSize = sendBufferSize;
        }

        public class Builder
        {
            public SocketType socketType { get; private set; } = SocketType.Stream;
            public ProtocolType protocolType { get; private set; } = ProtocolType.Tcp;
            public string ip { get; private set; }
            public int port { get; private set; }
            public bool noDelay { get; private set; } = DEFAULT_NO_DELAY;
            public int recvBufferSize { get; private set; } = DEFAULT_RECV_BUFFER_SIZE;
            public int sendBufferSize { get; private set; } = DEFAULT_SEND_BUFFER_SIZE;

            public Builder(string ip, int port)
            {
                if (string.IsNullOrEmpty(ip))
                {
                    throw new ArgumentException($"Invalid {nameof(ip)} - ip({ip})");
                }

                if (0 > port)
                {
                    throw new ArgumentException($"Invalid {nameof(port)} - port({port})");
                }

                this.ip = ip;
                this.port = port;
            }

            public SocketClientConfig Build()
            {
                return new SocketClientConfig(socketType, protocolType, ip, port, noDelay, recvBufferSize, sendBufferSize);
            }

            public Builder SetSocketType(SocketType socketType)
            {
                if (SocketType.Unknown == socketType)
                {
                    throw new ArgumentException($"Invalid {nameof(socketType)} - socket Type({socketType})");
                }

                this.socketType = socketType;

                return this;
            }

            public Builder SetProtocolType(ProtocolType protocolType)
            {
                if (ProtocolType.Unknown == protocolType)
                {
                    throw new ArgumentException($"Invalid {nameof(protocolType)} - protocol Type({protocolType})");
                }

                this.protocolType = protocolType;

                return this;
            }

            public Builder SetIp(string ip)
            {
                if (string.IsNullOrEmpty(ip))
                {
                    throw new ArgumentException($"Invalid {nameof(ip)} - ip({ip})");
                }

                this.ip = ip;

                return this;
            }

            public Builder SetPort(int port)
            {
                if (0 > port)
                {
                    throw new ArgumentException($"Invalid {nameof(port)} - port({port})");
                }

                this.port = port;

                return this;
            }

            public Builder SetNoDelay(bool noDelay)
            {
                this.noDelay = noDelay;

                return this;
            }

            public Builder SetRecvBufferSize(int recvBufferSize)
            {
                if (0 >= recvBufferSize)
                {
                    throw new ArgumentException($"{nameof(recvBufferSize)} cannot be equal to or less than zero");
                }

                this.recvBufferSize = recvBufferSize;

                return this;
            }

            public Builder SetSendBufferSize(int sendBufferSize)
            {
                if (0 >= sendBufferSize)
                {
                    throw new ArgumentException($"{nameof(sendBufferSize)} cannot be equal to or less than zero");
                }

                this.sendBufferSize = sendBufferSize;

                return this;
            }
        }
    }
}