using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;
    private readonly INotificationSink? _notificationSink;

    public OrderService(IOrderStore store, INotificationSink? notificationSink = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _notificationSink = notificationSink;
    }

    public event EventHandler<OrderChangedEventArgs>? Changed;

    private void OnChanged(Order order, OrderChangeKind kind)
    {
        var change = new OrderChangedEventArgs(order.Id, kind,
                                                order.Lines.Count, order.Total);
        Changed?.Invoke(this, change);
        _notificationSink?.Notify(change);
    }

    public Order CreateOrder(string customerId)
    {
        string id = "ORD-" + Guid.NewGuid().ToString("N")[..8];
        var order = Order.Create(id, customerId);
        _store.Add(order);
        OnChanged(order, OrderChangeKind.OrderCreated);
        return order;
    }

    public void AddLine(string orderId, string productId, string name,
                        decimal price, int quantity)
    {
        var order = Require(orderId);
        order.AddLine(productId, name, price, quantity);
        _store.Update(order);
        OnChanged(order, OrderChangeKind.LineAdded);
    }

    public void Confirm(string orderId)
    {
        var order = Require(orderId);
        order.Confirm();
        _store.Update(order);
        OnChanged(order, OrderChangeKind.OrderConfirmed);
    }

    public void Cancel(string orderId)
    {
        var order = Require(orderId);
        order.Cancel();
        _store.Update(order);
        OnChanged(order, OrderChangeKind.OrderCancelled);
    }

    public IReadOnlyList<Order> All() => _store.List();
    public Order? Find(string id) => _store.GetById(id);

    public bool Delete(string id)
    {
        var order = _store.GetById(id);
        if (order is null || !_store.Remove(id)) return false;
        OnChanged(order, OrderChangeKind.OrderRemoved);
        return true;
    }

    public IReadOnlyList<Order> FindAll(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList().AsReadOnly();
    }

    public IReadOnlyList<Order> Search(Func<Order, bool> predicate) => FindAll(predicate);

    private Order Require(string id) => _store.GetById(id)
        ?? throw new InvalidOperationException($"Немає замовлення з id={id}.");
}
