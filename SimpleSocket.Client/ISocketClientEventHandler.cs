using System;
using System.Threading.Tasks;

namespace SimpleSocket.Client
{
    public interface ISocketClientEventHandler
    {
        void OnSocketClientClosed(SocketClient client);
        void OnReceived(SocketClient client, object receivedData);
        void OnError(SocketClient client, Exception ex, string message);
    }
}