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

using System.Collections.Generic;
using System.Linq;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Domain.Equipment;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;

namespace PAI.FRATIS.SFL.Services.Equipment
{
    public interface IChassisService : IEntityServiceBase<Chassis>, IInstallableEntity
    {
        /// <summary>
        /// Gets a collection of all Chassis
        /// regardless of their IsDomestic or IsEnabled status
        /// </summary>
        /// <returns></returns>
        ICollection<Chassis> GetChassis(int subscriberId);

        /// <summary>
        /// Gets a collection of all enabled Chassis
        /// regardless of their IsDomestic status
        /// </summary>
        /// <returns></returns>
        ICollection<Chassis> GetEnabledChassis(int subscriberId);


        /// <summary>
        /// Gets a collection of all valid, enabled Domestic Chassis
        /// This includes Chassis with IsDomestic set to null
        /// </summary>
        /// <returns></returns>
        ICollection<Chassis> GetDomesticChassis(int subscriberId);

        /// <summary>
        /// Gets a collection of all valid, enabled International Chassis
        /// This includes Chassis with IsDomestic set to null
        /// </summary>
        /// <returns></returns>
        ICollection<Chassis> GetInternationalChassis(int subscriberId);

        IEnumerable<Chassis> GetCompatibleChassisForContainer(int subscriberId, string containerName);
    }

    public class ChassisService : EntityServiceBase<Chassis>, IChassisService
    {
        public ChassisService(IRepository<Chassis> repository, ICacheManager cacheManager)
            : base(repository, cacheManager)
        {
        }

        public IEnumerable<Chassis> GetCompatibleChassisForContainer(int subscriberId, string containerName)
        {
            var result = from chassis in _repository.Select().Where(p => p.SubscriberId == subscriberId)
                         where chassis.DisplayName.StartsWith(containerName.Replace("Container", "Chassis").Replace("HC", "").Trim())
                         select chassis;
            return result;
        }

        public ICollection<Chassis> GetChassis(int subscriberId)
        {
            return InternalSelect().Where(p => p.SubscriberId == subscriberId).OrderBy(p => p.DisplayName).ToList();
        }

        public ICollection<Chassis> GetEnabledChassis(int subscriberId)
        {
            return InternalSelect().Where(p => p.SubscriberId == subscriberId).Where(p => p.Enabled).OrderBy(p => p.DisplayName).ToList();
        }

        public ICollection<Chassis> GetDomesticChassis(int subscriberId)
        {
            return InternalSelect().Where(p => p.SubscriberId == subscriberId && p.IsDomestic != false).Where(p => p.Enabled).OrderBy(p => p.DisplayName).ToList();
        }

        public ICollection<Chassis> GetInternationalChassis(int subscriberId)
        {
            return InternalSelect().Where(p => p.SubscriberId == subscriberId && p.IsDomestic != true).Where(p => p.Enabled).OrderBy(p => p.DisplayName).ToList();
        }

        public void Install(int subscriberId)
        {
            var existingChassiss = GetChassis(subscriberId);
            var isChanged = false;

            foreach (var chassis in GetChassisList(subscriberId))
            {
                var x = existingChassiss.FirstOrDefault(p => p.SubscriberId == subscriberId && p.DisplayName.ToLower() == chassis.DisplayName.ToLower());
                if (x == null || x.Id == 0)
                {
                    // add new record
                    isChanged = true;
                    Insert(chassis, false);
                }
            }

            if (isChanged)
                _repository.SaveChanges();
        }

        private IEnumerable<Chassis> GetChassisList(int subscriberId)
        {
            var chassis = new List<Chassis>
                {
                    new Chassis()
                        {
                            SubscriberId = subscriberId,
                            DisplayName = "Chassis 20, 2 Axles",
                            Enabled = true,
                            IsDomestic = false
                        },
                    new Chassis()
                        {
                            SubscriberId = subscriberId,
                            DisplayName = "Chassis 20, 3 Axles",
                            Enabled = true,
                            IsDomestic = false
                        },
                    new Chassis()
                        {
                            SubscriberId = subscriberId,
                            DisplayName = "Chassis 40",
                            Enabled = true,
                            IsDomestic = false
                        },
                    new Chassis()
                        {
                            SubscriberId = subscriberId,
                            DisplayName = "Chassis 45",
                            Enabled = true,
                            IsDomestic = false
                        },
                    new Chassis()
                        {
                            SubscriberId = subscriberId,
                            DisplayName = "Chassis 48",
                            Enabled = true,
                            IsDomestic = true
                        },
                    new Chassis()
                        {
                            SubscriberId = subscriberId,
                            DisplayName = "Chassis 53",
                            Enabled = true,
                            IsDomestic = true
                        },
                    new Chassis()
                        {
                            SubscriberId = subscriberId,
                            DisplayName = "Trailer",
                            Enabled = true,
                            IsDomestic = null       // used for both International and Domestic
                        },
                };

            return chassis;
        }
    }
}