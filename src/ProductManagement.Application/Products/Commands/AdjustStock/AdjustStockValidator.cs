using FluentValidation;

namespace ProductManagement.Application.Products.Commands.AdjustStock;

public sealed class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Delta).NotEqual(0);
    }
}
