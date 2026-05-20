using MediatR;
using ProductManagement.Application.Common.Exceptions;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products.Commands.DeleteProduct;

public sealed class DeleteProductHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        repository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
