namespace MyMusic.Domain.Contracts.Repository;

public interface IRepository<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken);

    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Fügt eine neue Entität hinzu; wird erst mit <see cref="SaveChangesAsync"/> persistiert.</summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>Markiert eine Entität als geändert; wird erst mit <see cref="SaveChangesAsync"/> persistiert.</summary>
    void Update(TEntity entity);

    /// <summary>Markiert eine Entität zum Löschen; wird erst mit <see cref="SaveChangesAsync"/> persistiert.</summary>
    void Remove(TEntity entity);

    /// <summary>Schreibt alle vorgemerkten Änderungen in die Datenbank.</summary>
    /// <returns>Die Anzahl der geänderten Datensätze.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
