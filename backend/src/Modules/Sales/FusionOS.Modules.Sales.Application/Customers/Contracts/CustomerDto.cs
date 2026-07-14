namespace FusionOS.Modules.Sales.Application.Customers.Contracts;

public sealed record CustomerDto(Guid Id, string Name, string Code, string? ContactEmail, decimal CreditLimit, bool IsActive, DateTimeOffset CreatedAt);
