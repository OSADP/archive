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
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using PAI.FRATIS.Data;
using PAI.FRATIS.SFL.Common;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;

namespace PAI.FRATIS.SFL.Data
{
    /// <summary>
    /// Entity Framework repository
    /// </summary>
    public partial class EfRepository<T> : IRepository<T> where T : class
    {
        private readonly IDbContext _context;
        private IDbSet<T> _entities;
        private readonly IIncluder _includer;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="context">Object context</param>
        /// <param name="includer">Eager loading property includer</param>
        public EfRepository(IDbContext context, IIncluder includer)
        {
            _context = context;
            _includer = includer;
        }

        public T GetById(object id)
        {
            return Entities.Find(id);
        }

        /// <summary>
        /// Attaches the entity to the underlying context of this reposity.  
        /// It is placed in the Unchanged state as if it was just read from the database
        /// </summary>
        /// <param name="entity">The entity to attach</param>
        /// <param name="markDirty">Marks the newly attached entity as modified</param>
        /// <returns></returns>
        public void Attach(T entity, bool markDirty = false)
        {
            _context.Attach(entity, markDirty);
        }

        public void Detach(T entity)
        {
            _context.Detach(entity);
        }

        public void MarkDirty(T entity)
        {
            _context.MarkDirty(entity);
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public object GetContext()
        {
            return _context;
        }

        public void Insert(T entity, bool saveChanges = true)
        {
            try
            {
                Entities.Add(entity);

                if (saveChanges)
                {
                    _context.SaveChanges();
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                var msg = GetMessageFromDbEntityValidationException(dbEx);
                var fail = new Exception(msg, dbEx);
                throw fail;
            }
        }

        public void Update(T entity, bool saveChanges = true)
        {
            try
            {
                if (saveChanges)
                {
                    _context.SaveChanges();
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                var msg = GetMessageFromDbEntityValidationException(dbEx);
                var fail = new Exception(msg, dbEx);
                throw fail;
            }
        }

        public void Delete(T entity, bool saveChanges = true)
        {
            try
            {
                Entities.Remove(entity);

                if (saveChanges)
                {
                    _context.SaveChanges();
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                var msg = GetMessageFromDbEntityValidationException(dbEx);
                var fail = new Exception(msg, dbEx);
                throw fail;
            }
        }

        private IDbSet<T> Entities
        {
            get { return _entities ?? (_entities = _context.Set<T>()); }
        }

        /// <summary>
        /// Selects query
        /// </summary>
        public IQueryable<T> Select()
        {
            return Entities;
        }

        /// <summary>
        /// Selects query with included properties
        /// </summary>
        /// <param name="includeProperties"></param>
        public IQueryable<T> SelectWith(params Expression<Func<T, object>>[] includeProperties)
        {
            return includeProperties.Aggregate(Select(), (current, includeProperty) => _includer.Include(current, includeProperty));
        }

        public IQueryable<T> SelectWith(params string[] includeProperties)
        {
            return includeProperties.Aggregate(Select(), (current, includeProperty) => _includer.Include(current, includeProperty));
        }

        [ExcludeFromCodeCoverage]
        private string GetMessageFromDbEntityValidationException(DbEntityValidationException dbEx)
        {
            var sb = new StringBuilder();
            foreach (var validationErrors in dbEx.EntityValidationErrors)
            {
                foreach (var validationError in validationErrors.ValidationErrors)
                {
                    sb.AppendFormat("Property: {0} Error: {1}" + Environment.NewLine,
                        validationError.PropertyName, validationError.ErrorMessage);
                }
            }
            return sb.ToString();
        }

    }
}