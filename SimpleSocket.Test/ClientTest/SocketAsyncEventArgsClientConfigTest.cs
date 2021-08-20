using NUnit.Framework;
using SimpleSocket.Client;

namespace SimpleSocket.Test.ClientTest
{
    [TestFixture]
    public class SocketAsyncEventArgsClientConfigTest
    {
        [Test]
        public void SettingTest()
        {
            var builder = new SocketAsyncEventArgsClientConfig.Builder();

            TestUtil.Assert_Exception(() =>
            {
                builder.SetRecvBufferSize(0);
            });
            
            TestUtil.Assert_Exception(() =>
            {
                builder.SetSendBufferSize(0);
            });
          
            const int recvBufferSize = 8888;
            const int sendBufferSize = 7777;

            builder.SetRecvBufferSize(recvBufferSize)
                .SetSendBufferSize(sendBufferSize);

            var config = builder.Build();
            
            Assert.NotNull(config);
            Assert.AreEqual(config.recvBufferSize, recvBufferSize);
            Assert.AreEqual(config.sendBufferSize, sendBufferSize);
        }
    }
}