using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Order> Orders()
    {
        var result = new List<Order>();
        string runId = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        for (int n = 1; n <= 16; n++)
        {
            var order = Order.Create($"SEED-{runId}-{n:000}", $"CLIENT-{(n - 1) % 6 + 1:000}");
            order.AddLine($"PRD-{n:000}", $"Демонстраційний товар {n}", 100m + n * 25m, n % 3 + 1);
            if (n % 4 == 0) order.Confirm();
            result.Add(order);
        }
        return result.AsReadOnly();
    }
}
