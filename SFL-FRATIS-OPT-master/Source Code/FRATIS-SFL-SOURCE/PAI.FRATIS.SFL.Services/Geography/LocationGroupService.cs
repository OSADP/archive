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
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Services.Core;
using System;
using System.Collections.Generic;
using PAI.FRATIS.SFL.Services.Core.Caching;

namespace PAI.FRATIS.SFL.Services.Geography
{
    /// <summary>The LocationGroupService interface.</summary>
    public interface ILocationGroupService : IEntityServiceBase<LocationGroup>, IInstallableEntity
    {
        ICollection<LocationGroup> GetLocationGroups(int subscriberId);

        ICollection<LocationGroup> GetLocationGroupsByParentId(int parentId);

        ICollection<LocationGroup> GetByIds(IEnumerable<int> ids);

        ICollection<LocationGroup> GetHomeGroups(int subscriberId);

        IQueryable<LocationGroup> GetByName(int subscriberId, IEnumerable<string> names);

        int GetTerminalLocationGroupId(int subscriberId);
    }

    /// <summary>The location group service.</summary>
    public class LocationGroupService : EntityServiceBase<LocationGroup>, ILocationGroupService, IInstallableEntity
    {
        /// <summary>Initializes a new instance of the <see cref="LocationGroupService"/> class.</summary>
        /// <param name="repository">The repository.</param>
        /// <param name="cacheManager">The cache manager.</param>
        public LocationGroupService(IRepository<LocationGroup> repository, ICacheManager cacheManager)
            : base(repository, cacheManager)
        {
        }

        /// <summary>The get location groups.</summary>
        /// <returns>The <see cref="ICollection"/>.</returns>
        public ICollection<LocationGroup> GetLocationGroups(int subscriberId)
        {
            return Select().Where(p => p.SubscriberId == subscriberId).ToList();
        }

        public ICollection<LocationGroup> GetLocationGroupsByParentId(int parentId)
        {
            return Select().Where(p => p.ParentId == parentId).ToList();
            
        }

        public ICollection<LocationGroup> GetByIds(IEnumerable<int> ids)
        {
            return Select().Where(m => ids.Contains(m.Id)).ToList();
        }

        public ICollection<LocationGroup> GetHomeGroups(int subscriberId)
        {
            return Select().Where(p => p.SubscriberId == subscriberId && p.IsHomeLocation).OrderBy(p => p.Name).Take(100).ToList();
        }

        public int GetTerminalLocationGroupId(int subscriberId)
        {
            return GetByName(subscriberId, new[] { "Terminal", "Marine Terminal" }).Select(p => p.Id).FirstOrDefault();
        }

        public IQueryable<LocationGroup> GetByName(int subscriberId, IEnumerable<string> names)
        {
            return Select().Where(p => p.SubscriberId == subscriberId && names.Contains(p.Name));
        }

        public void Install(int subscriberId)
        {
            var existingLocationGroups = GetLocationGroups(subscriberId);
            var isChanged = false;

            foreach (var lg in this.GetInternalLocationGroups(subscriberId))
            {
                var x = existingLocationGroups.FirstOrDefault(p => p.Name.ToLower() == lg.Name.ToLower());
                if (x == null || x.Id == 0)
                {
                    // add new record
                    lg.SubscriberId = subscriberId;
                    isChanged = true;
                    Insert(lg, false);
                }
            }

            if (isChanged)
            {
                _repository.SaveChanges();
            }
        }

        private IEnumerable<LocationGroup> GetInternalLocationGroups(int subscriberId)
        {
            var locationGroups = new List<LocationGroup>
                {
                    new LocationGroup()
                        {
                            SubscriberId = subscriberId,
                            Name = "Home",
                            IsHomeLocation = true
                        },
                    new LocationGroup()
                        {
                            SubscriberId = subscriberId,
                            Name = "Yard",
                            IsHomeLocation = true
                        },
                    new LocationGroup()
                        {
                            SubscriberId = subscriberId,
                            Name = "Customer",
                            IsHomeLocation = false
                        },
                    new LocationGroup()
                        {
                            SubscriberId = subscriberId,
                            Name = "Terminal",
                            IsHomeLocation = false
                        },
                    new LocationGroup()
                        {
                            SubscriberId = subscriberId,
                            Name = "Intermodal Facility",
                            IsHomeLocation = false
                        },

                };

            return locationGroups;
        }
    }
}