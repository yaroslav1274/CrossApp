using Core.Domain;

namespace Core;

// Демонстраційна вибірка з 16 замовлень для обох сховищ.
public static class SampleData
{
    public static IReadOnlyList<Order> Orders()
    {
        var result = new List<Order>();
        for (int n = 1; n <= 16; n++)
        {
            var order = Order.Create($"SEED-{n:000}", $"CLIENT-{(n - 1) % 6 + 1:000}");
            order.AddLine($"PRD-{n:000}", $"Демонстраційний товар {n}", 100m + n * 25m, n % 3 + 1);
            if (n % 4 == 0) order.Confirm();
            result.Add(order);
        }
        return result.AsReadOnly();
    }
}
