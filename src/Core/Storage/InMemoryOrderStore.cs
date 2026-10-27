using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryOrderStore : IOrderStore
{
    private readonly Dictionary<string, Order> _items =
        new(StringComparer.OrdinalIgnoreCase);

    public InMemoryOrderStore(IEnumerable<Order>? seed = null)
    {
        foreach (var item in seed ?? [])
            Add(item);
    }

    private static Order Copy(Order item) => Order.FromDto(item.ToDto());

    public IReadOnlyList<Order> List() =>
        _items.Values.Select(Copy).ToList().AsReadOnly();

    public Order? GetById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _items.TryGetValue(id, out var item) ? Copy(item) : null;
    }

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!_items.TryAdd(item.Id, Copy(item)))
            throw new InvalidOperationException($"Замовлення з id={item.Id} уже існує.");
    }

    public void Update(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Немає замовлення з id={item.Id} для оновлення.");
        _items[item.Id] = Copy(item);
    }

    public bool Remove(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _items.Remove(id);
    }
}
