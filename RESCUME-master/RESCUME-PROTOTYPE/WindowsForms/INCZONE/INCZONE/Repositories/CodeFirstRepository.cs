using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using INCZONE.Interfaces;

namespace INCZONE.Repositories
{
    public class CodeFirstRepository<T> : IRepository<T> where T : class
    {
        public CodeFirstRepository(DbContext context)
        {
            _dbContext = context;
            _dbSet = context.Set<T>();
        }
        public IQueryable<T> FindAll()
        {
            return _dbSet;
        }
        public IQueryable<T> OfType<T>()
        {
            return _dbSet.OfType<T>();
        }
        public IQueryable<T> FindWhere(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }
        public T FindById(int id)
        {
            return _dbSet.Find(id);
        }
        public void Add(T newEntity)
        {
            _dbSet.Add(newEntity);            
        }
        public void Remove(T entity)
        {            
            _dbSet.Remove(entity);
        }
        public void Set(int id, T entity)
        {
            var entry = _dbContext.Entry(entity);
            _dbSet.Attach(entity);
            entry.State = EntityState.Modified;
        }
        public void Attach(T newEntity)
        {            
            _dbSet.Attach(newEntity);
           
        }
       
        protected readonly DbContext _dbContext;
        protected readonly DbSet<T> _dbSet;
    }
}
