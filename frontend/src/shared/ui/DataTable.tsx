import React, { useState, useMemo, type ReactNode } from 'react';
import { ArrowUpDown, ArrowUp, ArrowDown, Download, Eye, Search, ChevronLeft, ChevronRight } from 'lucide-react';
import { TableSkeleton } from './Skeleton';
import { Button } from './Button';

export interface DataTableColumn<T> {
  header: string;
  accessorKey?: keyof T;
  render: (row: T) => ReactNode;
  sortable?: boolean;
}

interface DataTableProps<T> {
  columns: DataTableColumn<T>[];
  rows: T[] | undefined;
  isLoading: boolean;
  isError?: boolean;
  errorMessage?: string;
  emptyMessage: string;
  rowKey: (row: T) => string;
  enableSearch?: boolean;
  enableExport?: boolean;
  enablePagination?: boolean;
  pageSizeDefault?: number;
  bulkActions?: (selectedRows: T[]) => ReactNode;
}

export function DataTable<T>({
  columns,
  rows = [],
  isLoading,
  isError,
  errorMessage = 'Could not load data.',
  emptyMessage,
  rowKey,
  enableSearch = true,
  enableExport = true,
  enablePagination = true,
  pageSizeDefault = 10,
  bulkActions,
}: DataTableProps<T>) {
  const [searchTerm, setSearchTerm] = useState('');
  const [sortCol, setSortCol] = useState<string | null>(null);
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('asc');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(pageSizeDefault);
  const [selectedKeys, setSelectedKeys] = useState<Set<string>>(new Set());
  const [hiddenCols, setHiddenCols] = useState<Set<string>>(new Set());
  const [showColMenu, setShowColMenu] = useState(false);

  // Filtered rows
  const filteredRows = useMemo(() => {
    if (!rows) return [];
    if (!searchTerm.trim()) return rows;
    const term = searchTerm.toLowerCase();
    return rows.filter((row) =>
      Object.values(row as Record<string, any>).some((val) =>
        val !== null && val !== undefined && String(val).toLowerCase().includes(term)
      )
    );
  }, [rows, searchTerm]);

  // Sorted rows
  const sortedRows = useMemo(() => {
    if (!sortCol) return filteredRows;
    const colObj = columns.find((c) => c.header === sortCol);
    if (!colObj || !colObj.accessorKey) return filteredRows;

    return [...filteredRows].sort((a, b) => {
      const valA = (a as any)[colObj.accessorKey!];
      const valB = (b as any)[colObj.accessorKey!];
      if (valA === valB) return 0;
      if (valA === null || valA === undefined) return 1;
      if (valB === null || valB === undefined) return -1;
      const res = valA < valB ? -1 : 1;
      return sortDir === 'asc' ? res : -res;
    });
  }, [filteredRows, sortCol, sortDir, columns]);

  // Paginated rows
  const paginatedRows = useMemo(() => {
    if (!enablePagination) return sortedRows;
    const start = (currentPage - 1) * pageSize;
    return sortedRows.slice(start, start + pageSize);
  }, [sortedRows, enablePagination, currentPage, pageSize]);

  const totalPages = Math.ceil(sortedRows.length / pageSize) || 1;

  const handleSort = (colHeader: string) => {
    if (sortCol === colHeader) {
      if (sortDir === 'asc') setSortDir('desc');
      else {
        setSortCol(null);
        setSortDir('asc');
      }
    } else {
      setSortCol(colHeader);
      setSortDir('asc');
    }
  };

  const handleSelectAll = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.checked) {
      setSelectedKeys(new Set(paginatedRows.map(rowKey)));
    } else {
      setSelectedKeys(new Set());
    }
  };

  const handleSelectRow = (key: string) => {
    const next = new Set(selectedKeys);
    if (next.has(key)) next.delete(key);
    else next.add(key);
    setSelectedKeys(next);
  };

  const toggleCol = (header: string) => {
    const next = new Set(hiddenCols);
    if (next.has(header)) next.delete(header);
    else next.add(header);
    setHiddenCols(next);
  };

  const exportCSV = () => {
    if (!sortedRows.length) return;
    const visibleColumns = columns.filter((c) => !hiddenCols.has(c.header));
    const headers = visibleColumns.map((c) => c.header).join(',');
    const csvRows = sortedRows.map((row) =>
      visibleColumns
        .map((c) => {
          const val = c.accessorKey ? (row as any)[c.accessorKey] : '';
          return `"${String(val ?? '').replace(/"/g, '""')}"`;
        })
        .join(',')
    );
    const blob = new Blob([[headers, ...csvRows].join('\n')], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `export-${Date.now()}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  };

  const visibleColumns = columns.filter((c) => !hiddenCols.has(c.header));
  const selectedRowsList = sortedRows.filter((r) => selectedKeys.has(rowKey(r)));

  return (
    <div className="space-y-3">
      {/* Table Toolbar */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        {enableSearch && (
          <div className="relative flex-1 min-w-[200px] max-w-md">
            <Search className="absolute left-3 top-2.5 h-4 w-4 text-text-muted" />
            <input
              type="text"
              placeholder="Search table..."
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value);
                setCurrentPage(1);
              }}
              className="w-full rounded-lg border border-border bg-surface pl-9 pr-3 py-1.5 text-sm focus:border-primary focus:outline-none"
            />
          </div>
        )}

        <div className="flex items-center gap-2 ml-auto">
          {/* Column Visibility Menu */}
          <div className="relative">
            <Button
              type="button"
              variant="secondary"
              className="text-xs flex items-center gap-1.5"
              onClick={() => setShowColMenu(!showColMenu)}
            >
              <Eye size={14} /> Columns
            </Button>
            {showColMenu && (
              <div className="absolute right-0 top-full z-20 mt-1 w-48 rounded-lg border border-border bg-surface p-2 shadow-xl">
                <p className="px-2 py-1 text-xs font-semibold uppercase text-text-muted">Toggle Columns</p>
                <div className="mt-1 space-y-1">
                  {columns.map((col) => (
                    <label key={col.header} className="flex items-center gap-2 px-2 py-1 text-xs hover:bg-surface-muted rounded cursor-pointer">
                      <input
                        type="checkbox"
                        checked={!hiddenCols.has(col.header)}
                        onChange={() => toggleCol(col.header)}
                        className="rounded border-border"
                      />
                      {col.header}
                    </label>
                  ))}
                </div>
              </div>
            )}
          </div>

          {enableExport && (
            <Button type="button" variant="secondary" className="text-xs flex items-center gap-1.5" onClick={exportCSV}>
              <Download size={14} /> Export CSV
            </Button>
          )}
        </div>
      </div>

      {/* Bulk Action Bar */}
      {selectedKeys.size > 0 && (
        <div className="flex items-center justify-between rounded-lg bg-primary/10 px-4 py-2 text-sm text-primary">
          <span>{selectedKeys.size} item(s) selected</span>
          {bulkActions && <div>{bulkActions(selectedRowsList)}</div>}
        </div>
      )}

      {/* Main Table Content */}
      {isLoading && <TableSkeleton rows={pageSize} cols={columns.length} />}

      {isError && (
        <div className="rounded-lg border border-danger/30 bg-danger/10 p-4 text-center text-sm text-danger" role="alert">
          {errorMessage}
        </div>
      )}

      {!isLoading && !isError && (
        <div className="overflow-x-auto rounded-xl border border-border bg-surface shadow-xs">
          <table className="w-full min-w-[32rem] text-left text-sm">
            <thead>
              <tr className="border-b border-border bg-surface-muted/50 text-xs font-medium uppercase tracking-wider text-text-muted">
                {bulkActions && (
                  <th scope="col" className="p-3 w-10 text-center">
                    <input
                      type="checkbox"
                      onChange={handleSelectAll}
                      checked={paginatedRows.length > 0 && paginatedRows.every((r) => selectedKeys.has(rowKey(r)))}
                      className="rounded border-border"
                    />
                  </th>
                )}
                {visibleColumns.map((col) => (
                  <th
                    key={col.header}
                    scope="col"
                    className="px-4 py-3 whitespace-nowrap cursor-pointer select-none hover:text-text"
                    onClick={() => col.sortable !== false && col.accessorKey && handleSort(col.header)}
                  >
                    <div className="flex items-center gap-1.5">
                      <span>{col.header}</span>
                      {col.sortable !== false && col.accessorKey && (
                        <span className="text-text-muted">
                          {sortCol === col.header ? (
                            sortDir === 'asc' ? <ArrowUp size={14} /> : <ArrowDown size={14} />
                          ) : (
                            <ArrowUpDown size={12} className="opacity-40" />
                          )}
                        </span>
                      )}
                    </div>
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-border/60">
              {paginatedRows.map((row) => {
                const key = rowKey(row);
                const isSelected = selectedKeys.has(key);
                return (
                  <tr
                    key={key}
                    className={`transition-colors hover:bg-surface-muted/60 ${isSelected ? 'bg-primary/5' : ''}`}
                  >
                    {bulkActions && (
                      <td className="p-3 text-center">
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() => handleSelectRow(key)}
                          className="rounded border-border"
                        />
                      </td>
                    )}
                    {visibleColumns.map((col) => (
                      <td key={col.header} className="px-4 py-3 whitespace-nowrap text-text">
                        {col.render(row)}
                      </td>
                    ))}
                  </tr>
                );
              })}

              {paginatedRows.length === 0 && (
                <tr>
                  <td colSpan={visibleColumns.length + (bulkActions ? 1 : 0)} className="py-8 text-center text-text-muted">
                    {emptyMessage}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* Pagination Footer */}
      {enablePagination && !isLoading && sortedRows.length > 0 && (
        <div className="flex flex-wrap items-center justify-between gap-3 text-xs text-text-muted pt-1">
          <div>
            Showing <span className="font-semibold text-text">{(currentPage - 1) * pageSize + 1}</span> to{' '}
            <span className="font-semibold text-text">{Math.min(currentPage * pageSize, sortedRows.length)}</span> of{' '}
            <span className="font-semibold text-text">{sortedRows.length}</span> results
          </div>

          <div className="flex items-center gap-4">
            <div className="flex items-center gap-1.5">
              <span>Per page:</span>
              <select
                value={pageSize}
                onChange={(e) => {
                  setPageSize(Number(e.target.value));
                  setCurrentPage(1);
                }}
                className="rounded-md border border-border bg-surface px-2 py-1 text-xs text-text"
              >
                <option value={5}>5</option>
                <option value={10}>10</option>
                <option value={25}>25</option>
                <option value={50}>50</option>
              </select>
            </div>

            <div className="flex items-center gap-1">
              <Button
                type="button"
                variant="secondary"
                disabled={currentPage === 1}
                onClick={() => setCurrentPage((p) => p - 1)}
                className="px-2 py-1"
              >
                <ChevronLeft size={14} />
              </Button>
              <span className="px-2">
                Page {currentPage} of {totalPages}
              </span>
              <Button
                type="button"
                variant="secondary"
                disabled={currentPage === totalPages}
                onClick={() => setCurrentPage((p) => p - 1)}
                className="px-2 py-1"
              >
                <ChevronRight size={14} />
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
