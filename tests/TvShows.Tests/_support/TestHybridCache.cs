using Microsoft.Extensions.Caching.Hybrid;

namespace TvShows.Tests._support;

/// <summary>
/// Minimal HybridCache fake for unit tests. Always invokes the factory (no caching),
/// records tags removed via RemoveByTagAsync.
/// </summary>
internal sealed class TestHybridCache : HybridCache
{
    public List<string> RemovedTags { get; } = new();
    public List<string> RemovedKeys { get; } = new();

    public override ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
        => factory(state, cancellationToken);

    public override ValueTask SetAsync<T>(
        string key,
        T value,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        RemovedKeys.Add(key);
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        RemovedTags.Add(tag);
        return ValueTask.CompletedTask;
    }
}
