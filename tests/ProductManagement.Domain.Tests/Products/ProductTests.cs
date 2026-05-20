using FluentAssertions;
using ProductManagement.Domain.Common;
using ProductManagement.Domain.Products;
using ProductManagement.Domain.Products.Events;

namespace ProductManagement.Domain.Tests.Products;

public class ProductTests
{
    private static Product MakeProduct(int stock = 10, decimal price = 9.99m) =>
        Product.Create(
            name: "Widget",
            description: "A widget",
            price: Money.Create(price, "USD"),
            stockQuantity: stock,
            category: ProductCategory.Electronics);

    [Fact]
    public void Create_ValidInputs_InitializesFieldsAndRaisesCreatedEvent()
    {
        var product = MakeProduct();

        product.Id.Should().NotBe(Guid.Empty);
        product.Name.Should().Be("Widget");
        product.Price.Amount.Should().Be(9.99m);
        product.StockQuantity.Should().Be(10);
        product.IsActive.Should().BeTrue();
        product.CreatedAt.Should().Be(product.UpdatedAt);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreatedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Create_NameTooShort_Throws(string name)
    {
        var act = () => Product.Create(name, "d", Money.Create(1m, "USD"), 1, ProductCategory.Electronics);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_NameTooLong_Throws()
    {
        var longName = new string('x', Product.NameMaxLength + 1);
        var act = () => Product.Create(longName, "d", Money.Create(1m, "USD"), 1, ProductCategory.Electronics);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_NegativeStock_Throws()
    {
        var act = () => Product.Create("Widget", "d", Money.Create(1m, "USD"), -1, ProductCategory.Electronics);
        act.Should().Throw<DomainException>().WithMessage("*Stock*negative*");
    }

    [Fact]
    public void ChangePrice_NewAmount_RaisesPriceChangedEvent()
    {
        var product = MakeProduct(price: 9.99m);
        product.ClearDomainEvents();

        product.ChangePrice(Money.Create(12.50m, "USD"));

        product.Price.Amount.Should().Be(12.50m);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductPriceChangedEvent>()
            .Which.OldAmount.Should().Be(9.99m);
    }

    [Fact]
    public void ChangePrice_SameAmount_NoEvent()
    {
        var product = MakeProduct(price: 9.99m);
        product.ClearDomainEvents();

        product.ChangePrice(Money.Create(9.99m, "USD"));

        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ChangePrice_DifferentCurrency_Throws()
    {
        var product = MakeProduct();
        var act = () => product.ChangePrice(Money.Create(10m, "EUR"));
        act.Should().Throw<DomainException>().WithMessage("*currency*");
    }

    [Fact]
    public void DepleteStock_PartialReduction_NoDepletedEvent()
    {
        var product = MakeProduct(stock: 10);
        product.ClearDomainEvents();

        product.DepleteStock(3);

        product.StockQuantity.Should().Be(7);
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DepleteStock_BringsStockToZero_RaisesDepletedEvent()
    {
        var product = MakeProduct(stock: 5);
        product.ClearDomainEvents();

        product.DepleteStock(5);

        product.StockQuantity.Should().Be(0);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductStockDepletedEvent>();
    }

    [Fact]
    public void DepleteStock_GreaterThanAvailable_Throws()
    {
        var product = MakeProduct(stock: 2);

        var act = () => product.DepleteStock(3);

        act.Should().Throw<DomainException>().WithMessage("*negative*");
        product.StockQuantity.Should().Be(2);
    }

    [Fact]
    public void Update_ChangesNameDescriptionCategory_BumpsUpdatedAt()
    {
        var product = MakeProduct();
        var originalUpdated = product.UpdatedAt;
        var later = originalUpdated.AddMinutes(5);

        product.Update("Gadget", "new desc", ProductCategory.Apparel, later);

        product.Name.Should().Be("Gadget");
        product.Description.Should().Be("new desc");
        product.Category.Should().Be(ProductCategory.Apparel);
        product.UpdatedAt.Should().Be(later);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var product = MakeProduct();
        product.Deactivate();
        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void AdjustStock_PositiveDelta_IncreasesStock_NoEvent()
    {
        var product = MakeProduct(stock: 10);
        product.ClearDomainEvents();
        var originalUpdated = product.UpdatedAt;
        var later = originalUpdated.AddMinutes(1);

        product.AdjustStock(5, later);

        product.StockQuantity.Should().Be(15);
        product.UpdatedAt.Should().Be(later);
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AdjustStock_NegativeDelta_PartialReduction_NoDepletedEvent()
    {
        var product = MakeProduct(stock: 10);
        product.ClearDomainEvents();

        product.AdjustStock(-3);

        product.StockQuantity.Should().Be(7);
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AdjustStock_NegativeDeltaBringsStockToZero_RaisesDepletedEvent()
    {
        var product = MakeProduct(stock: 5);
        product.ClearDomainEvents();

        product.AdjustStock(-5);

        product.StockQuantity.Should().Be(0);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductStockDepletedEvent>();
    }

    [Fact]
    public void AdjustStock_WouldGoNegative_Throws()
    {
        var product = MakeProduct(stock: 2);
        product.ClearDomainEvents();

        var act = () => product.AdjustStock(-3);

        act.Should().Throw<DomainException>().WithMessage("*negative*");
        product.StockQuantity.Should().Be(2);
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AdjustStock_ZeroDelta_Throws()
    {
        var product = MakeProduct(stock: 10);

        var act = () => product.AdjustStock(0);

        act.Should().Throw<DomainException>();
        product.StockQuantity.Should().Be(10);
    }
}
