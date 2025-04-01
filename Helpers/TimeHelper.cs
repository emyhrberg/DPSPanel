using System;

namespace DPSPanel.Helpers
{
    public static class TimeHelper
    {
        // TimeSpanFactory is a factory method that creates a TimeSpan object from a given number of ticks.
        public static TimeSpan ToSeconds(float ticks)
        {
            // 1 tick = 1/60 seconds
            return TimeSpan.FromSeconds(ticks / 60f);
        }
    }
}