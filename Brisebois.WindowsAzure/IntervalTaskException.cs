using System;
using System.Runtime.Serialization;

namespace Brisebois.WindowsAzure
{
    [Serializable]
    public class IntervalTaskException : Exception
    {
        public IntervalTaskException()
        {
            // Add any type-specific logic, and supply the default message.
        }

        public IntervalTaskException(string message)
            : base(message)
        {
            // Add any type-specific logic.
        }
        public IntervalTaskException(string message, Exception innerException) :
            base(message, innerException)
        {
            // Add any type-specific logic for inner exceptions.
        }
        protected IntervalTaskException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        {
            // Implement type-specific serialization constructor logic.
        }
    }
}