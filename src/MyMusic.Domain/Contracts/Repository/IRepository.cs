namespace MyMusic.Domain.Contracts.Repository;

/// <summary>
/// Generischer Datenzugriffsvertrag für Entitäten mit dem Schlüsseltyp <typeparamref name="TKey"/>.
/// </summary>
public interface IRepository<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    /// <summary>Lädt eine Entität anhand ihrer ID oder liefert <c>null</c>, wenn keine gefunden wird.</summary>
    /// <param name="id">Die ID der gesuchten Entität.</param>
    /// <param name="cancellationToken">Token zum Abbrechen des Vorgangs.</param>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken);

    /// <summary>Lädt alle Entitäten dieses Typs.</summary>
    /// <param name="cancellationToken">Token zum Abbrechen des Vorgangs.</param>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Fügt eine neue Entität hinzu; wird erst mit <see cref="SaveChangesAsync"/> persistiert.</summary>
    /// <param name="entity">Die anzulegende Entität.</param>
    /// <param name="cancellationToken">Token zum Abbrechen des Vorgangs.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>Markiert eine Entität als geändert; wird erst mit <see cref="SaveChangesAsync"/> persistiert.</summary>
    /// <param name="entity">Die geänderte Entität.</param>
    void Update(TEntity entity);

    /// <summary>Markiert eine Entität zum Löschen; wird erst mit <see cref="SaveChangesAsync"/> persistiert.</summary>
    /// <param name="entity">Die zu löschende Entität.</param>
    void Remove(TEntity entity);

    /// <summary>Schreibt alle vorgemerkten Änderungen in die Datenbank.</summary>
    /// <param name="cancellationToken">Token zum Abbrechen des Vorgangs.</param>
    /// <returns>Die Anzahl der geänderten Datensätze.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
