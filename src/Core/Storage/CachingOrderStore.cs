using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingOrderStore : IOrderStore
{
    private readonly IOrderStore _inner;
    private Dictionary<string, Order> _cache = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    public int Hits { get; private set; }
    public int Misses { get; private set; }
    public int Invalidations { get; private set; }

    public CachingOrderStore(IOrderStore inner) =>
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    private static Order Copy(Order item) => Order.FromDto(item.ToDto());

    private void LoadIfNecessary()
    {
        if (_loaded) { Hits++; return; }
        Misses++;
        _cache = _inner.List().ToDictionary(x => x.Id, Copy,
            StringComparer.OrdinalIgnoreCase);
        _loaded = true;
    }

    private void Invalidate()
    {
        _cache.Clear();
        _loaded = false;
        Invalidations++;
    }

    public IReadOnlyList<Order> List()
    {
        LoadIfNecessary();
        return _cache.Values.Select(Copy).ToList().AsReadOnly();
    }

    public Order? GetById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        LoadIfNecessary();
        return _cache.TryGetValue(id, out var item) ? Copy(item) : null;
    }

    public void Add(Order item) { _inner.Add(item); Invalidate(); }
    public void Update(Order item) { _inner.Update(item); Invalidate(); }
    public bool Remove(string id)
    {
        var deleted = _inner.Remove(id);
        if (deleted) Invalidate();
        return deleted;
    }
}
