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

using System.Data.Entity;

namespace PAI.FRATIS.SFL.Data
{
    public interface IDbContext
    {
        /// <summary>
        /// Saves all changes made in this context to the underlying database
        /// </summary>
        /// <returns>The number of objects written to the underlying database.</returns>
        int SaveChanges();

        /// <summary>
        /// Returns a DbSet instance for access to entities of the given type in the context, the ObjectStateManager, and the underlying store.
        /// </summary>
        /// <typeparam name="TEntity">A set for the given entity type.</typeparam>
        /// <returns>TEntity: The type entity for which a set should be returned.</returns>
        IDbSet<TEntity> Set<TEntity>() where TEntity : class;

        /// <summary>
        /// Attaches the entity to the context.  It is placed in the Unchanged state as if it was just read from the database
        /// </summary>
        /// <typeparam name="TEntity">The type of object to be attached</typeparam>
        /// <param name="entity">The entity to attach</param>
        /// <param name="markDirty">Marks the newly attached entity as modified</param>
        /// <returns></returns>
        TEntity Attach<TEntity>(TEntity entity, bool markDirty = false) where TEntity : class;

        void MarkDirty<TEntity>(TEntity entity) where TEntity : class;

        void Detach<TEntity>(TEntity entity) where TEntity : class;
    }
}
