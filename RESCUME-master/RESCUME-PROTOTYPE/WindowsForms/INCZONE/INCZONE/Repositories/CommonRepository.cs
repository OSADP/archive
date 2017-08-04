using System;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using INCZONE.Interfaces;

namespace INCZONE.Repositories
{
    public class CommonRepository<T> : IRepository<T> where T : class, IEntity
    {
        public CommonRepository(ObjectContext context)
        {
            _objectSet = context.CreateObjectSet<T>();
        }
        public IQueryable<T> FindAll()
        {
            return _objectSet;
        }
        public IQueryable<T> OfType<T>()
        {
            return _objectSet.OfType<T>();
        }
        public IQueryable<T> FindWhere(Expression<Func<T, bool>> predicate)
        {
            return _objectSet.Where(predicate);
        }
        public T FindById(int id)
        {
            return _objectSet.Single(o => o.Id == id);
        }
        public void Add(T newEntity)
        {
            _objectSet.AddObject(newEntity);
        }
        public void Remove(T entity)
        {
            _objectSet.DeleteObject(entity);
        }
        public void Set(int id, T entity)
        {
            _objectSet.First(o => o.Id == id);
            _objectSet.ApplyCurrentValues(entity);
        }
        public void Attach(T newEntity)
        {
            _objectSet.Attach(newEntity);
           
        }
       
        protected ObjectSet<T> _objectSet;
    }
}
