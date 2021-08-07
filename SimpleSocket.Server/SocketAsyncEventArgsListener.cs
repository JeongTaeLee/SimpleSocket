using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleSocket.Server
{
    public class SocketAsyncEventArgsListener : SocketListener
    {
        private SocketAsyncEventArgs _acceptArgs = null;

        public SocketAsyncEventArgsListener(SocketListenerConfig listenerConfig) : base(listenerConfig)
        {
        }

        private void StartAccept()
        {
            _acceptArgs.AcceptSocket = null;

            try
            {
                var willRaiseEvent = socket.AcceptAsync(_acceptArgs);
                if (!willRaiseEvent)
                {
                    ProcessAccept(_acceptArgs);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            
        }
        
        private async void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                await OnAccept(e.AcceptSocket);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                StartAccept();
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }
        
        protected override void OnStart()
        {
            try
            {
                _acceptArgs = new SocketAsyncEventArgs();
                _acceptArgs.Completed += OnAcceptCompleted;
            }
            catch
            {
                _acceptArgs?.Dispose();
                _acceptArgs = null;
                
                throw;
            }
        }

        protected override void OnClose()
        {
            _acceptArgs?.Dispose();
            _acceptArgs = null;
        }
    }
}