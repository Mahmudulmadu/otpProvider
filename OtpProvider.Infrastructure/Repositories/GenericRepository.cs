using Microsoft.EntityFrameworkCore;
using OtpProvider.Application.Interfaces;
using OtpProvider.Application.Interfaces.IRepository;


//namespace OtpProvider.Infrastructure.Repositories
//{
//    public class GenericRepository<T> : IRepository<T> where T : class
//    {
//        protected readonly DbContext _context;
//        protected readonly DbSet<T> _set;


//        public GenericRepository(DbContext context)
//        {
//            _context = context;
//            _set = _context.Set<T>();
//        }


//        public virtual Task<T?> GetAsync(params object[] keyValues)
//        => _set.FindAsync(keyValues).AsTask();


//        public virtual IQueryable<T> Query() => _set.AsQueryable();
//        public virtual IQueryable<T> Query(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
//        => _set.Where(predicate).AsQueryable();


//        public virtual Task AddAsync(T entity, CancellationToken ct = default)
//        {
//            _set.Add(entity);
//            return Task.CompletedTask;
//        }


//        public virtual void Update(T entity) => _set.Update(entity);
//        public virtual void Remove(T entity) => _set.Remove(entity);
//    }
//}