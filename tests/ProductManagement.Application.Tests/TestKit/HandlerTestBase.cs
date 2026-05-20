using Moq;
using ProductManagement.Application.Common.Interfaces;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Tests.TestKit;

public abstract class HandlerTestBase
{
    protected Mock<IProductRepository> RepositoryMock { get; } = new(MockBehavior.Strict);
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; } = new(MockBehavior.Strict);

    protected IProductRepository Repository => RepositoryMock.Object;
    protected IUnitOfWork UnitOfWork => UnitOfWorkMock.Object;

    protected static Product SampleProduct(
        string name = "Widget",
        decimal price = 9.99m,
        string currency = "USD",
        int stock = 10,
        ProductCategory category = ProductCategory.Electronics) =>
        Product.Create(name, "desc", Money.Create(price, currency), stock, category);

    protected void ExpectSaveChanges() =>
        UnitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
}
