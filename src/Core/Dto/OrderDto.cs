namespace Core.Dto;

public record OrderLineDto(string ProductId, string Name, decimal Price, int Quantity);
public record OrderDto(string Id, string CustomerId, string Status, List<OrderLineDto> Lines);
