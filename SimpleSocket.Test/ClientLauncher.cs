using System;
using SimpleSocket.Client;

namespace SimpleSocket.Test
{
    public class ClientLauncher<TClient> : IDisposable
        where TClient : BaseSocketClient
    {
        public readonly TClient client = null;

        public ClientLauncher(TClient client)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.client.Start();
        }

        public void Dispose()
        {
            client.Close();
        }
    }
}