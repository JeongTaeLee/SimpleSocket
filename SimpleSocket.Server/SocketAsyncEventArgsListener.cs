using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleSocket.Server
{
    public class SocketAsyncEventArgsListener : BaseSocketListener
    {
        private SocketAsyncEventArgs _acceptArgs = null;

        public SocketAsyncEventArgsListener()
        {
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Socket acceptSocket = null;

            if (e.SocketError != SocketError.Success)
            {
                var errorCode = (int) e.SocketError;
                
                //리스너 소켓이 닫혔을 경우 나오는 에러코드.
                if (errorCode == 995 || errorCode == 10004 || errorCode == 10038)
                {
                    return;
                }
                
                OnError(new SocketException(errorCode));
            }
            else
            {
                acceptSocket = e.AcceptSocket;
            }

            e.AcceptSocket = null;
            
            var willRaiseEvent = false;
            try
            {
                willRaiseEvent = socket.AcceptAsync(_acceptArgs);
            }
            catch (ObjectDisposedException)
            {
                willRaiseEvent = true;
            }
            catch (NullReferenceException)
            {
                willRaiseEvent = true;
            }
            catch (Exception ex)
            {
                willRaiseEvent = true;
                OnError(ex);
            }

            if (acceptSocket != null)
            {
                OnAccept(acceptSocket);
            }

            if (!willRaiseEvent)
            {
                ProcessAccept(e);
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }
        
        protected override void InternalOnStart()
        {
            try
            {
                _acceptArgs = new SocketAsyncEventArgs();
                _acceptArgs.Completed += OnAcceptCompleted;

                if (!socket.AcceptAsync(_acceptArgs))
                {
                    ProcessAccept(_acceptArgs);
                }
            }
            catch
            {
                _acceptArgs?.Dispose();
                _acceptArgs = null;
                
                throw;
            }
        }

        protected override void InternalOnClose()
        {
            if (_acceptArgs != null)
            {
                _acceptArgs.Completed -= OnAcceptCompleted;
                _acceptArgs.Dispose();

                _acceptArgs = null;
            }
        }
    }
}