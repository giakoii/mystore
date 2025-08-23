using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces.Repositories;

namespace MyStoreManagement.Infrastructure.Repositories;

public class UnitOfWork(DbContext context) : IUnitOfWork
{
 
    /// <summary>
    /// The database context for the unit of work.
    /// </summary>
    public void Dispose()
    {
        context?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Begin a new transaction and execute the provided action.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    public async Task BeginTransactionAsync(Func<Task<bool>> action, CancellationToken cancellationToken = default)
    {
        // Begin transaction
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Execute action
            if (await action())
            {
                await transaction.CommitAsync(cancellationToken);
            }
            else
            {
                await transaction.RollbackAsync(cancellationToken);
            }
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    /// <summary>
    /// Save all changes to the database
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="needLogicalDelete"></param>
    /// <returns></returns>
    public async Task<int> SaveChangesAsync(string userName, CancellationToken cancellationToken = default, bool needLogicalDelete = false)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}