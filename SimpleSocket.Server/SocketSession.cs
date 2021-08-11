using System;
using System.Net.Sockets;
using System.Threading;

namespace SimpleSocket.Server
{
    public class InvalidSocketSessionStateInMethodException : Exception
    {
        public InvalidSocketSessionStateInMethodException(int oldState, int correctState, string calledMethodName,
            Exception innerException = null)
            : base($"Invalid session status. " +
                   $"The \"{calledMethodName}\" method can only be called in the \"{SocketSessionState.Name(correctState)}\" state - " +
                   $"Invalid state({SocketSessionState.Name(oldState)})", innerException)
        {
        }
    }

    // 값 바꾸지 말 것!
    public class SocketSessionState
    {
        public const int IDLE = 1; // 초기 상태.

        public const int STARTING = 100; // 시작 중. 
        public const int RUNNING = 101; // 작동 중

        public const int TERMINATING = 200; // 종료 중.
        public const int TERMINATED = 201; // 종료됨.

        public static string Name(int state) => state switch
        {
            IDLE => nameof(IDLE),
            STARTING => nameof(STARTING),
            RUNNING => nameof(RUNNING),
            TERMINATING => nameof(TERMINATING),
            TERMINATED => nameof(TERMINATED),
            _ => "Error"
        };
    }

    public abstract class SocketSession : ISocketSession
    {
        private int _state = SocketSessionState.IDLE;
        public int state => _state;

        private Action<SocketSession> _onClose = null;

        private ISocketSessionEventHandler _socketSessionEventHandler = null;

        public string id { get; private set; } = string.Empty;
        public Socket socket { get; private set; } = null;

        protected virtual void InternalOnStart()
        {
        }

        protected virtual void InternalOnClose()
        {
        }

        protected virtual void OnError(Exception ex, string msg = "")
        {
            _socketSessionEventHandler?.OnError(this, ex, msg);
        }

        public void Start(string sessionId, Socket sck, Action<SocketSession> onClose)
        {
            var oldState = Interlocked.CompareExchange(
                ref _state
                , SocketSessionState.STARTING
                , SocketSessionState.IDLE);
            
            if (SocketSessionState.IDLE != oldState)
            {
                throw new InvalidSocketSessionStateInMethodException(
                    oldState
                    , SocketSessionState.IDLE
                    , nameof(Start));
            }

            id = string.IsNullOrEmpty(sessionId) 
                ? throw new ArgumentException(null, nameof(sessionId)) 
                : sessionId;
            
            socket = sck ?? throw new ArgumentNullException(nameof(sck));
            _onClose = onClose ?? throw new ArgumentNullException(nameof(onClose));

            try
            {
                InternalOnStart();
            }
            catch
            {
                socket = null;
                throw;
            }
        }

        public void Close()
        {
            var oldState = Interlocked.CompareExchange(
                ref _state
                , SocketSessionState.TERMINATING
                , SocketSessionState.RUNNING);
            
            if (SocketSessionState.RUNNING != oldState)
            {
                throw new InvalidSocketSessionStateInMethodException(
                    oldState
                    , SocketSessionState.RUNNING
                    , nameof(Close));
            }

            InternalOnClose();
            _onClose.Invoke(this);
        }

        public void OnStarted()
        {
            try
            {
                Interlocked.Exchange(ref _state, SocketSessionState.RUNNING);
                
                _socketSessionEventHandler?.OnSocketSessionStarted(this);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public void OnClosed()
        {
            try
            {
                Interlocked.Exchange(ref _state, SocketSessionState.TERMINATED);
                
                _socketSessionEventHandler?.OnSocketSessionClosed(this);
                
                id = string.Empty;
                socket = null;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public void Send(byte[] buffer)
        {
            if (SocketSessionState.RUNNING != _state)
            {
                throw new InvalidSocketSessionStateInMethodException(
                    _state
                    , SocketSessionState.RUNNING
                    , nameof(Send));
            }

            Send(buffer, 0, buffer.Length);
        }

        public void Send(ArraySegment<byte> segment)
        {
            if (SocketSessionState.RUNNING != _state)
            {
                throw new InvalidSocketSessionStateInMethodException(
                    _state
                    , SocketSessionState.RUNNING
                    , nameof(Send));
            }

            Send(segment.Array, segment.Offset, segment.Count);
        }

        public virtual void Send(byte[] buffer, int offset, int length)
        {
            if (SocketSessionState.RUNNING != _state)
            {
                throw new InvalidSocketSessionStateInMethodException(
                    _state
                    , SocketSessionState.RUNNING
                    , nameof(Send));
            }

            socket.Send(buffer, offset, length, SocketFlags.None);
        }

        public SocketSession SetSocketSessionEventHandler(ISocketSessionEventHandler socketSessionEventHandler)
        {
            _socketSessionEventHandler = socketSessionEventHandler ??
                                         throw new ArgumentNullException(nameof(socketSessionEventHandler));
            return this;
        }
    }
}