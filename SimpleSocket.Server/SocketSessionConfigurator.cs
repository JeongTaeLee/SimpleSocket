using System;

namespace SimpleSocket.Server
{
    public class SocketSessionConfigurator
    {
        private readonly SocketSession _socketSession = null;
        
        public SocketSessionConfigurator(SocketSession socketSession)
        {
            _socketSession = socketSession ?? throw new ArgumentNullException(nameof(socketSession));
        }

        public SocketSessionConfigurator SetSocketSessionEventHandler(ISocketSessionEventHandler socketSessionEventHandler)
        {
            _socketSession.SetSocketSessionEventHandler(socketSessionEventHandler);
            return this;
        }
        
    }
}