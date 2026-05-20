using MediatR;
using ProductManagement.Application.Common.Exceptions;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products.Commands.AdjustStock;

public sealed class AdjustStockHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<AdjustStockCommand>
{
    public async Task Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        product.AdjustStock(request.Delta);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
