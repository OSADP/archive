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
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Web;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Services.Core;
using System.Collections.Generic;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.Wrappers.WebFleet;
using PAI.FRATIS.Wrappers.WebFleet.Model;

namespace PAI.FRATIS.SFL.Services.Geography
{
    public interface ILocationService : IEntitySubscriberServiceBase<Location>, IInstallableEntity
    {
        Location GetByDisplayName(int subscriberId, string name);

        Location GetByIdWithAll(int id);
    }

    public class LocationService : EntitySubscriberServiceBase<Location>, ILocationService, IInstallableEntity
    {
        private readonly ILocationGroupService _locationGroupService;

        private readonly IWebFleetAddressService _webFleetAddressService;


        public LocationService(IRepository<Location> repository, ICacheManager cacheManager, ILocationGroupService locationGroupService, IWebFleetAddressService webFleetAddressService)
            : base(repository, cacheManager)
        {
            _locationGroupService = locationGroupService;
            _webFleetAddressService = webFleetAddressService;
        }

        public Location GetByDisplayName(int subscriberId, string name)
        {
            return SelectWithAll().FirstOrDefault(f => f.SubscriberId == subscriberId && f.DisplayName == name && f.IsDeleted == false);
        }

        public Location GetByWebFleetId(int subscriberId, string id)
        {
            return Select().FirstOrDefault(f => f.SubscriberId == subscriberId && f.WebFleetId == id);
        }

        public IQueryable<Location> SelectWithAll(int subscriberId)
        {
            return _repository.Select().Where(p => p.SubscriberId == subscriberId && p.IsDeleted == false);
        }

        public string GetCacheKey(int id)
        {
            return string.Format("loc-{0}", id);
        }
        public Location GetByIdWithAll(int id)
        {
            //var key = GetCacheKey(id);
            //if (_cacheManager.IsSet(key))
            //{
            //    return _cacheManager.Get<Location>(key);
            //}

            var item = SelectWithAll().FirstOrDefault(m => m.Id == id);
            //_cacheManager.Set(key, item, 120);
            return item;
        }

        /// <summary>
        /// Gets the First Location provided for the specified Home Location Group Id
        /// Does not work for non-home LocationGroups
        /// </summary>
        /// <param name="locationGroupId">Id for IsHome location group</param>
        /// <returns>Default Location if found, Null if not found or invalid</returns>
        public Location GetDefaultLocation(int locationGroupId)
        {
            Location result = null;
            var lg = _locationGroupService.GetById(locationGroupId);
            //if (lg != null && lg.IsHomeLocation)
            //{
            //    result = GetByLocationGroups(new[] { locationGroupId }).Take(1).OrderBy(p => p.Id).FirstOrDefault();
            //}
            return result;
        }

        protected override IQueryable<Location> InternalSelect()
        {
            return _repository.Select();
        }

        public void Install(int subscriberId)
        {
            var homeLocationGroup = _locationGroupService.Select().FirstOrDefault(p => p.Name == "Home");
            if (homeLocationGroup == null)
            {
                _locationGroupService.Install(subscriberId);
                homeLocationGroup = _locationGroupService.Select().FirstOrDefault(p => p.Name == "Home");
            }

            Install(subscriberId, homeLocationGroup);
        }

        public void Install(int subscriberId, LocationGroup homeLocationGroup = null)
        {
            var homeLocation = Select().FirstOrDefault(p => p.DisplayName == "Home");
            if (homeLocation == null || !homeLocation.Latitude.HasValue || !homeLocation.Longitude.HasValue || (homeLocation.Longitude == 0 && homeLocation.Latitude == 0))
            {
                var defaultLocation = new Location()
                {
                    SubscriberId = subscriberId,
                    DisplayName = "Home",
                    City = "City of Industry",
                    State = "Florida",
                    StreetNumber = string.Empty,
                    Street = string.Empty,
                    Latitude = 34.0167,
                    Longitude = -117.9500,
                    Email = string.Empty,
                    Phone = string.Empty,
                };

                Insert(defaultLocation, true);
            }
        }


        public override void Update(Location entity, bool saveChanges = true)
        {
            base.Update(entity, saveChanges);

            var key = GetCacheKey(entity.Id);
            _cacheManager.Set(key, entity, 900);
        }
       
        public IEnumerable<Location> GetByWebFleetId(int subscriberId, IEnumerable<string> ids)
        {
            return SelectWithAll().Where(p => p.SubscriberId == subscriberId && ids.Contains(p.WebFleetId));
        }

        public void SyncByWebFleetLocationId(string webFleetLocationId)
        {
            IDictionaryEnumerator enumerator = HttpContext.Current.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                HttpContext.Current.Cache.Remove(enumerator.Key.ToString());
            }

            WebFleetAddress address =
                this._webFleetAddressService.GetAddresses(webFleetLocationId).FirstOrDefault(
                    p => p.WebFleetId == webFleetLocationId);
            Location localAddress = this.Select().FirstOrDefault(p => p.WebFleetId == webFleetLocationId);

            if (address != null && address.WebFleetId.Length > 0)
            {
                if (address.StreetAddress != null && address.StreetNumber != null)
                {
                    if (address.StreetAddress.StartsWith(address.StreetNumber + " "))
                    {
                        address.StreetAddress =
                            address.StreetAddress.Substring(address.StreetNumber.ToString().Length + 1);
                    }
                }

                if (localAddress != null)
                {
                    localAddress.DisplayName = address.DisplayName;
                    localAddress.Longitude = address.Longitude;
                    localAddress.Latitude = address.Latitude;
                    localAddress.City = address.City;
                    localAddress.State = address.State;
                    localAddress.Zip = address.Zip;
                    localAddress.StreetAddress = address.StreetAddress;
                    localAddress.StreetNumber = address.StreetNumber.ToString(CultureInfo.InvariantCulture);
                    localAddress.Latitude = address.Latitude;
                    localAddress.Longitude = address.Longitude;
                    localAddress.Phone = address.Phone;
                    localAddress.Email = address.Email;
                    localAddress.LegacyId = address.WebFleetId;
                    localAddress.WebFleetId = address.WebFleetId;
                    localAddress.IsDeleted = false;

                    this.Update(localAddress, false);
                }
                else if (address.Latitude.HasValue && address.Longitude.HasValue)
                {
                    if (address.StreetAddress != null && address.StreetNumber != null)
                    {
                        if (address.StreetAddress.StartsWith(address.StreetNumber + " "))
                        {
                            address.StreetAddress =
                                address.StreetAddress.Substring(address.StreetNumber.ToString().Length + 1);
                        }
                    }

                    localAddress = new Location
                    {
                        DisplayName = address.DisplayName,
                        City = address.City,
                        State = address.State,
                        Zip = address.Zip,
                        StreetAddress = address.StreetAddress,
                        StreetNumber = address.StreetNumber.ToString(CultureInfo.InvariantCulture),
                        Latitude = address.Latitude,
                        Longitude = address.Longitude,
                        Phone = address.Phone,
                        Email = address.Email,
                        LegacyId = address.WebFleetId,
                        WebFleetId = address.WebFleetId
                    };

                    this.Insert(localAddress, false);
                }
            }

            this.SaveChanges();
        }

    }
}