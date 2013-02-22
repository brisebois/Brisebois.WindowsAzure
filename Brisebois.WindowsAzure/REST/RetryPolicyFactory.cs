using System;
using Microsoft.Practices.TransientFaultHandling;

namespace Brisebois.WindowsAzure.REST
{
    public static class RetryPolicyFactory
    {
        public static RetryPolicy MakeHttpRetryPolicy()
        {
            return Exponential(new HttpTransientErrorDetectionStrategy(true));
        }

        private static RetryPolicy Exponential(ITransientErrorDetectionStrategy stgy,
                                               int retryCount = 10,
                                               double maxBackoffDelayInSeconds = 1024,
                                               double delta = 2)
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