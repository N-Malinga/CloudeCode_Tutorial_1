using MediatR;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal PriceAmount,
    string PriceCurrency,
    ProductCategory Category) : IRequest;
