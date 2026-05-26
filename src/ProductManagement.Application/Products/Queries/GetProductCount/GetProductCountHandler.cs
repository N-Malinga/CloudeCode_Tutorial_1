using MediatR;
using ProductManagement.Application.Common.Interfaces;

namespace ProductManagement.Application.Products.Queries.GetProductCount;

public sealed class GetProductCountHandler(IProductRepository repository)
    : IRequestHandler<GetProductCountQuery, GetProductCountResult>
{
    public async Task<GetProductCountResult> Handle(GetProductCountQuery request, CancellationToken cancellationToken)
    {
        var count = await repository.CountAsync(request.Category, request.IsActive, cancellationToken);
        return new GetProductCountResult(count);
    }
}
