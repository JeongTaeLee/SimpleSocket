using System;
using NUnit.Framework;
using SimpleSocket.Server;

namespace SimpleSocket.Test.ServerTest
{
    [TestFixture]
    public class SocketAsyncEventArgsServerConfigTest
    {
        [Test]
        public void SettingTest()
        {
            var builder = new SocketAsyncEventArgsServerConfig.Builder();

            
            TestUtil.Assert_Exception(() =>
            {
                builder.SetMaxConnection(0);
            });
            
            TestUtil.Assert_Exception(() =>
            {
                builder.SetRecvBufferSize(0);
            });
            
            TestUtil.Assert_Exception(() =>
            {
                builder.SetSendBufferSize(0);
            });
            
            var maxConnection = 9999;
            var recvBufferSize = 8888;
            var sendBufferSize = 7777;

            builder.SetMaxConnection(maxConnection)
                .SetRecvBufferSize(recvBufferSize)
                .SetSendBufferSize(sendBufferSize);

            var config = builder.Build();
            
            Assert.NotNull(config);
            Assert.AreEqual(config.maxConnection, maxConnection);
            Assert.AreEqual(config.recvBufferSize, recvBufferSize);
            Assert.AreEqual(config.sendBufferSize, sendBufferSize);
        }
    }
}