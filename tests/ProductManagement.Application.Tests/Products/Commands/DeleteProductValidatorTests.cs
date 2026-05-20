using FluentAssertions;
using FluentValidation.TestHelper;
using ProductManagement.Application.Products.Commands.DeleteProduct;

namespace ProductManagement.Application.Tests.Products.Commands;

public class DeleteProductValidatorTests
{
    private readonly DeleteProductValidator _validator = new();

    [Fact]
    public void ValidCommand_NoErrors() =>
        _validator.TestValidate(new DeleteProductCommand(Guid.NewGuid())).IsValid.Should().BeTrue();

    [Fact]
    public void EmptyId_Fails() =>
        _validator.TestValidate(new DeleteProductCommand(Guid.Empty))
            .ShouldHaveValidationErrorFor(c => c.Id);
}
