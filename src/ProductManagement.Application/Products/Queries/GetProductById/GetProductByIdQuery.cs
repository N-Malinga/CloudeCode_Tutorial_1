using MediatR;
using ProductManagement.Application.Products.Dtos;

namespace ProductManagement.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;
