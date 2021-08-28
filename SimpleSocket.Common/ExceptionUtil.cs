using System;

namespace SimpleSocket.Common
{
    public static class ExceptionUtil
    {
        public static InvalidOperationException IOEVariableNotSet(string variableName)
        {
            throw new InvalidOperationException($"{variableName} not set.");
        }

        public static InvalidOperationException IOEReturnedNull(string name)
        {
            throw new InvalidOperationException($"{name} returned null.");
        }
    }
}
