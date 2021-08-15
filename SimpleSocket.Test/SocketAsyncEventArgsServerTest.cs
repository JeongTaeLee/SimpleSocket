using System;
using System.Text;
using NUnit.Framework;
using SimpleSocket.Common;
using SimpleSocket.Server;

namespace SimpleSocket.Test
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
            var server = new SocketAsyncEventArgsServer(new SocketAsyncEventArgsServerConfig()
            {
                maxConnection = 1000,
                recvBufferSize = 1024,
                sendBufferSize = 1024,
            }, new GenericMessageFilterFactory<TestFilter>());
            
            server.Start();
            Assert.AreEqual(server.running, true);
            
            
            server.Close();
            Assert.AreEqual(server.running, false);
        }
        
    }
}