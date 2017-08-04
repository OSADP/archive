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
using PAI.Drayage.Optimization.Reporting.Services;
using PAI.Drayage.Optimization.Services;
using PAI.FRATIS.SFL.Optimization.Adapter.Mapping;
using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.SFL.Services.Orders;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Domain.Planning;

namespace PAI.FRATIS.SFL.Optimization.Adapter.Services
{
    public interface IPlanDescriptor
    {
        Dictionary<int, List<string>> GetDriverPlanVerboseRoutes(Plan plan);
    }

    public class PlanDescriptor : IPlanDescriptor
    {
        private readonly ILocationService _locationService;

        private readonly IStopActionService _stopActionService;

        private readonly IDriverService _driverService;

        private readonly IMapperService _mapperService;

        private readonly ICollection<StopAction> _stopActions;

        private IList<Driver> _drivers = null;

        public PlanDescriptor(ILocationService locationService, IStopActionService stopActionService, IDriverService driverService, /*IReportingService reportingService, */IMapperService mapperService)
        {
            _locationService = locationService;
            _stopActionService = stopActionService;
            _driverService = driverService;
            _mapperService = mapperService;
            _stopActions = _stopActionService.GetStopActions();
        }

        private string GetStopAction(int id)
        {
            var sa = _stopActions.FirstOrDefault(p => p.Id == id);
            return sa != null ? sa.ShortName : string.Empty;
        }

        private string GetDriverName(int id)
        {
            if (_drivers == null)
            {
                _drivers = _driverService.Select().ToList();
            }

            var result = _drivers.FirstOrDefault(p => p.Id == id);
            return result != null ? result.DisplayName : string.Empty;
        }

        private readonly Dictionary<int, string> _locationNamesById = new Dictionary<int, string>();
        private string GetLocationName(int id)
        {
            string result;
            try
            {
                if (!_locationNamesById.TryGetValue(id, out result))
                {
                    var entity = _locationService.GetById(id);
                    if (entity != null)
                    {
                        result = entity.DisplayName;
                        _locationNamesById[id] = result;
                    }
                }
            }
            catch (Exception e)
            {
                result = string.Format("LocationId {0}", id);
            }

            return result;
        }

        public Dictionary<int, List<string>> GetDriverPlanVerboseRoutes(Plan plan)
        {
            var result = new Dictionary<int, List<string>>();
            
            return result;
        }
    }
}