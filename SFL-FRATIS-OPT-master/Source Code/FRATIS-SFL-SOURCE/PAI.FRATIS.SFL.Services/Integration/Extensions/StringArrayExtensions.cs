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

namespace PAI.FRATIS.SFL.Services.Integration.Extensions
{
    /// <summary>
    /// Static Extension Methods for Array of Strings for Parsing Values
    /// </summary>
    public static class StringArrayExtensions
    {
        public static string GetString(this string[] array, int index)
        {
            try
            {
                return ((string)array.GetValue(index)).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool GetBool(this string[] array, int index)
        {
            try
            {
                var result = false;
                var x = (string) array.GetValue(index);
                switch (x.ToLower())
                {
                    case "y":
                    case "yes":
                    case "t":
                    case "true":
                        result = true;
                        break;
                    case "n":
                    case "no":
                    case "f":
                    case "false":
                        result = false;
                        break;
                    default:
                        bool.TryParse(x, out result);
                        break;
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        public static int GetInt(this string[] array, int index)
        {
            var result = 0;
            Int32.TryParse(array.GetString(index), out result);
            return result;
        }

        public static double GetDouble(this string[] array, int index)
        {
            var result = 0.0;
            double.TryParse(array.GetString(index), out result);
            return result;
        }

        public static DateTime GetDate(this string[] array, int index)
        {
            try
            {
                var result = DateTime.MinValue;
                var x = (string)array.GetValue(index);
                DateTime.TryParse(x, out result);
                return result;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime GetCustomDate(this string[] array, int index)
        {
            int month = 1, day = 1, year = 1;
            var dateString = array.GetString(index);
            if (dateString.Length == 7)
            {
                dateString = dateString.Substring(1);
                Int32.TryParse(dateString.Substring(0, 2), out year);
                Int32.TryParse(dateString.Substring(2, 2), out month);
                Int32.TryParse(dateString.Substring(4, 2), out day);
            }
            else
            {
                throw new Exception("Invalid custom date format: " + dateString);
            }

            try
            {
                return new DateTime(2000 + year, month, day);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not parse date time: {0}", dateString), ex);
            }
        }

        public static DateTime GetCustomTime(this string[] array, int index, DateTime appendToDateTime)
        {
            int hour = 0, minute = 0;

            var timeString = array.GetString(index);
            if (timeString.Length < 5)
            {
                if (timeString.Length < 3)
                {
                    Int32.TryParse(timeString, out minute);
                }
                else
                {
                    Int32.TryParse(timeString.Substring(0, timeString.Length - 2), out hour);
                    Int32.TryParse(timeString.Substring(timeString.Length - 2), out minute);
                }
            }

            try
            {
                return appendToDateTime.AddHours(hour).AddMinutes(minute);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not parse time string: {0}", timeString), ex);
            }
        }   

        public static DateTime GetCustomDateTime(this string[] array, int dateIndex, int timeIndex)
        {
            var date = GetCustomDate(array, dateIndex);
            return GetCustomTime(array, timeIndex, date);       
        }
    }
}