using System;

namespace ThatDeveloperDad.iFX.TypeConversionExtensions;

public static class TimeSpanConverters
{
    public static string ToHumanReadable(this TimeSpan timespan)
    {
        string minutes = timespan.Minutes > 0 
            ? $"{timespan.Minutes} min" 
            : "";
        string seconds = timespan.Seconds > 0 
            ? $" {timespan.Seconds} sec" 
            : "";
        string millisec = timespan.Milliseconds > 0 
            ? $" {timespan.Milliseconds}" 
            : "";
        string microsec = timespan.Microseconds > 0 
            ? $".{timespan.Microseconds}"
            : "";
        string nanosec = timespan.Nanoseconds > 0
            ? $".{timespan.Nanoseconds}"
            : "";

        string formatted = $"{minutes}{seconds}{millisec}{microsec}{nanosec}ms";
        return formatted;
    }

    
}
