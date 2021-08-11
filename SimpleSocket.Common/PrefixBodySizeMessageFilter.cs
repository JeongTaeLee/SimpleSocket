using System;

namespace SimpleSocket.Common
{
    public abstract class PrefixBodySizeMessageFilter : IMessageFilter
    {
        public readonly int prefixSize = 0;
        
        private bool _parsedPrefix = false;
        private int _bodySize = 0;

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
            
            if (!_parsedPrefix)
            {
                if (prefixSize > length)
                {
                    return null;
                }

                _bodySize = ParsingBodySize(buffer, offset, prefixSize);
                _parsedPrefix = true;
                
                readSize += prefixSize;
                offset += prefixSize;
            }

            var remainSize = (length - readSize); 
            if (remainSize > _bodySize)
            {
                return null;
            }

            try
            {
                return ParsingBodySize(buffer, offset, _bodySize);
            }
            finally
            {
                readSize += _bodySize;
                
                _bodySize = 0;
                _parsedPrefix = false;
            }
        }
        
        protected abstract int ParsingBodySize(byte[] buffer, int offset, int length);
    }
}