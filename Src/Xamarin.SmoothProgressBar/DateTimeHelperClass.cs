using System;

namespace Xamarin
{
    public class DateTimeHelperClass
    {
        public static long CurrentUnixTimeMillis()
        {
            var unixTime = DateTime.Now.ToUniversalTime() -
                           new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (long)unixTime.TotalMilliseconds;
        }
    }
}