using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Code;

public interface IAggregate
{
    public Guid Id { get; }
}

public class Repository<T> where T : IAggregate
{
    // Pretend this repository is multi-tenant , where the User ID actually has some effect.

    private readonly ConcurrentDictionary<Guid, T> _storage = new ();

    public async Task SaveAsync(Guid userId, T aggregate, CancellationToken ct)
    {
        await Task.CompletedTask;
        _storage[aggregate.Id] = aggregate;
    }

    public async Task<T> LoadAsync(Guid userId, Guid id, CancellationToken ct)
    {
        await Task.CompletedTask;
        return _storage[id] ?? throw new Exception("Not found");
    }

    public async IAsyncEnumerable<T> LoadAllAsync(Guid userId, [EnumeratorCancellation] CancellationToken ct)
    {
        await Task.CompletedTask;
        foreach (var agg in _storage.Values)
            yield return agg;
    }
}

// Custom repository since there is no owner of users.
public class UserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _storage = new ();

    public async Task SaveAsync(User aggregate, CancellationToken ct)
    {
        await Task.CompletedTask;
        _storage[aggregate.Id] = aggregate;
    }

    public async Task<User> LoadAsync(Guid id, CancellationToken ct)
    {
        await Task.CompletedTask;
        return _storage[id] ?? throw new Exception("Not found");
    }

    public async IAsyncEnumerable<User> LoadAllAsync([EnumeratorCancellation] CancellationToken ct)
    {
        await Task.CompletedTask;
        foreach (var agg in _storage.Values)
            yield return agg;
    }
}

