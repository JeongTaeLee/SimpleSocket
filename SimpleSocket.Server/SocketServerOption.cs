
using System;

public class SocketServerOption
{
    public const bool DEFAULT_NO_DELAY = false;
    public const int MAX_CONNECTION = 30000;
    public const int DEFAULT_RECV_BUFFER_SIZE = 1024;
    public const int DEFAULT_SEND_BUFFER_SIZE = 1024;

    public readonly bool noDelay;
    public readonly int maxConnection;
    public readonly int recvBufferSize;
    public readonly int sendBufferSize;

    private SocketServerOption(bool noDelay, int maxConnection, int recvBufferSize, int sendBufferSize)
    {
        if (0 >= recvBufferSize)
        {
            throw new ArgumentException($"{nameof(recvBufferSize)} cannot be equal to or less than zero");
        }

        if (0 >= sendBufferSize)
        {
            throw new ArgumentException($"{nameof(sendBufferSize)} cannot be equal to or less than zero");
        }
        
        if (0 >= maxConnection)
        {
            throw new ArgumentException($"{nameof(maxConnection)} cannot be equal to or less than zero");
        }

        this.noDelay = noDelay;
        this.maxConnection = maxConnection;
        this.recvBufferSize = recvBufferSize;
        this.sendBufferSize = sendBufferSize;
    }

    public class Builder
    {
        public bool noDelay { get; private set; } = DEFAULT_NO_DELAY;
        public int maxConnection { get; private set; } = MAX_CONNECTION;
        public int recvBufferSize { get; private set; } = DEFAULT_RECV_BUFFER_SIZE;
        public int sendBufferSize { get; private set; } = DEFAULT_SEND_BUFFER_SIZE;

        public SocketServerOption Build()
        {
            return new SocketServerOption(noDelay, maxConnection, recvBufferSize, sendBufferSize);
        }

        public Builder SetNoDelay(bool noDelay)
        {
            this.noDelay = noDelay;
            return this;
        }

        public Builder SetRecvBufferSize(int recvBufferSize)
        {
            if (0 >= recvBufferSize)
            {
                throw new ArgumentException($"{nameof(recvBufferSize)} cannot be equal to or less than zero");
            }

            this.recvBufferSize = recvBufferSize;

            return this;
        }

        public Builder SetSendBufferSize(int sendBufferSize)
        {
            if (0 >= sendBufferSize)
            {
                throw new ArgumentException($"{nameof(sendBufferSize)} cannot be equal to or less than zero");
            }

            this.sendBufferSize = sendBufferSize;

            return this;
        }

        public Builder SetMaxConnection(int maxConnection)
        {
            if (0 >= maxConnection)
            {
                throw new ArgumentException($"{nameof(maxConnection)} cannot be equal to or less than zero");
            }

            this.maxConnection = maxConnection;

            return this;
        }
    }
}