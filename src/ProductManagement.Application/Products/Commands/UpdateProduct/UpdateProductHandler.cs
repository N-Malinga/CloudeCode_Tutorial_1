using MediatR;
using ProductManagement.Application.Common.Exceptions;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        product.Update(request.Name, request.Description, request.Category);
        product.ChangePrice(Money.Create(request.PriceAmount, request.PriceCurrency));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
