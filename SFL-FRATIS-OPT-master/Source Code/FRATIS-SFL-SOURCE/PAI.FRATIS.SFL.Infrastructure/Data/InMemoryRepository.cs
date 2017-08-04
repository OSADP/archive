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
using System.Linq.Expressions;

namespace PAI.FRATIS.SFL.Infrastructure.Data
{
    /// <summary>
    /// Basic In-Memory Repository Implementation
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class InMemoryRepository<TEntity> : IRepository<TEntity> where TEntity : class 
    {
        private readonly IList<TEntity> _entities = new List<TEntity>();

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public object GetContext()
        {
            return null;
        }

        public object GetObjectContext()
        {
            return null;
        }

        public void SetConnectionString(string connectionString)
        {
            return;
        }

        public void Insert(TEntity entity, bool saveChanges = true)
        {
            this._entities.Add(entity);
        }

        public void Update(TEntity entity, bool saveChanges = true)
        {
        }

        public void Delete(TEntity entity, bool saveChanges = true)
        {
            this._entities.Remove(entity);
        }

        public IQueryable<TEntity> Select()
        {
            return this._entities.AsQueryable();
        }

        public IQueryable<TEntity> SelectWith(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return this._entities.AsQueryable();
        }

        public TEntity GetById(object id)
        {
            return default(TEntity); //_entities.SingleOrDefault(e => e.Id == (int)id);
        }

        public void Attach(TEntity entity, bool markDirty = false)
        {
        }

        public void Detach(TEntity entity)
        {
        }

        public void MarkDirty(TEntity entity)
        {
        }

        public IQueryable<TEntity> SelectWith(params string[] includeProperties)
        {
            return this._entities.AsQueryable();
        }

        public void Clear()
        {
            this._entities.Clear();
        }
    }
}
