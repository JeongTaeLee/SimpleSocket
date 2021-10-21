using System;
using System.Net.Sockets;
using SimpleSocket.Common;
using SimpleSocket.Server.Components;

namespace SimpleSocket.Server
{
    public class SocketAsyncEventArgsServer : BaseSocketServer
    {
        private readonly BufferManager _recvBufferManager = null;
        
        public SocketAsyncEventArgsServer(SocketServerOption option, IMessageFilterFactory msgFilterFactory)
            : base(option, msgFilterFactory)
        {
            _recvBufferManager = new BufferManager(option.recvBufferSize * option.maxConnection, option.recvBufferSize);
        }

        protected override void InternalOnStart()
        {
            _recvBufferManager.InitBuffer();
        }

        protected override BaseSocketListener CreateListener()
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
                    
                    // TODO @jeongtae.lee : �� �� Ǯ�� ������� ����.
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