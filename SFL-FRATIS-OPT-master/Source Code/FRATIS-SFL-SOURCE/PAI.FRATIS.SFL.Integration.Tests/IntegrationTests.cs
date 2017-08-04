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
using NUnit.Framework;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Common.Infrastructure.Engine;
using PAI.FRATIS.SFL.Services.Authentication;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.SFL.Services.Equipment;
using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.SFL.Services.Information;
using PAI.FRATIS.SFL.Services.Logging;
using PAI.FRATIS.SFL.Services.Orders;
using PAI.FRATIS.SFL.Tests;

namespace PAI.FRATIS.SFL.Integration.Tests
{
    public class IntegrationTests : TestsBase
    {
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

        [Test]
        public void Can_add_subscriber()
        {
            var subscriber = new Subscriber() { Name = "Second  Subscriber" };
            _subscriberService.Insert(subscriber);
            Assert.That(subscriber != null);
            Assert.That(subscriber.Id > 0);
            Console.WriteLine(subscriber.Id);
        }
        
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
}
