using FluentAssertions;
using Moq;
using ProductManagement.Application.Products.Commands.CreateProduct;
using ProductManagement.Application.Tests.TestKit;
using ProductManagement.Domain.Common;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Tests.Products.Commands;

public class CreateProductHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ValidCommand_PersistsAndReturnsId()
    {
        RepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        ExpectSaveChanges();

        var handler = new CreateProductHandler(Repository, UnitOfWork);
        var command = new CreateProductCommand("Widget", "desc", 9.99m, "USD", 5, ProductCategory.Electronics);

        var id = await handler.Handle(command, CancellationToken.None);

        id.Should().NotBe(Guid.Empty);
        RepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidPrice_ThrowsDomainException()
    {
        var handler = new CreateProductHandler(Repository, UnitOfWork);
        var command = new CreateProductCommand("Widget", "desc", -1m, "USD", 5, ProductCategory.Electronics);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        RepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
