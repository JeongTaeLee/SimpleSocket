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
        static SocketAsyncEventArgsClientConfig argsConfig = new SocketAsyncEventArgsClientConfig.Builder().Build();
        static SocketClientConfig config = new SocketClientConfig.Builder("127.0.0.1", 9199).Build();

        static async Task Main(string[] args)
        {
            var argsConfig = new SocketAsyncEventArgsClientConfig.Builder().Build();
            var config = new SocketClientConfig.Builder("127.0.0.1", 9199).Build();

            Parallel.For(0, 100, (index) =>
            {
                RunClient(index);
            });

            while (true)
            {

            }
        }

        static void RunClient(int index)
        {
            var msg = $"Hello world - {index}";
            var msgBytes = Encoding.UTF8.GetBytes(msg);
            var sizeBytes = BitConverter.GetBytes(msgBytes.Length);
            
            var totalBytes = new byte[msgBytes.Length + sizeBytes.Length];
            Buffer.BlockCopy(sizeBytes, 0, totalBytes, 0, sizeBytes.Length);
            Buffer.BlockCopy(msgBytes, 0, totalBytes, sizeBytes.Length, msgBytes.Length);

            var client = new SocketAsyncEventArgsClient(argsConfig, config, new TestFilter());
            client.Start();

            client.Send(totalBytes);

            client.Close();
            Console.WriteLine($"Request - {index}");
        }

    }
}
