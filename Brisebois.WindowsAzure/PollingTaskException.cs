using System;
using System.Runtime.Serialization;

namespace Brisebois.WindowsAzure
{
    [Serializable]
    public class PollingTaskException : Exception
    {
        public PollingTaskException()
        {
            // Add any type-specific logic, and supply the default message.
        }

        public PollingTaskException(string message)
            : base(message)
        {
            // Add any type-specific logic.
        }
        public PollingTaskException(string message, Exception innerException) :
            base(message, innerException)
        {
            // Add any type-specific logic for inner exceptions.
        }
        protected PollingTaskException(SerializationInfo info,
                                       StreamingContext context)
            : base(info, context)
        {
            // Implement type-specific serialization constructor logic.
        }
    }
}