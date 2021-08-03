using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleSocket.Server
{
    public abstract class SocketServer
    {
        private class ListenerPair
        {
            public SocketListenerConfig config { get; private set; } = null;
            public SocketListener listener { get; private set; } = null;

            public ListenerPair(SocketListenerConfig config)
            {
                this.config = config ?? throw new ArgumentNullException(nameof(config));
            }

            public void SetListener(SocketListener listener)
            {
                this.listener = listener ?? throw new ArgumentNullException(nameof(listener));
            }
        }

        private readonly ConcurrentDictionary<(string, int), ListenerPair> _listenerPairs =
            new ConcurrentDictionary<(string, int), ListenerPair>();

        private readonly ConcurrentDictionary<string, SocketSession> _sessions =
            new ConcurrentDictionary<string, SocketSession>();

        public bool running { get; private set; } = false;

        public Action<Exception, string> onError { get; set; } = null;

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
                        listener.Start();
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

            var listener = CreateListener(config);
            listener.onError += OnError;
            listener.onAccept += OnAccept;
            if (listener == null)
            {
                throw new InvalidOperationException($"{nameof(CreateListener)} returned null.");
            }

            _listenerPairs[(config.ip, config.port)].SetListener(listener);

            listener.Start();
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

        private void InternalOnSessionClose(SocketSession closeSession)
        {
            try
            {
                if (!_sessions.TryRemove(closeSession.sessionId, out var session))
                {
                    return;
                }
                
                OnSessionClose(closeSession);
            }
            catch (Exception ex)
            {
                OnError(ex);
                throw;
            }

        }

        protected void OnError(Exception ex, string message = "")
        {   
            onError?.Invoke(ex, message);
        }

        // 
        protected virtual ValueTask<bool> OnAccept(Socket sck)
        {
            var newSessionId = GenAndBookingSessionId();

            try
            {
                if (string.IsNullOrEmpty(newSessionId))
                {
                    throw new Exception("Session Id generation failed");
                }

                var newSession = CreateSession(newSessionId);
                newSession.onClose = InternalOnSessionClose;
                
                _sessions[newSessionId] = newSession;

                newSession.Start(sck);

                return ValueTask.FromResult(true);
            }
            catch (Exception ex)
            {
                OnError(ex, "[SocketServer.OnAccept] Socket accept failed");

                _sessions.TryRemove(newSessionId, out var _);

                return ValueTask.FromResult(false);
            }
        }

        protected virtual void OnSessionClose(SocketSession closeSession)  { }

        protected virtual void OnStart() { }
        
        protected virtual void OnClose() { }
        
        protected abstract SocketListener CreateListener(SocketListenerConfig config);
        
        protected abstract SocketSession CreateSession(string sessionId);

        //
        public void Start()
        {
            try
            {
                StartAllListener();

                OnStart();
                
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
            
            OnClose();
            
            StopAllListener();
        }

        public SocketServer AddListener(SocketListenerConfig config)
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

        public SocketServer RemoveListener(string ip, int port)
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