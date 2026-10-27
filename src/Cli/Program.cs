using Core;
using Core.Abstractions;
using Core.Storage;
using Core.Services;
using Core.Domain;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

if (args.Contains("--lab05-extra"))
{
    RunLab05Extras(args);
    return 0;
}
if (args.Contains("--lab05"))
{
    RunLab05(args);
    return 0;
}


if (args.Length > 0 && args[0] == "--lab04")
{
    RunLab04();
    return 0;
}

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");
if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

ImportResult<ProductDto> result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    var ext => new ImportResult<ProductDto>([], [$"Формат файлу '{ext}' не підтримується"])
};

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
    Console.WriteLine($" {p.Id,-10} {p.Name,-35} {p.Price,10:F2}");

if (result.Errors.Count > 0)
{
    Console.WriteLine($"\nПропущено рядків: {result.Errors.Count}");
    foreach (string error in result.Errors)
        Console.WriteLine($" ! {error}");
}

int accepted = result.Items.Count;
int skipped = result.Errors.Count;
int total = accepted + skipped;
double errorPercent = total > 0 ? (double)skipped / total * 100 : 0;
Console.WriteLine($"\nСтатистика: усього {total} / прийнято {accepted} / " +
                  $"пропущено {skipped} / {errorPercent:F1}% помилок");
return 0;

static void RunLab04()
{
    Console.WriteLine("=== Сценарій 1: успішні операції ===");
    Order order = Order.Create("ORD-001", "CLIENT-001");
    order.AddLine("PRD-1", "Ноутбук", 25000m, 1);
    order.AddLine("PRD-2", "Миша", 800m, 2);
    Console.WriteLine(order);

    order.Confirm();
    Console.WriteLine(order);

    Console.WriteLine("\n=== Сценарій 2: відмови при порушенні інваріантів ===");
    TryDo("додавання рядка після підтвердження",
        () => order.AddLine("PRD-3", "Клавіатура", 1500m, 1));
    TryDo("порожній ідентифікатор клієнта",
        () => Order.Create("ORD-002", "   "));
    TryDo("від’ємна кількість",
        () => Order.Create("ORD-003", "CLIENT-003")
            .AddLine("PRD-4", "Монітор", 5000m, -5));
    TryDo("підтвердження порожнього замовлення",
        () => Order.Create("ORD-004", "CLIENT-004")
            .Confirm());
    TryDo("повернення підтвердженого замовлення в чернетку",
        () => order.ChangeStatus(OrderStatus.Draft));

    Console.WriteLine($"Стан після відмов: {order.Status}; " +
        $"позицій: {order.Lines.Count}; сума: {order.Total:F2}");

    Console.WriteLine("\n=== Сценарій 3: перетворення DTO ===");
    OrderDto dto = order.ToDto();
    Order restored = Order.FromDto(dto);
    Console.WriteLine($"Відновлено: {restored}");

    TryDo("відновлення DTO з нульовою кількістю", () =>
        Order.FromDto(new OrderDto("ORD-BAD", "CLIENT-005", "Draft",
            [new OrderLineDto("PRD-X", "Товар", 100m, 0)])));

    Order cancelled = Order.Create("ORD-005", "CLIENT-006");
    cancelled.Cancel();
    Console.WriteLine($"Статус скасованого після DTO: {Order.FromDto(cancelled.ToDto()).Status}");
}

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"[ПОМИЛКА ПЕРЕВІРКИ] {title}: виняток не виник");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"[OK] {title}: {ex.GetType().Name} — {ex.Message}");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"[OK] {title}: {ex.GetType().Name} — {ex.Message}");
    }
}
static void RunLab05(string[] arguments)
{
    string path = Path.Combine(ProjectRoot(), "data", "orders.json");
    bool useFile = arguments.Contains("--file");

    IOrderStore store = arguments.Contains("--factory")
        ? StoreFactory.Create(arguments, path)
        : useFile
            ? new FileOrderStore(path, SampleData.Orders())
            : new InMemoryOrderStore(SampleData.Orders());
    if (arguments.Contains("--cache") && !arguments.Contains("--factory"))
        store = new CachingOrderStore(store);

    var service = new OrderService(store);
    Console.WriteLine("=== Лабораторна 5: сервіс і два сховища ===");
    Console.WriteLine($"Реалізація: {store.GetType().Name}");
    Console.WriteLine($"Режим: {(useFile ? "Файл JSON" : "Пам'ять")}");
    if (useFile) Console.WriteLine($"Шлях: {path}");

    int before = service.All().Count;
    Console.WriteLine($"Замовлень на початку: {before}");
    Console.WriteLine("Початкові приклади:");
    foreach (var item in service.All().Take(3))
        Console.WriteLine($" {item.Id,-14} {item.CustomerId,-12} {item.Status,-10} {item.Total,10:F2}");

    Order created = service.CreateOrder("CLIENT-LAB5");
    service.AddLine(created.Id, "PRD-101", "Ноутбук", 25000m, 1);
    service.AddLine(created.Id, "PRD-102", "Миша", 800m, 2);
    service.Confirm(created.Id);
    Order saved = service.Find(created.Id)!;
    Console.WriteLine($"Створено, оновлено та знайдено за ID: {saved.Id}; {saved.Status}; {saved.Total:F2}");
    Console.WriteLine($"Замовлень наприкінці: {service.All().Count}");

    Console.WriteLine("\n=== Сценарії відмови ===");
    TryDo("дублікат ID", () => store.Add(Order.Create(saved.Id, "CLIENT-X")));
    TryDo("неіснуючий ID", () => service.Confirm("UNKNOWN-ORDER"));
    TryDo("зміна підтвердженого", () => service.AddLine(saved.Id, "PRD-103", "Кабель", 50m, 1));
    Console.WriteLine($"Після відмов: {service.Find(saved.Id)!.Status}; {service.Find(saved.Id)!.Total:F2}");
    if (store is CachingOrderStore cache)
        Console.WriteLine($"Кеш: hits={cache.Hits}; misses={cache.Misses}; invalidations={cache.Invalidations}");
}

static void RunLab05Extras(string[] arguments)
{
    Console.WriteLine("=== Додаткове 1: декоратор CachingOrderStore ===");
    var cache = new CachingOrderStore(new InMemoryOrderStore(SampleData.Orders()));
    var cachedService = new OrderService(cache);
    Console.WriteLine($"Перше читання: {cachedService.All().Count} (промах кешу)");
    Console.WriteLine($"Друге читання: {cachedService.All().Count} (влучання в кеш)");
    Order added = cachedService.CreateOrder("CLIENT-EXTRA");
    Console.WriteLine($"Після створення: {cachedService.All().Count} (кеш оновлено)");
    Console.WriteLine($"Hits={cache.Hits}, Misses={cache.Misses}, Invalidations={cache.Invalidations}");

    Console.WriteLine("\n=== Додаткове 2: пошук з Func<Order,bool> ===");
    IReadOnlyList<Order> confirmed = cachedService.Search(o => o.Status == OrderStatus.Confirmed);
    IReadOnlyList<Order> expensive = cachedService.Search(o => o.Total > 500m);
    Console.WriteLine($"Підтверджених замовлень: {confirmed.Count}");
    Console.WriteLine($"Замовлень з Total > 500: {expensive.Count}");
    foreach (var item in confirmed.Take(3))
        Console.WriteLine($" {item.Id,-12} {item.Status,-10} {item.Total:F2}");

    Console.WriteLine("\n=== Додаткове 3: StoreFactory.Create(args) ===");
    string path = Path.Combine(ProjectRoot(), "data", "orders.json");
    IOrderStore factoryStore = StoreFactory.Create(arguments, path);
    Console.WriteLine($"Фабрика повернула: {factoryStore.GetType().Name}");
    Console.WriteLine($"Знайдено через фабрику: {new OrderService(factoryStore).All().Count}");
    Console.WriteLine("Порівняння: у звичайному --lab05 класи обираються в Program.cs; з --factory — у StoreFactory.");
}

static string ProjectRoot()
{
    foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        var dir = new DirectoryInfo(start);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "CrossApp.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }
    }
    return Directory.GetCurrentDirectory();
}
