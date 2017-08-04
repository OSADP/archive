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
using System.Collections.Generic;
using System.Linq;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;

namespace PAI.FRATIS.SFL.Services.Geography
{
    public interface ILocationHoursService : IEntityServiceBase<LocationHours>
    {
        ICollection<DayOfWeek> GetDaysOfWeek(string startDay, string endDay);

        ICollection<LocationHoursDayOfWeek> GetLocationHoursDaysOfWeek(ICollection<DayOfWeek> daysOfWeek);
    }

    /// <summary>The location group service.</summary>
    public class LocationHoursService : EntityServiceBase<LocationHours>, ILocationHoursService
    {
        public LocationHoursService(IRepository<LocationHours> repository, ICacheManager cacheManager)
            : base(repository, cacheManager)
        {
        }

        public ICollection<DayOfWeek> GetDaysOfWeek(string startDay, string endDay)
        {
            var result = new List<DayOfWeek>();

            var allDaysOfWeek = new List<string> { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            var startIndex = allDaysOfWeek.IndexOf(startDay);
            var endIndex = allDaysOfWeek.IndexOf(endDay);

            if (startIndex >= 0 && endIndex >= 0)
            {
                for (var i = startIndex; i <= endIndex; i++)
                {
                    result.Add((DayOfWeek)Enum.Parse(typeof(DayOfWeek), allDaysOfWeek[i], true));
                }
            }
            return result;
        }

        public ICollection<LocationHoursDayOfWeek> GetLocationHoursDaysOfWeek(ICollection<DayOfWeek> daysOfWeek)
        {
            var result = new List<LocationHoursDayOfWeek>();
            if (daysOfWeek != null)
            {
                result.AddRange(daysOfWeek.Select(day => new LocationHoursDayOfWeek() { DayOfWeek = day }));
            }
            return result;
        }
    }
}