using System;
using SimpleSocket.Server;

namespace SimpleSocket.Test
{
    public class ServerLauncher<TServer> : IDisposable 
        where TServer : SocketServer
    {
        public readonly TServer server;
        
        public ServerLauncher(TServer server)
        {
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            this.server.Start();
        }

        public void Dispose()
        {
            server.Close();
        }
    }
}