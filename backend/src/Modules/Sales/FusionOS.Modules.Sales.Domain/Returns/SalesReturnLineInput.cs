namespace FusionOS.Modules.Sales.Domain.Returns;

/// <summary>
/// Request-time input for a SalesReturnLine. Deliberately carries no ConditionNotes/
/// Restocked — those are only knowable once the goods are physically inspected on
/// return, which is why they are captured later, at Complete() time, via
/// SalesReturnLineCompletion instead of here.
/// </summary>
public sealed record SalesReturnLineInput(Guid ProductId, decimal Quantity);
