# Предметна область: Замовлення

## Назва

**Система управління замовленнями**

## Сутності

1. **Клієнт** — особа, яка оформлює замовлення.
2. **Товар** — товар, який можна придбати.
3. **Замовлення** — оформлена покупка клієнта.
4. **Рядок замовлення** — позиція замовлення, що містить конкретний товар, його кількість та ціну.

## Призначення застосунку

Застосунок призначений для обліку клієнтів, товарів і замовлень, а також для керування складом замовлень та їх окремими позиціями.

## Лабораторна робота 4: доменна модель

- Головна сутність: `Order`; пов'язана сутність: `OrderLine`.
- `Order.Create()` перевіряє непорожні `Id` і `CustomerId`.
- `Order.AddLine()` перевіряє непорожні `ProductId` та `Name`, ціну `>= 0` і кількість `> 0`.
- Додавати позиції можна лише до замовлення у стані `Draft`.
- Підтверджувати порожнє замовлення заборонено.
- `Confirm()` і `Cancel()` викликають `ChangeStatus()` — центральну перевірку переходів.
- Дозволені переходи статусів: `Draft -> Confirmed`, `Draft -> Cancelled`, `Confirmed -> Cancelled`.
- Зворотні переходи або повторне підтвердження заборонені.
- `Order.Lines` — лише для читання (`AsReadOnly`); `Status` має `private set`.
- `ToDto()` зберігає статус і рядки, а `FromDto()` повторно перевіряє дані через `Create`, `AddLine` і `ChangeStatus`.

### Запуск

Із кореня проєкту за наявності .NET 8 SDK:

```bash
dotnet run --project src/Cli/Cli.csproj --framework net8.0 -- --lab04
```

Для демонстрації імпорту лабораторної 3 (також із кореня проєкту):

```bash
dotnet run --project src/Cli/Cli.csproj --framework net8.0 -- data/sample.csv
```

### Важливо

У вихідному архіві `OrderDto` мав булеве поле `IsConfirmed`. У цій підготовленій
копії воно замінене рядковим полем `Status`, інакше після `ToDto`/`FromDto`
неможливо відновити статус `Cancelled`.


## Лабораторна робота 5: сервісний шар, два сховища, ручний DI

- Інтерфейс `Core.Abstractions.IOrderStore`: `List`, `GetById`, `Add`, `Update`, `Remove`.
- Реалізації: `InMemoryOrderStore` (словник) та `FileOrderStore` (JSON).
- `Core.Services.OrderService` отримує сховище як `IOrderStore` у конструкторі, викликає методи сутності `Order`.
- `Core.SampleData.Orders()` генерує 16 валідних демонстраційних замовлень.
- `src/Cli/Program.cs` — composition root для ручного вибору без DI-контейнера.
- Файлове сховище пише `OrderDto` до `data/orders.json` і читає через `Order.FromDto()`; 16 початкових записів додаються лише тоді, коли файла ще немає.
- Обидва сховища віддають відокремлені копії замовлень: зміни фіксуються викликом `Update()`.

### Запуск (з кореня проєкту)

```powershell
dotnet build src/Cli/Cli.csproj --framework net8.0
dotnet run --project src/Cli/Cli.csproj --framework net8.0 -- --lab05
dotnet run --project src/Cli/Cli.csproj --framework net8.0 -- --lab05 --file
dotnet run --project src/Cli/Cli.csproj --framework net8.0 -- --lab05-extra
dotnet run --project src/Cli/Cli.csproj --framework net8.0 -- --lab05-extra --file --cache
dotnet run --project src/Cli/Cli.csproj --framework net8.0 -- --lab05 --factory --file
```

Перший запуск без `--file`: 16 → 17 записів, повторний теж 16 → 17.
З `--file`: спочатку 16 → 17, повторно 17 → 18 (якщо JSON-файла ще не було).
Для чистої демонстрації файлу збережіть резервну копію та видаліть **лише** `data/orders.json`.

### Додаткові завдання №5

1. `CachingOrderStore` — декоратор `IOrderStore`, після змін очищує кеш; лічильники `Hits`, `Misses`, `Invalidations`.
2. `OrderService.Search(Func<Order,bool>)` — фільтрація замовлень за довільним предикатом.
3. `Cli.StoreFactory.Create(args, path)` — альтернативний спосіб винесення створення сховища із `Program.cs`.

### Попередні лабораторні

Збережені запуски `--lab04` (ЛР4) та звичайний імпорт CSV/JSON (ЛР3).
Перевірте зміни, зробіть коміт `lab05`; файл `data/orders.json` ігнорується Git.

Технічне очищення для вимоги ЛР5: у `Core/Import/ProductJsonImporter.cs` прибрано зайвий `Console.WriteLine(items)`; сам алгоритм імпорту залишено без змін.
