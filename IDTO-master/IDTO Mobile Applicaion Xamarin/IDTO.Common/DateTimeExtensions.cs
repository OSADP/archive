using System;

namespace IDTO.Common
{
	public static class DateTimeExtensions
	{
		public static DateTime ToDateTimeUTC(this long milliseconds)
		{
			DateTime dtEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime dateValue = dtEpoch.AddMilliseconds (milliseconds);

			return dateValue;
		}

		public static string GetTodayTomorrowString(this DateTime dtSent)
		{
			string todayTomorrowString = "Today";
			if (!DateTime.Now.Date.Equals (dtSent.Date))
				todayTomorrowString = dtSent.ToString ("M/d");
				
			return todayTomorrowString;

		}

		public static string GetTimeString(this DateTime dtSent)
		{

			string timeString = dtSent.ToString ("h:mm");
			return timeString;

		}

		public static string GetTimeAmPm(this DateTime dtSent)
		{
			string amPmString = dtSent.ToString ("tt");
			return amPmString;

		}
	}
}