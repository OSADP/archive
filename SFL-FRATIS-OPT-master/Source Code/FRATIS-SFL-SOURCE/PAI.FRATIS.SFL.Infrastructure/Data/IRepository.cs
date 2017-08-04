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
using System.Linq.Expressions;

namespace PAI.FRATIS.SFL.Infrastructure.Data
{
    /// <summary>
    /// Generic repository interface
    /// </summary>
    /// <typeparam name="TEntity">entity type</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        void SaveChanges();

        object GetContext();

        object GetObjectContext();

        void SetConnectionString(string connectionString);

        /// <summary>
        /// Insert entity into the repository
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="saveChanges"></param>
        void Insert(TEntity entity, bool saveChanges = true);

        /// <summary>
        /// Update entity in the repository
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="saveChanges"></param>
        void Update(TEntity entity, bool saveChanges = true);

        /// <summary>
        /// Delete entity from the repository
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="saveChanges"></param>
        void Delete(TEntity entity, bool saveChanges = true);

        /// <summary>
        /// Selects query
        /// </summary>
        IQueryable<TEntity> Select();

        /// <summary>
        /// Selects query with included properties
        /// </summary>
        /// <param name="includeProperties"></param>
        IQueryable<TEntity> SelectWith(params Expression<Func<TEntity, object>>[] includeProperties);

        /// <summary>
        /// Selects query with included properties
        /// </summary>
        /// <param name="includeProperties"></param>
        IQueryable<TEntity> SelectWith(params string[] includeProperties);

        /// <summary>
        /// Gets entity by id
        /// </summary>
        TEntity GetById(object id);
        
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
    }


}
