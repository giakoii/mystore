namespace Shared.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Begin a new transaction.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BeginTransactionAsync(Func<Task<bool>> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save all changes.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="needLogicalDelete"></param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(string userName, CancellationToken cancellationToken = default, bool needLogicalDelete = false);
}