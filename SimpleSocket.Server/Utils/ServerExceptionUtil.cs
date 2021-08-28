using System;

namespace SimpleSocket.Server
{
    public class ServerExceptionUtil
    {
        public static InvalidOperationException IOEInvalidSessionState(int sessionState)
        {
            return new InvalidOperationException($"Invalid session state - State({SocketSessionState.Name(sessionState)})");
        }
    }
}
