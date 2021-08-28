using System;
using System.Net;
using System.Net.Sockets;
using SimpleSocket.Common;

namespace SimpleSocket.Server
{
    public abstract class SocketListener
    {
        private Func<SocketListener, Socket, bool> _onAccept = null;
        private Action<SocketListener, Exception, string> _onError = null;
     
        protected Socket socket { get; private set; } = null;

        public SocketListenerConfig listenerConfig { get; private set; } = null;
        public bool running { get; private set; } = false;
        
        public SocketListener()
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
            , Func<SocketListener, Socket, bool> onAccept
            , Action<SocketListener, Exception, string> onError)
        {
            listenerConfig = listenerConfig_ ?? throw new ArgumentNullException(nameof(listenerConfig_));
            _onAccept = onAccept ?? throw new ArgumentNullException(nameof(onAccept));
            _onError = onError ?? throw new ArgumentNullException(nameof(onError));

            try
            {    
                socket = new Socket(listenerConfig.socketType, listenerConfig.protocolType);
                socket.Bind(new IPEndPoint(IPAddress.Parse(listenerConfig.ip), listenerConfig.port));
                socket.Listen(listenerConfig.backlog);

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

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