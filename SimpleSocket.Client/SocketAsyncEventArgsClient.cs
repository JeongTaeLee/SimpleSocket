using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using SimpleSocket.Common;

namespace SimpleSocket.Client
{
    public class SocketAsyncEventArgsClient : SocketClient
    {
        private int _originOffset = 0;
        private int _currentOffset = 0;
        
        private SocketAsyncEventArgs _recvArgs = null; 
        
        public readonly SocketAsyncEventArgsClientConfig socketAsyncEventArgsClientConfig = null;
        
        public SocketAsyncEventArgsClient(SocketAsyncEventArgsClientConfig socketAsyncEventArgsClientConfig
            , SocketClientConfig socketClientConfig
            , IMessageFilter messageFilter) : base (socketClientConfig, messageFilter)
        {
            this.socketAsyncEventArgsClientConfig = socketAsyncEventArgsClientConfig;
        }

        private void StartReceive(SocketAsyncEventArgs args)
        {
            var willRaiseEvent = socket.ReceiveAsync(args);
            if (!willRaiseEvent)
            {
                ProcessReceive(args);
            }
        }
        
        private void RecvArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                var totalReadByte = 0;

                try
                {
                    while (args.BytesTransferred > totalReadByte)
                    {
                        var readSize = 0;

                        try
                        {
                            var message =
                                messageFilter.Filtering(args.Buffer, _currentOffset, args.Count, out readSize);

                            if (message == null)
                            {
                                break;
                            }

                            OnReceive(message);
                        }
                        finally
                        {
                            totalReadByte += readSize;
                            _currentOffset += readSize;
                        }
                    }

                }
                catch (Exception ex)
                {
                    OnError(ex, "Session receive error!");

                    if (SocketClientState.RUNNING == state)
                    {
                        Close();
                    }
                }
                finally
                {
                    if (args.BytesTransferred <= totalReadByte)
                    {
                        _currentOffset = _originOffset;
                    }

                    if (SocketClientState.RUNNING == state)
                    {
                        StartReceive(args);    
                    }
                }
            }
            else
            {
                if (SocketClientState.RUNNING == state)
                {
                    Close();
                }
            }
        }
        
        protected override void InternalOnStart()
        {
            base.InternalOnStart();

            _recvArgs = new SocketAsyncEventArgs();
            _recvArgs.SetBuffer(new byte[socketAsyncEventArgsClientConfig.recvBufferSize]
                , 0
                , socketAsyncEventArgsClientConfig.recvBufferSize);
            _recvArgs.Completed += RecvArgs_Completed;

            _originOffset = _recvArgs.Offset;
            _currentOffset = _originOffset;
            
            StartReceive(_recvArgs);
        }

        protected override void InternalOnClose()
        {
            base.InternalOnClose();

            _recvArgs.Completed -= RecvArgs_Completed;
        }

        public override void Send(byte[] buffer)
        {
            if (SocketClientState.RUNNING != state)
            {
                throw new InvalidSocketClientStateInMethodException(
                    state
                    , SocketClientState.RUNNING
                    , nameof(Send));
            }
            
            Send(buffer, 0, buffer.Length);
        }

        public override void Send(ArraySegment<byte> segment)
        {
            if (SocketClientState.RUNNING != state)
            {
                throw new InvalidSocketClientStateInMethodException(
                    state
                    , SocketClientState.RUNNING
                    , nameof(Send));
            }

            Send(segment.Array, segment.Offset, segment.Count);
        }

        public override void Send(byte[] buffer, int offset, int length)
        {
            if (SocketClientState.RUNNING != state)
            {
                throw new InvalidSocketClientStateInMethodException(
                    state
                    , SocketClientState.RUNNING
                    , nameof(Send));
            }

            socket.Send(buffer, offset, length, SocketFlags.None);
        }

        public override Task SendAsync(byte[] buffer)
        {
            if (SocketClientState.RUNNING != state)
            {
                throw new InvalidSocketClientStateInMethodException(
                    state
                    , SocketClientState.RUNNING
                    , nameof(Send));
            }
            
            return SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length));
        }

        public override Task SendAsync(byte[] buffer, int offset, int length)
        {
            if (SocketClientState.RUNNING != state)
            {
                throw new InvalidSocketClientStateInMethodException(
                    state
                    , SocketClientState.RUNNING
                    , nameof(Send));
            }
            
            return SendAsync(new ArraySegment<byte>(buffer, offset, length));
        }

        public override Task SendAsync(ArraySegment<byte> segment)
        {               
            if (SocketClientState.RUNNING != state)
            {
                throw new InvalidSocketClientStateInMethodException(
                    state
                    , SocketClientState.RUNNING
                    , nameof(Send));
            }
            
            return socket.SendAsync(segment, SocketFlags.None);
        }
    }
}