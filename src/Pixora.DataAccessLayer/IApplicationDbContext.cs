namespace Pixora.DataAccessLayer;

public interface IApplicationDbContext
{
    Task CreateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    Task DeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    Task DeleteAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class;

    ValueTask<T?> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : class;

    IQueryable<T> GetData<T>(bool trackingChanges = false) where T : class;

    Task<int> SaveAsync(CancellationToken cancellationToken = default);

    Task ExecuteTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);

    Task<T> ExecuteTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
}