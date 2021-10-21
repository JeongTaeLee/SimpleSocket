using System;
using System.Threading.Tasks;

namespace SimpleSocket.Client
{
    public interface ISocketClientEventHandler
    {
        void OnSocketClientClosed(BaseSocketClient client);
        void OnReceived(BaseSocketClient client, object receivedData);
        void OnError(BaseSocketClient client, Exception ex, string message);
    }
}