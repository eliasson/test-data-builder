#nullable enable

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Code;

public interface IAggregate
{
    public Guid Id { get; }
}

public class Repository<T> where T : IAggregate
{
    private ConcurrentDictionary<Guid, T> Storage = new ();

    public async Task SaveAsync(T aggregate, CancellationToken ct)
    {
        await Task.CompletedTask;
        Storage[aggregate.Id] = aggregate;
    }

    public async Task<T> LoadAsync(Guid id, CancellationToken ct)
    {
        await Task.CompletedTask;
        return Storage[id] ?? throw new Exception("Not found");
    }

    public async IAsyncEnumerable<T> LoadAllAsync([EnumeratorCancellation] CancellationToken ct)
    {
        await Task.CompletedTask;
        foreach (var agg in Storage.Values)
            yield return agg;
    }
}
