using System;
using System.Net.Sockets;

namespace SimpleSocket.Server
{
    public class SocketAsyncEventArgsSession : SocketSession
    {
        public readonly SocketAsyncEventArgs recvEventArgs = null;
        
        public SocketAsyncEventArgsSession(SocketServer server, string sessionId, SocketAsyncEventArgs recvEventArgs) 
            : base(server, sessionId)
        {
            this.recvEventArgs = recvEventArgs ?? throw new ArgumentNullException(nameof(recvEventArgs));
        }

        private void StartReceive(SocketAsyncEventArgs args)
        {
            var willRaiseEvent = socket.SendAsync(args);
            if (!willRaiseEvent)
            {
                ProcessReceive(args);
            }
        }
        
        private void ProcessReceive(SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                StartReceive(args);    
            }
            else
            {
                Close();
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            
            StartReceive(recvEventArgs);
        }
    }
}