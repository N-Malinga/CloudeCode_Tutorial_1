using FluentValidation;

namespace ProductManagement.Application.Products.Queries.GetProductCount;

public sealed class GetProductCountValidator : AbstractValidator<GetProductCountQuery>
{
    public GetProductCountValidator()
    {
        RuleFor(x => x.Category!.Value)
            .IsInEnum()
            .When(x => x.Category.HasValue);
    }
}
