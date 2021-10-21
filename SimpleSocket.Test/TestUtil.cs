using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using SimpleSocket.Client;
using SimpleSocket.Server;

namespace SimpleSocket.Test
{
    public static class TestUtil
    {
        private static readonly IPEndPoint DefaultLoopbackEndPoint = new IPEndPoint(IPAddress.Loopback, port: 0);
        
        public static int GetFreePortNumber()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(DefaultLoopbackEndPoint);
                return ((IPEndPoint) socket.LocalEndPoint).Port;
            }
        }
        
        public static void Assert_Exception(Action action)
        {
            try
            {
                action?.Invoke();
                Assert.Fail();
            }
            catch (Exception) 
            { 

            }
        }
        
        public static void Assert_Exception<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action?.Invoke();
                Assert.Fail();
            }
            catch (TException) 
            { 
            }
        }

        public static void Connect(string ip, int port)
        {
            using (var socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                socket.Connect(ipEndPoint);
            }
        }

        public static bool TryConnect(string ip, int port, out Exception throwedEx)
        {
            throwedEx = null;
            
            try
            {
                using (var socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
                {
                    var ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                    socket.Connect(ipEndPoint);
                }

                return true;
            }
            catch (Exception e)
            {
                throwedEx = e;
                
                return false;
            }
        }
        
        public static bool TryConnect(string ip, int port)
        {
            try
            {
                using (var socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
                {
                    var ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                    socket.Connect(ipEndPoint);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public static ServerLauncher<TServer> ToServerLauncher<TServer>(this TServer server)
            where TServer : BaseSocketServer
        {
            return new ServerLauncher<TServer>(server);
        }
        
        public static ClientLauncher<TClient> ToClientLauncher<TClient>(this TClient client)
            where TClient : BaseSocketClient
        {
            return new ClientLauncher<TClient>(client);
        }
    }
}