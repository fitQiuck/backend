using RenessansAPI.Domain.Common;
using System.Linq.Expressions;

namespace RenessansAPI.DataAccess.IRepository;

public interface IGenericRepository<T> where T : Auditable
{
    IQueryable<T> GetAll(Expression<Func<T, bool>> expression = null, string[] includes = null);
    ValueTask<T> GetAsync(Expression<Func<T, bool>> expression, string[] includes = null);
    ValueTask<T> CreateAsync(T entity);
    ValueTask<bool> DeleteAsync(T entity);
    T Update(T entity);
    public ValueTask SaveChangesAsync();
}
