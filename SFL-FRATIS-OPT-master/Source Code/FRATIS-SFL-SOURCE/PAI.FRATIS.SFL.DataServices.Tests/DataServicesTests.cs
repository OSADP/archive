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
using System.Linq;
using NUnit.Framework;
using Ninject;
using Ninject.Extensions.Conventions;
using PAI.Drayage.EnhancedOptimization.Model;
using PAI.Drayage.EnhancedOptimization.Services;
using PAI.Drayage.Optimization.Common;
using PAI.Drayage.Optimization.Function;
using PAI.Drayage.Optimization.Geography;
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Equipment;
using PAI.Drayage.Optimization.Model.Orders;
using PAI.Drayage.Optimization.Model.Planning;
using PAI.Drayage.Optimization.Reporting.Services;
using PAI.Drayage.Optimization.Services;
using PAI.FRATIS.Data;
using PAI.FRATIS.SFL.Common;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Common.Infrastructure.Engine;
using PAI.FRATIS.SFL.Data;
using PAI.FRATIS.SFL.Domain.Equipment;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Domain.Subscribers;
using PAI.FRATIS.SFL.Domain.Users;
using PAI.FRATIS.SFL.Infrastructure;
using PAI.FRATIS.SFL.Optimization.Adapter.Mapping;
using PAI.FRATIS.SFL.Services.Authentication;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.SFL.Services.Equipment;
using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.SFL.Services.Information;
using PAI.FRATIS.SFL.Services.Logging;
using PAI.FRATIS.SFL.Services.Orders;
using PAI.FRATIS.SFL.Tests;
using PAI.FRATIS.SFL.Web.Framework.Mapping;
using Driver = PAI.FRATIS.SFL.Domain.Orders.Driver;
using IRouteStopService = PAI.FRATIS.SFL.Services.Orders.IRouteStopService;
using Job = PAI.FRATIS.SFL.Domain.Orders.Job;
using Location = PAI.Drayage.Optimization.Model.Location;
using NullLogger = PAI.Infrastructure.NullLogger;
using RouteStop = PAI.FRATIS.SFL.Domain.Orders.RouteStop;

namespace PAI.FRATIS.SFL.DataServices.Tests
{
    public class EntityTests : TestsBase
    {
        #region Member Variables

        private IDomainModelMapper _mappingService;
        private IUserService _userService;
        private IDriverService _driverService;
        private IVehicleService _vehicleService;
        private IStateService _stateService;
        private IChassisService _chassisService;
        private ILocationService _locationService;
        private ILocationGroupService _locationGroupService;
        private IJobService _jobService;
        private IJobGroupService _jobGroupService;

        private IContainerOwnerService _containerOwnerService;
        private IContainerService _containerService;
        private IStopActionService _stopActionService;

        private IRouteStopService _routeStopService;

        private ILocationDistanceService _locationDistanceService;

        private IWeatherCityService _weatherCityService;

        private IRepository<User> _userRepository;
        private IDbContext _dbContext;
        private IIncluder _includer;

        private ISubscriberService _subscriberService;

        #endregion

        [SetUp]
        public void SetUp()
        {
            Kernel.Bind<IEngine>().To<Engine>().InSingletonScope();

            // Run installation tasks
            Kernel.Bind(x =>
                        x.FromAssemblyContaining<StateService>()
                         .SelectAllClasses()
                         .BindAllInterfaces()
                         .Configure(f => f.InTransientScope()));

            // Bind Database Repository 
            Kernel.Rebind(typeof(IRepository<>)).To(typeof(EfRepository<>));

            Kernel.Bind<IDbContext>().To<DataContext>().InSingletonScope()
              .WithConstructorArgument("nameOrConnectionString", ConnectionStringManager.ConnectionString);

            Kernel.Bind<IIncluder>().To<DbIncluder>().InTransientScope();

            Kernel.Rebind<ICacheManager>().To<MemoryCacheManager>().InThreadScope();

            Kernel.Rebind<ILogService>().To<LogService>().InThreadScope();

            _jobGroupService = Kernel.Get<IJobGroupService>();

            _chassisService = Kernel.Get<IChassisService>();
            _containerOwnerService = Kernel.Get<IContainerOwnerService>();
            _containerService = Kernel.Get<IContainerService>();
            _stopActionService = Kernel.Get<IStopActionService>();

            _subscriberService = Kernel.Get<ISubscriberService>();

            _driverService = Kernel.Get<IDriverService>();
            _vehicleService = Kernel.Get<IVehicleService>();
            _userService = Kernel.Get<IUserService>();
            _stateService = Kernel.Get<IStateService>();
            _locationService = Kernel.Get<ILocationService>();
            _locationDistanceService = Kernel.Get<ILocationDistanceService>();

            Kernel.Rebind<IDomainModelMapper>().To<DomainModelAutoMapper>().InThreadScope();
            _mappingService = Kernel.Get<IDomainModelMapper>();

            _jobService = Kernel.Get<IJobService>();
            _routeStopService = Kernel.Get<IRouteStopService>();
            _locationGroupService = Kernel.Get<ILocationGroupService>();
            _weatherCityService = Kernel.Get<IWeatherCityService>();

            AutoMapperInitializer.Initialize();
        }

        [Test]
        public void Can_get_users()
        {
            var activeUsers = _userService.Select().ToList();
            Assert.That(activeUsers != null && activeUsers.Any(), "No active users found in repository");
        }

        [Explicit]
        [Test]
        public void Can_add_subscriber()
        {
            var subscriber = new Subscriber() { Name = "Second  Subscriber" };
            _subscriberService.Insert(subscriber);
            Assert.That(subscriber != null);
            Assert.That(subscriber.Id > 0);
            Console.WriteLine(subscriber.Id);
        }

        [Explicit]
        [Test]
        public void Fix_jobgroup()
        {
            var jobs = _jobService.SelectWithAll().Where(p => p.SubscriberId == 1).ToList();
            var jobGroups = _jobGroupService.Select().Where(p => p.Name != "Unspecified").OrderBy(p => p.Name).ToList();
            foreach (var j in jobs)
            {
                if (j.JobGroupId > 2)
                {
                    if (j.RouteStops.Count > 2)
                    {
                        var ts = new TimeSpan(j.RouteStops[1].WindowStart).Add(new TimeSpan(0, 30, 0));
                        if (ts.TotalHours < 5 || (ts.TotalHours >= 17 && ts.TotalHours <= 24))
                        {
                            j.JobGroupId = jobGroups[0].Id;
                        }
                        else
                        {
                            j.JobGroupId = jobGroups[1].Id;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Cannot handle id {0}", j.Id);
                    }
                }
            }
        }

        [Explicit]
        [Test]
        public void Can_update_subscriber()
        {
            var subscriber = _subscriberService.SelectWithAll().FirstOrDefault();
            if (subscriber != null)
            {
                subscriber.Name = string.Format("{0} Updated", subscriber.Name);
                subscriber.IsActive = true;
                _subscriberService.Update(subscriber);
            }
        }

        [Explicit]
        [Test]
        public void Can_add_sample_job_orders()
        {
            var subscriber = _subscriberService.Select().FirstOrDefault();
            var location = _locationService.Select().FirstOrDefault(p => p.SubscriberId == subscriber.Id);
            var sa = _stopActionService.GetByShortName("NA");
            var job = new Job()
                {
                    Subscriber = subscriber,
                    OrderNumber = "TESTORDER",
                    BillOfLading = "BILLOFLADING",
                    BookingNumber = "BOOKING",
                };

            _jobService.Insert(job);

            var routeStops = new List<RouteStop>()
                {
                    new RouteStop()
                        { Job = job, Subscriber = subscriber, SortOrder = 0, Location = location, StopAction = sa, },
                    new RouteStop()
                        { Job = job, Subscriber = subscriber, SortOrder = 1, Location = location, StopAction = sa, },
                };

            _routeStopService.Insert(routeStops[0], false);
            _routeStopService.Insert(routeStops[1], false);
            _routeStopService.SaveChanges();
        }

        [Explicit]
        [Test]
        public void Can_add_admin_user()
        {
            var subscriber = _subscriberService.SelectWithAll().Where(p => p.Id == 2).FirstOrDefault();
            var user = _userService.Select().FirstOrDefault(p => p.Username == "admin2");
            if (user == null)
            {
                user = new User() { Active = true, IsAdmin = true, Username = "admin2", Subscriber = subscriber };
                _userService.SetUserPassword(user, PasswordFormat.Hashed, "admin2");
                _userService.Insert(user, true);   
                Console.WriteLine("User added {0}", user.Id);                
            }
            else
            {
                Console.WriteLine("User exists {0}", user.Id);                
            }

            Assert.That(user != null && user.Id > 0, "User not created or does not exist");
        }

        [Test]
        public void Can_get_subscibers_with_users()
        {
            var subscribers = _subscriberService.SelectWithAll().ToList();
            foreach (var s in subscribers)
            {
                Console.WriteLine("{0}", s.Name);
                foreach (var u in s.Users)
                {
                    Console.WriteLine("\t{0}", u.Username);
                }
            }
        }

        [Explicit]
        [Test]
        public void Can_add_jobgroup()
        {
            var jg = new JobGroup() { Name = "Unspecified" };
            _jobGroupService.Insert(jg);

            Assert.That(jg != null && jg.Id > 0, "not created or does not exist");
        }

        [Test]
        public void Can_get_drivers()
        {
            var items = _driverService.Select().ToList();
            Assert.That(items != null && items.Any(), "No items found in repository");
        }

        [Test]
        public void Can_get_vehicles()
        {
            var items = _vehicleService.Select().ToList();
            Assert.That(items != null && items.Any(), "No items found in repository");
        }
    }

    public class Hazmat_jobs_should_only_go_to_hazmat_drivers : TestsBase
    {
        Driver hazmatDriver;
        Driver nonHazmatDriver;
        Job job;

        [SetUp]
        public void Setup()
        {
            hazmatDriver =
                new Driver
                    {
                        AvailableDrivingHours = 10,
                        AvailableDutyHours = 14,
                        DisplayName = "Hazmat Driver",
                        EarliestStartTime = new TimeSpan(0, 0, 0).Ticks,
                        Id = 1,
                        IsFlatbed = false,
                        IsHazmat = true,
                        StartingLocation = new Domain.Geography.Location()
                    };
            nonHazmatDriver =
                new Driver
                    {
                        AvailableDrivingHours = 10,
                        AvailableDutyHours = 14,
                        DisplayName = "Hazmat Driver",
                        EarliestStartTime = new TimeSpan(0, 0, 0).Ticks,
                        Id = 1,
                        IsFlatbed = false,
                        IsHazmat = false,
                        StartingLocation = new Domain.Geography.Location()
                    };
            job = new Job();
            job.IsHazmat = true;

        }
    }
}
