using System;
using System.Net.Http.Headers;
using System.Net.Sockets;

namespace SimpleSocket.Server
{
    public class SocketAsyncEventArgsSession : SocketSession
    {
        private readonly int _originOffset = 0;
        private int _currentOffset = 0;

        public readonly SocketAsyncEventArgs recvEventArgs = null;
        
        public SocketAsyncEventArgsSession(SocketAsyncEventArgs recvEventArgs)
        {
            this.recvEventArgs = recvEventArgs ?? throw new ArgumentNullException(nameof(recvEventArgs));
            _originOffset = this.recvEventArgs.Offset;
            _currentOffset = _originOffset;
        }

        private void RecvEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        private void StartReceive(SocketAsyncEventArgs args)
        {
            var willRaiseEvent = socket.ReceiveAsync(args);
            if (!willRaiseEvent)
            {
                ProcessReceive(args);
            }
        }
        
        private async void ProcessReceive(SocketAsyncEventArgs args)
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

                            await OnReceive(message);
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

                    if (SocketSessionState.RUNNING == state)
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

                    if (SocketSessionState.RUNNING == state)
                    {
                        StartReceive(args);    
                    }
                }
            }
            else
            {
                if (SocketSessionState.RUNNING == state)
                {
                    Close();
                }
            }
        }

        protected override void InternalOnStart()
        {
            base.InternalOnStart();

            this.recvEventArgs.Completed += RecvEventArgs_Completed;

            StartReceive(recvEventArgs);
        }

        protected override void InternalOnClose()
        {
            _currentOffset = _originOffset;

            recvEventArgs.Completed -= RecvEventArgs_Completed;
        }
    }
}