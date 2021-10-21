using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;
using SimpleSocket.Common;

namespace SimpleSocket.Server
{
    public abstract class BaseSocketServer
    {
        private class ListenerPair
        {
            public SocketListenerConfig config { get; private set; } = null;
            public BaseSocketListener listener { get; private set; } = null;

            public ListenerPair(SocketListenerConfig config)
            {
                this.config = config ?? throw new ArgumentNullException(nameof(config));
            }

            public void SetListener(BaseSocketListener listener)
            {
                this.listener = listener ?? throw new ArgumentNullException(nameof(listener));
            }
        }

        private readonly ConcurrentDictionary<(string, int), ListenerPair> _listenerPairs =
            new ConcurrentDictionary<(string, int), ListenerPair>();

        private readonly ConcurrentDictionary<string, SocketSession> _sessions =
            new ConcurrentDictionary<string, SocketSession>();

        private readonly IMessageFilterFactory _messageFilterFactory = null;

        public readonly SocketServerOption option = null;

        public bool running { get; private set; } = false;

        public Action<SocketSessionConfigurator> onSessionConfiguration { get; set; } = null;
        public Action<Exception, string> onError { get; set; } = null;

        public BaseSocketServer(SocketServerOption option, IMessageFilterFactory messageFilterFactory)
        {
            this.option = option ?? throw new ArgumentNullException(nameof(option));
            this._messageFilterFactory = messageFilterFactory ?? throw new ArgumentNullException(nameof(messageFilterFactory));
        }

        //
        private void StartAllListener()
        {
            foreach (var listenerPair in _listenerPairs)
            {
                var listener = listenerPair.Value.listener;
                if (listener != null)
                {
                    if (!listener.running)
                    {
                        listener.Start(listenerPair.Value.config, OnAcceptFromListener, OnErrorFromListener);
                    }

                    continue;
                }

                StartListener(listenerPair.Value.config);
            }
        }

        private void StopAllListener()
        {
            foreach (var listenerPair in _listenerPairs)
            {
                try
                {
                    var listener = listenerPair.Value.listener;
                    if (listener == null || !listener.running)
                    {
                        continue;
                    }

                    listener.Close();
                }
                catch (Exception ex)
                {
                    OnError(ex, "[SocketServer.StopAllListener] listener stop failed");
                }
            }
        }

        private void StartListener(SocketListenerConfig config)
        {
            if (!_listenerPairs.ContainsKey((config.ip, config.port)))
            {
                throw new InvalidOperationException($"{config.ip}:{config.port} not exists.");
            }

            var listener = CreateListener();
            if (listener == null)
            {
                throw ExceptionUtil.IOEReturnedNull(nameof(CreateListener));
            }

            _listenerPairs[(config.ip, config.port)].SetListener(listener);

            listener.Start(config, OnAcceptFromListener, OnErrorFromListener);

        }

        private string GenAndBookingSessionId()
        {
            var id = string.Empty;

            do
            {
                id = Guid.NewGuid().ToString();
            } while (!_sessions.TryAdd(id, default(SocketSession)));

            return id;
        }

        private void OnSessionClosed(SocketSession closeSocketSession)
        {
            try
            {
                if (!_sessions.TryRemove(closeSocketSession.id, out var session))
                {
                    return;
                }

                InternalOnSessionClosed(closeSocketSession);

                closeSocketSession.socket.Dispose();

                closeSocketSession.OnClosed();
            }
            catch (Exception ex)
            {
                OnError(ex);
                throw;
            }
        }

        protected bool OnAcceptFromListener(BaseSocketListener listener, Socket socket)
        {
            if (_sessions.Count >= option.maxConnection)
            {
                return false;
            }

            string newSessionId = null;

            try
            {
                socket.NoDelay = option.noDelay;

                newSessionId = GenAndBookingSessionId();
                if (string.IsNullOrEmpty(newSessionId))
                {
                    throw new Exception("Session Id generation failed.");
                }

                var newMsgFilterFactory = _messageFilterFactory.Create();
                if (newMsgFilterFactory == null)
                {
                    throw ExceptionUtil.IOEReturnedNull("Message filter factory");
                }

                var newSession = CreateSession(newSessionId);
                if (newSession == null)
                {
                    throw ExceptionUtil.IOEReturnedNull(nameof(CreateSession));
                }

                _sessions[newSessionId] = newSession;

                newSession.Initialize(newSessionId, socket, newMsgFilterFactory, OnSessionClosed);

                onSessionConfiguration?.Invoke(new SocketSessionConfigurator(newSession));

                Task.Run(() =>
                {
                    try
                    {
                        newSession.OnStarted();

                        if (newSession.running)
                        {
                            newSession.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError(ex, "[SocketServer.OnAccept] Socket accept failed");

                        _sessions.TryRemove(newSessionId, out var _);
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                OnError(ex, "[SocketServer.OnAccept] Socket accept failed");

                _sessions.TryRemove(newSessionId, out var _);

                return false;
            }
        }

        protected void OnErrorFromListener(BaseSocketListener listener, Exception ex, string message)
        {
            OnError(ex, $"Error! listener({listener.listenerConfig.ip}:{listener.listenerConfig.port}) - {message}");
        }

        protected void OnError(Exception ex, string message = "")
        {
            onError?.Invoke(ex, message);
        }

        protected virtual void InternalOnSessionClosed(SocketSession closeSocketSession) { }

        protected virtual void InternalOnStart() { }

        protected virtual void InternalOnClose() { }

        protected abstract BaseSocketListener CreateListener();

        protected abstract SocketSession CreateSession(string sessionId);

        //
        public void Start()
        {
            try
            {
                StartAllListener();

                InternalOnStart();

                running = true;
            }
            catch
            {
                StopAllListener();
                throw;
            }
        }

        public void Close()
        {
            running = false;

            InternalOnClose();

            StopAllListener();
        }

        public BaseSocketServer AddListener(SocketListenerConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (string.IsNullOrEmpty(config.ip))
            {
                throw new ArgumentException($"Invalid {nameof(config)}.{nameof(config.ip)} - ({config.ip})");
            }

            if (0 > config.port)
            {
                throw new ArgumentException($"Invalid {nameof(config)}.{nameof(config.port)} - {config.port}");
            }

            if (!_listenerPairs.TryAdd((config.ip, config.port), new ListenerPair(config)))
            {
                throw new InvalidOperationException($"{config.ip}:{config.port} listener config already exists.");
            }

            if (running)
            {
                try
                {
                    StartListener(config);
                }
                catch
                {
                    _listenerPairs.TryRemove((config.ip, config.port), out var _);
                    throw;
                }
            }

            return this;
        }

        public BaseSocketServer RemoveListener(string ip, int port)
        {
            if (!_listenerPairs.TryRemove((ip, port), out var listenerPair))
            {
                throw new Exception("[SocketServer.RemoveListener] Listener that does not exist.");
            }

            listenerPair.listener.Close();

            return this;
        }
    }
}