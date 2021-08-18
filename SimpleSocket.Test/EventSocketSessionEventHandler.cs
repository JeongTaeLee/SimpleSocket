using System;
using System.Threading.Tasks;
using SimpleSocket.Server;

namespace SimpleSocket.Test
{
    public class EventSocketSessionEventHandler : ISocketSessionEventHandler
    {

        private event Action<ISocketSession> _onSocketSessionStarted;
        public event Action<ISocketSession> onSocketSessionStarted
        {
            add => _onSocketSessionStarted += value;
            remove => _onSocketSessionStarted -= value;
        }
        
        private event Action<ISocketSession> _onSocketSessionClosed;
        public event Action<ISocketSession> onSocketSessionClosed
        {
            add => _onSocketSessionClosed += value;
            remove => _onSocketSessionClosed -= value;
        }

        private event Action<ISocketSession, object> _onReceived;
        public event Action<ISocketSession, object> onReceived
        {
            add => _onReceived += value;
            remove => _onReceived -= value;
        }

        private event Action<ISocketSession, Exception, string> _onError;
        public event Action<ISocketSession, Exception, string> onError
        {
            add => _onError += value;
            remove => _onError -= value;
        }
        
        void ISocketSessionEventHandler.OnSocketSessionStarted(ISocketSession session)
        {
            _onSocketSessionStarted?.Invoke(session);
        }
        
        void ISocketSessionEventHandler.OnSocketSessionClosed(ISocketSession session)
        {
            _onSocketSessionClosed?.Invoke(session);
        }

        ValueTask ISocketSessionEventHandler.OnReceived(ISocketSession session, object receivedData)
        {
            _onReceived?.Invoke(session, receivedData);
            return new ValueTask();
        }

        void ISocketSessionEventHandler.OnError(ISocketSession session, Exception ex, string message)
        {
            _onError?.Invoke(session, ex, message);
        }
    }
}