using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(OrderChangedEventArgs change) =>
        Console.WriteLine($"[ConsoleSink] {change.Kind}: {change.OrderId}, " +
                          $"позицій: {change.LinesCount}, сума: {change.Total:F2}");
}
