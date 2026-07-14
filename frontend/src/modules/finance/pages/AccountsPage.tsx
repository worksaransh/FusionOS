import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { CrudListPage } from '../../../shared/ui/CrudListPage';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useAccountOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';
import { JournalEntriesPanel } from './JournalEntriesPanel';

const ACCOUNT_TYPES = ['Asset', 'Liability', 'Equity', 'Revenue', 'Expense'] as const;

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  name: z.string().min(1, 'Name is required').max(200),
  accountType: z.enum(ACCOUNT_TYPES),
  parentAccountId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Account'),
});
type FormValues = z.infer<typeof schema>;

interface AccountDto {
  id: string;
  code: string;
  name: string;
  accountType: string;
  parentAccountId: string | null;
  isActive: boolean;
  createdAt: string;
}

/**
 * Chart of Accounts — the first Phase 2 (Financial Backbone) slice
 * (05_MODULE_ROADMAP.md). Everything else in Finance builds on this. Parent
 * account is picked via the shared EntityCombobox (self-reference, so the
 * options are this same page's own account list).
 */
export function AccountsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const accountOptions = useAccountOptions(companyId);

  const accountsQuery = useQuery({
    queryKey: ['accounts', companyId],
    queryFn: () => apiClient.get<PagedResult<AccountDto>>(`/finance/accounts?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', accountType: 'Asset', parentAccountId: '' },
  });

  const createAccount = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<AccountDto>('/finance/accounts', {
        companyId,
        code: values.code,
        name: values.name,
        accountType: values.accountType,
        parentAccountId: values.parentAccountId || null,
      }),
    onSuccess: () => {
      reset({ code: '', name: '', accountType: 'Asset', parentAccountId: '' });
      queryClient.invalidateQueries({ queryKey: ['accounts', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage accounts.</p>;
  }

  return (
    <div>
      <CrudListPage<AccountDto>
      title="Chart of Accounts"
      description="General Ledger accounts — Finance, Phase 2"
      rows={accountsQuery.data?.data}
      isLoading={accountsQuery.isLoading}
      isError={accountsQuery.isError}
      errorMessage="Could not load accounts."
      emptyMessage="No accounts yet — create the first one above."
      rowKey={(row) => row.id}
      columns={[
        { header: 'Code', render: (row) => row.code },
        { header: 'Name', render: (row) => row.name },
        { header: 'Type', render: (row) => row.accountType },
        { header: 'Parent', render: (row) => (row.parentAccountId ? accountOptions.options.find((a) => a.id === row.parentAccountId)?.label ?? row.parentAccountId : '—') },
        { header: 'Created', render: (row) => new Date(row.createdAt).toLocaleDateString() },
      ]}
      form={
        <form onSubmit={handleSubmit((values) => createAccount.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="1000" {...field} />
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
            Account type
            <Controller
              control={control}
              name="accountType"
              render={({ field }) => (
                <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                  {ACCOUNT_TYPES.map((type) => (
                    <option key={type} value={type}>{type}</option>
                  ))}
                </select>
              )}
            />
            {errors.accountType && <span className="text-xs text-danger">{errors.accountType.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Parent account (optional)
            <Controller
              control={control}
              name="parentAccountId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={accountOptions.options}
                  isLoading={accountOptions.isLoading}
                  onSearchChange={accountOptions.onSearchChange}
                  placeholder="Search accounts…"
                />
              )}
            />
            {errors.parentAccountId && <span className="text-xs text-danger">{errors.parentAccountId.message}</span>}
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create account'}</Button>
          </div>
        </form>
      }
      />
      <JournalEntriesPanel />
    </div>
  );
}
