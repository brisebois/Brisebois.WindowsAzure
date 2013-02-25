using System;
using System.Net;

namespace Brisebois.WindowsAzure.REST
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/23/asynchronously-calling-rest-services-from-a-windows-azure-role/
    /// </summary>
    public class RestClientException : Exception
    {
        public readonly HttpStatusCode code;
        public readonly string content;

        public RestClientException(HttpStatusCode code, string content)
        {
            this.code = code;
            this.content = content;
        }
    }
}