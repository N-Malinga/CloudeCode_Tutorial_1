using FluentAssertions;
using Moq;
using ProductManagement.Application.Common.Exceptions;
using ProductManagement.Application.Products.Commands.DeleteProduct;
using ProductManagement.Application.Tests.TestKit;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Tests.Products.Commands;

public class DeleteProductHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ExistingProduct_RemovesAndSaves()
    {
        var product = SampleProduct();
        RepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        RepositoryMock.Setup(r => r.Remove(product));
        ExpectSaveChanges();

        var handler = new DeleteProductHandler(Repository, UnitOfWork);
        await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        RepositoryMock.Verify(r => r.Remove(product), Times.Once);
        UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingProduct_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        RepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new DeleteProductHandler(Repository, UnitOfWork);

        await FluentActions
            .Awaiting(() => handler.Handle(new DeleteProductCommand(id), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();

        UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
