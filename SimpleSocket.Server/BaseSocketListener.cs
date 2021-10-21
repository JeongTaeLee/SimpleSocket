using System;
using System.Net;
using System.Net.Sockets;
using SimpleSocket.Common;

namespace SimpleSocket.Server
{
    public abstract class BaseSocketListener
    {
        private Func<BaseSocketListener, Socket, bool> _onAccept = null;
        private Action<BaseSocketListener, Exception, string> _onError = null;
     
        protected Socket socket { get; private set; } = null;

        public SocketListenerConfig listenerConfig { get; private set; } = null;
        public bool running { get; private set; } = false;
        
        public BaseSocketListener()
        {
        }

        protected void OnAccept(Socket sck)
        {
            var result = _onAccept.Invoke(this, sck);
            if (!result)
            {
                sck.SafeClose();
            }
        }

        protected virtual void InternalOnStart() { }
        
        protected virtual void InternalOnClose() { }
        
        protected virtual void OnError(Exception ex, string msg = null)
        {   
            _onError?.Invoke(this, ex, msg);
        }
        
        public void Start(SocketListenerConfig listenerConfig_
            , Func<BaseSocketListener, Socket, bool> onAccept
            , Action<BaseSocketListener, Exception, string> onError)
        {
            listenerConfig = listenerConfig_ ?? throw new ArgumentNullException(nameof(listenerConfig_));
            _onAccept = onAccept ?? throw new ArgumentNullException(nameof(onAccept));
            _onError = onError ?? throw new ArgumentNullException(nameof(onError));

            try
            {    
                socket = new Socket(listenerConfig.socketType, listenerConfig.protocolType);
                socket.Bind(new IPEndPoint(IPAddress.Parse(listenerConfig.ip), listenerConfig.port));
                socket.Listen(listenerConfig.backlog);

                InternalOnStart();
                
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
            if (!running)
            {
                return;
            }

            running = false;
            
            InternalOnClose();
            
            socket?.Dispose();
            socket = null;
        }

    }
}