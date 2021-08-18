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


            try
            {
                builder.SetRecvBufferSize(0);
                Assert.Fail();
            }
            catch { }

            try
            {
                builder.SetSendBufferSize(0);
                Assert.Fail();
            }
            catch { }
            
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