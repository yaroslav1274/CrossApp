using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public bool IsConfirmed { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal Total => _lines.Sum(l => l.Price * l.Quantity);

    private Order(string id, string customerId, bool isConfirmed)
    {
        Id = id;
        CustomerId = customerId;
        IsConfirmed = isConfirmed;
    }

    public static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор замовлення обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Ідентифікатор клієнта не може бути порожнім", nameof(customerId));

        return new Order(id.Trim(), customerId.Trim(), false);
    }

    public void AddLine(string productId, string name, decimal price, int quantity)
    {
        if (IsConfirmed)
            throw new InvalidOperationException($"Замовлення {Id} вже підтверджене, рядки додавати не можна");

        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("ID товару не може бути порожнім", nameof(productId));

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Ціна товару не може бути від'ємною");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Кількість у рядку має бути більшою за нуль");

        _lines.Add(new OrderLine(productId.Trim(), name.Trim(), price, quantity));
    }

    public void Confirm()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException($"Неможливо підтвердити порожнє замовлення {Id}");

        IsConfirmed = true;
    }

    public OrderDto ToDto()
    {
        var lineDtos = _lines.Select(l => new OrderLineDto(l.ProductId, l.Name, l.Price, l.Quantity)).ToList();
        return new OrderDto(Id, CustomerId, IsConfirmed, lineDtos);
    }

    public static Order FromDto(OrderDto dto)
    {
        var order = new Order(dto.Id, dto.CustomerId, dto.IsConfirmed);
        foreach (var line in dto.Lines)
        {
            order._lines.Add(new OrderLine(line.ProductId, line.Name, line.Price, line.Quantity));
        }
        return order;
    }

    public override string ToString() =>
        $"Замовлення {Id} (Клієнт: {CustomerId}) - {(IsConfirmed ? "Підтверджено" : "Чернетка")}, Сума: {Total}";
}