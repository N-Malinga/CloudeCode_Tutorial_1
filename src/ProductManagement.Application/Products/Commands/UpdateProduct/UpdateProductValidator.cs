using FluentValidation;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(Product.NameMinLength, Product.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(Product.DescriptionMaxLength);

        RuleFor(x => x.PriceAmount).GreaterThan(0);
        RuleFor(x => x.PriceCurrency).NotEmpty().Length(3);

        RuleFor(x => x.Category)
            .IsInEnum()
            .NotEqual(ProductCategory.Unspecified);
    }
}
