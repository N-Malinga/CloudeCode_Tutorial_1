using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Common.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    void Remove(Product product);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> QueryPagedAsync(
        ProductCategory? category,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
