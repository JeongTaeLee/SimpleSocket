using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleSocket.Client;
using SimpleSocket.Common;

namespace SimpleSocket.ConsoleApp02
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

    class ClientEventHandler : ISocketClientEventHandler
    {
        public void OnSocketClientClosed(SocketClient client)
        {

        }

        public void OnReceived(SocketClient client, object receivedData)
        {

        }

        public void OnError(SocketClient client, Exception ex, string message)
        {
            throw ex;
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var argsConfig = new SocketAsyncEventArgsClientConfig.Builder().Build();
            var config = new SocketClientConfig.Builder("127.0.0.1", 9199).Build();

            for (int index = 0; index < 100; ++index)
            {
                var client = new SocketAsyncEventArgsClient(argsConfig, config, new TestFilter());
                client.Start();
                client.Close();

                Console.WriteLine($"{index} Request!");
            }

            await Task.Delay(100000);
            //while (true)
            //{

            //}
        }
    }
}
