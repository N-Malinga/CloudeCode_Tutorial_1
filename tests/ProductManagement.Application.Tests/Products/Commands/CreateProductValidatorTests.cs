using FluentAssertions;
using FluentValidation.TestHelper;
using ProductManagement.Application.Products.Commands.CreateProduct;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Tests.Products.Commands;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    private static CreateProductCommand Valid() =>
        new("Widget", "desc", 9.99m, "USD", 5, ProductCategory.Electronics);

    [Fact]
    public void ValidCommand_NoErrors()
    {
        _validator.TestValidate(Valid()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void ShortName_Fails(string name)
    {
        var result = _validator.TestValidate(Valid() with { Name = name });
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void LongName_Fails()
    {
        var longName = new string('x', Product.NameMaxLength + 1);
        _validator.TestValidate(Valid() with { Name = longName })
            .ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositivePrice_Fails(decimal price)
    {
        _validator.TestValidate(Valid() with { PriceAmount = price })
            .ShouldHaveValidationErrorFor(c => c.PriceAmount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void BadCurrency_Fails(string currency)
    {
        _validator.TestValidate(Valid() with { PriceCurrency = currency })
            .ShouldHaveValidationErrorFor(c => c.PriceCurrency);
    }

    [Fact]
    public void NegativeStock_Fails()
    {
        _validator.TestValidate(Valid() with { StockQuantity = -1 })
            .ShouldHaveValidationErrorFor(c => c.StockQuantity);
    }

    [Fact]
    public void UnspecifiedCategory_Fails()
    {
        _validator.TestValidate(Valid() with { Category = ProductCategory.Unspecified })
            .ShouldHaveValidationErrorFor(c => c.Category);
    }
}
