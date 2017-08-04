using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDTO.WebAPI
{
    public static class ExtensionMethods
    {
        public static DateTime ToDateTimeUTC(this long milliseconds)
        {
            DateTime dtEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime dateValue = dtEpoch.AddMilliseconds(milliseconds);

            return dateValue;
        }

        public static string GetTodayTomorrowString(this DateTime dtSent)
        {
            string todayTomorrowString = "Today";
            if (!DateTime.Now.Date.Equals(dtSent.Date))
                todayTomorrowString = dtSent.ToString("M/d");

            return todayTomorrowString;

        }

        public static string GetTimeString(this DateTime dtSent)
        {

            string timeString = dtSent.ToString("h:mm");
            return timeString;

        }

        public static string GetTimeAmPm(this DateTime dtSent)
        {
            string amPmString = dtSent.ToString("tt");
            return amPmString;

        }

        public static int GetWalkTime_min(int walkTime)
        {
            float time_min = (float)(walkTime) / 60.0f;
            int itime_min = (int)Math.Round(time_min);

            return itime_min;
        }

        public static int GetDuration_min(int duration)
        {
            float dur_min = (float)(duration) / 60000.0f;
            int iduration_min = (int)Math.Round(dur_min);

            return iduration_min;
        }
    }
}