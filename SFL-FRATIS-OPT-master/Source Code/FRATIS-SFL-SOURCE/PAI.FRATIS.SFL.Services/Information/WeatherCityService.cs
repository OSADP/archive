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

using System.Linq;

using PAI.FRATIS.ExternalServices.Weather;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;

namespace PAI.FRATIS.SFL.Services.Information
{
    public interface IWeatherCityService : IEntitySubscriberServiceBase<WeatherCity>, IInstallableEntity
    {
        WeatherCity GetByCityCode(int subscriberId, string cityCode);
    }

    public class WeatherCityService : EntitySubscriberServiceBase<WeatherCity>, IWeatherCityService
    {
        private readonly IWeatherService _yahooWeatherService;

        public WeatherCityService(IRepository<WeatherCity> repository, ICacheManager cacheManager, IWeatherService yahooWeatherService)
            : base(repository, cacheManager)
        {
            _yahooWeatherService = yahooWeatherService;
        }

        public void Install(int subscriberId)
        {
            var existingCities = GetBySubscriberId(subscriberId).ToList();
            if (!existingCities.Any())
            {
                this.Insert(new WeatherCity()
                    {
                        DisplayName = "Miami, Florida",
                        CityCode = _yahooWeatherService.GetCityCode("Miami", "Florida"),
                        SubscriberId = subscriberId
                    });
            }
        }

        public WeatherCity GetByCityCode(int subscriberId, string cityCode)
        {
            return Select().FirstOrDefault(p => p.SubscriberId == subscriberId && p.CityCode == cityCode);
        }
    }
}