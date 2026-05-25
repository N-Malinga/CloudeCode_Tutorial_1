using FluentValidation;

namespace ProductManagement.Application.Products.Commands.BulkAdjustPrice;

public sealed class BulkAdjustPriceValidator : AbstractValidator<BulkAdjustPriceCommand>
{
    public BulkAdjustPriceValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a defined product category.");

        RuleFor(x => x.Percentage)
            .InclusiveBetween(-90m, 500m).WithMessage("Percentage must be between -90 and 500.");

        RuleFor(x => x.Percentage)
            .NotEqual(0m).WithMessage("Percentage must be non-zero.");
    }
}
