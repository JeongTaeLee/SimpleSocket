namespace SimpleSocket.Common
{
    public interface IMessageFilter
    {
        object Filtering(byte[] buffer, int offset, int length, out int readSize);
    }
}