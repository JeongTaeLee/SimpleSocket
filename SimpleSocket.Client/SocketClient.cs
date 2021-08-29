using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using SimpleSocket.Client.Utils;
using SimpleSocket.Common;

namespace SimpleSocket.Client
{       
    public static class SocketClientState
    {
        public const int IDLE = 1; // 초기 상태.

        public const int STARTING = 100; // 시작 중. 
        public const int RUNNING = 101; // 작동 중

        public const int TERMINATING = 200; // 종료 중.
        public const int TERMINATED = 201; // 종료됨.

        public static string Name(int state)
        {
            switch (state)
            {
                case IDLE: return nameof(IDLE);
                case STARTING: return nameof(STARTING);
                case RUNNING: return nameof(RUNNING);
                case TERMINATING: return nameof(TERMINATING);
                case TERMINATED: return nameof(TERMINATED);
                default: return "Error!";
            }   
        }
    }
    
    public abstract class SocketClient
    {
        private int _state = SocketClientState.IDLE;
        public int state => _state;
        
        protected readonly IMessageFilter messageFilter = null;
        
        protected Socket socket { get; private set; } = null;
        protected ISocketClientEventHandler socketClientEventHandler { get; private set; } = null;
        
        public readonly SocketClientConfig config  = null;
        
        public SocketClient(SocketClientConfig config, IMessageFilter messageFilter)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.messageFilter = messageFilter ?? throw new ArgumentNullException(nameof(messageFilter));
        }
        
        protected void OnReceive(object message)
        {
            socketClientEventHandler?.OnReceived(this, message);
        }
        
        protected void OnError(Exception ex, string message = "")
        {
            socketClientEventHandler?.OnError(this, ex, message);
        }

        protected virtual void InternalOnStart() { }

        protected virtual void InternalOnClose() { }

        public void Start()
        {
            var oldState = Interlocked.CompareExchange(ref _state, SocketClientState.STARTING, SocketClientState.IDLE);

            if (SocketClientState.IDLE != oldState)
            {
                throw ClientExceptionUtil.IOEInvalidSessionState(oldState);
            }
            
            try
            {
                var ipAddress = IPAddress.Parse(config.ip);
                var endPoint = new IPEndPoint(ipAddress, config.port);

                socket = new Socket(config.socketType, config.protocolType);
                socket.Connect(endPoint);

                InternalOnStart();

                _state = SocketClientState.RUNNING;
            }
            catch
            {
                socket?.SafeClose();
                socket = null;
                
                throw;
            }
        }

        public void Close()
        {            
            var oldState = Interlocked.CompareExchange(ref _state, SocketClientState.TERMINATING, SocketClientState.RUNNING);
            if (SocketClientState.RUNNING != oldState)
            {
                return;
            }
            
            InternalOnClose();

            socket?.SafeClose();
            socket = null;

            Interlocked.Exchange(ref _state, SocketClientState.TERMINATED);

            socketClientEventHandler?.OnSocketClientClosed(this);
        }

        public abstract void Send(byte[] buffer);
        public abstract void Send(ArraySegment<byte> segment);
        public abstract void Send(byte[] buffer, int offset, int length);

        public abstract Task SendAsync(byte[] buffer);
        public abstract Task SendAsync(byte[] buffer, int offset, int length);
        public abstract Task SendAsync(ArraySegment<byte> segment);

        public SocketClient SetSocketClientEventHandler(ISocketClientEventHandler handler)
        {
            socketClientEventHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }
    }
}