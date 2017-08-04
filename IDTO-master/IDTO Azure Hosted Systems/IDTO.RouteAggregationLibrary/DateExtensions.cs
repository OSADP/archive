using System;

namespace IDTO.RouteAggregationLibrary
{
    /// <summary>
    /// Extension methods to support additional checks to the DateTime type.
    /// </summary>
    public static class DateExtensions
    {
        public static DateTime FirstDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime date)
        {
            DateTime nextMonth = date.AddMonths(1);
            return new DateTime(nextMonth.Year, nextMonth.Month, 1, 23, 59, 59, 999).AddDays(-1);
        }

        public static bool IsLastDayOfMonth(this DateTime date)
        {
            DateTime testDate = date.AddDays(1);
            return testDate.Month == date.Month + 1;            
        }

        public static long ConvertToMillisecondsSinceJan1970(this DateTime dateToConvert)
        {
            DateTime jan1970 = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan ts = dateToConvert.Subtract(jan1970);
            return (long)Math.Ceiling(ts.TotalMilliseconds);
        }

        //private long GetMillisecondsSinceJan1970(DateTime dateToConvert)
        //{
        //    DateTime jan1970 = new DateTime(1970, 1, 1, 0, 0, 0);
        //    TimeSpan ts = dateToConvert.Subtract(jan1970);
        //    return (long)Math.Ceiling(ts.TotalMilliseconds);
        //}

    }
}
