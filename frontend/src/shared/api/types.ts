/** Mirrors FusionOS.BuildingBlocks.Application.Abstractions.PagedResult<T> (08_API_STANDARDS.md §4). */
export interface PagedResult<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}
