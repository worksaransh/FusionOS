import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../../shared/api/client';
import { Card } from '../../../shared/ui/Card';
import { PageHeader } from '../../../shared/ui/PageHeader';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue';

interface AuditLogEntryDto {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  actorId: string;
  actorEmail: string | null;
  companyId: string;
  branchId: string | null;
  occurredAt: string;
  correlationId: string;
}

interface PagedResult<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

/**
 * Read side of the insert-only audit trail (Phase H4, 2026-07-14 sprint).
 * Every Create/Update/Deactivate/PermissionsUpdated/etc. IAuditableCommand has
 * been writing rows here since AuditBehavior was added — this is the first UI
 * that reads them back. Gated server-side by "core.audit.read". This page has
 * no create form (read-only), so it uses DataTable directly rather than
 * CrudListPage, which always expects a form node.
 *
 * Search box added in Phase M5 (2026-07-15 — Search completion): matches on
 * EntityType or Action server-side (ListAuditLogEntriesQuery), debounced the
 * same way EntityCombobox debounces its own search (shared/api/entityOptions.ts).
 */
export function AuditLogPage() {
  const { companyId } = useActiveCompany();
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, 250);

  const auditQuery = useQuery({
    queryKey: ['audit-log', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId: companyId!, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<AuditLogEntryDto>>(`/core/audit-log?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to view the audit log.</p>;
  }

  return (
    <div>
      <PageHeader title="Audit log" description="Every recorded write action for this company, most recent first." />

      <label className="mb-3 flex max-w-sm flex-col gap-1 text-sm">
        Search by entity type or action
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="e.g. Invoice, Deactivated…"
          className="rounded-md border border-border bg-surface px-2 py-1.5"
        />
      </label>

      <Card>
        <DataTable<AuditLogEntryDto>
          columns={[
            { header: 'Timestamp', render: (row) => new Date(row.occurredAt).toLocaleString() },
            { header: 'Entity', render: (row) => row.entityType },
            { header: 'Entity ID', render: (row) => <span className="font-mono text-xs">{row.entityId}</span> },
            { header: 'Action', render: (row) => row.action },
            { header: 'User', render: (row) => row.actorEmail ?? row.actorId },
          ]}
          rows={auditQuery.data?.data}
          isLoading={auditQuery.isLoading}
          isError={auditQuery.isError}
          errorMessage="Could not load the audit log."
          emptyMessage="No audit entries yet."
          rowKey={(row) => row.id}
        />
      </Card>
    </div>
  );
}
