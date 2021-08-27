using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleSocket.Server;

namespace SimpleSocket.Test.ServerTest
{
    public class SocketAsyncEventArgsListenerTest
    {
        // TODO @jeongtae.lee : Parallel(병렬)로 커넥션 요청시 일부 요청이 누락되는 버그가 있음 수정 필요.
        [TestCase(5000)]
        public void ConnectTest(long testConnectCount)
        { 
            var listenerIp = "0.0.0.0";
            var listenerPort = TestUtil.GetFreePortNumber();
            var connectedCount = 0L;
            
            var listener = new SocketAsyncEventArgsListener();
            listener.Start(new SocketListenerConfig.Builder(listenerIp, listenerPort)
                .SetBacklog(10000)
                .Build()
                , (listener, socket) =>
                {
                    ++connectedCount;
                    return true;
                }
                , (listener, ex, msg) =>
                {
                    throw ex;
                });

            for (int index = 0; index < testConnectCount; ++index)
            {
                TestUtil.Connect("127.0.0.1", listenerPort);
            }
            
            Thread.Sleep(100);
            
            Assert.AreEqual(testConnectCount, connectedCount);
            
        }
    }
}