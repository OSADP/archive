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
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Objects;
using System.Linq;
using System.Threading.Tasks;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.SFL.Services.Geography;

namespace PAI.FRATIS.SFL.Services.Orders
{
    public enum OrderViewFilter
    {
        Invalid,
        Pending,
        Active,
        Assigned,
        Completed,
        Cancelled,
        Deleted,
        All
    }

    /// <summary>The JobService interface.</summary>
    public interface IJobService : IEntitySubscriberServiceBase<Job>
    {
        /// <summary>The assign.</summary>
        /// <param name="jobId">The job id.</param>
        /// <param name="assignedDriverId">The assigned driver id.</param>
        /// <param name="assignedUserId">The assigned user id.</param>
        /// <param name="isJobPairAssignment">The is job pair assignment.</param>
        /// <param name="driverJobIdSequence">The driver job id sequence.</param>
        void Assign(
            int jobId,
            int? assignedDriverId,
            int assignedUserId,
            bool isJobPairAssignment = false,
            IList<int> driverJobIdSequence = null);

        void ReorderStops(Job job, bool saveNow = false);

        bool IsOrderNumberUnique(int subscriberId, string orderNumber, bool? isDeleted = null);

        IQueryable<Job> GetFilteredQueryable(OrderViewFilter filter, int subscriberId, DateTime? startDateUtc, DateTime? endDateUtc, bool selectWithAll = true);

        /// <summary>The get by job pair id.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="ICollection"/>.</returns>
        ICollection<Job> GetByJobPairId(int id);

        Job GetByOrderNumber(int subscriberId, string orderNumber);

        ICollection<Location> GetJobLocations(Job job);

        void DeletePermanently(Job entity, bool saveChanges);

        HashSet<int> GetAssignedDriverIds(int subscriberId, DateTime? dueDate, int jobGroupId);
            
        /// <summary>Gets a collection of jobs based on the specified ids.</summary>
        /// <param name="ids">The ids of the jobs to be fetched.</param>
        /// <returns>The <see cref="ICollection"/>.</returns>
        ICollection<Job> GetJobs(IEnumerable<int> ids);

        /// <summary>The update job pair details.</summary>
        /// <param name="jobPairId">The job pair id.</param>
        /// <param name="containerOwnerId">The container owner id.</param>
        /// <param name="containerId">The container id.</param>
        /// <param name="chassisId">The chassis id.</param>
        void UpdateJobPairDetails(int jobPairId, int? containerOwnerId, int? containerId, int? chassisId,
            string pickupNumber, string bookingNumber, string containerNumber, string billOfLading);

        /// <summary>The get orders.</summary>
        /// <param name="startDateRange">The start date range.</param>
        /// <param name="endDateRange">The end date range.</param>
        /// <param name="jobGroupId">The job group id.</param>
        /// <param name="showUnassignedOrders">The show unassigned orders.</param>
        /// <param name="showAssignedOrders">The show assigned orders.</param>
        /// <returns>The <see cref="IQueryable"/>.</returns>
        IQueryable<Job> GetOrders(
            int subscriberId,
            DateTime? startDateRange,
            DateTime? endDateRange,
            int? jobGroupId = 0,
            bool showUnassignedOrders = true,
            bool showAssignedOrders = true);

        /// <summary>The get job template type.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="JobTemplateType"/>.</returns>
        JobTemplateType GetJobTemplateType(string name);

        /// <summary>The get by id with all.</summary>
        /// <param name="entityId">The entity id.</param>
        /// <returns>The <see cref="Job"/>.</returns>
        Job GetByIdWithAll(int entityId);

        Job Clone(
            Job job,
            DateTime? dueDate,
            bool cloneRouteStops = true,
            string orderNumber = "",
            bool copyJobPairId = false,
            int? jobGroupIdOverride = null,
            JobTemplateType templateType = JobTemplateType.Unspecified,
            bool copyDriverAssignment = true,
            bool copyPickNumber = true,
            bool copyBookingNumber = true,
            bool copyOrderNumber = true);


        /// <summary>The select with all.</summary>
        /// <returns>The <see cref="IQueryable"/>.</returns>
        IQueryable<Job> SelectWithAll(int subscriberId);

        void UpdateJobStatus(Job job, JobStatus status, bool saveNow = true);

        JobStatus GetJobStatus(string statusString);

        IEnumerable<Job> GetDriverJobs(int driverId, DateTime? dueDate, int? jobGroupId);

        void SetShift(Job jobWithRoutes, IList<JobGroup> jobGroups, bool saveNow = true);
    }

    /// <summary>The job service.</summary>
    public class JobService : EntitySubscriberServiceBase<Job>, IJobService
    {
        /// <summary>The _location service.</summary>
        private readonly ILocationService _locationService;

        /// <summary>The _route stop service.</summary>
        private readonly IRouteStopService _routeStopService;

        /// <summary>The _stop action service.</summary>
        private readonly IStopActionService _stopActionService;

        private readonly IJobGroupService _jobGroupService;

        private readonly IDateTimeHelper _dateTimeHelper;

        private readonly ILocationDistanceService _locationDistanceService;

        /// <summary>Initializes a new instance of the <see cref="JobService"/> class.</summary>
        /// <param name="repository">The repository.</param>
        /// <param name="cacheManager">The cache manager.</param>
        /// <param name="routeStopService">The route stop service.</param>
        /// <param name="stopActionService">The stop action service.</param>
        public JobService(IRepository<Job> repository, ICacheManager cacheManager, IRouteStopService routeStopService, IStopActionService stopActionService, IJobGroupService jobGroupService, IDateTimeHelper dateTimeHelper, ILocationDistanceService locationDistanceService)
            : base(repository, cacheManager)
        {
            _routeStopService = routeStopService;
            _stopActionService = stopActionService;
            _jobGroupService = jobGroupService;
            _dateTimeHelper = dateTimeHelper;
            _locationDistanceService = locationDistanceService;
        }

        public void SetIsValid(Job job, bool value, bool saveNow = true)
        {
            if (job != null)
            {
                job.IsValid = value;
                InsertOrUpdate(job, saveNow);
            }
        }

        private int GetStopIndex(IList<RouteStop> stops)
        {
            var result = -1;
            if (stops.Count > 1)
            {
                result = stops.Count - 2;
            }
            else if (stops.Any())
            {
                result = 0;
            }
            return result;
        }

        public void SetShift(Job jobWithRoutes, IList<JobGroup> jobGroups, bool saveNow = true)
        {
            if (jobWithRoutes == null || jobGroups == null || !jobGroups.Any() || jobWithRoutes.RouteStops == null || !jobWithRoutes.RouteStops.Any()) return;

            var orderedStops = jobWithRoutes.RouteStops.OrderBy(p => p.SortOrder).ToList();
            var stop = orderedStops[GetStopIndex(orderedStops)];
            var ts = new TimeSpan(stop.WindowEnd);

            var compareMin = TimeSpan.FromHours(5);
            var compareMax = TimeSpan.FromHours(17);
            while (ts.TotalDays > 1.000)
            {
                ts = ts.Subtract(TimeSpan.FromDays(1));
            }

            JobGroup jg = null;
            jg = ts >= compareMax || ts <= compareMin 
                ? jobGroups[1] 
                : jobGroups[0];

            if (jobWithRoutes.JobGroupId == null || jobWithRoutes.JobGroupId != jg.Id)
            {
                jobWithRoutes.JobGroup = jg;
                jobWithRoutes.JobGroupId = jg.Id;
                Update(jobWithRoutes, saveNow);
            }
        }

        public JobStatus GetJobStatus(string statusString)
        {
            var values = Enum.GetValues(typeof(JobStatus)).Cast<JobStatus>();
            return values.FirstOrDefault(status => statusString == status.ToString());
        }

        public void Assign(int jobId, int? assignedDriverId, int assignedUserId, bool isJobPairAssignment = false, IList<int> driverJobIdSequence = null)
        {
            var job = GetById(jobId);

            if (job != null)
            {
                ICollection<Job> jobs = new[] { job };
                if (isJobPairAssignment && job.JobPairId.HasValue && job.JobPairId > 0)
                {
                    jobs = GetByJobPairId(job.JobPairId.Value);
                }

                foreach (var j in jobs)
                {
                    j.AssignedDriverId = assignedDriverId;
                    j.AssignedDateTime = DateTime.UtcNow;
                    j.JobStatus = JobStatus.Assigned;

                    if (driverJobIdSequence != null)
                    {
                        j.DriverSortOrder = driverJobIdSequence.IndexOf(job.Id) + 1;
                    }

                    Update(j, false);
                }

                var jobCount = 0;
                if (driverJobIdSequence != null)
                {
                    foreach (var j in GetJobs(driverJobIdSequence))
                    {
                        j.DriverSortOrder = jobCount++;
                        Update(j, false);
                    }                    
                }

                SaveChanges();
            }
        }

        public void ReorderStops(Job job, bool saveNow = false)
        {
            if (job != null && job.RouteStops != null)
            {
                int count = 0;
                foreach (var rs in job.RouteStops)
                {
                    rs.SortOrder = count;
                    count++;
                }
                InsertOrUpdate(job, saveNow);
            }
        }

        /// <summary>
        /// Determines if the provided OrderNumber exists within a Job in the information store
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <param name="isDeleted"></param>
        /// <returns></returns>
        public bool IsOrderNumberUnique(int subscriberId, string orderNumber, bool? isDeleted = null)
        {
            var query = this.Select().Where(p => p.SubscriberId == subscriberId);
            return isDeleted.HasValue
                ? query.FirstOrDefault(p => (p.OrderNumber == orderNumber || p.OrderNumber.StartsWith(orderNumber + "-")) && p.IsDeleted == isDeleted) == null
                       : query.FirstOrDefault(p => (p.OrderNumber == orderNumber || p.OrderNumber.StartsWith(orderNumber + "-"))) == null;
        }

        public IQueryable<Job> GetFilteredQueryable(OrderViewFilter filter, int subscriberId, DateTime? startDateUtc, DateTime? endDateUtc, bool selectWithAll = true)
        {
            var query = startDateUtc.HasValue && endDateUtc.HasValue
                ? GetBySubscriberId(subscriberId, selectWithAll).Where(p => p.DueDate.HasValue && p.DueDate >= startDateUtc.Value && p.DueDate <= endDateUtc.Value)
                : GetBySubscriberId(subscriberId, selectWithAll);

            switch (filter)
            {
                case OrderViewFilter.Assigned:
                    query = query.Where(p => !p.IsDeleted && p.JobStatus == JobStatus.Assigned);
                    break;
                case OrderViewFilter.Invalid:
                    query = query.Where(p => !p.IsDeleted 
                        && p.JobStatus != JobStatus.Cancelled &&
                        (!p.IsValidNullable.HasValue || !p.IsValidNullable.Value));
                    break;
                case OrderViewFilter.Pending:
                    query =
                        query.Where(
                            p =>
                            !p.IsDeleted &&
                            p.JobStatus == JobStatus.Unassigned
                            && p.IsValidNullable.HasValue && p.IsValidNullable.Value
                            && p.IsTerminalOrder
                            && p.IsTerminalReady.HasValue
                            && p.IsTerminalReady.Value);
                    break;
                case OrderViewFilter.Active:
                    var baseQuery = query;
                    query = query.Where(p => 
                        !p.IsDeleted && p.JobStatus == JobStatus.Unassigned && p.IsValidNullable.HasValue && p.IsValidNullable.Value)
                        .Except(baseQuery.Where(p => p.IsValidNullable.HasValue && p.IsValidNullable.Value 
                            && p.IsTerminalOrder && (!p.IsTerminalReady.HasValue || !p.IsTerminalReady.Value)));
                    break;
                case OrderViewFilter.Completed:
                    query = query.Where(p => !p.IsDeleted && p.JobStatus == JobStatus.Completed);
                    break;
                case OrderViewFilter.Cancelled:
                    query = query.Where(p => !p.IsDeleted && p.JobStatus == JobStatus.Cancelled);
                    break;
                case OrderViewFilter.Deleted:
                    query = query.Where(p => p.IsDeleted && p.IsValidNullable.HasValue);
                    break;
            }

            return query;
        }

        /// <summary>The get preparatory orders.</summary>
        /// <returns>The <see cref="ICollection"/>.</returns>
        public ICollection<Job> GetPreparatoryOrders(int subscriberId)
        {
            return SelectWithAll().Where(m => m.SubscriberId == subscriberId && m.IsDeleted == false && m.IsPreparatory).ToList();
        }

        public ICollection<Job> GetByJobPairId(int id)
        {
            return SelectWithAll().Where(m => m.JobPairId == id).ToList();
        }

        public Job GetByOrderNumber(int subscriberId, string orderNumber)
        {
            return SelectWithAll().FirstOrDefault(p => p.SubscriberId == subscriberId && p.OrderNumber == orderNumber);
        }

        public ICollection<Location> GetJobLocations(Job job)
        {
            var result = new List<Location>();

            if (job != null)
            {
                if (job.RouteStops == null && job.Id > 0)
                {
                    job = GetByIdWithAll(job.Id);
                }

                if (job.RouteStops != null)
                {
                    result = job.RouteStops.Select(p => p.Location).ToList();
                }
            }
            return result;
        }

        public void DeletePermanently(Job entity, bool saveChanges)
        {
            if (entity != null)
            {
                _repository.Delete(entity, saveChanges);
            }
        }

        public HashSet<int> GetAssignedDriverIds(int subscriberId, DateTime? dueDate, int jobGroupId)
        {
            return
                new HashSet<int>(
                    GetOrders(subscriberId, dueDate, dueDate, jobGroupId).Where(p => p.AssignedDriverId != null).Select(p => p.Id));
        }

        public ICollection<Job> GetJobs(IEnumerable<int> ids)
        {
            var query = Select().Where(p => ids.Contains(p.Id)).ToList();
            return ids.Select(id => query.FirstOrDefault(p => p.Id == id)).Where(item => item != null).ToList();
        }

        public void UpdateJobPairDetails(int jobPairId, int? containerOwnerId, int? containerId, int? chassisId, 
            string pickupNumber, string bookingNumber, string containerNumber, string billOfLading)
        {
            foreach (var job in GetByJobPairId(jobPairId))
            {
                job.ContainerOwnerId = containerOwnerId;
                job.ContainerId = containerId;
                job.ChassisId = chassisId;
                job.BookingNumber = bookingNumber;
                job.PickupNumber = pickupNumber;
                job.ContainerNumber = containerNumber;
                job.BillOfLading = billOfLading;
                Update(job, false);
            }

            SaveChanges();
        }

        /// <summary>The get orders.</summary>
        /// <param name="startDateRange">The start date range.</param>
        /// <param name="endDateRange">The end date range.</param>
        /// <param name="jobGroupId">The job group id.</param>
        /// <param name="unassignedOrdersOnly">The unassigned orders only.</param>
        /// <returns>The <see cref="IQueryable"/>.</returns>
        public IQueryable<Job> GetOrders(int subscriberId, DateTime? startDateRange, DateTime? endDateRange, int? jobGroupId = 0, bool showUnassignedOrders = true, bool showAssignedOrders = true)
        {
            var query = SelectWithAll().Where(p => p.SubscriberId == subscriberId);
            
            if (!(showAssignedOrders && showUnassignedOrders))
            {
                if (showUnassignedOrders)
                {
                    query = query.Where(p => p.AssignedDriverId == null);
                }
                else if (showAssignedOrders)
                {
                    query = query.Where(p => p.AssignedDriverId != null);
                }
            }

            if (startDateRange.HasValue)
            {
                query = query.Where(p => p.DueDate >= startDateRange);
            }

            if (endDateRange.HasValue)
            {
                query = query.Where(p => p.DueDate <= endDateRange);
            }

            if (jobGroupId.HasValue && jobGroupId > 0)
            {
                query = query.Where(p => p.JobGroupId == jobGroupId);
            }

            var items = query.ToList();
            return query;
        }


        private DateTime? DateTimeAddDays(DateTime? dt, int days)
        {
            return dt.HasValue ? dt.Value.AddDays(days) : dt;
        }


        /// <summary>The create template job from original.</summary>
        /// <param name="originalJob">The original job.</param>
        /// <param name="templateType">The template type.</param>
        /// <param name="leg">The leg.</param>
        /// <param name="yardPull">The yard pull.</param>
        /// <param name="emptyMove">The empty move.</param>
        /// <param name="terminalLocation">The terminal location.</param>
        /// <param name="customerLocation">The customer location.</param>
        /// <param name="yardLocation">The yard location.</param>
        private void CreateTemplateJobFromOriginal(Job originalJob, JobTemplateType templateType, int leg, bool yardPull, bool emptyMove, Location terminalLocation, Location customerLocation, Location yardLocation)
        {
            var result = Clone(originalJob, originalJob.DueDate);
            result.RouteStops.Clear();
        }

        /// <summary>The get job locations.</summary>
        /// <param name="job">The job.</param>
        /// <param name="terminalLocationId">The terminal location id.</param>
        /// <param name="customerLocationId">The customer location id.</param>
        /// <param name="yardLocationId">The yard location id.</param>
        private void GetJobLocations(Job job, out int? terminalLocationId, out int? customerLocationId, out int? yardLocationId)
        {
            terminalLocationId = null;
            customerLocationId = null;
            yardLocationId = null;

            if (job != null && job.JobTemplateType > 0 && job.RouteStops != null && job.RouteStops.Count > 0)
            {
                switch (job.JobTemplateType)
                {
                    case JobTemplateType.Import:
                        if (job.RouteStops.Count >= 2)
                        {
                            terminalLocationId = job.RouteStops.First().LocationId;
                            customerLocationId = job.RouteStops.Last().LocationId;
                        }

                        break;
                    case JobTemplateType.ImportLiveUnload:
                        if (job.RouteStops.Count > 2)
                        {
                            terminalLocationId = job.RouteStops.First().LocationId;
                            customerLocationId = job.RouteStops[1].LocationId;
                        }

                        break;
                    case JobTemplateType.Export:
                        if (job.RouteStops.Count >= 2)
                        {
                            customerLocationId = job.RouteStops.First().LocationId;
                            terminalLocationId = job.RouteStops.Last().LocationId;
                        }

                        break;
                    case JobTemplateType.ExportLiveLoad:
                        if (job.RouteStops.Count > 2)
                        {
                            terminalLocationId = job.RouteStops.First().LocationId;
                            customerLocationId = job.RouteStops[1].LocationId;
                        }

                        break;
                }
            }
        }

        /// <summary>The get value of.</summary>
        /// <param name="enumName">The enum name.</param>
        /// <param name="enumConst">The enum const.</param>
        /// <returns>The <see cref="int"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static int GetValueOf(string enumName, string enumConst)
        {
            Type enumType = Type.GetType(enumName);
            if (enumType == null)
            {
                throw new ArgumentException("Specified enum type could not be found", "enumName");
            }

            object value = Enum.Parse(enumType, enumConst);
            return Convert.ToInt32(value);
        }

        /// <summary>The get job template type.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="JobTemplateType"/>.</returns>
        public JobTemplateType GetJobTemplateType(string name)
        {
            try
            {
                var value = Enum.Parse(typeof(JobTemplateType), name.Replace(" ", string.Empty), true);
                return (JobTemplateType)value;
            }
            catch (Exception ex)
            {
                return JobTemplateType.Unspecified;
            }
        }

        /// <summary>The get by id with all.</summary>
        /// <param name="entityId">The entity id.</param>
        /// <returns>The <see cref="Job"/>.</returns>
        public Job GetByIdWithAll(int entityId)
        {
            var result = SelectWithAll().FirstOrDefault(entity => entity.Id == entityId);
            if (result != null && result.RouteStops != null)
            {            
                result.RouteStops = result.RouteStops.OrderBy(p => p.SortOrder).ToList();
            }
            return result;
        }

        /// <summary>
        /// Deletes all associations to this job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public bool ClearJob(int jobId)
        {
            var ret = true;

            return ret;
        }

        private void AddRouteStop(Job job, string stopAction, int? locationId, int stopDelay, long windowStart, long windowEnd)
        {
            var sa = _stopActionService.GetByShortName(stopAction);
            if (sa != null && sa.Id > 0)
            {
                AddRouteStop(job, sa.Id, locationId, stopDelay, windowStart, windowEnd);
            }
        }
        
        public void AdjustJobForShift(Job job, string shiftName, bool saveNow = true)
        {
            if (job == null) return;
            foreach (var rs in job.RouteStops)
            {
                AdjustRouteStopForShift(rs, shiftName, false);
            }

            if (saveNow)
            {
                InsertOrUpdate(job);
            }
        }

        /// <summary>
        /// Adjusts the routestop default time windows for the provided shift name
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="shiftName"></param>
        /// <param name="saveNow"></param>
        public void AdjustRouteStopForShift(RouteStop rs, string shiftName, bool saveNow = true)
        {
            if (rs == null) return;
            switch (shiftName.ToLower())
            {
                case "day":
                    rs.WindowStart = new TimeSpan(3, 0, 0).Ticks;
                    rs.WindowEnd = new TimeSpan(18, 0, 0).Ticks;
                    break;
                case "night":
                    rs.WindowStart = new TimeSpan(18, 0, 0).Ticks;
                    rs.WindowEnd = new TimeSpan(1, 3, 0, 0).Ticks;
                    break;
                default:
                    rs.WindowStart = TimeSpan.Zero.Ticks;
                    rs.WindowEnd = new TimeSpan(23, 59, 0).Ticks;
                    break;

            }
        }

        private void AddRouteStop(Job job, int stopActionId, int? locationId, int stopDelay, long windowStart, long windowEnd, Job previousJob = null)
        {
            var jobGroup = string.Empty;
            if (job != null && job.JobGroup != null) jobGroup = job.JobGroup.Name;

            if (job != null)
            {
                DateTime? jobDueDate = null;
                if (job.DueDate.HasValue)
                {
                    jobDueDate = job.DueDate;
                }
                else if (previousJob != null)
                {
                    jobDueDate = previousJob.DueDate;
                }

                if (job.RouteStops == null)
                {
                    job.RouteStops = new Collection<RouteStop>();
                }

                if (windowStart == 0)
                {
                    switch (jobGroup)
                    {
                        case "Day":
                            windowStart = new TimeSpan(3, 0, 0).Ticks;
                            break;
                        case "Night":
                            windowStart = new TimeSpan(18, 0, 0).Ticks;
                            break;
                    }
                }

                if (windowEnd == 0)
                {
                    switch (jobGroup)
                    {
                        case "Day":
                            windowEnd = new TimeSpan(18, 0, 0).Ticks;
                            break;
                        case "Night":
                            windowEnd = new TimeSpan(1, 3, 0, 0).Ticks;
                            break;
                        default:
                            windowEnd = new TimeSpan(23, 59, 0).Ticks;
                            break;
                    }
                }
                var routeStop = new RouteStop()
                                    {
                                        JobId = job.Id,
                                        StopActionId = stopActionId,
                                        LocationId = locationId,
                                        WindowStart = windowStart,
                                        WindowEnd = windowEnd,
                                        StopDelay = stopDelay,
                                        SortOrder = job.RouteStops.Count
                                    };
                job.RouteStops.Add(routeStop);
            }
        }

        /// <summary>
        /// Updates the Job Status for a given order
        /// If the Job IsPreparatory, then a Transmitted or Pending status is not allowed
        /// </summary>
        /// <param name="job"></param>
        /// <param name="status"></param>
        /// <param name="saveNow"></param>
        public void UpdateJobStatus(Job job, JobStatus status, bool saveNow = true)
        {
            if (job != null)
            {
                if (job.JobStatus != status)
                {
                    // TODO - enable this block only after Baseline Phase is complete for Marine Terminal - AJH
                    //if (job.IsPreparatory)
                    //{
                    //    if (status == JobStatus.Unassigned)
                    //    {
                    //        // status cannot be Pending, must be OnHold since job IsPreparatory
                    //        job.JobStatus = JobStatus.Waiting;
                    //        InsertOrUpdate(job, saveNow);                        
                    //    }
                    //}
                    //else
                    {
                        job.JobStatus = status;
                        InsertOrUpdate(job, saveNow);                        
                    }
                }
            }
        }

        public IEnumerable<Job> GetDriverJobs(int driverId, DateTime? dueDate, int? jobGroupId)
        {
            return Select().Where(p => p.AssignedDriverId == driverId && p.DueDate == dueDate && p.JobGroupId == jobGroupId).OrderBy(p => p.DriverSortOrder).ThenBy(p => p.Id);
        }


        public override void Delete(Job entity, bool saveChanges = true)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            // Mark deleted instead
            entity.IsDeleted = true;
            _repository.Update(entity, saveChanges);

            // remove entity from cache
            if (_enableCaching)
            {
                _cacheManager.RemoveByPattern(string.Format(CacheByIdPatternKey, entity.Id));
            }
        }

        public override void Insert(Job entity, bool saveChanges = true)
        {
            //if (entity.RouteStops != null && entity.RouteStops.Count > 2)
            //{
            //    // todo - cache job groups
            //    var jobGroups = 
            //        _jobGroupService.Select()
            //            .Where(p => p.SubscriberId == entity.SubscriberId && p.Name != "Unspecified")
            //            .OrderBy(p => p.Name)
            //            .ToList();

            //    var ts = new TimeSpan(entity.RouteStops[1].WindowStart).Add(new TimeSpan(0, 30, 0));
            //    if (ts.TotalHours < 5 || (ts.TotalHours >= 17 && ts.TotalHours <= 24))
            //    {
            //        entity.JobGroupId = jobGroups[0].Id;
            //    }
            //    else
            //    {
            //        entity.JobGroupId = jobGroups[1].Id;
            //    }
            //}

            if (entity.DataCollection == null)
            {
                entity.DataCollection = new JobDataCollection() { };
            }

            base.Insert(entity, saveChanges);
        }

        public override void Update(Job entity, bool saveChanges = true)
        {
            if (!entity.IsValid)
            {
                entity.LocationDistanceProcessedDate = null;
            }

            //if (entity.RouteStops != null && entity.RouteStops.Count > 2)
            //{
            //    // todo - cache job groups
            //    var jobGroups =
            //        _jobGroupService.Select()
            //            .Where(p => p.SubscriberId == entity.SubscriberId && p.Name != "Unspecified")
            //            .OrderBy(p => p.Name)
            //            .ToList();

            //    var ts = new TimeSpan(entity.RouteStops[1].WindowStart);
            //    if (ts.TotalHours < 5 || (ts.TotalHours >= 17 && ts.TotalHours < 29))
            //    {
            //        entity.JobGroupId = jobGroups[1].Id;
            //        entity.JobGroup = jobGroups[1];
            //    }
            //    else
            //    {
            //        entity.JobGroupId = jobGroups[0].Id;
            //        entity.JobGroup = jobGroups[0];
            //    }
            //}

            if (entity.DataCollection == null)
            {
                entity.DataCollection = new JobDataCollection() { };
            }

            base.Update(entity, saveChanges);
        }

        protected override IQueryable<Job> InternalSelect()
        {
            return base.InternalSelect();
        }

        public override IQueryable<Job> SelectWithAll()
        {
            return _repository.SelectWith(
                "Chassis",
                "Container",
                "ChassisOwner",
                "ContainerOwner",
                "RouteStops",
                "RouteStops.Location",
                "AssignedDriver",
                "RouteStops.StopAction",
                "JobGroup");
        }

        public IQueryable<Job> SelectWithAll(int subscriberId)
        {
            return this.SelectWithAll()
                .Where(f => f.SubscriberId == subscriberId && f.IsDeleted == false);
        }

        public Job Clone(Job job, DateTime? dueDate, bool cloneRouteStops = true, string orderNumber = "",
            bool copyJobPairId = false, int? jobGroupIdOverride = null, JobTemplateType templateType = JobTemplateType.Unspecified,
            bool copyDriverAssignment = true, bool copyPickNumber = true, bool copyBookingNumber = true, bool copyOrderNumber = true)
        {
            if (job == null) return null;
            //job = GetByIdWithAll(job.Id);

            // create the new Job
            var clonedJob = new Job()
            {
                SubscriberId = job.SubscriberId, 
                DueDate = dueDate,
                JobGroupId = jobGroupIdOverride > 0 ? jobGroupIdOverride : job.JobGroupId,
                ChassisId = job.ChassisId,
                ChassisOwnerId = job.ChassisOwnerId,
                ContainerId = job.ContainerId,
                ContainerOwnerId = job.ContainerOwnerId,
                IsTransmitted = false,
                JobStatus = JobStatus.Unassigned,
            };

            if (clonedJob.JobGroupId.HasValue && clonedJob.JobGroupId > 0 && (clonedJob.JobGroup == null || clonedJob.JobGroup.Id != clonedJob.JobGroupId))
            {
                clonedJob.JobGroup = _jobGroupService.GetById(clonedJob.JobGroupId.Value);
            }

            if (copyDriverAssignment)
            {
                clonedJob.AssignedDriverId = job.AssignedDriverId;
            }

            if (copyPickNumber)
            {
                clonedJob.PickupNumber = job.PickupNumber;
            }

            if (copyBookingNumber)
            {
                clonedJob.BookingNumber = job.BookingNumber;
            }

            if (copyOrderNumber)
            {
                clonedJob.OrderNumber = job.OrderNumber;
            }

            clonedJob.JobTemplateType = templateType != JobTemplateType.Unspecified ? templateType : job.JobTemplateType;

            if (clonedJob.JobTemplateType.ToString().ToLower().StartsWith("import"))
            {
                clonedJob.IsPreparatory = true;
            }

            if (jobGroupIdOverride.HasValue)
            {
                clonedJob.JobGroupId = jobGroupIdOverride;
            }

            if (copyJobPairId)
            {
                clonedJob.JobPairId = job.JobPairId;
            }

            if (orderNumber.Length > 0)
            {
                clonedJob.OrderNumber = orderNumber;
            }

            Insert(clonedJob);
            if (clonedJob.Id > 0)
            {
                clonedJob = GetById(clonedJob.Id);
            }

            var clonedRoutes = new List<RouteStop>();
            if (cloneRouteStops)
            {
                int count = 0;
                foreach (var route in job.RouteStops.OrderBy(rs => rs.SortOrder))
                {
                    var minutes = 30;
                    if (route.StopDelay.HasValue && route.StopDelay.Value > 0) minutes = (int) route.StopDelay.Value;

                    clonedRoutes.Add(
                        CreateJobRouteStop(clonedJob, count++, route.LocationId, minutes, route.StopActionId, route.WindowStart, route.WindowEnd, false));
                }

                for (int i = 0; i < clonedRoutes.Count; i++ )
                {
                    var route = clonedRoutes[i];
                    clonedJob.RouteStops.Add(route);
                    _routeStopService.Insert(route);    // TODO batch save
                }
            }

            Update(clonedJob);
            clonedJob = GetById(clonedJob.Id);

            return clonedJob;
        }

        private RouteStop CreateJobRouteStop(Job job, int sortOrder, int? locationId, int? minutes, int? stopActionId, long windowStart, long windowEnd, bool bSave = false)
        {
            if (job == null || job.Id == 0) throw new Exception("Job is invalid");
            var result = new RouteStop()
            {
                SubscriberId = job.SubscriberId,
                JobId = job.Id,
                LocationId = locationId,
                StopDelay = minutes,
                StopActionId = stopActionId,
                SortOrder = sortOrder,
                WindowStart = windowStart,
                WindowEnd = windowEnd,
            };

            if (bSave)
            {
                _routeStopService.Insert(result);

                job.RouteStops.Add(result);
                Update(job);
                job = GetByIdWithAll(job.Id);
            }
            return result;
            
        }

        private RouteStop CreateJobRouteStop(Job job, int sortOrder, int? locationId, int? minutes, string actionShortName, long windowStart, long windowEnd, bool bSave = false)
        {
            var sa = _stopActionService.GetByShortName(actionShortName);
            if (sa == null) throw new Exception("Unable to Fetch Stop Action: " + actionShortName);

            return CreateJobRouteStop(job, sortOrder, locationId, minutes, sa.Id, windowStart, windowEnd, bSave);
        }

        #region Determine if Property is Dirty

        
        private bool IsDirtyProperty(ObjectContext ctx, object entity, string propertyName)
        {
           ObjectStateEntry entry;
           if (ctx.ObjectStateManager.TryGetObjectStateEntry(entity, out entry))
           {
               int propIndex = this.GetPropertyIndex(entry, propertyName);

               if (propIndex != -1)
               {
                   var oldValue = entry.OriginalValues[propIndex];
                   var newValue = entry.CurrentValues[propIndex];

                   return !Equals(oldValue, newValue);
               }
               else
               {
                   throw new ArgumentException(String.Format("Cannot find original value for property '{0}' in entity entry '{1}'",
                           propertyName,
                           entry.EntitySet.ElementType.FullName));
               }
           }

           return false;
        }


        private int GetPropertyIndex(ObjectStateEntry entry, string propertyName)
        {
           OriginalValueRecord record = entry.GetUpdatableOriginalValues();

           for (int i = 0; i != record.FieldCount; i++)
           {
               FieldMetadata metaData = record.DataRecordInfo.FieldMetadata[i];
               if (metaData.FieldType.Name == propertyName)
               {
                   return metaData.Ordinal;
               }
           }
           return -1;
        }



        #endregion
    }
}
