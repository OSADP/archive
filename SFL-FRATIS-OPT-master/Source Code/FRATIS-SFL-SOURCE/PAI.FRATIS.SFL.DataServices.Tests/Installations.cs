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
using Ninject;
using Ninject.Extensions.Conventions;
using PAI.FRATIS.Data;
using PAI.FRATIS.SFL.Common;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Common.Infrastructure.Engine;
using PAI.FRATIS.SFL.Data;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Domain.Users;
using PAI.FRATIS.SFL.Infrastructure;
using PAI.FRATIS.SFL.Services.Authentication;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.SFL.Services.Equipment;
using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.SFL.Services.Information;
using PAI.FRATIS.SFL.Services.Logging;
using PAI.FRATIS.SFL.Services.Orders;
using PAI.FRATIS.SFL.Tests;
using PAI.FRATIS.SFL.Web.Framework.Mapping;

namespace PAI.FRATIS.SFL.DataServices.Tests
{
    public class Installations : TestsBase
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

            Kernel.Bind<IDbContext>().To<DataContext>().InTransientScope()
              .WithConstructorArgument("nameOrConnectionString", ConnectionStringManager.ConnectionString);

            Kernel.Bind<IIncluder>().To<DbIncluder>().InTransientScope();

            Kernel.Rebind<ICacheManager>().To<MemoryCacheManager>().InThreadScope();

            Kernel.Rebind<ILogService>().To<LogService>().InThreadScope();

            _jobGroupService = Kernel.Get<IJobGroupService>();

            _chassisService = Kernel.Get<IChassisService>();
            _containerOwnerService = Kernel.Get<IContainerOwnerService>();
            _containerService = Kernel.Get<IContainerService>();
            _stopActionService = Kernel.Get<IStopActionService>();

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

        //[Test]
        //public void Install_stopactions()
        //{
        //    var existingActions = _stopActionService.Select().ToList();
        //    List<StopAction> stopActions = null;

        //    if (existingActions.Count == 0)
        //    {
        //        _stopActionService.Install();
        //        stopActions = _stopActionService.Select().ToList();
        //    }

        //    Assert.That(existingActions.Count != 0, "Stop Actions already installed");
        //    Assert.That(stopActions != null && stopActions.Count > 0, "Stop Actions failed to install - count still 0");
        //}

        [Test]
        public void Install_placeholder_driver()
        {
            var location = _locationService.Select().FirstOrDefault();
            var placeholderDriver = new Driver()
                {
                    DisplayName = "Placeholder Driver",
                    IsPlaceholderDriver=true,
                    StartingLocation = location,
                    EarliestStartTime = 0,
                    AvailableDrivingHours = 11,
                    AvailableDutyHours = 14,
                };

            _driverService.Insert(placeholderDriver);
        }

        //[Test]
        //public void Install_jobgroups()
        //{
        //    var existingGroups = _jobGroupService.Select().ToList();
        //    List<JobGroup> groups = null;

        //    if (existingGroups.Count == 0)
        //    {
        //        _jobGroupService.Install();
        //        groups = _jobGroupService.Select().ToList();
        //    }

        //    Assert.That(existingGroups.Count != 0, "Groups already installed");
        //    Assert.That(groups != null && groups.Count > 0, "Groups failed to install - count still 0");
        //}
    }
}
