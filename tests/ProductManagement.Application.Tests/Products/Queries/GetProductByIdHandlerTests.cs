using FluentAssertions;
using Moq;
using ProductManagement.Application.Common.Exceptions;
using ProductManagement.Application.Products.Queries.GetProductById;
using ProductManagement.Application.Tests.TestKit;
using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Tests.Products.Queries;

public class GetProductByIdHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ExistingProduct_ReturnsDto()
    {
        var product = SampleProduct();
        RepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new GetProductByIdHandler(Repository);
        var dto = await handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

        dto.Id.Should().Be(product.Id);
        dto.Name.Should().Be(product.Name);
        dto.Price.Amount.Should().Be(product.Price.Amount);
    }

    [Fact]
    public async Task Handle_MissingProduct_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        RepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new GetProductByIdHandler(Repository);

        await FluentActions
            .Awaiting(() => handler.Handle(new GetProductByIdQuery(id), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
