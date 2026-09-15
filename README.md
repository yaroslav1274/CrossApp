# CrossApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область: **Замовлення**. Сутності: **Customer (клієнт), Product (товар), Order (замовлення), OrderLine (рядок замовлення)**.

Призначення: **оформлення замовлень і підрахунок сум**.

## Структура рішення
CrossApp/
├── CrossApp.sln
└── src/
    ├── Core/
    │   └── Core.csproj (Бібліотека класів, відокремлена логіка)
    └── Cli/
        └── Cli.csproj (Консольний застосунок, точка входу)

## Команди
Збірка проєкту:
dotnet build

Запуск консольного застосунку:
dotnet run --project src/Cli

Публікація (framework-dependent):
dotnet publish src/Cli -c Release -r win-x64 --self-contained false

Публікація (self-contained):
dotnet publish src/Cli -c Release -r win-x64 --self-contained true

## Порівняння режимів публікації
| RID | Режим | Розмір | Чи потрібен встановлений .NET Runtime |
| :--- | :--- | :--- | :--- |
| `win-x64` | Framework-dependent | ~ 0.2 МБ | Так |
| `win-x64` | Self-contained | ~ 70 МБ | Ні |

## Середовище
.NET SDK 10.0, Windows 11 x64
