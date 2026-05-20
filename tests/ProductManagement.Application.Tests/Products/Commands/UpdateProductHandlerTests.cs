using FluentAssertions;
using Moq;
using ProductManagement.Application.Common.Exceptions;
using ProductManagement.Application.Products.Commands.UpdateProduct;
using ProductManagement.Application.Tests.TestKit;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Tests.Products.Commands;

public class UpdateProductHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ExistingProduct_UpdatesAndSaves()
    {
        var product = SampleProduct();
        RepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        ExpectSaveChanges();

        var handler = new UpdateProductHandler(Repository, UnitOfWork);
        var command = new UpdateProductCommand(product.Id, "Gadget", "new desc", 12.50m, "USD", ProductCategory.Apparel);

        await handler.Handle(command, CancellationToken.None);

        product.Name.Should().Be("Gadget");
        product.Price.Amount.Should().Be(12.50m);
        product.Category.Should().Be(ProductCategory.Apparel);
        UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingProduct_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        RepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new UpdateProductHandler(Repository, UnitOfWork);
        var command = new UpdateProductCommand(id, "Gadget", "new desc", 12.50m, "USD", ProductCategory.Apparel);

        await FluentActions
            .Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();

        UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
