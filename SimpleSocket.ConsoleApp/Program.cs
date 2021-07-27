using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SimpleSocket.ConsoleApp
{
    class ListenerPair
    {
        public string config { get; private set; } = null;
        public string listener { get; private set; } = null;
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var eventArgs = new SocketAsyncEventArgs();
            eventArgs.Dispose();
            Console.WriteLine("Close");
        }
    }
}