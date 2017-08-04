using System;
using System.Threading;
using System.Threading.Tasks;

namespace Repository
{
    public interface IUnitOfWork : IUnitOfWorkForService
    {
        void Save();
        Task<int> SaveAsync();
        Task<int> SaveAsync(CancellationToken cancellationToken);
        void Dispose(bool disposing);       
    }

    public interface IUnitOfWorkForService : IDisposable
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : EntityBase;
    }
}