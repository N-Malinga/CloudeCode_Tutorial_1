using FluentAssertions;
using ProductManagement.Domain.Common;
using ProductManagement.Domain.Products;

namespace ProductManagement.Domain.Tests.Products;

public class MoneyTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsMoney()
    {
        var money = Money.Create(9.99m, "usd");

        money.Amount.Should().Be(9.99m);
        money.Currency.Should().Be("USD");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_NonPositiveAmount_Throws(decimal amount)
    {
        var act = () => Money.Create(amount, "USD");
        act.Should().Throw<DomainException>().WithMessage("*greater than zero*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankCurrency_Throws(string currency)
    {
        var act = () => Money.Create(1m, currency);
        act.Should().Throw<DomainException>().WithMessage("*Currency is required*");
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Create_NonThreeLetterCurrency_Throws(string currency)
    {
        var act = () => Money.Create(1m, currency);
        act.Should().Throw<DomainException>().WithMessage("*3-letter*");
    }

    [Fact]
    public void Equality_SameComponents_AreEqual()
    {
        var a = Money.Create(5m, "USD");
        var b = Money.Create(5m, "usd");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentAmount_AreNotEqual()
    {
        var a = Money.Create(5m, "USD");
        var b = Money.Create(6m, "USD");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }
}
