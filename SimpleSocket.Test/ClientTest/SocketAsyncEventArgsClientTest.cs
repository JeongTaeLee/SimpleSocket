using NUnit.Framework;
using SimpleSocket.Client;
using SimpleSocket.Common;
using SimpleSocket.Server;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSocket.Test.ClientTest
{
    [TestFixture]
    public class SocketAsyncEventArgsClientTest
    {
        private class TestFilter : PrefixBodySizeMessageFilter
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

        [Test]
        public async Task HandleTest()
        {
            const int sendedCount = 100;

            var sendMsgStr = "Hello world!";
            var sendMsgBytes = CreateMessage(sendMsgStr);

            var errorCount = 0;
            var closedCount = 0;
            var receivedCount = 0;

            var sessionEventHandler = new EventSocketSessionEventHandler();
            sessionEventHandler.onSocketSessionStarted += (ssn) =>
            {
                Parallel.For(0, sendedCount, (idx) =>
                {
                    ssn.Send(sendMsgBytes);
                });

                ssn.Close();
            };
            sessionEventHandler.onError += (ssn, ex, msg) =>
            {
                Interlocked.Increment(ref errorCount);
            };

            var clientEventHandler = new EventSocketClientEventHandler();
            clientEventHandler.onSocketClientClosed += (ssn) =>
            {
                Interlocked.Increment(ref closedCount);
            };
            clientEventHandler.onReceived += (client, msg) =>
            {
                if (sendMsgStr == msg.ToString())
                {
                    Interlocked.Increment(ref receivedCount);
                }
            };
            clientEventHandler.onError += (client, ex, msg) =>
            {
                Interlocked.Increment(ref errorCount);
            };


            var server = new SocketAsyncEventArgsServer(new SocketServerOption.Builder().Build()
                , new GenericMessageFilterFactory<TestFilter>());

            server.onSessionConfiguration = (cnfger) =>
            {
                cnfger.SetSocketSessionEventHandler(sessionEventHandler);
            };
            server.onError += (ex, msg) =>
            {
                Interlocked.Increment(ref errorCount);
            };

            var serverIp = "0.0.0.0";
            var serverPort = TestUtil.GetFreePortNumber();

            server.AddListener(new SocketListenerConfig.Builder(serverIp, serverPort).Build());

            using (var serverLauncher = server.ToServerLauncher())
            {
                var config = new SocketClientConfig.Builder("127.0.0.1", serverPort).Build();
                var client = new SocketAsyncEventArgsClient(config, new TestFilter());
                client.SetSocketClientEventHandler(clientEventHandler);

                using (var clientLauncher = client.ToClientLauncher())
                {
                    await Task.Delay(1000);
                }
            }

            Assert.AreEqual(0, errorCount);
            Assert.AreEqual(1, closedCount);
            Assert.AreEqual(sendedCount, receivedCount);
        }


        private byte[] CreateMessage(string data)
        {
            var testBody = Encoding.UTF8.GetBytes(data);
            var testHeader = BitConverter.GetBytes(testBody.Length);

            var testBuffer = new byte[4 + testBody.Length];
            Buffer.BlockCopy(testHeader, 0, testBuffer, 0, testHeader.Length);
            Buffer.BlockCopy(testBody, 0, testBuffer, testHeader.Length, testBody.Length);

            return testBuffer;
        }
    }
}
