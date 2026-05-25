using MediatR;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products.Commands.BulkAdjustPrice;

public sealed record BulkAdjustPriceCommand(ProductCategory Category, decimal Percentage)
    : IRequest<BulkAdjustPriceResult>;
