using System;
using System.Net.Sockets;
using NUnit.Framework;
using SimpleSocket.Server;

namespace SimpleSocket.Test.ServerTest
{
    [TestFixture]
    public class SocketListenerConfigTest
    {
        [Test]
        public void SettingTest()
        {
            SocketListenerConfig.Builder builder = null;

            try
            {
                builder = new SocketListenerConfig.Builder("0.0.0.0", -1);
                Assert.Fail();
            }
            catch { }

            try
            {
                builder = new SocketListenerConfig.Builder(string.Empty, 1);
                Assert.Fail();
            }
            catch { }

            builder = new SocketListenerConfig.Builder("0.0.0.0", 1);

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

            try
            {
                builder.SetBacklog(0);
                Assert.Fail();
            }
            catch { }
            
            const string ip = "127.0.0.1";
            const int port = 1919;
            const int backlog = 999;
            const SocketType socketType = SocketType.Dgram;
            const ProtocolType protocolType = ProtocolType.Udp;

            builder.SetIp(ip)
                .SetPort(port)
                .SetBacklog(backlog)
                .SetProtocolType(protocolType)
                .SetSocketType(socketType);

            var config = builder.Build();
            Assert.NotNull(config);
            Assert.AreEqual(config.ip, ip);
            Assert.AreEqual(config.port, port);
            Assert.AreEqual(config.backlog, backlog);
            Assert.AreEqual(config.protocolType, protocolType);
            Assert.AreEqual(config.socketType, socketType);
        }
    }
}