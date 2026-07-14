import type { ReactNode } from 'react';
import { PageHeader } from './PageHeader';
import { Card } from './Card';
import { DataTable, type DataTableColumn } from './DataTable';

interface CrudListPageProps<T> {
  title: string;
  description?: string;
  form: ReactNode;
  columns: DataTableColumn<T>[];
  rows: T[] | undefined;
  isLoading: boolean;
  isError: boolean;
  errorMessage: string;
  emptyMessage: string;
  rowKey: (row: T) => string;
}

/**
 * Shared list+form layout used by every Phase 1 module's first slice
 * (Product, Warehouse, Supplier, Customer) — 06_UI_UX_DESIGN_SYSTEM.md §2.
 * Keeps the four near-identical CRUD screens from re-implementing the same
 * table/error/empty-state chrome four times. The table itself is DataTable
 * (shared/ui/DataTable.tsx), reused by every secondary "Panel" component too.
 */
export function CrudListPage<T>({
  title,
  description,
  form,
  columns,
  rows,
  isLoading,
  isError,
  errorMessage,
  emptyMessage,
  rowKey,
}: CrudListPageProps<T>) {
  return (
    <div>
      <PageHeader title={title} description={description} />

      <Card className="mb-6">{form}</Card>

      <Card>
        <DataTable
          columns={columns}
          rows={rows}
          isLoading={isLoading}
          isError={isError}
          errorMessage={errorMessage}
          emptyMessage={emptyMessage}
          rowKey={rowKey}
        />
      </Card>
    </div>
  );
}
