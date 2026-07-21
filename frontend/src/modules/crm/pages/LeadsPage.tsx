import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue';
import { useCrmAccountOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';
import { ActivitiesPanel } from './ActivitiesPanel';
import { ContactsPanel } from './ContactsPanel';
import { CrmAccountsPanel } from './CrmAccountsPanel';
import { OpportunitiesPanel } from './OpportunitiesPanel';

const SEARCH_DEBOUNCE_MS = 250;

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  contactEmail: z.string().email('Must be a valid email').max(200).or(z.literal('')),
  contactPhone: z.string().max(30).or(z.literal('')),
  source: z.string().max(100).or(z.literal('')),
});
type FormValues = z.infer<typeof schema>;

const assignAccountSchema = z.object({
  accountId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Account'),
});
type AssignAccountFormValues = z.infer<typeof assignAccountSchema>;

interface LeadDto {
  id: string;
  name: string;
  contactEmail: string | null;
  contactPhone: string | null;
  source: string | null;
  status: string;
  accountId: string | null;
}

/**
 * Leads — CRM's first real frontend slice (backend has existed since the
 * Manufacturing/CRM/Quality backend-only pass; this closes the "frontend
 * panel deferred" gap flagged in docs/PROJECT_TRACKER.md). Top-level page for
 * /crm, same shape as Finance's AccountsPage: a create form + list here, with
 * OpportunitiesPanel/CrmAccountsPanel/ContactsPanel/ActivitiesPanel (CRM depth
 * pass, 2026-07-20) rendered as sibling panels below it.
 */
export function LeadsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);
  const [assigningAccountLeadId, setAssigningAccountLeadId] = useState<string | null>(null);

  const leadsQuery = useQuery({
    queryKey: ['leads', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<LeadDto>>(`/crm/leads?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', contactEmail: '', contactPhone: '', source: '' },
  });

  const createLead = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<LeadDto>('/crm/leads', {
        companyId,
        name: values.name,
        contactEmail: values.contactEmail || null,
        contactPhone: values.contactPhone || null,
        source: values.source || null,
      }),
    onSuccess: () => {
      reset({ name: '', contactEmail: '', contactPhone: '', source: '' });
      queryClient.invalidateQueries({ queryKey: ['leads', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const qualifyLead = useMutation({
    mutationFn: (id: string) => apiClient.post<LeadDto>(`/crm/leads/${id}/qualify`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['leads', companyId] }),
  });

  const disqualifyLead = useMutation({
    mutationFn: (id: string) => apiClient.post<LeadDto>(`/crm/leads/${id}/disqualify`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['leads', companyId] }),
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage leads.</p>;
  }

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Leads</h1>
      <p className="mb-4 text-sm text-text-muted">Raw prospects — New → Qualified → Converted / Disqualified. CRM, Phase 4.</p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createLead.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Name
            <Controller
              control={control}
              name="name"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Contact email (optional)
            <Controller
              control={control}
              name="contactEmail"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.contactEmail && <span className="text-xs text-danger">{errors.contactEmail.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Contact phone (optional)
            <Controller
              control={control}
              name="contactPhone"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.contactPhone && <span className="text-xs text-danger">{errors.contactPhone.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Source (optional)
            <Controller
              control={control}
              name="source"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Website, referral…" {...field} />
              )}
            />
            {errors.source && <span className="text-xs text-danger">{errors.source.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create lead'}</Button>
          </div>
        </form>
        {createLead.isError && createLead.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createLead.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <label className="mb-3 flex flex-col gap-1 text-sm sm:w-72">
          Search
          <input
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            placeholder="Search by name…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </label>
        <DataTable
          columns={[
            { header: 'Name', render: (row: LeadDto) => row.name },
            { header: 'Contact', render: (row: LeadDto) => row.contactEmail ?? row.contactPhone ?? '—' },
            { header: 'Source', render: (row: LeadDto) => row.source ?? '—' },
            { header: 'Status', render: (row: LeadDto) => row.status },
            { header: 'Account', render: (row: LeadDto) => (row.accountId ? `${row.accountId.slice(0, 8)}…` : '—') },
            {
              header: 'Actions',
              render: (row: LeadDto) => (
                <div className="flex items-center gap-2">
                  {row.status === 'New' && (
                    <>
                      <Button type="button" variant="secondary" disabled={qualifyLead.isPending} onClick={() => qualifyLead.mutate(row.id)}>
                        Qualify
                      </Button>
                      <Button type="button" variant="danger" disabled={disqualifyLead.isPending} onClick={() => disqualifyLead.mutate(row.id)}>
                        Disqualify
                      </Button>
                    </>
                  )}
                  <Button type="button" variant="secondary" onClick={() => setAssigningAccountLeadId(row.id)}>
                    {row.accountId ? 'Change account' : 'Assign account'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={leadsQuery.data?.data}
          isLoading={leadsQuery.isLoading}
          isError={leadsQuery.isError}
          errorMessage="Could not load leads."
          emptyMessage="No leads yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {(qualifyLead.isError || disqualifyLead.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that lead.</p>
      )}

      {(() => {
        const assigningAccountLead = leadsQuery.data?.data.find((l) => l.id === assigningAccountLeadId) ?? null;
        return assigningAccountLead ? (
          <LeadAssignAccountPanel
            companyId={companyId}
            lead={assigningAccountLead}
            onClose={() => setAssigningAccountLeadId(null)}
          />
        ) : null;
      })()}

      <OpportunitiesPanel />
      <CrmAccountsPanel />
      <ContactsPanel />
      <ActivitiesPanel />
    </div>
  );
}

interface LeadAssignAccountPanelProps {
  companyId: string;
  lead: LeadDto;
  onClose: () => void;
}

/** Links a Lead to a CRM Account via POST .../{id}/assign-account (CRM depth pass) — AccountId is nullable, so a blank pick unassigns it. */
function LeadAssignAccountPanel({ companyId, lead, onClose }: LeadAssignAccountPanelProps) {
  const queryClient = useQueryClient();
  const crmAccountOptions = useCrmAccountOptions(companyId);

  const { control, handleSubmit, formState: { errors, isSubmitting } } = useForm<AssignAccountFormValues>({
    resolver: zodResolver(assignAccountSchema),
    defaultValues: { accountId: lead.accountId ?? '' },
  });

  const assignAccount = useMutation({
    mutationFn: (values: AssignAccountFormValues) =>
      apiClient.post<LeadDto>(`/crm/leads/${lead.id}/assign-account`, {
        companyId,
        accountId: values.accountId || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leads', companyId] });
      onClose();
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Assign account — {lead.name}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => assignAccount.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Account (leave blank to unassign)
          <Controller
            control={control}
            name="accountId"
            render={({ field }) => (
              <EntityCombobox
                value={field.value}
                onChange={field.onChange}
                options={crmAccountOptions.options}
                isLoading={crmAccountOptions.isLoading}
                onSearchChange={crmAccountOptions.onSearchChange}
                placeholder="Search accounts…"
              />
            )}
          />
          {errors.accountId && <span className="text-xs text-danger">{errors.accountId.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save account'}</Button>
          {assignAccount.isError && (
            <span role="alert" className="text-sm text-danger">Could not assign that account.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
