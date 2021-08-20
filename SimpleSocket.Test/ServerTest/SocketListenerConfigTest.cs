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

            TestUtil.Assert_Exception(() =>
            {
                builder = new SocketListenerConfig.Builder("0.0.0.0", -1);
            });

            TestUtil.Assert_Exception(() =>
            {
                builder = new SocketListenerConfig.Builder(string.Empty, 1);
            });
            
            builder = new SocketListenerConfig.Builder("0.0.0.0", 1);

            TestUtil.Assert_Exception(() =>
            {
                builder.SetProtocolType(ProtocolType.Unknown);
            });
            
            TestUtil.Assert_Exception(() =>
            {
                builder.SetSocketType(SocketType.Unknown);
            });
            
            TestUtil.Assert_Exception(() =>
            {
                builder.SetBacklog(0);
            });
            
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