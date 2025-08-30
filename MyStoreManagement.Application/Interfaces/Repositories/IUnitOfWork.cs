namespace MyStoreManagement.Application.Interfaces.Repositories;

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
    /// <returns></returns>
    Task<int> SaveChangesAsync();
}