namespace FusionOS.Modules.Procurement.Application.SupplierContracts.Contracts;

public sealed record SupplierContractDto(
    Guid Id,
    Guid SupplierId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string Terms,
    string Status);
