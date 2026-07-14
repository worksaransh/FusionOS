namespace FusionOS.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Shared paginated envelope for every module's list queries — 08_API_STANDARDS.md §4.
/// Defined once here so Inventory, Warehouse, Procurement, Sales, etc. don't each
/// redefine their own copy.
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Data, int Page, int PageSize, int TotalCount);
