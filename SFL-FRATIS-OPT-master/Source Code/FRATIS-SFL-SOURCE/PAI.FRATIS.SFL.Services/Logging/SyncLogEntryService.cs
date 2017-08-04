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
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain.Logging;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;

namespace PAI.FRATIS.SFL.Services.Logging
{
    public interface ISyncLogEntryService : IEntitySubscriberServiceBase<SyncLogEntry>
    {
        void AddEntry(int subscriberId, string title, string message, int jobErrorCount, int updatedJobCount,
            int createdJobCount, int existingJobCount, int recreatedRouteStopCount, int locationErrorCount);

        DateTime GetLastSyncTime(int subscriberId);
    }

    public class SyncLogEntryService : EntitySubscriberServiceBase<SyncLogEntry>, ISyncLogEntryService
    {
        public SyncLogEntryService(IRepository<SyncLogEntry> repository, ICacheManager cacheManager) : base(repository, cacheManager)
        {
        }

        public void AddEntry(int subscriberId, string title, string message, int jobErrorCount, int updatedJobCount,
            int createdJobCount, int existingJobCount, int recreatedRouteStopCount, int locationErrorCount)
        {
            var entry = new SyncLogEntry()
            {
                SubscriberId = subscriberId,
                Title = title,
                Message = message,
                TimeStamp = DateTime.UtcNow,
                JobErrorCount = jobErrorCount,
                UpdatedJobCount = updatedJobCount,
                CreatedJobCount = createdJobCount,
                LocationErrorCount = locationErrorCount,
                RouteStopsRecreatedCount = recreatedRouteStopCount,
                ExistingJobCount = existingJobCount
            };

            this.Insert(entry);
        }

        public DateTime GetLastSyncTime(int subscriberId)
        {
            var result = this.Select()
                .Where(p => p.SubscriberId == subscriberId)
                .OrderByDescending(p => p.TimeStamp)
                .Take(1)
                .Select(p => p.TimeStamp).ToList().FirstOrDefault();

            return result.HasValue ? result.Value : DateTime.MinValue;
        }
    }
}