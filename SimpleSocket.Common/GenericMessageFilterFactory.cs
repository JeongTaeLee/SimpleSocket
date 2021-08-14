namespace SimpleSocket.Common
{
    public class GenericMessageFilterFactory<TMessageFilter> : IMessageFilterFactory
        where TMessageFilter : IMessageFilter, new()
    {
        public IMessageFilter Create()
        {
            return new TMessageFilter();
        }
    }
}