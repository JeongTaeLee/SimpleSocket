using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using NUnit.Framework;
using SimpleSocket.Client;
using SimpleSocket.Common;
using SimpleSocket.Server;

namespace SimpleSocket.Test.ServerTest
{
    [TestFixture]
    public class SocketAsyncEventArgsServerTest
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

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public async Task HandleTest()
        {
            const int testConnectCount = 500;

            var startedCount = 0;
            var closedCount = 0;
            var receivedCount = 0;
            var errorCount = 0;

            var testMsgStr = "Hello world";
            var testMsgBytes = CreateMessage(testMsgStr);

            var sessionHandler = new EventSocketSessionEventHandler();
            sessionHandler.onSocketSessionStarted += (ssn) =>
            {
                Interlocked.Increment(ref startedCount);
            };
            sessionHandler.onSocketSessionClosed += (ssn) =>
            {
                Interlocked.Increment(ref closedCount);
            };
            sessionHandler.onReceived += (ssn, msg) =>
            {
                if (testMsgStr == msg.ToString())
                {
                    Interlocked.Increment(ref receivedCount);
                }
            };
            sessionHandler.onError += (session, exception, arg3) =>
            {
                Interlocked.Increment(ref errorCount);
            };

            var server = CreateServer(sessionHandler);
            server.onError += (ex, msg) =>
            {
                ++errorCount;
            };

            var serverIp = "0.0.0.0";
            var serverPort = TestUtil.GetFreePortNumber();
            server.AddListener(new SocketListenerConfig.Builder(serverIp, serverPort).Build());

            List<Task> tasks = new List<Task>();

            using (var serverLauncher = server.ToServerLauncher())
            {
                for (int index = 0; index < testConnectCount; ++index)
                {
                    tasks.Add(RunClient(testMsgBytes));
                }
            }

            await Task.WhenAll(tasks);
            await Task.Delay(5000);

            Assert.AreEqual(0, errorCount);

            Assert.AreEqual(testConnectCount, startedCount);
            Assert.AreEqual(testConnectCount, closedCount);
            Assert.AreEqual(testConnectCount, receivedCount);

            async Task RunClient(byte[] sendByte)
            {
                var client = CreateClient("127.0.0.1", serverPort);
                using (var clientLauncher = client.ToClientLauncher())
                {
                    // send
                    client.Send(sendByte);
                }
            }
        }

        [Test]
        public void ListenerAddRemoveTest()
        {
            var server = CreateServer();

            var listenerIp = "0.0.0.0";
            var localIp = "127.0.0.1";

            var beforeServerStartPort = TestUtil.GetFreePortNumber();
            server.AddListener(new SocketListenerConfig.Builder(listenerIp, beforeServerStartPort).Build());

            // 서버 시작 전 추가된 리스너 포트에 연결 시도.
            Assert.IsFalse(TestUtil.TryConnect(localIp, beforeServerStartPort));

            server.Start();

            // 서버 시작 후 시작 전 추가되었던 리스너 포트로 연결 시도.
            Assert.IsTrue(TestUtil.TryConnect(localIp, beforeServerStartPort));

            var afterServerStartPort = TestUtil.GetFreePortNumber();
            server.AddListener(new SocketListenerConfig.Builder(listenerIp, afterServerStartPort).Build());

            // 시작 후 추가한 리스너 포트로 연결.
            Assert.IsTrue(TestUtil.TryConnect(localIp, afterServerStartPort));

            server.RemoveListener(listenerIp, beforeServerStartPort);

            // 리스너 종료 후 연결.
            Assert.IsFalse(TestUtil.TryConnect(localIp, beforeServerStartPort));

            server.Close();

            // 서버 종료 후 연결.
            Assert.IsFalse(TestUtil.TryConnect(localIp, afterServerStartPort));
        }

        private SocketAsyncEventArgsServer CreateServer(ISocketSessionEventHandler handler = null)
        {
            var server = new SocketAsyncEventArgsServer(
                new SocketAsyncEventArgsServerConfig.Builder().Build()
                , new GenericMessageFilterFactory<TestFilter>());

            if (handler != null)
            {
                server.onNewSocketSessionConnected += sessionConfigurator =>
                {
                    sessionConfigurator.SetSocketSessionEventHandler(handler);
                };
            }

            return server;
        }

        private SocketAsyncEventArgsClient CreateClient(string ip, int port, ISocketClientEventHandler handler = null)
        {
            var client = new SocketAsyncEventArgsClient(
                new SocketAsyncEventArgsClientConfig.Builder().Build()
                , new SocketClientConfig.Builder(ip, port).Build()
                , new TestFilter());

            if (handler != null)
            {
                client.SetSocketClientEventHandler(handler);
            }

            return client;
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