using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SimpleSocket.Common;

namespace SimpleSocket.Server
{
    public static class SocketSessionState
    {
        public const int NORMAL = 1; // 초기 상태.

        public const int TERMINATING = 200; // 종료 중.
        public const int TERMINATED = 201; // 종료됨.

        public static string Name(int state) => state switch
        {
            NORMAL => nameof(NORMAL),
            TERMINATING => nameof(TERMINATING),
            TERMINATED => nameof(TERMINATED),
            _ => "Error"
        };
    }

    public abstract class SocketSession : ISocketSession
    {
        private int _state = SocketSessionState.NORMAL;
        public int state => _state;

        private Action<SocketSession> _onClose = null;
        protected IMessageFilter messageFilter { get; private set; } = null;

        public string id { get; private set; } = string.Empty;
        public Socket socket { get; private set; } = null;
        public ISocketSessionEventHandler socketSessionEventHandler { get; private set; } = null;
        public bool running => (_state != SocketSessionState.TERMINATING && _state != SocketSessionState.TERMINATED); 

        protected virtual void InternalOnStart()
        {
        }

        protected virtual void InternalOnClose()
        {
        }

        protected ValueTask OnReceive(object message)
        {
            if (socketSessionEventHandler != null)
            {
                return socketSessionEventHandler.OnReceived(this, message);
            }

            return new ValueTask();
        }

        protected void OnError(Exception ex, string msg = "")
        {
            socketSessionEventHandler?.OnError(this, ex, msg);
        }

        public void Initialize(string sessionId, Socket socket_, IMessageFilter messageFilter_, Action<SocketSession> onClose)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentNullException(nameof(sessionId));
            }

            if (socket_ == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            if (messageFilter_ == null)
            {
                throw new ArgumentNullException(nameof(messageFilter));
            }

            if (onClose == null)
            {
                throw new ArgumentNullException(nameof(_onClose));
            }

            id = sessionId;
            socket = socket_;
            messageFilter = messageFilter_;
            _onClose = onClose;
        }

        public void Start()
        {
            try
            {
                if (_state == SocketSessionState.TERMINATING || _state == SocketSessionState.TERMINATED)
                {
                    throw ServerExceptionUtil.IOEInvalidSessionState(_state);
                }

                if (string.IsNullOrEmpty(id))
                {
                    throw ExceptionUtil.IOEVariableNotSet(nameof(id));
                }

                if (socket == null)
                {
                    throw ExceptionUtil.IOEVariableNotSet(nameof(socket));
                }

                if (messageFilter == null)
                {
                    throw ExceptionUtil.IOEVariableNotSet(nameof(messageFilter));
                }

                if (_onClose == null)
                {
                    throw ExceptionUtil.IOEVariableNotSet(nameof(_onClose));
                }

                InternalOnStart();
            }
            catch
            {
                id = string.Empty;
                socket = null;
                messageFilter = null;
                _onClose = null;
                throw;
            }
        }

        public void Close()
        {
            // 종료 중이거나 종료이면 캔슬
            if (!running)
            {
                return;
            }

            Interlocked.Exchange(ref _state, SocketSessionState.TERMINATING);

            InternalOnClose();

            _onClose.Invoke(this);
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
            if (!running)
            {
                throw ServerExceptionUtil.IOEInvalidSessionState(_state);
            }

            socket.Send(buffer, offset, length, SocketFlags.None);
        }

        public virtual Task SendAsync(byte[] buffer)
        {
            return SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length));
        }

        public virtual Task SendAsync(byte[] buffer, int offset, int length)
        {            
            return SendAsync(new ArraySegment<byte>(buffer, offset, length));
        }

        public virtual Task SendAsync(ArraySegment<byte> segment)
        {
            if (!running)
            {
                throw ServerExceptionUtil.IOEInvalidSessionState(_state);
            }

            return socket.SendAsync(segment, SocketFlags.None);
        }

        public SocketSession SetSocketSessionEventHandler(ISocketSessionEventHandler socketSessionEventHandler)
        {
            this.socketSessionEventHandler = socketSessionEventHandler ??
                                         throw new ArgumentNullException(nameof(socketSessionEventHandler));
            return this;
        }
    }
}