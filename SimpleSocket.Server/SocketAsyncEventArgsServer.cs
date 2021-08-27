using System;
using System.Net.Sockets;
using SimpleSocket.Common;
using SimpleSocket.Server.Components;

namespace SimpleSocket.Server
{
    public class SocketAsyncEventArgsServer : SocketServer
    {
        private readonly SocketAsyncEventArgsServerConfig _socketAsyncEventArgsConfig = null;
        
        private readonly BufferManager _recvBufferManager = null;
        
        public SocketAsyncEventArgsServer(SocketAsyncEventArgsServerConfig socketAsyncEventArgsConfig, IMessageFilterFactory msgFilterFactory)
            : base(msgFilterFactory)
        {
            _socketAsyncEventArgsConfig = socketAsyncEventArgsConfig ?? throw new ArgumentNullException(nameof(socketAsyncEventArgsConfig));

            _recvBufferManager = new BufferManager(
                _socketAsyncEventArgsConfig.recvBufferSize * _socketAsyncEventArgsConfig.maxConnection,
                _socketAsyncEventArgsConfig.recvBufferSize);
        }

        protected override void InternalOnStart()
        {
            _recvBufferManager.InitBuffer();
        }

        protected override SocketListener CreateListener()
        {
            return new SocketAsyncEventArgsListener();
        }

        protected override SocketSession CreateSession(string sessionId)
        {
            var recvEventArgs = new SocketAsyncEventArgs
            {
                UserToken = this
            };
            
            _recvBufferManager.SetBuffer(recvEventArgs);

            return new SocketAsyncEventArgsSession(recvEventArgs);
        }

        protected override void InternalOnSessionClosed(SocketSession closeSocketSession)
        {
            if (closeSocketSession == null)
            {
                throw new ArgumentNullException(nameof(closeSocketSession));
            }
            
            try
            {
                if (closeSocketSession is SocketAsyncEventArgsSession convertedSocketSession)
                {
                    var recvEventArgs = convertedSocketSession.recvEventArgs;

                    _recvBufferManager.FreeBuffer(recvEventArgs);
                    
                    // TODO @jeongtae.lee : 추 후 풀링 방식으로 변경.
                    recvEventArgs.Dispose();
                }
                else
                {
                    throw new Exception($"Invalid session type - Invalid session type({closeSocketSession.GetType().FullName})");
                }
            }
            catch (Exception e)
            {
                OnError(e, "Socket close failed");
            }
        }
    }
}