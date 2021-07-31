using System;
using System.Net.Sockets;

namespace SimpleSocket.Server
{
    public abstract class SocketSession
    {
        protected readonly SocketServer server = null;

        protected Socket socket { get; private set; } = null;

        public string sessionId { get; private set; } = string.Empty;
        
        public bool running { get; private set; } = false;
        
        protected virtual void OnStart() { }
        
        protected virtual void OnClose() { }

        public SocketSession(SocketServer server, string sessionId)
        {
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            this.sessionId = string.IsNullOrEmpty(sessionId)
                ? throw new ArgumentException(nameof(sessionId))
                : sessionId;
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
            
            socket = null;
            
            running = false;
        }
    }
}