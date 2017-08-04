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
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Domain.Equipment;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.Wrappers.WebFleet;
using PAI.FRATIS.Wrappers.WebFleet.Model;

namespace PAI.FRATIS.SFL.Services.Orders
{
    /// <summary>The driverService interface.</summary>
    public interface IDriverService : IEntitySubscriberServiceBase<Driver>, IInstallableEntity
    {
        /// <summary>The select with all.</summary>
        /// <returns>The <see cref="IQueryable"/>.</returns>
        IQueryable<Driver> SelectWithAll(int subscriberId);

        IQueryable<Driver> SelectWithMessages(int subscriberId);

        IQueryable<Driver> GetUnusedDrivers(int subscriberId, DateTime? dueDate, int jobGroupId);

        /// <summary>The get by id with all.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="Driver"/>.</returns>
        Driver GetByIdWithAll(int id);

        /// <summary>The get by name.</summary>
        /// <param name="driverName">The driver name.</param>
        /// <returns>The <see cref="Driver"/>.</returns>
        Driver GetByName(int subscriberId, string driverName);

        void SetEarliestStartTime(int subscriberId, long ticks);

        void WebFleetSync(int subscriberId, int startingLocationId);
    }

    /// <summary>The driver service.</summary>
    public class DriverService : EntitySubscriberServiceBase<Driver>, IDriverService
    {
        private readonly IJobService _jobService;

        private readonly IVehicleService _vehicleService;

        private readonly ILocationService _locationService;

        private readonly IWebFleetObjectService _webFleetObjectService;

        /// <summary>Initializes a new instance of the <see cref="DriverService"/> class.</summary>
        /// <param name="repository">The repository.</param>
        /// <param name="cacheManager">The cache manager.</param>
        public DriverService(IRepository<Driver> repository, ICacheManager cacheManager, IJobService jobService, ILocationService locationService, IVehicleService vehicleService, IWebFleetObjectService webFleetObjectService)
            : base(repository, cacheManager)
        {
            _jobService = jobService;
            _locationService = locationService;
            _vehicleService = vehicleService;
            _webFleetObjectService = webFleetObjectService;
        }

        public IQueryable<Driver> SelectWithMessages(int subscriberId)
        {
            return _repository.SelectWith("JobGroups", "StartingLocation", "SentMessages", "ReceivedMessages").Where(p => p.SubscriberId == subscriberId);
        }

        public IQueryable<Driver> GetUnusedDrivers(int subscriberId, DateTime? dueDate, int jobGroupId)
        {
            var query = _jobService.Select().Where(p => p.SubscriberId == subscriberId && p.DueDate == dueDate);
            if (jobGroupId > 0)
            {
                query = query.Where(p => p.JobGroupId == jobGroupId);
            }

            query = query.Where(p => p.AssignedDriverId.HasValue);

            var ids = query.Select(p => p.AssignedDriverId).ToList();
            var unusedDrivers = Select().Where(p => !ids.Contains(p.Id) && !p.IsPlaceholderDriver).OrderBy(p => p.DisplayName);
            return unusedDrivers;
        }

        /// <summary>The get by id with all.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="Driver"/>.</returns>
        public Driver GetByIdWithAll(int id)
        {
            return SelectWithAll().FirstOrDefault(m => m.Id == id);
        }

        /// <summary>The get by name.</summary>
        /// <param name="driverName">The driver name.</param>
        /// <returns>The <see cref="Driver"/>.</returns>
        public Driver GetByName(int subscriberId, string driverName)
        {
            return SelectWithAll().FirstOrDefault(f => f.SubscriberId == subscriberId && f.DisplayName == driverName);
        }

        public void SetEarliestStartTime(int subscriberId, long ticks)
        {
            foreach (var d in Select().Where(p => p.SubscriberId == subscriberId).ToList())
            {
                d.EarliestStartTime = ticks;
                Update(d, false);
            }

            SaveChanges();
        }

        public void WebFleetSync(int subscriberId, int startingLocationId)
        {
            ICollection<WebFleetDriver> webfleetDrivers = _webFleetObjectService.GetDrivers();
            List<Driver> localDrivers = this.Select().Where(p => p.Id > 0).ToList();

            
            if (this.UpdateDriversToLocalDb(subscriberId, startingLocationId, localDrivers, webfleetDrivers))
            {
                this.SaveChanges();
            }
        }

        private bool UpdateDriversToLocalDb(int subscriberId, int startingLocationId, IList<Driver> localDrivers, IEnumerable<WebFleetDriver> webfleetDrivers)
        {
            bool changesMade = false;
            foreach (WebFleetDriver webfleetDriver in webfleetDrivers)
            {
                Driver existingDriver = localDrivers.FirstOrDefault(p => p.LegacyId == webfleetDriver.DriverNumber.ToUpper());
                if (existingDriver == null)
                {
                    // add locally
                    this.Insert(
                        new Driver
                        {
                            SubscriberId = subscriberId,
                            DisplayName = webfleetDriver.Name,
                            LegacyId = webfleetDriver.DriverNumber.ToUpper(),
                            StartingLocationId = startingLocationId,
                            Phone = webfleetDriver.Phone ?? string.Empty,
                            Email = webfleetDriver.Email ?? string.Empty,
                            Position = new WebFleetLocation()
                        },
                        false);
                    changesMade = true;
                }
                else
                {
                    if (existingDriver.DisplayName != webfleetDriver.Name
                        ||
                        (!string.IsNullOrEmpty(existingDriver.Phone) && !string.IsNullOrEmpty(webfleetDriver.Phone)
                         && existingDriver.Phone != webfleetDriver.Phone)
                        ||
                        (!string.IsNullOrEmpty(existingDriver.Email) && !string.IsNullOrEmpty(webfleetDriver.Email)
                         && existingDriver.Email != webfleetDriver.Email))
                    {
                        existingDriver.DisplayName = webfleetDriver.Name;
                        existingDriver.Phone = webfleetDriver.Phone ?? string.Empty;
                        existingDriver.Email = webfleetDriver.Email ?? string.Empty;
                        this.Update(existingDriver, false);
                        changesMade = true;
                    }
                }
            }
            return changesMade;
        }

        public int? GetDriverId(int? vehicleId)
        {
            return vehicleId.HasValue ? _vehicleService.GetDriverId(vehicleId.Value) : null;
        }

        /// <summary>The select with all.</summary>
        /// <returns>The <see cref="IQueryable"/>.</returns>
        public IQueryable<Driver> SelectWithAll(int subscriberId)
        {
            return _repository.SelectWith("StartingLocation").Where(p => p.SubscriberId == subscriberId);
        }

        public void Install(int subscriberId)
        {
            // install default location if needed
            if (_locationService.Select().FirstOrDefault(p => p.DisplayName == "Home") == null)
            {
                _locationService.Install(subscriberId);
            }

            var defaultLocation = _locationService.Select().FirstOrDefault(p => p.DisplayName == "Home");

            if (Select().FirstOrDefault(p => p.IsPlaceholderDriver) == null)
            {
                var placeholderDriver = new Driver()
                    {
                        SubscriberId = subscriberId,
                        DisplayName = "Placeholder Driver",
                        FirstName = "Placeholder",
                        LastName = "Driver",
                        IsPlaceholderDriver = true,
                        AvailableDrivingHours = 11,
                        AvailableDutyHours = 14,
                        StartingLocation = defaultLocation
                    };

                Insert(placeholderDriver);
            }
        }
    }
}
