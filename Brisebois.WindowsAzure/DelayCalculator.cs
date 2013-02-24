using System;

namespace Brisebois.WindowsAzure
{
    /// <summary>
    /// Details: https://alexandrebrisebois.wordpress.com/2013/02/19/calculating-an-exponential-back-off-delay-based-on-failed-attempts/
    /// </summary>
    public static class DelayCalculator
    {
        public static Int64 ExponentialDelay(int failedAttempts,
                                             int maxDelayInSeconds = 1024)
        {
            //Attempt 1     0s	    0s
            //Attempt 2	    2s	    2s
            //Attempt 3	    4s	    4s
            //Attempt 4	    8s	    8s
            //Attempt 5	    16s	    16s
            //Attempt 6	    32s	    32s

            //Attempt 7	    64s	    1m 4s
            //Attempt 8	    128s	2m 8s
            //Attempt 9	    256s	4m 16s
            //Attempt 10	512	    8m 32s
            //Attempt 11	1024	17m 4s
            //Attempt 12	2048	34m 8s

            //Attempt 13	4096	1h 8m 16s
            //Attempt 14	8192	2h 16m 32s
            //Attempt 15	16384	4h 33m 4s

            var delayInSeconds = ((1d / 2d) * (Math.Pow(2d, failedAttempts) - 1d));

            return maxDelayInSeconds < delayInSeconds
                       ? Convert.ToInt64(maxDelayInSeconds)
                       : Convert.ToInt64(delayInSeconds);
        }
    }
}