using ProductManagement.Domain.Common;

namespace ProductManagement.Domain.Products.Events;

public sealed record ProductCreatedEvent(
    Guid ProductId,
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    int StockQuantity,
    ProductCategory Category,
    DateTime OccurredOnUtc) : IDomainEvent;
