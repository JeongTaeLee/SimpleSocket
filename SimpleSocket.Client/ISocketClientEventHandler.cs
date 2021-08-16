using System;
using System.Threading.Tasks;

namespace SimpleSocket.Client
{
    public interface ISocketClientEventHandler
    {
        void OnSocketClientClosed(SocketClient session);
        void OnReceived(SocketClient session, object receivedData);
        void OnError(SocketClient session, Exception ex, string message);
    }
}