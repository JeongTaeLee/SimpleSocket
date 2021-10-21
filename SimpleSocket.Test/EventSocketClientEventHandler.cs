using System;
using SimpleSocket.Client;

namespace SimpleSocket.Test
{
    public class EventSocketClientEventHandler : ISocketClientEventHandler
    {
        private event Action<BaseSocketClient> _onSocketClientClosed;
        public event Action<BaseSocketClient> onSocketClientClosed
        {
            add => _onSocketClientClosed += value;
            remove => _onSocketClientClosed -= value;
        }

        private event Action<BaseSocketClient, object> _onReceived;
        public event Action<BaseSocketClient, object> onReceived
        {
            add => _onReceived += value;
            remove => _onReceived -= value;
        }

        private event Action<BaseSocketClient, Exception, string> _onError;
        public event Action<BaseSocketClient, Exception, string> onError
        {
            add => _onError += value;
            remove => _onError -= value;
        }
        
        void ISocketClientEventHandler.OnSocketClientClosed(BaseSocketClient client)
        {
            _onSocketClientClosed?.Invoke(client);
        }

        void ISocketClientEventHandler.OnReceived(BaseSocketClient client, object receivedData)
        {
            _onReceived?.Invoke(client, receivedData);
        }

        void ISocketClientEventHandler.OnError(BaseSocketClient client, Exception ex, string message)
        {
            _onError?.Invoke(client, ex, message);
        }
    }
}