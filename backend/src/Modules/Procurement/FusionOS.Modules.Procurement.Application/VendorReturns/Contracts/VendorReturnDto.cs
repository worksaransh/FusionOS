namespace FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;

public sealed record VendorReturnDto(Guid Id, Guid PurchaseOrderId, Guid ProductId, Guid WarehouseId, decimal Quantity, string Reason, string Status, DateTimeOffset ReturnDate);

/// <summary>Single place that turns a VendorReturn aggregate into its DTO, shared by every handler that returns one.</summary>
public static class VendorReturnMapper
{
    public static VendorReturnDto ToDto(Domain.VendorReturns.VendorReturn vendorReturn) => new(
        vendorReturn.Id, vendorReturn.PurchaseOrderId, vendorReturn.ProductId, vendorReturn.WarehouseId,
        vendorReturn.Quantity, vendorReturn.Reason, vendorReturn.Status.ToString(), vendorReturn.ReturnDate);
}
