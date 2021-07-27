using System.Net.Sockets;

namespace SimpleSocket.Server
{
    public static class SocketExtensions
    {
        public static void SafeClose(this Socket sck)
        {
            if (sck == null)
            {
                return;
            }

            try
            {
                sck.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                sck.Close();
            }
        }
    }
}