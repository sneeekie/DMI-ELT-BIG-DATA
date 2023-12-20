using Microsoft.EntityFrameworkCore;
using Transform.IRepository;

namespace Transform.Repository
{
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected readonly EfDbContext _context;
        protected readonly DbSet<TEntity> _set;

        public BaseRepository(EfDbContext context)
        {
            _context = context;
        }

        public void Add(TEntity entity)
        {
            _context.Add(entity);
        }

        public async Task Save(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}