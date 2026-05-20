using MediatR;

namespace ProductManagement.Application.Products.Commands.AdjustStock;

public sealed record AdjustStockCommand(Guid Id, int Delta) : IRequest;
