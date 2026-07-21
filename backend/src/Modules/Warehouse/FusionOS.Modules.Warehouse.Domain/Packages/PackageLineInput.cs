namespace FusionOS.Modules.Warehouse.Domain.Packages;

/// <summary>Input shape for Package.Create — mirrors PickListLineInput; never touches the database itself.</summary>
public sealed record PackageLineInput(Guid ProductId, decimal Quantity);
