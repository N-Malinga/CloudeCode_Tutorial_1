using ProductManagement.Domain.Common;

namespace ProductManagement.Domain.Products.Events;

public sealed record ProductPriceChangedEvent(
    Guid ProductId,
    decimal OldAmount,
    decimal NewAmount,
    string Currency,
    DateTime OccurredOnUtc) : IDomainEvent;
