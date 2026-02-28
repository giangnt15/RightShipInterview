using RightShip.Core.Domain.Repositories;

namespace RightShip.Core.Application.Uow
{
    /// <summary>
    /// The interface for the unit of work pattern.
    /// Abstracts the transaction management to support other persistence frameworks.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Start the unit of work
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        Task StartAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Commit the transaction
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The number of rows affected</returns>
        Task<int> CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollback the transaction
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The number of rows affected</returns>
        Task RollbackAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a repository for a specific type
        /// </summary>
        /// <typeparam name="T">The type of the repository</typeparam>
        /// <returns>The repository</returns>
        TRepository GetRepository<TRepository>() where TRepository : IRepository;
    }
}
