using FluentAssertions;
using Moq;
using ProductManagement.Application.Products.Queries.GetProductsPaged;
using ProductManagement.Application.Tests.TestKit;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Tests.Products.Queries;

public class GetProductsPagedHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ReturnsPagedResults_ProjectedToDtos()
    {
        var items = new List<Product>
        {
            SampleProduct(name: "Alpha"),
            SampleProduct(name: "Beta"),
        };

        RepositoryMock
            .Setup(r => r.QueryPagedAsync(
                ProductCategory.Electronics,
                "a",
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 2));

        var handler = new GetProductsPagedHandler(Repository);
        var result = await handler.Handle(
            new GetProductsPagedQuery(1, 10, ProductCategory.Electronics, "a"),
            CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("Alpha");
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyPagedResult()
    {
        RepositoryMock
            .Setup(r => r.QueryPagedAsync(
                It.IsAny<ProductCategory?>(),
                It.IsAny<string?>(),
                1,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>(), 0));

        var handler = new GetProductsPagedHandler(Repository);
        var result = await handler.Handle(new GetProductsPagedQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }
}
