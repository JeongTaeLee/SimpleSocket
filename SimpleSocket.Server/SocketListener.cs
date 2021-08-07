using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleSocket.Server
{
    public abstract class SocketListener
    {
        protected Socket socket { get; private set; } = null;

        public SocketListenerConfig listenerConfig { get; private set; } = null;

        public bool running { get; private set; } = false;

        public Action<Exception, string> onError { get; set; }
        public Func<Socket, ValueTask<bool>> onAccept { get; set; }
        
        public SocketListener(SocketListenerConfig listenerConfig)
        {
            this.listenerConfig = listenerConfig ?? throw new ArgumentNullException(nameof(listenerConfig));
        }

        protected virtual async Task<bool> OnAccept(Socket sck)
        {
            var result = await onAccept.Invoke(sck);
            if (!result)
            {
                sck.SafeClose();
                return false;
            }

            return true;
        }

        protected virtual void OnStart() { }
        
        protected virtual void OnClose() { }
        
        protected virtual void OnError(Exception ex, string msg = null)
        {   
            onError?.Invoke(ex, msg);
        }
        
        public void Start()
        {
            try
            {    
                if (onAccept == null)
                {
                    throw new InvalidOperationException($"{nameof(onAccept)} is null");
                }
                
                socket = new Socket(listenerConfig.socketType, listenerConfig.protocolType);
                
                OnStart();

                running = true;
            }
            catch
            {
                socket?.Dispose();
                socket = null;
                
                // 예외는 호출부에서 처리.
                throw;
            }
        }

        public void Close()
        {
            running = false;
            
            OnClose();
            
            socket?.Close();
            socket = null;
        }

    }
}