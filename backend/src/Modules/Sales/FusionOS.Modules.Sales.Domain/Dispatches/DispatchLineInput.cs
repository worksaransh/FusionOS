namespace FusionOS.Modules.Sales.Domain.Dispatches;

public sealed record DispatchLineInput(Guid ProductId, decimal QuantityDispatched);
