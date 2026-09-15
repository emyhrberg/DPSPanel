using System;

namespace DPSPanel.Core.Utilities
{
    public static class TimeHelper
    {
        // TimeSpanFactory is a factory method that creates a TimeSpan object from a given number of ticks.
        public static TimeSpan FromSeconds(double seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }
    }
}