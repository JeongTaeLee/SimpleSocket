using System;
using System.Net.Sockets;

namespace SimpleSocket.Client
{
    public class SocketClientConfig
    {        
        public readonly SocketType socketType;
        public readonly ProtocolType protocolType;
        public readonly string ip;
        public readonly int port;

        private SocketClientConfig( SocketType socketType, ProtocolType protocolType, string ip, int port)
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

            this.socketType = socketType;
            this.protocolType = protocolType;
            this.ip = ip;
            this.port = port;
        }

        public class Builder
        {
            public SocketType socketType { get; private set; } = SocketType.Stream;
            public ProtocolType protocolType { get; private set; } = ProtocolType.Tcp;
            public string ip { get; private set; }
            public int port { get; private set; }

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
                => new SocketClientConfig(socketType, protocolType, ip, port);
            
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
        }
    }
}