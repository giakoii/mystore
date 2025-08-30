using Microsoft.EntityFrameworkCore;
using MyStoreManagement.Application.Interfaces.Repositories;
using MyStoreManagement.Infrastructure.Contexts;

namespace MyStoreManagement.Infrastructure.Repositories;

public class UnitOfWork(MyStoreManagementContext context) : IUnitOfWork
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
    /// <returns></returns>
    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }
}