using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace GreenSpace.Application.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // Sync methods
        List<T> GetAll();
        void Add(T entity);
        void Update(T entity);
        bool Remove(T entity);

        // Async methods
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(object id);
        Task<T> AddAsync(T entity);

        Task<T> UpdateAsync(T entity);
        Task<bool> RemoveAsync(T entity);
        // Prepare methods (without save)
        void PrepareCreate(T entity);
        void PrepareUpdate(T entity);
        void PrepareRemove(T entity);
        int Save();
        Task<int> SaveAsync();

        // Queryable
        IQueryable<T> GetAllQueryable();

        Task<bool> RemoveMultipleEntitiesAsync(List<T> entities);

        Task<List<T>> AddMultipleAsync(List<T> entities);
    }
}