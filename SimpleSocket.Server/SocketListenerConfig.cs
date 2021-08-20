using System;
using System.Net.Sockets;
using System.Resources;

namespace SimpleSocket.Server
{
    public class SocketListenerConfig
    {
        public readonly SocketType socketType;
        public readonly ProtocolType protocolType;
        public readonly string ip;
        public readonly int port;
        public readonly int backlog;

        private SocketListenerConfig(SocketType socketType, ProtocolType protocolType, string ip, int port, int backlog)
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
            
            if (0 >= backlog)
            {
                throw new ArgumentException($"Invalid {nameof(port)} - port({port})");
            }
            
            this.socketType = socketType;
            this.protocolType = protocolType;
            this.ip = ip;
            this.port = port;
            this.backlog = backlog;
        }
        
        public class Builder
        {
            public SocketType socketType { get; private set; } = SocketType.Stream;
            public ProtocolType protocolType { get; private set; } = ProtocolType.Tcp;
            public string ip { get; private set; }
            public int port { get; private set; }
            public int backlog { get; private set; } = 1000;

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
            
            public SocketListenerConfig Build()
                =>  new SocketListenerConfig(socketType, protocolType, ip, port, backlog);
            
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
                    throw new ArgumentException($"{nameof(ip)} is null or empty");
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

            public Builder SetBacklog(int backlog)
            {
                if (0 >= backlog)
                {
                    throw new ArgumentException($"Invalid {nameof(port)} - port({port})");
                }

                this.backlog = backlog;

                return this;
            }
        }
    }
}