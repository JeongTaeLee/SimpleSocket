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
                bool willRaiseEvent = socket.AcceptAsync(_acceptArgs);
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

        
        public override void Start()
        {
            base.Start();
            
            try
            {
                _acceptArgs = new SocketAsyncEventArgs();
                _acceptArgs.Completed += OnAcceptCompleted;
            }
            catch (Exception e)
            {
                _acceptArgs?.Dispose();
                _acceptArgs = null;
                
                throw;
            }
        }

        public override void Close()
        {
            _acceptArgs?.Dispose();
            _acceptArgs = null;
            
            base.Close();
        }
    }
}