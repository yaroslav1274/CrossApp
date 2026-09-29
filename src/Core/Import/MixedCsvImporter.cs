using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static MultiImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var warehouses = new List<WarehouseDto>();
        var errors = new List<string>();
        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            switch (ParseLine(line))
            {
                case ParseProductOk pOk:
                    products.Add(pOk.Value);
                    break;
                case ParseWarehouseOk wOk:
                    warehouses.Add(wOk.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new MultiImportResult(products, warehouses, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            ["P", "", _, _] or ["P", _, "", _]
                => new ParseFailed("Товар: Id або назва порожні"),

            ["P", var id, var name, var priceStr] when decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal p) && p >= 0
                => new ParseProductOk(new ProductDto(id, name, p)),

            ["P", ..]
                => new ParseFailed("Товар: очікую 4 колонки (P;Id;Назва;Ціна) або ціна некоректна"),

            ["W", "", _] or ["W", _, ""]
                => new ParseFailed("Склад: Id або назва порожні"),

            ["W", var id, var name]
                => new ParseWarehouseOk(new WarehouseDto(id, name)),

            ["W", ..]
                => new ParseFailed("Склад: очікую рівно 3 колонки (W;Id;Назва)"),

            [var prefix, ..]
                => new ParseFailed($"Невідомий префікс: '{prefix}'"),

            _ => new ParseFailed("Порожній або пошкоджений рядок")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseProductOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseWarehouseOk(WarehouseDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}