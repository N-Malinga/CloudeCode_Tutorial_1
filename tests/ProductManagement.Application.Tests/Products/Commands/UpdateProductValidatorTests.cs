using FluentAssertions;
using FluentValidation.TestHelper;
using ProductManagement.Application.Products.Commands.UpdateProduct;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Tests.Products.Commands;

public class UpdateProductValidatorTests
{
    private readonly UpdateProductValidator _validator = new();

    private static UpdateProductCommand Valid() =>
        new(Guid.NewGuid(), "Widget", "desc", 9.99m, "USD", ProductCategory.Electronics);

    [Fact]
    public void ValidCommand_NoErrors() =>
        _validator.TestValidate(Valid()).IsValid.Should().BeTrue();

    [Fact]
    public void EmptyId_Fails() =>
        _validator.TestValidate(Valid() with { Id = Guid.Empty })
            .ShouldHaveValidationErrorFor(c => c.Id);

    [Fact]
    public void ShortName_Fails() =>
        _validator.TestValidate(Valid() with { Name = "ab" })
            .ShouldHaveValidationErrorFor(c => c.Name);

    [Fact]
    public void NonPositivePrice_Fails() =>
        _validator.TestValidate(Valid() with { PriceAmount = 0 })
            .ShouldHaveValidationErrorFor(c => c.PriceAmount);

    [Fact]
    public void BadCurrency_Fails() =>
        _validator.TestValidate(Valid() with { PriceCurrency = "USDD" })
            .ShouldHaveValidationErrorFor(c => c.PriceCurrency);

    [Fact]
    public void UnspecifiedCategory_Fails() =>
        _validator.TestValidate(Valid() with { Category = ProductCategory.Unspecified })
            .ShouldHaveValidationErrorFor(c => c.Category);
}
