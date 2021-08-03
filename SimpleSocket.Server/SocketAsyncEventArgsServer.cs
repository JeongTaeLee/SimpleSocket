using System;
using System.Net.Sockets;
using SimpleSocket.Server.Components;

namespace SimpleSocket.Server
{
    public class SocketAsyncEventArgsServer : SocketServer
    {
        private readonly SocketAsyncEventArgsServerConfig _socketAsyncEventArgsConfig = null;
        
        private readonly BufferManager _recvBufferManager = null;
        
        public SocketAsyncEventArgsServer(SocketAsyncEventArgsServerConfig socketAsyncEvnetArgsConfig)
        {
            _socketAsyncEventArgsConfig = socketAsyncEvnetArgsConfig ?? throw new ArgumentNullException(nameof(socketAsyncEvnetArgsConfig));

            _recvBufferManager = new BufferManager(
                _socketAsyncEventArgsConfig.recvBufferSize,
                _socketAsyncEventArgsConfig.maxConnection);
        }

        protected override void OnStart()
        {
            _recvBufferManager.InitBuffer();
        }

        protected override SocketListener CreateListener(SocketListenerConfig config)
        {
            return new SocketAsyncEventArgsListener(config);
        }

        protected override SocketSession CreateSession(string sessionId)
        {
            var recvEventArgs = new SocketAsyncEventArgs
            {
                UserToken = this
            };
            
            _recvBufferManager.SetBuffer(recvEventArgs);

            return new SocketAsyncEventArgsSession(this, sessionId, recvEventArgs);
        }

        protected override void OnSessionClose(SocketSession closeSession)
        {
            if (closeSession == null)
            {
                throw new ArgumentNullException(nameof(closeSession));
            }
            
            try
            {
                if (closeSession is SocketAsyncEventArgsSession convertedSession)
                {
                    _recvBufferManager.FreeBuffer(convertedSession.recvEventArgs);
                }
                else
                {
                    throw new Exception($"Invalid session type - Invalid session type({closeSession.GetType().FullName})");
                }
            }
            catch (Exception e)
            {
                OnError(e, "Socket close failed");
            }
        }
    }
}