using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSocket.Client.Utils
{
    public static class ClientExceptionUtil
    {
        public static InvalidOperationException IOEInvalidSessionState(int sessionState)
        {
            return new InvalidOperationException($"Invalid session state - State({SocketClientState.Name(sessionState)})");
        }
    }
}
