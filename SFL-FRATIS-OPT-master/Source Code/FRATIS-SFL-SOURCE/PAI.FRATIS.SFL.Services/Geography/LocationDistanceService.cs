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
using System.Threading.Tasks;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.SFL.Services.Orders;

namespace PAI.FRATIS.SFL.Services.Geography
{
    public interface ILocationDistanceService : IEntityServiceBase<LocationDistance>
    {
        void Prefetch(IEnumerable<Location> locations);

        LocationDistance Get(Location startLocation, Location endLocation);

        bool CreateEmptyRecords(int subscriberId, DateTime orderDate, ICollection<Location> locations);

        IQueryable<LocationDistance> GetRecords(
            IEnumerable<Location> locations,
            DateTime? dueDate = null,
            int dayRange = 0,
            bool mustMatchStartAndEndLocaitons = false);

        IQueryable<LocationDistance> SelectWithAll();

        /// <summary>
        /// Updates the entity
        /// </summary>
        /// <param name="entity">Entity</param>
        Task UpdateAsync(LocationDistance entity, bool updateNow = false);
    }

    public class LocationDistanceService : EntityServiceBase<LocationDistance>, ILocationDistanceService
    {
        private readonly ILocationService _locationService;

        private readonly IRouteStopService _routeStopService;

        protected virtual string CacheByStartEndLocationPatternKey
        {
            get { return CachePatternKey + "start-{0}-end-{1}"; }
        }

        public LocationDistanceService(IRepository<LocationDistance> repository, ICacheManager cacheManager, IRouteStopService routeStopService, ILocationService locationService)
            : base(repository, cacheManager)
        {
            _routeStopService = routeStopService;
            _locationService = locationService;
        }

        public void Prefetch(IEnumerable<Location> locations)
        {
            var locationIds = locations.Select(f => f.Id).ToList();
            var allRecords = new List<LocationDistance>();

            int step = 25;
            for (int i = 0; i < locationIds.Count; i += step)
            {
                var ls = locationIds.Skip(i).Take(step);
                  
                var result = Select().Where(p => 
                    ls.Contains(p.StartLocationId.Value) || 
                    ls.Contains(p.EndLocationId.Value)).ToList();

                allRecords.AddRange(result);
            }

            foreach (var locationDistance in allRecords)
            {
                var key = GetKey(locationDistance.StartLocationId.Value, locationDistance.EndLocationId.Value);
                _cacheManager.Set(key, locationDistance, 1000);
            }
        }
        
        public LocationDistance Get(Location startLocation, Location endLocation)
        {
            Func<LocationDistance> getByLocations = () =>
                Select().FirstOrDefault(f => 
                    f.StartLocationId == startLocation.Id && 
                    f.EndLocationId == endLocation.Id && 
                    f.Distance != null && 
                    f.TravelTime != null);
            
            if (_enableCaching)
            {
                var key = GetKey(startLocation.Id, endLocation.Id);

                LocationDistance entity = null;

                if (_cacheManager.IsSet(key))
                {
                    entity = _cacheManager.Get<LocationDistance>(key);
                    _repository.Attach(entity);
                }

                if (entity == null)
                {
                    Console.WriteLine("Wahh");
                }
                
                return entity;
            }

            return getByLocations.Invoke();
        }

        private string GetKey(int startLocationId, int endLocationId)
        {
            if (startLocationId < endLocationId)
            {
                return string.Format(CacheByStartEndLocationPatternKey, startLocationId, endLocationId);
            }
            else
            {
                return string.Format(CacheByStartEndLocationPatternKey, endLocationId, startLocationId);
            }
        }

        /// <summary>
        /// Created LocationDistance records for the newly created Locations 
        /// within Job RouteStops.  Creates empty records for each location
        /// to every location already keyed into the system.
        /// </summary>
        public bool CreateEmptyRecords(int subscriberId, DateTime orderDate, ICollection<Location> locations)
        {
            var locationIds = new HashSet<int>(_routeStopService.GetLocationIds(subscriberId, orderDate, 1).ToList());
            var allLocations = _locationService.Select().Where(p => locationIds.Contains(p.Id)).ToList();

            var hs = new HashSet<Tuple<int, int>>();    // hashset wont be nec with cache
            foreach (var location in allLocations)
            {
                foreach (var newLocation in locations)
                {
                    if (newLocation.Id == location.Id) continue;

                    if (!hs.Contains(new Tuple<int, int>(location.Id, newLocation.Id))
                        && !hs.Contains(new Tuple<int, int>(newLocation.Id, location.Id)))
                    {
                        var existingRecords =
                            Select()
                                .Where(
                                    p =>
                                    (p.StartLocationId == newLocation.Id && p.EndLocationId == location.Id)
                                    || (p.StartLocationId == location.Id && p.EndLocationId == newLocation.Id)).ToList();

                        if (!existingRecords.Any())
                        {
                            // create it
                            var ld = new LocationDistance()
                                         {
                                             StartLocationId = location.Id,
                                             EndLocationId = newLocation.Id,
                                             CreatedDate = DateTime.UtcNow,
                                             SubscriberId = subscriberId
                                         };

                            Insert(ld, false);   
                        }

                        // add to cache so we remember for future iterations
                        hs.Add(new Tuple<int, int>(location.Id, newLocation.Id));
                    }
                }
            }

            SaveChanges();

            return locationIds.Count > 0;
        }

        public IQueryable<LocationDistance> GetRecords(IEnumerable<Location> locations, DateTime? dueDate = null, int dayRange = 0, bool mustMatchStartAndEndLocaitons = false)
        {
            var query = Select();
            var locationIds = (List<int>)locations.Select(p => p.Id);

            if (locationIds != null && locationIds.Any())
            {
                if (dueDate.HasValue)
                {
                    if (dayRange > 0)
                    {

                    }
                }
                else
                {
                    if (mustMatchStartAndEndLocaitons)
                    {
                        query = query.Where(p => p.StartLocationId.HasValue && locationIds.Contains((int)p.StartLocationId) && p.EndLocationId.HasValue && locationIds.Contains((int)p.EndLocationId));
                    }
                    else
                    {
                        query =
                            query.Where(
                                (p =>
                                 p.StartLocationId.HasValue && locationIds.Contains((int)p.StartLocationId)
                                 || (p.EndLocationId.HasValue && locationIds.Contains((int)p.EndLocationId))));
                    }
                
                }                
            }
            return query;
        }

        public IQueryable<LocationDistance> SelectWithAll()
        {
            return _repository.SelectWith("StartLocation", "EndLocation");
        }

        /// <summary>
        /// Updates the entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public async Task UpdateAsync(LocationDistance entity, bool updateNow = false)
        {
            Task.Factory.StartNew(() => Update(entity, updateNow));
        }
    }
}