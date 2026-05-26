using MediatR;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products.Queries.GetProductCount;

public sealed record GetProductCountQuery(
    ProductCategory? Category = null,
    bool? IsActive = null) : IRequest<GetProductCountResult>;
