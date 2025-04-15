using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoStream.Api.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a given date to its equivalent in the specified time zone.
        /// This method is particularly useful for comparing dates stored in the database, which, in this project, are assumed to be in the Eastern Time Zone ("Eastern Standard Time"),
        /// with the current date and time. 
        /// The current date and time, obtained via DateTime.Now, are local by default (DateTime.Now.Kind = Local). Applying this method to DateTime.Now ensures consistency
        /// with the database's time zone, facilitating accurate comparisons.
        /// </summary>
        public static DateTime InTimeZone(this DateTime dateTime, string timeZoneId = "Eastern Standard Time")
        {
            DateTime utcDateTime = dateTime.ToUniversalTime();
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
            return localDateTime;
        }

        /// <summary>
        /// Applies a specified offset to the given date. For example, with the default configuration, 6 PM will be adjusted to 6 PM - 5 hours.
        /// Note: This adjustment, unlike <see cref="InTimeZone(DateTime, string)">, modifies the time by the offset but does not convert the date to a different time zone.
        /// This method is particularly useful for specifying how runtime should interpret dates retrieved from databases or forms,
        /// assuming the dates are in a specified time zone. 
        /// By default, .NET treats database dates as Local and form dates as UTC. Using this method ensures that .NET handles the date as intended.
        /// </summary>
        public static DateTimeOffset AddOffset(this DateTime dateTime, string timeZoneId = "Eastern Standard Time")
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            TimeSpan localOffset = timeZone.GetUtcOffset(dateTime);

            if (dateTime.Kind == DateTimeKind.Utc)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            }

            DateTimeOffset localDateTimeOffset = new DateTimeOffset(dateTime, localOffset);
            return localDateTimeOffset;
        }
    }
}
