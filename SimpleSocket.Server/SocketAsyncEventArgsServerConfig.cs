namespace SimpleSocket.Server
{
    public class SocketAsyncEventArgsServerConfig
    {
        public int recvBufferSize { get; set; } = 1024;
        public int sendBufferSize { get; set; } = 1024;
        public int maxConnection { get; set; } = 30000;
    }
}