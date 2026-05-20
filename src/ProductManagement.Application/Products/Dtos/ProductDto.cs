using ProductManagement.Domain.Products;

namespace ProductManagement.Application.Products.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    MoneyDto Price,
    int StockQuantity,
    ProductCategory Category,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static ProductDto FromProduct(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        new MoneyDto(product.Price.Amount, product.Price.Currency),
        product.StockQuantity,
        product.Category,
        product.IsActive,
        product.CreatedAt,
        product.UpdatedAt);
}
