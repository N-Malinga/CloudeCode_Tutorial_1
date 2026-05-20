using MediatR;
using ProductManagement.Application.Common.Models;
using ProductManagement.Application.Products.Dtos;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products.Queries.GetProductsPaged;

public sealed record GetProductsPagedQuery(
    int Page = 1,
    int PageSize = 20,
    ProductCategory? Category = null,
    string? Search = null) : IRequest<PagedResult<ProductDto>>;
