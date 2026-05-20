using FluentValidation;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products.Queries.GetProductsPaged;

public sealed class GetProductsPagedValidator : AbstractValidator<GetProductsPagedQuery>
{
    public const int MaxPageSize = 100;
    public const int MaxSearchLength = 200;

    public GetProductsPagedValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);

        RuleFor(x => x.Search!)
            .MaximumLength(MaxSearchLength)
            .When(x => x.Search is not null);

        RuleFor(x => x.Category!.Value)
            .IsInEnum()
            .When(x => x.Category.HasValue);
    }
}
