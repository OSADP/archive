//    Copyright 2014 Productivity Apex Inc.
//        http://www.productivityapex.com/
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Linq;

namespace PAI.FRATIS.SFL.Services
{
    public interface IDateTimeHelper
    {
        DateTime GetLocalDateTimeNow();

        TimeZoneInfo GetTimeZoneInfo(string timeZoneId);

        TimeZoneInfo GetLocalTimeZoneInfo();

        DateTime ConvertToTime(DateTime dt, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone);

        DateTime ConvertUtcToLocalTime(DateTime dt);

        DateTime ConvertLocalToUtcTime(DateTime dt);

        DateTime ConvertUnixTimeToDateTime(double unixTime);

        double ConvertDateTimeToUnixTime(DateTime dt);
    }

    /// <summary>The date time helper.</summary>
    public class DateTimeHelper : IDateTimeHelper
    {
        public DateTime GetLocalDateTimeNow()
        {
            return ConvertUtcToLocalTime(DateTime.UtcNow);
        }

        public TimeZoneInfo GetTimeZoneInfo(string timeZoneId)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }

        public TimeZoneInfo GetLocalTimeZoneInfo()
        {
            return CurrentTimeZone;
        }

        private TimeZoneInfo _timeZone = null;
        public TimeZoneInfo CurrentTimeZone
        {
            get
            {
                return _timeZone ?? (_timeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            }
        }

        public DateTime ConvertToTime(DateTime dt, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone)
        {
            try
            {
                // TODO - below may not be necessary
                if (dt.Kind == DateTimeKind.Local)
                    sourceTimeZone = CurrentTimeZone;
                else if (dt.Kind == DateTimeKind.Utc)
                    sourceTimeZone = TimeZoneInfo.Utc;
                //else if (dt.Kind == DateTimeKind.Unspecified)
                  //  dt = new DateTime(dt.Ticks, DateTimeKind.Utc);

                // convert sourceTimeZone and dt to UTC in order to complete the conversion
                dt = TimeZoneInfo.ConvertTime(dt, sourceTimeZone, TimeZoneInfo.Utc);
                sourceTimeZone = TimeZoneInfo.Utc;

                // now convert to desired destination time zone
                if (dt.Kind == DateTimeKind.Utc)
                {
                    dt = TimeZoneInfo.ConvertTimeFromUtc(dt, destinationTimeZone);
                }
                else
                {
                    dt = TimeZoneInfo.ConvertTimeToUtc(dt, sourceTimeZone);
                }
                //dt = TimeZoneInfo.ConvertTime(dt, sourceTimeZone, destinationTimeZone);

                return dt;

            }
            catch (ArgumentException ex)
            {
                throw new Exception("ConvertToTime Argument Exception");
            }
        }

        public DateTime ConvertUtcToLocalTime(DateTime dt)
        {
            return dt.Kind == DateTimeKind.Local ? dt : ConvertToTime(dt, TimeZoneInfo.Utc, CurrentTimeZone);
        }

        public DateTime ConvertLocalToUtcTime(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc) return dt;
            return ConvertToTime(dt, CurrentTimeZone, TimeZoneInfo.Utc);
        }

        public DateTime ConvertUnixTimeToDateTime(double unixTime)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dt = dt.AddSeconds(unixTime);
            return dt;
        }

        public double ConvertDateTimeToUnixTime(DateTime dt)
        {
            return (dt - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}