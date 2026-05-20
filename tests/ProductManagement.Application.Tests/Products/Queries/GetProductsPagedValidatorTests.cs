using FluentAssertions;
using FluentValidation.TestHelper;
using ProductManagement.Application.Products.Queries.GetProductsPaged;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Tests.Products.Queries;

public class GetProductsPagedValidatorTests
{
    private readonly GetProductsPagedValidator _validator = new();

    [Fact]
    public void DefaultsAreValid() =>
        _validator.TestValidate(new GetProductsPagedQuery()).IsValid.Should().BeTrue();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositivePage_Fails(int page) =>
        _validator.TestValidate(new GetProductsPagedQuery(Page: page))
            .ShouldHaveValidationErrorFor(q => q.Page);

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void PageSizeOutOfRange_Fails(int pageSize) =>
        _validator.TestValidate(new GetProductsPagedQuery(PageSize: pageSize))
            .ShouldHaveValidationErrorFor(q => q.PageSize);

    [Fact]
    public void LongSearch_Fails()
    {
        var longSearch = new string('x', GetProductsPagedValidator.MaxSearchLength + 1);
        _validator.TestValidate(new GetProductsPagedQuery(Search: longSearch))
            .ShouldHaveValidationErrorFor(q => q.Search);
    }

    [Fact]
    public void NullSearch_NoError() =>
        _validator.TestValidate(new GetProductsPagedQuery(Search: null)).IsValid.Should().BeTrue();

    [Fact]
    public void InvalidCategoryValue_Fails() =>
        _validator.TestValidate(new GetProductsPagedQuery(Category: (ProductCategory)999))
            .ShouldHaveValidationErrorFor("Category.Value");
}
