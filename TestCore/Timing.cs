﻿namespace CodexDistTests.TestCore
{
    public static class Timing
    {
        public static TimeSpan HttpCallTimeout()
        {
            return TimeSpan.FromMinutes(10);
        }

        public static void RetryDelay()
        {
            Utils.Sleep(TimeSpan.FromSeconds(3));
        }
    }
}
