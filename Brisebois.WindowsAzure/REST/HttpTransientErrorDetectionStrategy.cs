using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Practices.TransientFaultHandling;

namespace Brisebois.WindowsAzure.REST
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/21/defining-an-http-transient-error-detection-strategy-for-rest-calls/
    /// </summary>
    public class HttpTransientErrorDetectionStrategy
        : ITransientErrorDetectionStrategy
    {
        private readonly List<HttpStatusCode> statusCodes =
            new List<HttpStatusCode>
                {
                    HttpStatusCode.GatewayTimeout,
                    HttpStatusCode.RequestTimeout,
                    HttpStatusCode.ServiceUnavailable,
                };

        public HttpTransientErrorDetectionStrategy(bool isNotFoundAsTransient = false)
        {
            if (isNotFoundAsTransient)
                statusCodes.Add(HttpStatusCode.NotFound);
        }

        public bool IsTransient(Exception ex)
        {
            var we = ex as WebException;
            if (we == null)
                return false;

            var response = we.Response as HttpWebResponse;

            var isTransient = response != null
                              && statusCodes.Contains(response.StatusCode);
            return isTransient;
        }
    }
}