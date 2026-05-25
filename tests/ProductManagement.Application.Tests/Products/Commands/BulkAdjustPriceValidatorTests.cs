using FluentAssertions;
using FluentValidation.TestHelper;
using ProductManagement.Application.Products.Commands.BulkAdjustPrice;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Tests.Products.Commands;

public class BulkAdjustPriceValidatorTests
{
    private readonly BulkAdjustPriceValidator _validator = new();

    private static BulkAdjustPriceCommand Valid() => new(ProductCategory.Electronics, -10m);

    [Fact]
    public void ValidCommand_NoErrors() =>
        _validator.TestValidate(Valid()).IsValid.Should().BeTrue();

    [Fact]
    public void UndefinedCategory_Fails() =>
        _validator.TestValidate(Valid() with { Category = (ProductCategory)999 })
            .ShouldHaveValidationErrorFor(c => c.Category);

    [Fact]
    public void DefinedCategory_NoError() =>
        _validator.TestValidate(Valid() with { Category = ProductCategory.Books })
            .ShouldNotHaveValidationErrorFor(c => c.Category);

    [Theory]
    [InlineData(-91)]
    [InlineData(501)]
    public void PercentageOutOfRange_Fails(int percentage) =>
        _validator.TestValidate(Valid() with { Percentage = percentage })
            .ShouldHaveValidationErrorFor(c => c.Percentage);

    [Theory]
    [InlineData(-90)]
    [InlineData(500)]
    [InlineData(-10)]
    [InlineData(5)]
    public void PercentageInRange_NoError(int percentage) =>
        _validator.TestValidate(Valid() with { Percentage = percentage })
            .ShouldNotHaveValidationErrorFor(c => c.Percentage);

    [Fact]
    public void ZeroPercentage_Fails() =>
        _validator.TestValidate(Valid() with { Percentage = 0m })
            .ShouldHaveValidationErrorFor(c => c.Percentage);
}
