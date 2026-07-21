import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue';
import type { PagedResult } from '../../../shared/api/types';

const SEARCH_DEBOUNCE_MS = 250;

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  industry: z.string().max(100).or(z.literal('')),
  website: z.string().max(200).or(z.literal('')),
});
type FormValues = z.infer<typeof schema>;

const editSchema = schema;
type EditFormValues = z.infer<typeof editSchema>;

interface AccountDto {
  id: string;
  name: string;
  industry: string | null;
  website: string | null;
  isActive: boolean;
  createdAt: string;
}

/**
 * Accounts — CRM depth pass (2026-07-20). The organization/company behind a Lead,
 * Opportunity, or Contact, distinct from Sales' Customer (see Account.cs doc comment).
 * Rendered as a sibling panel under LeadsPage, same stacking pattern as
 * OpportunitiesPanel/ContactsPanel/ActivitiesPanel.
 */
export function CrmAccountsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingAccountId, setEditingAccountId] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const accountsQuery = useQuery({
    queryKey: ['crm-accounts', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<AccountDto>>(`/crm/accounts?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', industry: '', website: '' },
  });

  const createAccount = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<AccountDto>('/crm/accounts', {
        companyId,
        name: values.name,
        industry: values.industry || null,
        website: values.website || null,
      }),
    onSuccess: () => {
      reset({ name: '', industry: '', website: '' });
      queryClient.invalidateQueries({ queryKey: ['crm-accounts', companyId] });
      queryClient.invalidateQueries({ queryKey: ['crm-account-options', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — AccountsController exposes this as a dedicated
  // POST .../{id}/deactivate action (never a DELETE), same convention as
  // CostCentersController/CustomersController.
  const deactivateAccount = useMutation({
    mutationFn: (accountId: string) => apiClient.post<AccountDto>(`/crm/accounts/${accountId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['crm-accounts', companyId] }),
  });

  if (!companyId) return null;

  const editingAccount = accountsQuery.data?.data.find((a) => a.id === editingAccountId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Accounts</h2>
      <p className="mb-3 text-xs text-text-muted">
        The organization behind a Lead, Opportunity, or Contact — distinct from a Sales Customer, which only exists once an opportunity is won.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createAccount.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
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
            Industry (optional)
            <Controller
              control={control}
              name="industry"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.industry && <span className="text-xs text-danger">{errors.industry.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Website (optional)
            <Controller
              control={control}
              name="website"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="https://…" {...field} />
              )}
            />
            {errors.website && <span className="text-xs text-danger">{errors.website.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create account'}</Button>
          </div>
        </form>
        {createAccount.isError && createAccount.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createAccount.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <label className="mb-3 flex flex-col gap-1 text-sm sm:w-72">
          Search
          <input
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            placeholder="Search by name or industry…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </label>
        <DataTable
          columns={[
            { header: 'Name', render: (row: AccountDto) => row.name },
            { header: 'Industry', render: (row: AccountDto) => row.industry ?? '—' },
            { header: 'Website', render: (row: AccountDto) => row.website ?? '—' },
            { header: 'Status', render: (row: AccountDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: AccountDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setEditingAccountId(row.id)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateAccount.isPending}
                    onClick={() => deactivateAccount.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={accountsQuery.data?.data}
          isLoading={accountsQuery.isLoading}
          isError={accountsQuery.isError}
          errorMessage="Could not load accounts."
          emptyMessage="No accounts yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateAccount.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that account.</p>
      )}

      {editingAccount && (
        <AccountEditPanel
          companyId={companyId}
          account={editingAccount}
          onClose={() => setEditingAccountId(null)}
        />
      )}
    </div>
  );
}

interface AccountEditPanelProps {
  companyId: string;
  account: AccountDto;
  onClose: () => void;
}

function AccountEditPanel({ companyId, account, onClose }: AccountEditPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: { name: account.name, industry: account.industry ?? '', website: account.website ?? '' },
  });

  const updateAccount = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<AccountDto>(`/crm/accounts/${account.id}`, {
        companyId,
        name: values.name,
        industry: values.industry || null,
        website: values.website || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['crm-accounts', companyId] });
      queryClient.invalidateQueries({ queryKey: ['crm-account-options', companyId] });
      onClose();
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof EditFormValues, { message: messages[0] });
        }
      }
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Edit account — {account.name}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateAccount.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
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
          Industry (optional)
          <Controller
            control={control}
            name="industry"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.industry && <span className="text-xs text-danger">{errors.industry.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Website (optional)
          <Controller
            control={control}
            name="website"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.website && <span className="text-xs text-danger">{errors.website.message}</span>}
        </label>
        <div className="col-span-full flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateAccount.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that account.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
