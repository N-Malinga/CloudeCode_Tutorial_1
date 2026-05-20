using Microsoft.EntityFrameworkCore;
using ProductManagement.Domain.Products;

namespace ProductManagement.Infrastructure.Persistence;

public sealed class ProductManagementDbContext(DbContextOptions<ProductManagementDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductManagementDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
