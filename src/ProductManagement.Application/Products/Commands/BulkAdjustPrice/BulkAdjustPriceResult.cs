namespace ProductManagement.Application.Products.Commands.BulkAdjustPrice;

public sealed record BulkAdjustPriceResult(int AdjustedCount, string Category, decimal Percentage);
