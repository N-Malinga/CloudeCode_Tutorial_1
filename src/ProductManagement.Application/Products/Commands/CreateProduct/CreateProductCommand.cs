using MediatR;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal PriceAmount,
    string PriceCurrency,
    int StockQuantity,
    ProductCategory Category) : IRequest<Guid>;
