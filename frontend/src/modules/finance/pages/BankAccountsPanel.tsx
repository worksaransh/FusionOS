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
import { useAccountOptions } from '../../../shared/api/entityOptions';
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue';
import type { PagedResult } from '../../../shared/api/types';

const SEARCH_DEBOUNCE_MS = 250;

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  name: z.string().min(1, 'Name is required').max(200),
  linkedAccountId: z.string().uuid('Pick a GL account'),
  bankName: z.string().max(200).optional(),
  accountNumberLast4: z.string().max(4, 'Only the last 4 digits — never the full account number').optional(),
});
type FormValues = z.infer<typeof schema>;

// Update deliberately excludes Code (the immutable business key) and
// LinkedAccountId (a structural link, not a mutable detail) — see
// UpdateBankAccountCommand.cs / BankAccountsController.Update.
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  bankName: z.string().max(200).optional(),
  accountNumberLast4: z.string().max(4, 'Only the last 4 digits — never the full account number').optional(),
});
type EditFormValues = z.infer<typeof editSchema>;

interface BankAccountDto {
  id: string;
  code: string;
  name: string;
  linkedAccountId: string;
  bankName: string | null;
  accountNumberLast4: string | null;
  isActive: boolean;
  createdAt: string;
}

/**
 * Bank Accounts — M8d, Finance depth: bank reconciliation. Master data,
 * same shape as CostCentersPanel plus a mandatory GL account picker
 * (LinkedAccountId, via the existing useAccountOptions hook — no new
 * entity-options hook needed for this half of the slice). Only the last 4
 * digits of an account number are ever collected here — see
 * BankAccount.cs's own doc comment for why. Rendered as a sibling panel
 * under AccountsPage, right after PayablesPanel; BankStatementLinesPanel
 * follows it and picks a bank account from this same list.
 */
export function BankAccountsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingBankAccountId, setEditingBankAccountId] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const accountOptions = useAccountOptions(companyId);

  const bankAccountsQuery = useQuery({
    queryKey: ['bank-accounts', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<BankAccountDto>>(`/finance/bank-accounts?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', linkedAccountId: '', bankName: '', accountNumberLast4: '' },
  });

  const createBankAccount = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<BankAccountDto>('/finance/bank-accounts', {
        companyId,
        code: values.code,
        name: values.name,
        linkedAccountId: values.linkedAccountId,
        bankName: values.bankName || null,
        accountNumberLast4: values.accountNumberLast4 || null,
      }),
    onSuccess: () => {
      reset({ code: '', name: '', linkedAccountId: '', bankName: '', accountNumberLast4: '' });
      queryClient.invalidateQueries({ queryKey: ['bank-accounts', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — BankAccountsController exposes this as a dedicated
  // POST .../{id}/deactivate action (never a DELETE), same convention as
  // CostCentersController/AccountsController.
  const deactivateBankAccount = useMutation({
    mutationFn: (bankAccountId: string) => apiClient.post<BankAccountDto>(`/finance/bank-accounts/${bankAccountId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['bank-accounts', companyId] }),
  });

  if (!companyId) return null;

  const editingBankAccount = bankAccountsQuery.data?.data.find((b) => b.id === editingBankAccountId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Bank Accounts</h2>
      <p className="mb-3 text-xs text-text-muted">
        Master data for a bank account, linked to a GL account it reconciles against. Only the last 4 digits of an
        account number are ever stored — never the full number (see BankAccount.cs).
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createBankAccount.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="OPS-CHECKING" {...field} />
              )}
            />
            {errors.code && <span className="text-xs text-danger">{errors.code.message}</span>}
          </label>
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
            Linked GL account
            <Controller
              control={control}
              name="linkedAccountId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={accountOptions.options}
                  isLoading={accountOptions.isLoading}
                  onSearchChange={accountOptions.onSearchChange}
                  placeholder="Search GL accounts…"
                />
              )}
            />
            {errors.linkedAccountId && <span className="text-xs text-danger">{errors.linkedAccountId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Bank name (optional)
            <Controller
              control={control}
              name="bankName"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Last 4 digits (optional)
            <Controller
              control={control}
              name="accountNumberLast4"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="1234" maxLength={4} {...field} />
              )}
            />
            {errors.accountNumberLast4 && <span className="text-xs text-danger">{errors.accountNumberLast4.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create bank account'}</Button>
          </div>
        </form>
        {createBankAccount.isError && createBankAccount.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createBankAccount.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <label className="mb-3 flex flex-col gap-1 text-sm sm:w-72">
          Search
          <input
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            placeholder="Search by code or name…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </label>
        <DataTable
          columns={[
            { header: 'Code', render: (row: BankAccountDto) => row.code },
            { header: 'Name', render: (row: BankAccountDto) => row.name },
            { header: 'Bank', render: (row: BankAccountDto) => row.bankName ?? '—' },
            { header: 'Account (last 4)', render: (row: BankAccountDto) => row.accountNumberLast4 ?? '—' },
            { header: 'Status', render: (row: BankAccountDto) => (row.isActive ? 'Active' : 'Inactive') },
            { header: 'Created', render: (row: BankAccountDto) => new Date(row.createdAt).toLocaleDateString() },
            {
              header: 'Actions',
              render: (row: BankAccountDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setEditingBankAccountId(row.id)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateBankAccount.isPending}
                    onClick={() => deactivateBankAccount.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={bankAccountsQuery.data?.data}
          isLoading={bankAccountsQuery.isLoading}
          isError={bankAccountsQuery.isError}
          errorMessage="Could not load bank accounts."
          emptyMessage="No bank accounts yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateBankAccount.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that bank account.</p>
      )}

      {editingBankAccount && (
        <BankAccountEditPanel
          companyId={companyId}
          bankAccount={editingBankAccount}
          onClose={() => setEditingBankAccountId(null)}
        />
      )}
    </div>
  );
}

interface BankAccountEditPanelProps {
  companyId: string;
  bankAccount: BankAccountDto;
  onClose: () => void;
}

function BankAccountEditPanel({ companyId, bankAccount, onClose }: BankAccountEditPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: {
      name: bankAccount.name,
      bankName: bankAccount.bankName ?? '',
      accountNumberLast4: bankAccount.accountNumberLast4 ?? '',
    },
  });

  const updateBankAccount = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<BankAccountDto>(`/finance/bank-accounts/${bankAccount.id}`, {
        companyId,
        name: values.name,
        bankName: values.bankName || null,
        accountNumberLast4: values.accountNumberLast4 || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bank-accounts', companyId] });
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
        <h3 className="text-base font-semibold text-text">Edit bank account — {bankAccount.code}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateBankAccount.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
          Bank name (optional)
          <Controller
            control={control}
            name="bankName"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Last 4 digits (optional)
          <Controller
            control={control}
            name="accountNumberLast4"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" maxLength={4} {...field} />
            )}
          />
          {errors.accountNumberLast4 && <span className="text-xs text-danger">{errors.accountNumberLast4.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateBankAccount.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that bank account.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
