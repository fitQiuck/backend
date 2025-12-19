using Microsoft.EntityFrameworkCore;
using RenessansAPI.DataAccess.AppDBContexts;
using RenessansAPI.DataAccess.IRepository;
using RenessansAPI.Domain.Common;
using System.Linq.Expressions;

namespace RenessansAPI.DataAccess.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : Auditable
{
    private readonly AppDbContext dbContext;
    private readonly DbSet<T> dbSet;

    public GenericRepository(AppDbContext appDbContext)
    {
        this.dbContext = appDbContext;
        this.dbSet = dbContext.Set<T>();
    }

    public IQueryable<T> GetAll(Expression<Func<T, bool>> expression = null, string[] includes = null)
    {
        IQueryable<T> query = dbSet.Where(x => x.DeletedBy == null);

        if (expression != null)
        {
            query = query.Where(expression);
        }

        if (includes != null)
        {
            foreach (var include in includes)
            {
                if (!string.IsNullOrEmpty(include))
                    query = query.Include(include);
            }
        }
        return query;
    }

    public async ValueTask<T> GetAsync(Expression<Func<T, bool>> expression, string[] includes = null)
        => await GetAll(expression, includes).FirstOrDefaultAsync();

    public async ValueTask<T> CreateAsync(T entity) =>
        (await dbContext.AddAsync(entity)).Entity;

    public async ValueTask<bool> DeleteAsync(T entity)
    {
        //entity.State = ItemState.Deleted;
        Update(entity);
        await SaveChangesAsync();
        return true;
    }

    public T Update(T entity)
        => dbSet.Update(entity).Entity;


    public async ValueTask SaveChangesAsync()
        => await dbContext.SaveChangesAsync();
}
