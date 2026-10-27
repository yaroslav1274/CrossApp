using Core.Dto;

namespace Core.Domain;

public enum OrderStatus
{
    Draft,
    Confirmed,
    Cancelled
}

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal Total => _lines.Sum(line => line.Price * line.Quantity);

    private Order(string id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Draft;
    }

    public static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор замовлення обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Ідентифікатор клієнта не може бути порожнім", nameof(customerId));

        return new Order(id.Trim(), customerId.Trim());
    }

    public void AddLine(string productId, string name, decimal price, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Замовлення {Id} у стані {Status}, рядки додавати не можна");

        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("ID товару не може бути порожнім", nameof(productId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва товару не може бути порожньою", nameof(name));

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Ціна товару не може бути від'ємною");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Кількість у рядку має бути більшою за нуль");

        _lines.Add(new OrderLine(productId.Trim(), name.Trim(), price, quantity));
    }

    public void Confirm() => ChangeStatus(OrderStatus.Confirmed);
    public void Cancel() => ChangeStatus(OrderStatus.Cancelled);

    public void ChangeStatus(OrderStatus newStatus)
    {
        Status = (Status, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) when _lines.Count > 0 => newStatus,
            (OrderStatus.Draft, OrderStatus.Cancelled) => newStatus,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => newStatus,
            (OrderStatus.Draft, OrderStatus.Confirmed) =>
                throw new InvalidOperationException($"Неможливо підтвердити порожнє замовлення {Id}"),
            _ => throw new InvalidOperationException($"Недопустимий перехід статусу з {Status} у {newStatus}.")
        };
    }

    public OrderDto ToDto()
    {
        var lineDtos = _lines
            .Select(line => new OrderLineDto(line.ProductId, line.Name, line.Price, line.Quantity))
            .ToList();

        return new OrderDto(Id, CustomerId, Status.ToString(), lineDtos);
    }

    public static Order FromDto(OrderDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.Lines is null)
            throw new ArgumentException("Список рядків замовлення відсутній", nameof(dto));
        if (!Enum.TryParse<OrderStatus>(dto.Status, ignoreCase: true, out var status) ||
            !Enum.IsDefined(status))
            throw new ArgumentException($"Невідомий статус замовлення: {dto.Status}", nameof(dto));

        var order = Create(dto.Id, dto.CustomerId);
        foreach (var line in dto.Lines)
        {
            if (line is null)
                throw new ArgumentException("Рядок замовлення не може бути null", nameof(dto));
            order.AddLine(line.ProductId, line.Name, line.Price, line.Quantity);
        }

        if (status != OrderStatus.Draft)
            order.ChangeStatus(status);
        return order;
    }

    public override string ToString() =>
        $"Замовлення {Id} (Клієнт: {CustomerId}) - Статус: {Status}, Сума: {Total:F2}";
}
