using MediatR;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var price = Money.Create(request.PriceAmount, request.PriceCurrency);
        var product = Product.Create(
            request.Name,
            request.Description,
            price,
            request.StockQuantity,
            request.Category);

        await repository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
}
