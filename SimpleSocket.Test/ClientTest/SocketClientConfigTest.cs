using System.Net.Sockets;
using NUnit.Framework;
using SimpleSocket.Client;

namespace SimpleSocket.Test.ClientTest
{
    [TestFixture]
    public class SocketClientConfigTest
    {
        [Test]
        public void SettingTest()
        {
            SocketClientConfig.Builder builder = null;

            try
            {
                builder = new SocketClientConfig.Builder("0.0.0.0", -1);
                Assert.Fail();
            }
            catch { }

            try
            {
                builder = new SocketClientConfig.Builder(string.Empty, 1);
                Assert.Fail();
            }
            catch { }
            
            builder = new SocketClientConfig.Builder("0.0.0.0", 1);

            try
            {
                builder.SetProtocolType(ProtocolType.Unknown);
                Assert.Fail();
            }
            catch { }

            try
            {
                builder.SetSocketType(SocketType.Unknown);
                Assert.Fail();
            }
            catch  { }

            const string ip = "127.0.0.1";
            const int port = 1919;
            const ProtocolType protocolType = ProtocolType.Udp;
            const SocketType socketType = SocketType.Dgram;

            builder.SetIp(ip)
                .SetPort(port)
                .SetProtocolType(protocolType)
                .SetSocketType(socketType);

            var config = builder.Build();
            Assert.NotNull(config);
            Assert.AreEqual(config.ip, ip);
            Assert.AreEqual(config.port, port);
            Assert.AreEqual(config.protocolType, protocolType);
            Assert.AreEqual(config.socketType, socketType);
        }
    }
}