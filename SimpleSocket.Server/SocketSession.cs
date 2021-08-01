using System;
using System.Net.Sockets;

namespace SimpleSocket.Server
{
    public abstract class SocketSession : ISession
    {
        // TODO @jeongtae.lee : session 상태 구현.
        
        public readonly SocketServer server = null;
        public readonly string sessionId = string.Empty;

        public Socket socket { get; private set; } = null;
        public bool running { get; private set; } = false;
        public Action<SocketSession> onClose { get; set; } = null;
        
        protected virtual void OnStart() { }
        
        protected virtual void OnClose() { }

        public SocketSession(SocketServer server, string sessionId)
        {
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            this.sessionId = string.IsNullOrEmpty(sessionId)
                ? throw new ArgumentException(null, nameof(sessionId)) : sessionId;
        }
        
        public void Start(Socket sck)
        {
            socket = sck ?? throw new ArgumentNullException(nameof(sck));
            
            try
            {
                OnStart();
                running = true;
            }
            catch
            {
                socket = null;
                throw;
            }
        }

        public void Close()
        {
            OnClose();
            onClose.Invoke(this);
            
            running = false;
        }

        public void Send(byte[] buffer)
        {
            Send(buffer, 0, buffer.Length);
        }

        public void Send(ArraySegment<byte> segment)
        {
            Send(segment.Array, segment.Offset, segment.Count);
        }
        
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            socket.Send(buffer, offset, length, SocketFlags.None);
        }
    }
}