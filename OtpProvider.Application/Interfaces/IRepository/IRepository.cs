using System.Linq.Expressions;


namespace OtpProvider.Application.Interfaces.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetAsync(params object[] keyValues);
        IQueryable<T> Query();
        IQueryable<T> Query(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity, CancellationToken ct = default);
        void Update(T entity);
        void Remove(T entity);
    }
}