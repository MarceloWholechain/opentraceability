using System.Collections.Concurrent;
using DiagnosticsTool.Models.Responses;

namespace DiagnosticsTool.Services;

/// <summary>
/// Thread-safe in-memory cache for up to 100 DiagnosticsEnvelope instances (any generic type argument).
/// FIFO eviction (oldest added removed first when capacity exceeded).
/// </summary>
public interface IDiagnosticsEnvelopeCache
{
    string Add<T>(DiagnosticsEnvelope<T> envelope);
    bool TryGet<T>(string id, out DiagnosticsEnvelope<T>? envelope);
    bool TryGet(string id, out IDiagnosticsEnvelope? envelope, out DateTime createdUtc);
    int Count { get; }
    IReadOnlyCollection<string> Ids { get; }
    IEnumerable<CacheEntryInfo> GetEntries();
}

public sealed record CacheEntryInfo(string Id, DateTime CreatedUtc, bool HasSchemaErrors, int RequestCount, int ValidationCount);

internal sealed class DiagnosticsEnvelopeCache : IDiagnosticsEnvelopeCache
{
    private const int Capacity = 100;

    private readonly ConcurrentDictionary<string, CacheItem> _items = new();
    private readonly ConcurrentQueue<string> _order = new();
    private readonly object _lock = new();

    private record CacheItem(IDiagnosticsEnvelope Envelope, DateTime CreatedUtc);

    public string Add<T>(DiagnosticsEnvelope<T> envelope)
    {
        var id = Guid.NewGuid().ToString("n");
        var item = new CacheItem(envelope, DateTime.UtcNow);
        lock (_lock)
        {
            _items[id] = item;
            _order.Enqueue(id);
            while (_items.Count > Capacity && _order.TryDequeue(out var oldestId))
            {
                _items.TryRemove(oldestId, out _);
            }
        }
        return id;
    }

    public bool TryGet<T>(string id, out DiagnosticsEnvelope<T>? envelope)
    {
        envelope = null;
        if (_items.TryGetValue(id, out var item) && item.Envelope is DiagnosticsEnvelope<T> typed)
        {
            envelope = typed;
            return true;
        }
        return false;
    }

    public bool TryGet(string id, out IDiagnosticsEnvelope? envelope, out DateTime createdUtc)
    {
        envelope = null; createdUtc = default;
        if (_items.TryGetValue(id, out var item))
        {
            envelope = item.Envelope;
            createdUtc = item.CreatedUtc;
            return true;
        }
        return false;
    }

    public IEnumerable<CacheEntryInfo> GetEntries()
    {
        foreach (var kvp in _items)
        {
            var env = kvp.Value.Envelope;
            yield return new CacheEntryInfo(kvp.Key, kvp.Value.CreatedUtc, env.HasSchemaErrors, env.Diagnostics.Requests.Count, env.Diagnostics.Validations.Count);
        }
    }

    public int Count => _items.Count;
    public IReadOnlyCollection<string> Ids => _items.Keys.ToList().AsReadOnly();
}
