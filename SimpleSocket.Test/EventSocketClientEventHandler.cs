using System;
using SimpleSocket.Client;

namespace SimpleSocket.Test
{
    public class EventSocketClientEventHandler : ISocketClientEventHandler
    {
        private event Action<SocketClient> _onSocketClientClosed;
        public event Action<SocketClient> onSocketClientClosed
        {
            add => _onSocketClientClosed += value;
            remove => _onSocketClientClosed -= value;
        }

        private event Action<SocketClient, object> _onReceived;
        public event Action<SocketClient, object> onReceived
        {
            add => _onReceived += value;
            remove => _onReceived -= value;
        }

        private event Action<SocketClient, Exception, string> _onError;
        public event Action<SocketClient, Exception, string> onError
        {
            add => _onError += value;
            remove => _onError -= value;
        }
        
        void ISocketClientEventHandler.OnSocketClientClosed(SocketClient client)
        {
            _onSocketClientClosed?.Invoke(client);
        }

        void ISocketClientEventHandler.OnReceived(SocketClient client, object receivedData)
        {
            _onReceived?.Invoke(client, receivedData);
        }

        void ISocketClientEventHandler.OnError(SocketClient client, Exception ex, string message)
        {
            _onError?.Invoke(client, ex, message);
        }
    }
}