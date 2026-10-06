using Core.Domain;

Console.WriteLine("=== Сценарій 1: Успіх ===");
Order order = Order.Create("ORD-001", "Група ФЕІ-33с");
order.AddLine("PRD-1", "Ноутбук", 25000m, 1);
order.AddLine("PRD-2", "Миша", 800m, 2);
Console.WriteLine(order);
order.Confirm();
Console.WriteLine(order);

Console.WriteLine("\n=== Сценарій 2: Порушення інваріантів ===");
TryDo("Додавання рядка до підтвердженого замовлення", () => order.AddLine("PRD-3", "Клавіатура", 1500m, 1));
TryDo("Створення замовлення без клієнта", () => Order.Create("ORD-002", "  "));
TryDo("Від'ємна кількість товару", () =>
{
    var invalidOrder = Order.Create("ORD-003", "Клієнт B");
    invalidOrder.AddLine("PRD-1", "Монітор", 5000m, -5);
});
TryDo("Підтвердження порожнього замовлення", () =>
{
    var emptyOrder = Order.Create("ORD-004", "Клієнт C");
    emptyOrder.Confirm();
});

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" [!] {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" [X] {title}: {ex.GetType().Name} - {ex.Message}");
    }
}