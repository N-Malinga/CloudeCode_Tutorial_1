using FluentAssertions;
using Moq;
using ProductManagement.Application.Products.Commands.BulkAdjustPrice;
using ProductManagement.Application.Tests.TestKit;
using ProductManagement.Domain.Common;
using ProductManagement.Domain.Products;
using ProductManagement.Domain.Products.Events;

namespace ProductManagement.Application.Tests.Products.Commands;

public class BulkAdjustPriceHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ActiveProductsInCategory_AdjustsAllAndSavesOnce()
    {
        var p1 = SampleProduct(price: 100m);
        var p2 = SampleProduct(price: 50m);
        p1.ClearDomainEvents();
        p2.ClearDomainEvents();
        var products = new List<Product> { p1, p2 };

        RepositoryMock
            .Setup(r => r.GetByCategoryAsync(ProductCategory.Electronics, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        ExpectSaveChanges();

        var handler = new BulkAdjustPriceHandler(Repository, UnitOfWork);
        var command = new BulkAdjustPriceCommand(ProductCategory.Electronics, -10m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.AdjustedCount.Should().Be(2);
        result.Category.Should().Be("Electronics");
        result.Percentage.Should().Be(-10m);
        p1.Price.Amount.Should().Be(90m);
        p2.Price.Amount.Should().Be(45m);
        p1.DomainEvents.Should().ContainSingle(e => e is ProductPriceChangedEvent);
        UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoProductsInCategory_ReturnsZeroAndDoesNotSave()
    {
        RepositoryMock
            .Setup(r => r.GetByCategoryAsync(ProductCategory.Books, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        var handler = new BulkAdjustPriceHandler(Repository, UnitOfWork);
        var command = new BulkAdjustPriceCommand(ProductCategory.Books, 5m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.AdjustedCount.Should().Be(0);
        result.Category.Should().Be("Books");
        UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProductWouldDropToZeroOrBelow_ThrowsAndDoesNotSave()
    {
        var ok = SampleProduct(price: 100m);
        var tooCheap = SampleProduct(price: 0.04m);
        var products = new List<Product> { ok, tooCheap };

        RepositoryMock
            .Setup(r => r.GetByCategoryAsync(ProductCategory.Electronics, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var handler = new BulkAdjustPriceHandler(Repository, UnitOfWork);
        var command = new BulkAdjustPriceCommand(ProductCategory.Electronics, -90m);

        await FluentActions
            .Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<DomainException>();

        UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
