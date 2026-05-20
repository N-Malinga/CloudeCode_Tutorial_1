using FluentAssertions;
using FluentValidation.TestHelper;
using ProductManagement.Application.Products.Commands.AdjustStock;

namespace ProductManagement.Application.Tests.Products.Commands;

public class AdjustStockValidatorTests
{
    private readonly AdjustStockValidator _validator = new();

    private static AdjustStockCommand Valid() => new(Guid.NewGuid(), -3);

    [Fact]
    public void ValidCommand_NoErrors() =>
        _validator.TestValidate(Valid()).IsValid.Should().BeTrue();

    [Fact]
    public void EmptyId_Fails() =>
        _validator.TestValidate(Valid() with { Id = Guid.Empty })
            .ShouldHaveValidationErrorFor(c => c.Id);

    [Fact]
    public void ZeroDelta_Fails() =>
        _validator.TestValidate(Valid() with { Delta = 0 })
            .ShouldHaveValidationErrorFor(c => c.Delta);

    [Theory]
    [InlineData(5)]
    [InlineData(-5)]
    public void NonZeroDelta_NoError(int delta) =>
        _validator.TestValidate(Valid() with { Delta = delta })
            .ShouldNotHaveValidationErrorFor(c => c.Delta);
}
