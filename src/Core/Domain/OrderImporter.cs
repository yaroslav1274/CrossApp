using Core.Dto;

namespace Core.Domain;

public record DomainImportResult<T>(IReadOnlyList<T> Entities, IReadOnlyList<string> Errors);

public static class OrderImporter
{
    public static DomainImportResult<Order> ProcessImport(ImportResult<OrderDto> importResult)
    {
        List<Order> entities = [];
        List<string> errors = [.. importResult.Errors];

        foreach (var dto in importResult.Items)
        {
            try
            {
                var order = Order.FromDto(dto);
                entities.Add(order);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                errors.Add($"Помилка створення замовлення {dto.Id}: {ex.Message}");
            }
        }

        return new DomainImportResult<Order>(entities.AsReadOnly(), errors.AsReadOnly());
    }
}