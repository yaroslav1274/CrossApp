using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileOrderStore : IOrderStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly Dictionary<string, Order> _cache =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path;
    private readonly List<Order> _seed;
    private bool _loaded;

    public FileOrderStore(string path, IEnumerable<Order>? seed = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = Path.GetFullPath(path);
        _seed = (seed ?? []).Select(Copy).ToList();
    }

    private static Order Copy(Order item) => Order.FromDto(item.ToDto());

    private void EnsureLoaded()
    {
        if (_loaded) return;
        // Завантаження відбувається один раз; повторний запуск читає файл.
        if (File.Exists(_path))
        {
            var json = File.ReadAllText(_path);
            var dtos = JsonSerializer.Deserialize<List<OrderDto>>(json) ?? [];
            foreach (var dto in dtos)
            {
                var order = Order.FromDto(dto); // повторна перевірка інваріантів
                if (!_cache.TryAdd(order.Id, order))
                    throw new InvalidDataException($"Дублікат id={order.Id} у файлі {_path}.");
            }
        }
        else
        {
            foreach (var item in _seed)
                if (!_cache.TryAdd(item.Id, Copy(item)))
                    throw new InvalidOperationException($"Повторюється початковий id={item.Id}.");
            Flush(); // створюємо файл лише для першого запуску
        }
        _loaded = true;
    }

    private void Flush()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var dtos = _cache.Values.Select(item => item.ToDto()).ToList();
        var json = JsonSerializer.Serialize(dtos, Options);
        File.WriteAllText(_path, json);
    }

    public IReadOnlyList<Order> List()
    {
        EnsureLoaded();
        return _cache.Values.Select(Copy).ToList().AsReadOnly();
    }

    public Order? GetById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        EnsureLoaded();
        return _cache.TryGetValue(id, out var item) ? Copy(item) : null;
    }

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        if (!_cache.TryAdd(item.Id, Copy(item)))
            throw new InvalidOperationException($"Замовлення з id={item.Id} уже існує.");
        Flush();
    }

    public void Update(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        if (!_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Немає замовлення з id={item.Id} для оновлення.");
        _cache[item.Id] = Copy(item);
        Flush();
    }

    public bool Remove(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;
        Flush();
        return true;
    }
}
