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
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;

namespace PAI.FRATIS.SFL.Services.Orders
{
    public interface IJobGroupService : IEntityServiceBase<JobGroup>, IInstallableEntity
    {
        ICollection<JobGroup> GetGroups(int subscriberId);

        ICollection<JobGroup> GetByIds(IEnumerable<int> ids);

        IQueryable<JobGroup> GetByName(int subscriberId, IEnumerable<string> names);
    }

    /// <summary>The job group service.</summary>
    public class JobGroupService : EntityServiceBase<JobGroup>, IJobGroupService
    {
        
        public JobGroupService(IRepository<JobGroup> repository, ICacheManager cacheManager)
            : base(repository, cacheManager)
        {
        }

        /// <summary>
        /// Gets the job groups ordered by ordering, name
        /// </summary>
        /// <returns></returns>
        public ICollection<JobGroup> GetGroups(int subscriberId)
        {
            return InternalSelect().Where(p => p.SubscriberId == subscriberId).OrderBy(m => m.Ordering).ThenBy(m => m.Name).ToList();
        }

        public ICollection<JobGroup> GetByIds(IEnumerable<int> ids)
        {
            return InternalSelect().Where(m => ids.Contains(m.Id)).ToList();
        }


        public IQueryable<JobGroup> GetByName(int subscriberId, IEnumerable<string> names)
        {
            return InternalSelect().Where(p => p.SubscriberId == subscriberId && names.Contains(p.Name));
        }

        public void Install(int subscriberId)
        {
            var existingGroups = GetGroups(subscriberId);
            var isChanged = false;

            foreach (var group in GetBuiltInGroupList())
            {
                var x = existingGroups.FirstOrDefault(p => p.Name.ToLower() == group.Name.ToLower());
                if (x == null || x.Id == 0)
                {
                    // add new record
                    group.SubscriberId = subscriberId;
                    isChanged = true;
                    Insert(group, false);
                }
            }

            if (isChanged)
            {
                _repository.SaveChanges();
            }
        }

        /// <summary>The get built in group list.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        private IEnumerable<JobGroup> GetBuiltInGroupList()
        {
            var groups = new List<JobGroup>
                             {
                                 new JobGroup() { Name = "Day", Ordering = 1, ShiftStartTime = new TimeSpan(7, 0, 0) },
                                 new JobGroup() { Name = "Night", Ordering = 2, ShiftStartTime = new TimeSpan(20, 0, 0) }
                             };
            return groups;
        }
    }
}
