using System;

namespace Durango.Utils;

public interface IServerTimeProvider
{
    double GetOffset();
}

public static class Times
{
    private static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static double UnixTimeNow()
    {
        return (DateTimeOffset.UtcNow - UnixEpoch).TotalSeconds;
    }

    public static double ToUnixTime(this DateTimeOffset targetTime)
    {
        return (targetTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
    }

    public static double ToUnixTime(this DateTime targetTime)
    {
        return (new DateTimeOffset(targetTime.ToUniversalTime()) - UnixEpoch).TotalSeconds;
    }

    public static DateTimeOffset UnixTimeToServerTime(double unixTime)
    {
        return UnixEpoch.AddSeconds(unixTime);
    }

    public static DateTime UnixTimeToDateTimeUtc(double unixTime)
    {
        return UnixEpoch.AddSeconds(unixTime).UtcDateTime;
    }

    public static DateTime UnixTimeToDateTimeLocal(double unixTime)
    {
        return UnixEpoch.AddSeconds(unixTime).LocalDateTime;
    }
}
