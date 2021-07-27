using System.Net.Sockets;

namespace SimpleSocket.Server
{
    public class SocketListenerConfig
    {
        public SocketType socketType { get; set; } = SocketType.Unknown;
        public ProtocolType protocolType { get; set; } = ProtocolType.Unknown;
        public string ip { get; set; } = string.Empty;
        public int port { get; set; } = -1;

    }
}