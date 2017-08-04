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

using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Services.Core;
using System.Collections.Generic;
using System.Linq;
using PAI.FRATIS.SFL.Domain.Equipment;
using PAI.FRATIS.SFL.Services.Core.Caching;

namespace PAI.FRATIS.SFL.Services.Orders
{
    public interface IVehicleService : IEntitySubscriberServiceBase<Vehicle>, IInstallableEntity
    {
        int? GetDriverId(int? vehicleId);

        Vehicle GetByLegacyId(string legacyId);

        IEnumerable<Vehicle> GetVehicles(int subscriberId);

        IQueryable<Vehicle> SelectWithAll();

        int? GetVehicleId(int? driverId);
    }

    public class VehicleService : EntitySubscriberServiceBase<Vehicle>, IVehicleService
    {
        public VehicleService(IRepository<Vehicle> repository, ICacheManager cacheManager)
            : base(repository, cacheManager)
        {
        }

        public int? GetDriverId(int? vehicleId)
        {
            if (vehicleId.HasValue)
            {
                var result = GetById(vehicleId.Value);
                return result != null ? result.DriverId : null;
            }
            return null;
        }

        public int? GetVehicleId(int? driverId)
        {
            if (driverId.HasValue)
            {
                var item = Select().FirstOrDefault(p => p.DriverId == driverId);
                if (item != null)
                {
                    return item.Id;
                }
            }

            return null;
        }

        public Vehicle GetByLegacyId(string legacyId)
        {
            return SelectWithAll().FirstOrDefault(p => p.LegacyId == legacyId);
        }

        public IEnumerable<Vehicle> GetVehicles(int subscriberId)
        {
            return SelectWithAll().Where(p => p.SubscriberId == subscriberId).OrderBy(p => p.Name);
        }

        public IQueryable<Vehicle> SelectWithAll()
        {
            return _repository.SelectWith(
                "Driver");
        }

        public override void Insert(Vehicle entity, bool saveChanges = true)
        {
            RemovePreviousDriverAssignmentIfAlreadyExists(entity, saveChanges);
            base.Insert(entity, saveChanges);
        }

        private void RemovePreviousDriverAssignmentIfAlreadyExists(Vehicle entity, bool saveChanges)
        {
            if (entity.DriverId.HasValue)
            {
                var existingVehicleWithDriverAssignment =
                    InternalSelect().FirstOrDefault(p => p.DriverId == entity.DriverId);

                if (existingVehicleWithDriverAssignment != null)
                {
                    existingVehicleWithDriverAssignment.DriverId = null;
                    Update(existingVehicleWithDriverAssignment, saveChanges);
                }
            } 
        }

        public override void Update(Vehicle entity, bool saveChanges = true)
        {
            RemovePreviousDriverAssignmentIfAlreadyExists(entity, saveChanges);
            base.Update(entity, saveChanges);
        }

        public void Install(int subscriberId)
        {
            var existingVehicles = this.Select().ToList();
            if (!existingVehicles.Any())
            {
                this.Insert(new Vehicle()
                    {
                        Name = "Sample Vehicle",
                        SubscriberId = subscriberId
                    });
            }
        }
    }
}
