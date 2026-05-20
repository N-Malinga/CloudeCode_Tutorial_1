using FluentAssertions;
using FluentValidation.TestHelper;
using ProductManagement.Application.Products.Queries.GetProductById;

namespace ProductManagement.Application.Tests.Products.Queries;

public class GetProductByIdValidatorTests
{
    private readonly GetProductByIdValidator _validator = new();

    [Fact]
    public void ValidId_NoErrors() =>
        _validator.TestValidate(new GetProductByIdQuery(Guid.NewGuid())).IsValid.Should().BeTrue();

    [Fact]
    public void EmptyId_Fails() =>
        _validator.TestValidate(new GetProductByIdQuery(Guid.Empty))
            .ShouldHaveValidationErrorFor(q => q.Id);
}
