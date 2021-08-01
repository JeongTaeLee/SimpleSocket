using System;

namespace SimpleSocket.Server
{
    public interface ISession
    {
        void Close();

        void Send(byte[] buffer);
        void Send(ArraySegment<byte> segment);
        void Send(byte[] buffer, int offset, int length);
    }
}