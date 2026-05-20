using ProductManagement.Domain.Common;
using ProductManagement.Domain.Products.Events;

namespace ProductManagement.Domain.Products;

public sealed class Product : Entity
{
    public const int NameMinLength = 3;
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Money Price { get; private set; } = default!;
    public int StockQuantity { get; private set; }
    public ProductCategory Category { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Product() { }

    private Product(
        Guid id,
        string name,
        string description,
        Money price,
        int stockQuantity,
        ProductCategory category,
        DateTime createdAtUtc)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        Category = category;
        CreatedAt = createdAtUtc;
        UpdatedAt = createdAtUtc;
        IsActive = true;
    }

    public static Product Create(
        string name,
        string description,
        Money price,
        int stockQuantity,
        ProductCategory category,
        DateTime? createdAtUtc = null)
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidateStock(stockQuantity);
        ArgumentNullException.ThrowIfNull(price);

        var timestamp = createdAtUtc ?? DateTime.UtcNow;
        var product = new Product(Guid.NewGuid(), name.Trim(), description ?? string.Empty, price, stockQuantity, category, timestamp);

        product.Raise(new ProductCreatedEvent(
            product.Id,
            product.Name,
            product.Price.Amount,
            product.Price.Currency,
            product.StockQuantity,
            product.Category,
            timestamp));

        return product;
    }

    public void Update(string name, string description, ProductCategory category, DateTime? updatedAtUtc = null)
    {
        ValidateName(name);
        ValidateDescription(description);

        Name = name.Trim();
        Description = description ?? string.Empty;
        Category = category;
        UpdatedAt = updatedAtUtc ?? DateTime.UtcNow;
    }

    public void ChangePrice(Money newPrice, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(newPrice);
        if (newPrice.Currency != Price.Currency)
            throw new DomainException("Cannot change price currency on an existing product.");

        if (newPrice == Price)
            return;

        var timestamp = updatedAtUtc ?? DateTime.UtcNow;
        var oldAmount = Price.Amount;
        Price = newPrice;
        UpdatedAt = timestamp;

        Raise(new ProductPriceChangedEvent(Id, oldAmount, newPrice.Amount, newPrice.Currency, timestamp));
    }

    public void DepleteStock(int quantity, DateTime? updatedAtUtc = null)
    {
        if (quantity <= 0)
            throw new DomainException("Stock change quantity must be positive.");
        if (StockQuantity - quantity < 0)
            throw new DomainException("Stock cannot go negative.");

        var timestamp = updatedAtUtc ?? DateTime.UtcNow;
        StockQuantity -= quantity;
        UpdatedAt = timestamp;

        if (StockQuantity == 0)
            Raise(new ProductStockDepletedEvent(Id, timestamp));
    }

    public void ReplenishStock(int quantity, DateTime? updatedAtUtc = null)
    {
        if (quantity <= 0)
            throw new DomainException("Replenishment quantity must be positive.");
        StockQuantity += quantity;
        UpdatedAt = updatedAtUtc ?? DateTime.UtcNow;
    }

    public void AdjustStock(int delta, DateTime? updatedAtUtc = null)
    {
        if (delta == 0)
            throw new DomainException("Stock adjustment delta must be non-zero.");
        if (StockQuantity + delta < 0)
            throw new DomainException("Stock cannot go negative.");

        var timestamp = updatedAtUtc ?? DateTime.UtcNow;
        StockQuantity += delta;
        UpdatedAt = timestamp;

        if (StockQuantity == 0)
            Raise(new ProductStockDepletedEvent(Id, timestamp));
    }

    public void Deactivate(DateTime? updatedAtUtc = null)
    {
        IsActive = false;
        UpdatedAt = updatedAtUtc ?? DateTime.UtcNow;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");
        var trimmed = name.Trim();
        if (trimmed.Length < NameMinLength || trimmed.Length > NameMaxLength)
            throw new DomainException($"Product name must be between {NameMinLength} and {NameMaxLength} characters.");
    }

    private static void ValidateDescription(string description)
    {
        if (description is not null && description.Length > DescriptionMaxLength)
            throw new DomainException($"Description cannot exceed {DescriptionMaxLength} characters.");
    }

    private static void ValidateStock(int stockQuantity)
    {
        if (stockQuantity < 0)
            throw new DomainException("Stock quantity cannot be negative.");
    }
}
