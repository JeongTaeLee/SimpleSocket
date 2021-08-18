using System;
using System.Text;
using NUnit.Framework;
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
        public void MainTest()
        {
            var server = new SocketAsyncEventArgsServer(
                new SocketAsyncEventArgsServerConfig.Builder()
                    .SetRecvBufferSize(1024)
                    .SetSendBufferSize(1024)
                    .SetMaxConnection(1000)
                    .Build()
                , new GenericMessageFilterFactory<TestFilter>());

            server.Start();
            Assert.AreEqual(server.running, true);


            server.Close();
            Assert.AreEqual(server.running, false);
        }
    }
}