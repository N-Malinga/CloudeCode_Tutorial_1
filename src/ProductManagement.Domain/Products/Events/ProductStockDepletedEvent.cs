using ProductManagement.Domain.Common;

namespace ProductManagement.Domain.Products.Events;

public sealed record ProductStockDepletedEvent(
    Guid ProductId,
    DateTime OccurredOnUtc) : IDomainEvent;
