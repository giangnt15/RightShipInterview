using RightShip.Core.Domain.Entities;

namespace RightShip.Core.Domain.Repositories;

/// <summary>
/// The interface repository for entities
/// </summary>
public interface IRepository
{

}

/// <summary>
/// The interface repository for entities
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRepository<T, TId> : IRepository where T : class, IEntity<TId>
{
    /// <summary>
    /// Load an entity by its id
    /// </summary>
    /// <param name="id">The id of the entity</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The entity</returns>
    Task<T> LoadAsync(TId id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Add an entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity</returns>
    Task<T> AddAsync(T entity);
    /// <summary>
    /// Update an entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="id">The id of the entity</param>
    /// <returns>The updated entity</returns>
    Task<T> UpdateAsync(T entity, TId id);
    /// <summary>
    /// Delete an entity
    /// </summary>
    /// <param name="id">The id of the entity</param>
    /// <returns>The deleted entity</returns>
    Task<T> DeleteAsync(TId id);
}