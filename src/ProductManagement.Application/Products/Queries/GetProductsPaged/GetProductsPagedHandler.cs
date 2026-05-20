using MediatR;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Application.Common.Models;
using ProductManagement.Application.Products.Dtos;

namespace ProductManagement.Application.Products.Queries.GetProductsPaged;

public sealed class GetProductsPagedHandler(IProductRepository repository)
    : IRequestHandler<GetProductsPagedQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await repository.QueryPagedAsync(
            request.Category,
            request.Search,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(ProductDto.FromProduct).ToList();
        return new PagedResult<ProductDto>(dtos, total, request.Page, request.PageSize);
    }
}
