using System;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Brisebois.WindowsAzure.Rest
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/21/defining-an-http-transient-error-detection-strategy-for-rest-calls/
    /// </summary>
    public static class RetryPolicyFactory
    {

        public static RetryPolicy MakeHttpRetryPolicy()
        {
            return MakeHttpRetryPolicy(10, false);
        }

        public static RetryPolicy MakeHttpRetryPolicy(int count,
                                                      bool notFoundIsTransient)
        {
            ITransientErrorDetectionStrategy strategy = new HttpTransientErrorDetectionStrategy(notFoundIsTransient);
            return Exponential(strategy, count);
        }
        
        private static RetryPolicy Exponential(ITransientErrorDetectionStrategy stgy,
                                               int count)
        {
            return Exponential(stgy, count, 1024d, 2d);
        }

        private static RetryPolicy Exponential(ITransientErrorDetectionStrategy stgy,
                                                int retryCount,
                                                double maxBackoffDelayInSeconds,
                                                double delta)
        {
            var maxBackoff = TimeSpan.FromSeconds(maxBackoffDelayInSeconds);
            var deltaBackoff = TimeSpan.FromSeconds(delta);
            var minBackoff = TimeSpan.FromSeconds(0);

            var exponentialBackoff = new ExponentialBackoff(retryCount,
                                                            minBackoff,
                                                            maxBackoff,
                                                            deltaBackoff);
            return new RetryPolicy(stgy, exponentialBackoff);
        }
    }
}