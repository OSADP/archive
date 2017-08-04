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
using PAI.FRATIS.SFL.Services.Core.Caching;

namespace PAI.FRATIS.SFL.Services.Core
{
    /// <summary>
    /// Represents the base class for basic <see cref="PAI.FRATIS.SFL.Domain.EntityBase"/> services
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public partial class EntityServiceBase<TEntity> : IEntityServiceBase<TEntity> where TEntity : EntityBase
    {
        protected readonly IRepository<TEntity> _repository;
        protected readonly ICacheManager _cacheManager;

        //private EntityManagedCache<TEntity> _managedCache = null;

        protected bool _enableCaching = true;
        protected bool _enableEventPublishing = true;

        private string _cachePatternKey;
        public virtual string CachePatternKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_cachePatternKey))
                {
                    string name = typeof(TEntity).FullName.Replace("+", ".");
                    _cachePatternKey = name + ".";
                }
                return _cachePatternKey;
            }
        }

        protected virtual string CacheByIdPatternKey
        {
            get { return CachePatternKey + "id-{0}"; }
        }

        public string CachePatternAllKey
        {
            get { return CachePatternKey + "All"; }
        }

        public EntityServiceBase(IRepository<TEntity> repository, ICacheManager cacheManager)
        {
            _repository = repository;
            _cacheManager = cacheManager;
        }

        public void SaveChanges()
        {
            _repository.SaveChanges();
        }

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">entity</param>
        public virtual void Delete(TEntity entity, bool saveChanges = true)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var archivedEntity = entity as IArchivedEntity;
            if (archivedEntity != null)
            {
                archivedEntity.IsDeleted = true;
                _repository.Update(archivedEntity as TEntity, true);
            }
            else
            {
                _repository.Delete(entity, saveChanges);
            }
            
            // remove entity from cache
            if (_enableCaching)
                _cacheManager.RemoveByPattern(string.Format(CacheByIdPatternKey, (object)entity.Id));

        }

        /// <summary>
        /// Gets an entity 
        /// </summary>
        /// <param name="entityId">entity identifier</param>
        /// <returns>Entity</returns>
        public void Delete(int entityId, bool saveChanges = true)
        {
            var entity = GetById(entityId);
            if (entity != null)
            {
                Delete(entity);
            }
        }

        /// <summary>
        /// Gets an entity 
        /// </summary>
        /// <param name="entityId">entity identifier</param>
        /// <returns>Entity</returns>
        public virtual TEntity GetById(int entityId)
        {
            if (_enableCaching)
            {
                var key = string.Format(CacheByIdPatternKey, entityId);

                return InternalGetById(entityId);

                //TEntity entity;
                //if (_cacheManager.Get(key, () => InternalGetById(entityId), out entity))
                //{
                //    _repository.Attach(entity);
                //}
                //return entity;
            }
            return InternalGetById(entityId);
        }

        /// <summary>
        /// Gets an <see cref="TEntity"/> by id
        /// Override to select eagerly when necessary
        /// </summary>
        /// <returns><see cref="TEntity"/></returns>
        protected virtual TEntity InternalGetById(int entityId)
        {

            return InternalSelect().FirstOrDefault(entity => entity.Id == entityId);
        }

        /// <summary>
        /// Inserts an entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Insert(TEntity entity, bool saveChanges = true)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (entity is EntitySubscriberBase)
            {
                var subscriberEntity = entity as EntitySubscriberBase;
                if ((!subscriberEntity.SubscriberId.HasValue || subscriberEntity.SubscriberId == 0) && (subscriberEntity.Subscriber == null || subscriberEntity.Subscriber.Id == 0))
                {
                    throw new Exception("SubscriberId not set");
                }
            }

            if (entity is IDatedEntity)
            {
                var datedEntity = entity as IDatedEntity;
                datedEntity.CreatedDate = DateTime.UtcNow;
            }

            _repository.Insert(entity, saveChanges);
        }

        /// <summary>
        /// Updates the entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Update(TEntity entity, bool saveChanges = true)
        {   
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (entity is EntitySubscriberBase)
            {
                var subscriberEntity = entity as EntitySubscriberBase;
                if ((!subscriberEntity.SubscriberId.HasValue || subscriberEntity.SubscriberId == 0) && (subscriberEntity.Subscriber == null || subscriberEntity.Subscriber.Id == 0))
                {
                    throw new Exception("SubscriberId not set");
                }
            }

            if (entity is IDatedEntity)
            {
                var datedEntity = entity as IDatedEntity;
                datedEntity.ModifiedDate = DateTime.UtcNow;
            }

            _repository.Update(entity, saveChanges);

            // remove entity from cache
            if (_enableCaching)
            {
                _cacheManager.RemoveByPattern(string.Format(CacheByIdPatternKey, (object)entity.Id));
            }

        }

        public void MarkDirty(TEntity entity)
        {
            this._repository.MarkDirty(entity);
        }

        /// <summary>
        /// Inserts or Updates the given entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="saveChanges"></param>
        public virtual void InsertOrUpdate(TEntity entity, bool saveChanges = true)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            // simple check for now, should probably check if EF attached
            if (entity.Id == 0)
            {
                Insert(entity, saveChanges);
            }
            else
            {
                Update(entity, saveChanges);
            }
        }

        public void InsertOrUpdate(ICollection<TEntity> entities, bool saveChanges = true)
        {
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    InsertOrUpdate(entity, false);
                }

                if (saveChanges)
                {
                    SaveChanges();
                }
            }
        }


        /// <summary>
        /// Gets an <see cref="IQueryable"/> of <see cref="TEntity"/>
        /// </summary>
        /// <returns><see cref="IQueryable"/> of <see cref="TEntity"/></returns>
        public virtual IQueryable<TEntity> Select()
        {
            return InternalSelect();
        }

        /// <summary>
        /// Selects with All / Repository Includes
        /// Virtual method intended to be overridden by each respective DataService if needed
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<TEntity> SelectWithAll()
        {
            return Select();
        }

        public void Attach(TEntity entity, bool markDirty = false)
        {
            if (entity != null)
            {
                _repository.Attach(entity, markDirty);
            }
        }

        public void Detach(TEntity entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets an <see cref="IQueryable"/> of <see cref="TEntity"/>
        /// Override to select eagerly when necessary
        /// </summary>
        /// <returns><see cref="IQueryable"/> of <see cref="TEntity"/></returns>
        protected virtual IQueryable<TEntity> InternalSelect()
        {
            return _repository.Select();
        }

        protected IQueryable<TEntity> InternalSelectSimple()
        {
            return _repository.Select();
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        public virtual void ClearCache()
        {
            if (_enableCaching)
                _cacheManager.RemoveByPattern(CachePatternKey);
        }

        
    }
}