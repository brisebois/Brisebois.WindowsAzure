using System;
using System.Net;

namespace Brisebois.WindowsAzure.REST
{
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