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
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Services.Core;
using System.Collections.Generic;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.SFL.Services.Geography;

namespace PAI.FRATIS.SFL.Services.Orders
{
    public interface IRouteStopService : IEntityServiceBase<RouteStop>
    {
        IEnumerable<RouteStop> GetByJobId(int jobId, int? subscriberId);

        void DeleteByJobId(int jobId);

        IEnumerable<int> GetLocationIds(int subscriberId, DateTime startDateRange, DateTime endDateRange);

        IEnumerable<int> GetLocationIds(int subscriberId, DateTime dueDate, int daysToInclude = 0,
            int? jobGroupId = null);

        IEnumerable<RouteStop> GetRouteStopsForJobs(IEnumerable<int?> jobIds);

        IEnumerable<RouteStop> GetRoutesWithLocation(
            IEnumerable<int?> locationIds, DateTime? dtSince = null, DateTime? dtUntil = null);

        IEnumerable<Job> GetJobsWithLocation(IEnumerable<int?> locationIds, DateTime? dtSince = null,
            DateTime? dtUntil = null);

        IEnumerable<Job> GetJobsWithLocationBySyncDate(
            IEnumerable<int?> locationIds, DateTime? dtSince, DateTime? dtUntil);

        IQueryable<RouteStop> SelectWithJobs();

        IEnumerable<int> GetJobIdsWithStopDate(IList<DateTime> jobDueDates, int jobGroupId, float stopWindowBeforeTicks);
    }

    public class RouteStopService : EntityServiceBase<RouteStop>, IRouteStopService
    {
        private readonly ILocationService _locationService;

        public RouteStopService(IRepository<RouteStop> repository, ICacheManager cacheManager, ILocationService locationService)
            : base(repository, cacheManager)
        {
            _locationService = locationService;
        }

        protected override IQueryable<RouteStop> InternalSelect()
        {
            return _repository.SelectWith("Location", "StopAction");
        }

        public IQueryable<RouteStop> SelectWithJobs()
        {
            return _repository.SelectWith("Location", "StopAction", "Job", "Job.RouteStops");
        }

        public IEnumerable<RouteStop> GetByJobId(int jobId, int? subscriberId)
        {
            var query = InternalSelect().Where(m => m.JobId == jobId);
            if (subscriberId.HasValue)
            {
                query = query.Where(p => p.SubscriberId == subscriberId.Value);
            }

            query = query.OrderBy(m => m.SortOrder).ThenBy(m => m.Id);
            return query.ToList();
        }

        public void DeleteByJobId(int jobId)
        {
            foreach (var rs in Select().Where(p => p.JobId == jobId).ToList())
            {
                Delete(rs, false);
            }

            SaveChanges();
        }

        public IEnumerable<int> GetLocationIds(int subscriberId, DateTime startDateRange, DateTime endDateRange)
        {
            var query = this.InternalSelect().Where(p => p.SubscriberId == subscriberId);
            return (IEnumerable<int>)query.Where(p => p.Job.DueDate >= startDateRange && p.Job.DueDate <= endDateRange && p.LocationId > 0).Select(p => p.LocationId);
        }

        public IEnumerable<int> GetLocationIds(int subscriberId, DateTime dueDate, int daysToInclude = 0, int? jobGroupId = null)
        {
            if (dueDate.Year == 1970 || dueDate.Year == 0001) return new List<int>();

            var query = this.InternalSelect().Where(p => p.SubscriberId == subscriberId);

            var upperDate = dueDate.AddDays(daysToInclude);
            var lowerDate = dueDate.AddDays(-daysToInclude);

            if (jobGroupId.HasValue)
            {
                return daysToInclude > 0 ? query.Where(p => p.Job.DueDate <= upperDate 
                    && p.Job.DueDate >= lowerDate 
                    && p.Job.JobGroupId == jobGroupId.Value
                    && p.LocationId > 0).Select(p => p.Location.Id)
                    : (IEnumerable<int>)query.Where(p => p.Job.DueDate == dueDate
                        && p.Job.JobGroupId == jobGroupId.Value && p.LocationId > 0).Select(p => p.LocationId);
            }

            // TODO urgent DueDate wont equal dueDate - make it a range
            return daysToInclude > 0 ? query.Where(
                p => p.Job.DueDate >= lowerDate && 
                    p.Job.DueDate <= upperDate && 
                    p.LocationId > 0).Select(p => p.Location.Id)
                    : query.Where(p => p.Job.DueDate == dueDate    // TODO urgent DueDate wont equal dueDate - make it a range
                    && p.LocationId > 0).Select(p => p.Location.Id);
        }

        public IEnumerable<RouteStop> GetRouteStopsForJobs(IEnumerable<int?> jobIds)
        {
            return InternalSelect().Where(p => jobIds.Contains(p.JobId)).ToList();
        }

        public IEnumerable<RouteStop> GetRoutesWithLocation(IEnumerable<int?> locationIds, DateTime? dtSince = null, DateTime? dtUntil = null)
        {
            if (dtSince.HasValue == false)
                return InternalSelect().Where(p => locationIds.Contains(p.LocationId)).ToList();

            var query =
                InternalSelect().Where(
                    p =>
                    locationIds.Contains(p.LocationId) && (p.CreatedDate >= dtSince.Value)
                    || (p.ModifiedDate >= dtSince.Value));

            if (dtUntil.HasValue)
            {
                query = query.Where(p => (p.CreatedDate <= dtUntil.Value)
                    || (p.ModifiedDate <= dtUntil.Value));
            }

            return query.ToList();
            
        }

        public IEnumerable<Job> GetPendingTerminalOrders(IEnumerable<int?> terminalLocationIds)
        {
            var jobs = GetJobsWithLocation(terminalLocationIds);
            return jobs.Where(j => j.GetTerminalJobType(terminalLocationIds) != TerminalJobType.Unspecified).ToList();
        }

        public IEnumerable<Job> GetJobsWithLocation(IEnumerable<int?> locationIds, DateTime? dtSince = null, DateTime? dtUntil = null)
        {
            if (dtSince.HasValue == false)
                return InternalSelect().Where(p => locationIds.Contains(p.LocationId)).Where(p => p.Job != null).Select(p => p.Job).ToList();

            var query =
                InternalSelect().Where(
                    p =>
                    locationIds.Contains(p.LocationId) && ((p.CreatedDate >= dtSince.Value)
                    || (p.ModifiedDate >= dtSince.Value) || (p.Job.CreatedDate <= dtSince.Value || p.Job.ModifiedDate >= dtSince.Value )));

            if (dtUntil.HasValue)
            {
                query = query.Where(p => (p.CreatedDate <= dtUntil.Value)
                    || (p.ModifiedDate <= dtUntil.Value) || p.Job.CreatedDate <= dtUntil.Value || p.Job.ModifiedDate <= dtUntil.Value);
            }

            return query.Where(p => p.Job != null).Select(p=>p.Job).ToList();
        }

        public IEnumerable<Job> GetJobsWithLocationBySyncDate(IEnumerable<int?> locationIds, DateTime? dtSince, DateTime? dtUntil)
        {
            return GetJobsWithLocation(locationIds, dtSince, dtUntil);
        }

        public IEnumerable<int> GetJobIdsWithStopDate(IList<DateTime> jobDueDates, int jobGroupId, float stopWindowBeforeTicks)
        {
            var result = new HashSet<int>();
            for (int i = 0; i < jobDueDates.Count; i++)
            {
                var jobDueDate = jobDueDates[i];
                var query = SelectWithJobs().Where(p => p.Job.DueDate.Value == jobDueDate && p.Job.IsDeleted == false
                                                                  && p.Job.JobStatus == Domain.Orders.JobStatus.Unassigned
                                                                  && p.Job.IsValidNullable.HasValue && p.Job.IsValidNullable.Value);


                if (i + 1 == jobDueDates.Count)
                {
                    // more than one date provided and this is the last date
                    // search up to Window, do not use job group
                    query = query.Where(p => p.WindowEnd < stopWindowBeforeTicks);
                }
                else
                {
                    // use job group
                    query = query.Where(p => p.Job.JobGroupId == jobGroupId);
                }



                var ids = query.Where(p => p.Job != null).Select(p => p.Job).Select(p => p.Id).ToList();
                foreach (var id in ids)
                {
                    result.Add(id);
                }
            }

            return result.ToList();
        }
    }
}
