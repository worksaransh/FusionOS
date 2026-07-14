import type { ReactNode } from 'react';

export interface DataTableColumn<T> {
  header: string;
  render: (row: T) => ReactNode;
}

interface DataTableProps<T> {
  columns: DataTableColumn<T>[];
  rows: T[] | undefined;
  isLoading: boolean;
  isError?: boolean;
  errorMessage?: string;
  emptyMessage: string;
  rowKey: (row: T) => string;
}

/**
 * The table/loading/error/empty-state chrome every list screen in this app
 * needs (06_UI_UX_DESIGN_SYSTEM.md §2) — extracted out of CrudListPage so the
 * secondary "Panel" components nested inside a parent page (ZonesPanel,
 * GoodsReceiptsPanel, PurchaseOrdersPanel, SalesOrdersPanel, InvoicesPanel,
 * DispatchesPanel, JournalEntriesPanel, StockLedgerPanel) can reuse the same
 * table markup without also getting CrudListPage's PageHeader, which doesn't
 * fit a subsection that already lives under its parent page's own heading.
 * CrudListPage itself is now just PageHeader + a form Card + this component.
 * The table is wrapped in a horizontally-scrolling container so wide tables
 * (5+ columns) degrade to a scrollable strip on narrow viewports instead of
 * overflowing the page (06_UI_UX_DESIGN_SYSTEM.md responsive breakpoints).
 */
export function DataTable<T>({ columns, rows, isLoading, isError, errorMessage, emptyMessage, rowKey }: DataTableProps<T>) {
  return (
    <>
      {isLoading && <p role="status" className="text-text-muted">Loading…</p>}
      {isError && <p role="alert" className="text-danger">{errorMessage}</p>}
      {rows && (
        <div className="overflow-x-auto">
          <table className="w-full min-w-[32rem] text-left text-sm">
            <thead>
              <tr className="border-b border-border text-text-muted">
                {columns.map((col) => (
                  <th key={col.header} scope="col" className="py-2 pr-4 whitespace-nowrap">
                    {col.header}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {rows.map((row) => (
                <tr key={rowKey(row)} className="border-b border-border last:border-0">
                  {columns.map((col) => (
                    <td key={col.header} className="py-2 pr-4">
                      {col.render(row)}
                    </td>
                  ))}
                </tr>
              ))}
              {rows.length === 0 && (
                <tr>
                  <td colSpan={columns.length} className="py-4 text-center text-text-muted">
                    {emptyMessage}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
