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
using PAI.Drayage.Optimization.Geography;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.Drayage.EnhancedOptimization.Model;
using PAI.FRATIS.SFL.Optimization.Adapter.Mapping;
using PAI.FRATIS.SFL.Optimization.Adapter.Services;

namespace PAI.FRATIS.SFL.Optimization.Adapter.Services
{
    /// <summary>The validation service.</summary>
    public class ValidationService : IValidationService
    {
        private readonly IOptimizationDateTimeHelper _optDateTimeHelper;

        private readonly IMapperService _mapDomainService;

        private readonly Drayage.EnhancedOptimization.Services.IValidationService _validationService;

        public ValidationService(IMapperService mapDomainService, Drayage.EnhancedOptimization.Services.IValidationService validationService, IOptimizationDateTimeHelper optDateTimeHelper)
        {
            _mapDomainService = mapDomainService;
            _validationService = validationService;
            _optDateTimeHelper = optDateTimeHelper;
        }

        /// <summary>
        /// Validates Job based on RouteStops
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public ValidationResult ValidateJob(PAI.FRATIS.SFL.Domain.Orders.Job job, bool validateLocations, IDistanceService distanceService)
        {
            var optimizationJob = new Drayage.Optimization.Model.Orders.Job();
            try
            {
                job.RouteStops = job.RouteStops.OrderBy(p => p.SortOrder).ToList();

                _mapDomainService.MapDomainToModel(job, optimizationJob, null);
            }
            catch(Exception ex)
            {
                return new ValidationResult()
                           {
                               Successful = false,
                               Errors = new List<string>() { "Invalid Route Stop" }
                           };
            }

            var result = _validationService.ValidateJob(optimizationJob, validateLocations, distanceService);

            try
            {
                return new ValidationResult() { Successful = result.Successful, Errors = result.Errors };

            }
            catch (Exception ex)
            {
                throw new Exception("Could not map ValidationResult using Automapper - verify AutoMapper profile loaded and configured correctly", ex);
            }
        }
    }
}
