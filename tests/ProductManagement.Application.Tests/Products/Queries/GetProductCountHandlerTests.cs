using FluentAssertions;
using Moq;
using ProductManagement.Application.Products.Queries.GetProductCount;
using ProductManagement.Application.Tests.TestKit;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Tests.Products.Queries;

public class GetProductCountHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_NoFilters_ReturnsTotalCount()
    {
        RepositoryMock
            .Setup(r => r.CountAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);

        var handler = new GetProductCountHandler(Repository);
        var result = await handler.Handle(new GetProductCountQuery(), CancellationToken.None);

        result.Count.Should().Be(7);
    }

    [Fact]
    public async Task Handle_WithFilters_PassesFiltersToRepository()
    {
        RepositoryMock
            .Setup(r => r.CountAsync(ProductCategory.Electronics, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var handler = new GetProductCountHandler(Repository);
        var result = await handler.Handle(
            new GetProductCountQuery(ProductCategory.Electronics, true),
            CancellationToken.None);

        result.Count.Should().Be(3);
        RepositoryMock.Verify(
            r => r.CountAsync(ProductCategory.Electronics, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
