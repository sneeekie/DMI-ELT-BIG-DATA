using System.Linq.Expressions;

namespace Transform.IRepository
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        void Add(TEntity entity);
        Task Save(CancellationToken cancellationToken);
    }
}
