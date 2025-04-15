using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GeoStream.Extensions
{
    public static class CustomExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var displayName = enumValue.GetType()
                                       .GetMember(enumValue.ToString())
                                       .FirstOrDefault()?
                                       .GetCustomAttribute<DisplayAttribute>()?
                                       .Name;

            return displayName ?? "Sin definir";
        }

        /// <summary>
        /// Converts a given date to its equivalent in the specified time zone.
        ///
        /// Use case 1: When sending dates to ScannersAPI that are created using DateTime.Now (where DateTime.Now.Kind = Local, i.e., the local system time).
        /// ScannersAPI expects dates sent from this client app to be in the local time zone.
        /// 
        /// Use case 2: Dates returned from ScannersApi are formatted in UTC. For display purposes, 
        /// this method converts those UTC dates into the desired time zone for proper representation 
        /// in the UI (e.g., America/New_York).
        /// </summary>
        public static DateTime InTimeZone(this DateTime dateTime, string timeZoneId = "America/New_York")
        {
            DateTime utcDateTime = dateTime.ToUniversalTime();
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
            return localDateTime;
        }
    }
}
