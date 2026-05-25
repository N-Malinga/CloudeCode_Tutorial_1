using MediatR;
using ProductManagement.Application.Common.Interfaces;

namespace ProductManagement.Application.Products.Commands.BulkAdjustPrice;

public sealed class BulkAdjustPriceHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<BulkAdjustPriceCommand, BulkAdjustPriceResult>
{
    public async Task<BulkAdjustPriceResult> Handle(BulkAdjustPriceCommand request, CancellationToken cancellationToken)
    {
        var products = await repository.GetByCategoryAsync(request.Category, cancellationToken);

        foreach (var product in products)
            product.AdjustPrice(request.Percentage);

        if (products.Count > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return new BulkAdjustPriceResult(products.Count, request.Category.ToString(), request.Percentage);
    }
}
