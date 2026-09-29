using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

Console.OutputEncoding = System.Text.Encoding.UTF8;

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
}

ImportResult<ProductDto> result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),

    
    var ext => new ImportResult<ProductDto>([], [$"Формат файлу '{ext}' не підтримується"])
};

Console.WriteLine($"Завантажено записів: {result.Items.Count}");

foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine($" {p.Id,-10} {p.Name,-35} {p.Price,10:F2}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"\nПропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($" ! {e}");
    }
}

int accepted = result.Items.Count;
int skipped = result.Errors.Count;
int total = accepted + skipped;

double errorPercent = total > 0 ? (double)skipped / total * 100 : 0;

Console.WriteLine($"\nСтатистика: усього {total} / прийнято {accepted} / " +
    $"пропущено {skipped} / {errorPercent:F1}% помилок");

return 0;