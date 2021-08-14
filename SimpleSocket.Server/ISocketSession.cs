using System;
using System.Threading.Tasks;

namespace SimpleSocket.Server
{
    public interface ISocketSession
    {
        void Close();

        void Send(byte[] buffer);
        void Send(ArraySegment<byte> segment);
        void Send(byte[] buffer, int offset, int length);

        Task SendAsync(byte[] buffer);
        Task SendAsync(ArraySegment<byte> segment);
        Task SendAsync(byte[] buffer, int offset, int length);

    }
}