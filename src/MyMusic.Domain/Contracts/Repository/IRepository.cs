namespace MyMusic.Domain.Contracts.Repository;

public interface IRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Ruft eine Entität asynchron anhand ihrer Id ab.
    /// </summary>
    /// <param name="id">Die Id der gesuchten Entität.</param>
    /// <param name="cancellationToken">Ein Token zur Überwachung von Abbruchanforderungen.</param>
    /// <returns>Die gefundene Entität, oder <see langword="null"/>, wenn keine gefunden wurde.</returns>
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Ruft alle Entitäten dieses Typs asynchron ab.
    /// </summary>
    /// <param name="cancellationToken">Ein Token zur Überwachung von Abbruchanforderungen.</param>
    /// <returns>Eine Liste, welche alle Entitäten dieses Typs enthält.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Ruft Entitäten dieses Typs asynchron gefiltert, sortiert und seitenweise ab. Filterung,
    /// Sortierung und Seitenbildung werden vollständig in der Datenbank ausgeführt.
    /// </summary>
    /// <param name="filter">Der Ausdruck, der die zurückzugebenden Entitäten einschränkt.</param>
    /// <param name="orderBy">Der Ausdruck, der die Sortierung der Entitäten festlegt.</param>
    /// <param name="page">Die 1-basierte Seitennummer.</param>
    /// <param name="pageSize">Die Anzahl der Entitäten pro Seite.</param>
    /// <param name="cancellationToken">Ein Token zur Überwachung von Abbruchanforderungen.</param>
    /// <returns>Die Entitäten der angeforderten Seite sowie die Gesamtanzahl aller Treffer.</returns>
    Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Fügt eine neue Entität asynchron hinzu.
    /// </summary>
    /// <param name="entity">Die hinzuzufügende Entität.</param>
    /// <param name="cancellationToken">Ein Token zur Überwachung von Abbruchanforderungen.</param>
    /// <returns>Ein Task, der den asynchronen Vorgang repräsentiert.</returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Markiert eine Entität als geändert.
    /// </summary>
    /// <param name="entity">Die zu aktualisierende Entität.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Markiert eine Entität zum Entfernen.
    /// </summary>
    /// <param name="entity">Die zu entfernende Entität.</param>
    void Remove(TEntity entity);

    /// <summary>
    /// Persistiert alle vorgemerkten Änderungen asynchron.
    /// </summary>
    /// <param name="cancellationToken">Ein Token zur Überwachung von Abbruchanforderungen.</param>
    /// <returns>Die Anzahl der in der Datenbank geänderten Datensätze.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
