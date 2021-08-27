using System;
using System.Text;
using System.Threading.Tasks;
using SimpleSocket.Common;
using SimpleSocket.Server;

namespace SimpleSocket.ConsoleApp
{
    class TestFilter : PrefixBodySizeMessageFilter
    {
        public TestFilter()
            : base(4)
        {
        }

        protected override int ParsingBodySize(byte[] buffer, int offset, int length)
        {
            return BitConverter.ToInt32(buffer, offset);
        }

        protected override object ParsingBody(byte[] buffer, int offset, int length)
        {
            return Encoding.UTF8.GetString(buffer, offset, length);
        }
    }

    class SocketSessionEventHandler : ISocketSessionEventHandler
    {
        private int connectCount = 0;
        private int disconnectCount = 0;

        public void OnSocketSessionStarted(ISocketSession session)
        {
            Console.WriteLine("Connect : " + connectCount++);
        }

        public void OnSocketSessionClosed(ISocketSession session)
        {
            Console.WriteLine("Disconnect : " + disconnectCount++);
        }

        public ValueTask OnReceived(ISocketSession session, object receivedData)
        {
            return ValueTask.CompletedTask;
        }

        public void OnError(ISocketSession session, Exception ex, string message)
        {
            throw ex;
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var sessionEventHandler = new SocketSessionEventHandler();

            var server = new SocketAsyncEventArgsServer(new SocketAsyncEventArgsServerConfig.Builder().Build(),
                new GenericMessageFilterFactory<TestFilter>());

            server.onNewSocketSessionConnected += delegate (SocketSessionConfigurator configurator)
            {
                configurator.SetSocketSessionEventHandler(sessionEventHandler);
            };

            server.AddListener(new SocketListenerConfig.Builder("0.0.0.0", 9199).SetBacklog(5000).Build());

            server.Start();

            Console.ReadLine();

            server.Close();
        }
    }
}