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
using System.Data.Objects;
using System.Linq;
using PAI.FRATIS.SFL.Domain;

namespace PAI.FRATIS.SFL.Services.Core
{
    /// <summary>
    /// Represents the interface for basic <see cref="PAI.FRATIS.SFL.Domain.EntityBase"/> services
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IEntityServiceBase<TEntity> where TEntity : EntityBase
    {
        /// <summary>
        /// Saves changes with the DbContext
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">entity</param>
        void Delete(TEntity entity, bool saveChanges = true);

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entityId">entity identifier</param>
        void Delete(int entityId, bool saveChanges = true);

        /// <summary>
        /// Gets an entity 
        /// </summary>
        /// <param name="entityId">entity identifier</param>
        /// <returns>Entity</returns>
        TEntity GetById(int entityId);

        /// <summary>
        /// Inserts an entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Insert(TEntity entity, bool saveChanges = true);

        /// <summary>
        /// Updates the entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Update(TEntity entity, bool saveChanges = true);

        /// <summary>
        /// Clear cache
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Gets an <see cref="IQueryable"/> of <see cref="TEntity"/>
        /// </summary>
        /// <returns><see cref="IQueryable"/> of <see cref="TEntity"/></returns>
        IQueryable<TEntity> Select();


        IQueryable<TEntity> SelectWithAll();
        /// <summary>
        /// Attaches the entity to the underlying context of this reposity.  
        /// It is placed in the Unchanged state as if it was just read from the database
        /// </summary>
        /// <param name="entity">The entity to attach</param>
        /// <param name="markDirty">Marks the newly attached entity as modified</param>
        /// <returns></returns>
        void Attach(TEntity entity, bool markDirty = false);

        void Detach(TEntity entity);

        void MarkDirty(TEntity entity);

        /// <summary>
        /// Inserts or Updates the given entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="saveChanges"></param>
        void InsertOrUpdate(TEntity entity, bool saveChanges = true);

        void InsertOrUpdate(ICollection<TEntity> entities, bool saveChanges = true);
    }
}