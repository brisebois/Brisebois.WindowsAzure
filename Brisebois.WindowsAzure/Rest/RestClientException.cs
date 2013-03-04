using System;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Brisebois.WindowsAzure.Rest
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/23/asynchronously-calling-rest-services-from-a-windows-azure-role/
    /// </summary>
    [Serializable]
    public class RestClientException : Exception
    {
        private readonly HttpStatusCode code;
        private readonly string content;

        public RestClientException(HttpStatusCode code, string content)
        {
            this.code = code;
            this.content = content;
        }

        public RestClientException()
        {
            // Add any type-specific logic, and supply the default message.
        }

        public RestClientException(string message)
            : base(message)
        {
            // Add any type-specific logic.
        }
        public RestClientException(string message, Exception innerException) :
            base(message, innerException)
        {
            // Add any type-specific logic for inner exceptions.
        }
        protected RestClientException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        {
            // Implement type-specific serialization constructor logic.
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            base.GetObjectData(info, context);

            info.AddValue("Code", code);
            info.AddValue("Content", content);
        }
        
        public HttpStatusCode Code
        {
            get { return code; }
        }

        public string Content
        {
            get { return content; }
        }
    }
}