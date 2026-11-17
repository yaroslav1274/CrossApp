using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class FileSink : INotificationSink
{
    private readonly string _path;

    public FileSink(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = Path.GetFullPath(path);
    }

    public void Notify(OrderChangedEventArgs change)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        string line = $"{change.At:yyyy-MM-dd HH:mm:ss zzz} {change.Kind,-14} " +
                      $"{change.OrderId} lines={change.LinesCount} total={change.Total:F2}";
        File.AppendAllText(_path, line + Environment.NewLine);
    }
}
