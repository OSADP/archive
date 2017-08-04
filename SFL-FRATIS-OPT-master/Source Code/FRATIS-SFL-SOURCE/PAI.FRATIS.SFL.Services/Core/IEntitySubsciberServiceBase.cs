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
using System.Linq;

using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Services.Core.Caching;

namespace PAI.FRATIS.SFL.Services.Core
{
    public interface IEntitySubscriberServiceBase<TEntity> : IEntityServiceBase<TEntity>
        where TEntity : EntitySubscriberBase
    {
        /// <summary>
        /// Gets all matching entities based upon the provided SubscriberId
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <param name="selectWithAll"></param>
        /// <returns></returns>
        IQueryable<TEntity> GetBySubscriberId(int subscriberId, bool selectWithAll = false);

        /// <summary>
        /// Gets the requested entity by the provided entityId
        /// while verifying SubscriberId ownership of the returned result
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        TEntity GetById(int entityId, int subscriberId);
    }

    public abstract class EntitySubscriberServiceBase<TEntity> : EntityServiceBase<TEntity>, IEntitySubscriberServiceBase<TEntity>
        where TEntity : EntitySubscriberBase
    {
        protected EntitySubscriberServiceBase(IRepository<TEntity> repository, ICacheManager cacheManager)
            : base(repository, cacheManager)
        {
        }

        public IQueryable<TEntity> GetBySubscriberId(int subscriberId, bool selectWithAll = false)
        {
            var query = selectWithAll ? SelectWithAll() : Select();
            query = query.Where(p => p.SubscriberId == subscriberId);
            return query;
        }

        /// <summary>
        /// Gets the requested entity by the provided entityId
        /// while verifying SubscriberId ownership of the returned result
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        public TEntity GetById(int entityId, int subscriberId)
        {
            var entity = base.GetById(entityId);
            if (entity != null && entity.SubscriberId != subscriberId)
            {
                throw new Exception("Subscriber Id Mismatch - Access Denied to entity");
            }
            return entity;
        }
    }
}