using System;
using System.Linq;
using System.Linq.Expressions;

namespace INCZONE.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> FindAll();
        /// <summary>
        /// For getting derived types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQueryable<T> OfType<T>();
        IQueryable<T> FindWhere(Expression<Func<T, bool>> predicate);
        T FindById(int id);
        void Add(T newEntity);
        void Remove(T entity);
        void Set(int id, T entity);//Update
        void Attach(T newEntity);
    }
}
