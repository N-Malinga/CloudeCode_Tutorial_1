using FluentAssertions;
using Moq;
using ProductManagement.Application.Common.Exceptions;
using ProductManagement.Application.Products.Commands.AdjustStock;
using ProductManagement.Application.Tests.TestKit;
using ProductManagement.Domain.Products;
using ProductManagement.Domain.Products.Events;

namespace ProductManagement.Application.Tests.Products.Commands;

public class AdjustStockHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ExistingProduct_AdjustsAndSaves()
    {
        var product = SampleProduct(stock: 10);
        RepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        ExpectSaveChanges();

        var handler = new AdjustStockHandler(Repository, UnitOfWork);
        var command = new AdjustStockCommand(product.Id, -3);

        await handler.Handle(command, CancellationToken.None);

        product.StockQuantity.Should().Be(7);
        UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeltaBringsStockToZero_AdjustsAndSaves()
    {
        var product = SampleProduct(stock: 4);
        RepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        ExpectSaveChanges();

        var handler = new AdjustStockHandler(Repository, UnitOfWork);
        var command = new AdjustStockCommand(product.Id, -4);

        await handler.Handle(command, CancellationToken.None);

        product.StockQuantity.Should().Be(0);
        product.DomainEvents.Should().ContainSingle(e => e is ProductStockDepletedEvent);
        UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingProduct_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        RepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new AdjustStockHandler(Repository, UnitOfWork);
        var command = new AdjustStockCommand(id, -1);

        await FluentActions
            .Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();

        UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
