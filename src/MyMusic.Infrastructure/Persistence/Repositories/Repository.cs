namespace MyMusic.Infrastructure.Persistence.Repositories;

public sealed class Repository<TEntity>(MyMusicDbContext context) : IRepository<TEntity>
    where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var filteredQuery = _dbSet.Where(filter);

        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        var items = await orderBy(filteredQuery)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
