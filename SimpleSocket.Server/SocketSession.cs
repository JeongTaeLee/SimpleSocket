using System;
using System.Net.Sockets;
using System.Threading;

namespace SimpleSocket.Server
{
    // TODO @jeongtae.lee : 수신, 발신 중에 종료 시 처리 구현.
    
    public class InvalidSocketSessionStateInMethodException : Exception
    {
        public InvalidSocketSessionStateInMethodException(string oldState, string correctStateName, string calledMethodName, Exception innerException = null)  
            : base($"Invalid session status. The \"{calledMethodName}\" method can only be called in the \"{correctStateName}\" state - Invalid state({oldState})", innerException)
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
    }

    public abstract class SocketSession : ISession
    {
        private int _state = SocketSessionState.IDLE;
        public int state => _state;
        
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
            var oldState = Interlocked.CompareExchange(ref _state, SocketSessionState.STARTING, SocketSessionState.IDLE); 
            if (SocketSessionState.IDLE != oldState)
            {
                throw new InvalidSocketSessionStateInMethodException(oldState.ToString(), nameof(SocketSessionState.IDLE), nameof(Start));
            }
            
            socket = sck ?? throw new ArgumentNullException(nameof(sck));
            
            try
            {
                OnStart();

                Interlocked.Exchange(ref _state, SocketSessionState.RUNNING);
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
            var oldState = Interlocked.CompareExchange(ref _state, SocketSessionState.TERMINATING, SocketSessionState.RUNNING); 
            if (SocketSessionState.RUNNING != oldState)
            {
                throw new InvalidSocketSessionStateInMethodException(oldState.ToString(), nameof(SocketSessionState.RUNNING), nameof(Close));
            }
            
            OnClose();
            onClose.Invoke(this);
            
            Interlocked.Exchange(ref _state, SocketSessionState.TERMINATED);
            running = false;
        }

        public void Send(byte[] buffer)
        {
            if (SocketSessionState.RUNNING != _state)
            {
                throw new InvalidSocketSessionStateInMethodException(_state.ToString(), nameof(SocketSessionState.RUNNING), nameof(Close));
            }
            
            Send(buffer, 0, buffer.Length);
        }

        public void Send(ArraySegment<byte> segment)
        {
            if (SocketSessionState.RUNNING != _state)
            {
                throw new InvalidSocketSessionStateInMethodException(_state.ToString(), nameof(SocketSessionState.RUNNING), nameof(Close));
            }
            
            Send(segment.Array, segment.Offset, segment.Count);
        }
        
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            if (SocketSessionState.RUNNING != _state)
            {
                throw new InvalidSocketSessionStateInMethodException(_state.ToString(), nameof(SocketSessionState.RUNNING), nameof(Close));
            }
            
            socket.Send(buffer, offset, length, SocketFlags.None);
        }
    }
}