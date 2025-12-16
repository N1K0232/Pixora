using System.Reflection;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pixora.Authentication;
using Pixora.DataAccessLayer.Entities.Common;

namespace Pixora.DataAccessLayer;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger) : AuthenticationDbContext(options), IApplicationDbContext
{
    public async Task CreateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        await Set<T>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        Set<T>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    public async ValueTask<T?> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var entity = await Set<T>().FindAsync([id], cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public IQueryable<T> GetData<T>(bool trackingChanges = false) where T : BaseEntity
    {
        var set = Set<T>();
        return trackingChanges ? set : set.AsNoTrackingWithIdentityResolution();
    }

    public async Task ExecuteTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        var strategy = Database.CreateExecutionStrategy();
        logger.LogInformation("Starting transaction execution");

        await strategy.ExecuteAsync(async (token) => await ExecuteTransactionInternalAsync(action, token), cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Transaction execution completed");
    }

    public async Task<T> ExecuteTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        var strategy = Database.CreateExecutionStrategy();
        logger.LogInformation("Starting transaction execution");

        var result = await strategy.ExecuteAsync(async (token) => await ExecuteTransactionInternalAsync(action, token), cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Transaction execution completed");

        return result;
    }

    public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.Entity.GetType()))
            .ToList();

        foreach (var entry in entries.Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State is EntityState.Modified)
            {
                entity.LastModifiedAt = DateTime.UtcNow;
            }
        }

        try
        {
            var result = await SaveChangesAsync(true, cancellationToken).ConfigureAwait(false);
            logger.LogDebug("Successfully updated {result} rows", result);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Saving changes failed due to an exception. Statement has been terminated");
            throw;
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseExceptionProcessor();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }

    private async Task ExecuteTransactionInternalAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        var transaction = await Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await action.Invoke(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while executing transaction");
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

            throw;
        }
        finally
        {
            await transaction.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task<T> ExecuteTransactionInternalAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        var transaction = await Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var result = await action.Invoke(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while executing transaction");
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

            throw;
        }
        finally
        {
            await transaction.DisposeAsync().ConfigureAwait(false);
        }
    }
}