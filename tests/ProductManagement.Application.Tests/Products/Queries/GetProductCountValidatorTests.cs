using FluentAssertions;
using FluentValidation.TestHelper;
using ProductManagement.Application.Products.Queries.GetProductCount;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Tests.Products.Queries;

public class GetProductCountValidatorTests
{
    private readonly GetProductCountValidator _validator = new();

    [Fact]
    public void DefaultsAreValid() =>
        _validator.TestValidate(new GetProductCountQuery()).IsValid.Should().BeTrue();

    [Fact]
    public void ValidCategory_NoError() =>
        _validator.TestValidate(new GetProductCountQuery(Category: ProductCategory.Electronics))
            .IsValid.Should().BeTrue();

    [Fact]
    public void InvalidCategoryValue_Fails() =>
        _validator.TestValidate(new GetProductCountQuery(Category: (ProductCategory)999))
            .ShouldHaveValidationErrorFor("Category.Value");
}
