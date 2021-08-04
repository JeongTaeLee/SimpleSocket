using System;
using System.Threading.Tasks;

namespace SimpleSocket.Server
{
    public interface ISocketSessionEventHandler
    {
        void OnSocketSessionStarted(ISocketSession session);
        void OnSocketSessionClosed(ISocketSession session);
        void OnReceived(ISocketSession session, object receivedData);
        void OnError(ISocketSession session, Exception ex, string message);
    }
}