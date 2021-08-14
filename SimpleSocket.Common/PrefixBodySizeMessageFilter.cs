using System;

namespace SimpleSocket.Common
{
    public abstract class PrefixBodySizeMessageFilter : IMessageFilter
    {
        public readonly int prefixSize = 0;

        public bool parsedPrefix { get; private set; } = false;
        public int bodySize { get; private set; } = 0;


        public PrefixBodySizeMessageFilter(int prefixSize)
        {
            if (0 > prefixSize)
            {
                throw new ArgumentException($"{nameof(prefixSize)} cannot be less than zero - fixedSize({prefixSize})");
            }
            
            this.prefixSize = prefixSize;
        }
        
        public object Filtering(byte[] buffer, int offset, int length, out int readSize)
        {
            readSize = 0;
            
            if (!parsedPrefix)
            {
                if (prefixSize > length)
                {
                    return null;
                }

                bodySize = ParsingBodySize(buffer, offset, prefixSize);
                parsedPrefix = true;
                
                readSize += prefixSize;
                offset += prefixSize;
            }

            var remainSize = (length - readSize); 
            if (remainSize < bodySize)
            {
                return null;
            }

            try
            {
                return ParsingBody(buffer, offset, bodySize);
            } 
            finally
            {
                readSize += bodySize;
                
                bodySize = 0;
                parsedPrefix = false;
            }
        }
        
        protected abstract int ParsingBodySize(byte[] buffer, int offset, int length);
        protected abstract object ParsingBody(byte[] buffer, int offset, int length);
    }
}