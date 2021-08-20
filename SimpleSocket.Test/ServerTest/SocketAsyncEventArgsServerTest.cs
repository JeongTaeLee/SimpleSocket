using System;
using System.Net.Http;
using System.Text;
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

        public SocketAsyncEventArgsServer CreateServer(ISocketSessionEventHandler handler = null)
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
    }
}