import { Controller, useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Trash2 } from 'lucide-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useAccountOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const lineSchema = z
  .object({
    accountId: z.string().uuid('Pick an account'),
    debit: z.string().refine((v) => v === '' || Number(v) >= 0, 'Debit cannot be negative'),
    credit: z.string().refine((v) => v === '' || Number(v) >= 0, 'Credit cannot be negative'),
  })
  .refine((l) => (Number(l.debit) || 0) > 0 !== (Number(l.credit) || 0) > 0, {
    message: 'Each line needs exactly one of debit or credit',
    path: ['debit'],
  });

const schema = z.object({
  reference: z.string().optional(),
  lines: z.array(lineSchema).min(2, 'A journal entry needs at least two lines'),
});
type FormValues = z.infer<typeof schema>;

interface JournalEntryLineDto {
  id: string;
  accountId: string;
  debit: number;
  credit: number;
  description: string | null;
}

interface JournalEntryDto {
  id: string;
  reference: string | null;
  status: string;
  entryDate: string;
  totalDebit: number;
  totalCredit: number;
  lines: JournalEntryLineDto[];
}

/**
 * General Ledger journal entries — the second Phase 2 slice, built on Accounts.
 * A journal entry must balance (total debit == total credit) before it can be
 * created at all; Post() is a separate step, mirroring the Draft -> Approved /
 * Draft -> Confirmed workflows already used by Purchase Orders and Sales Orders.
 * Each line's Account is picked via the shared EntityCombobox.
 */
export function JournalEntriesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const accountOptions = useAccountOptions(companyId);

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { reference: '', lines: [{ accountId: '', debit: '', credit: '' }, { accountId: '', debit: '', credit: '' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  const entriesQuery = useQuery({
    queryKey: ['journal-entries', companyId],
    queryFn: () => apiClient.get<PagedResult<JournalEntryDto>>(`/finance/journal-entries?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const createEntry = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<JournalEntryDto>('/finance/journal-entries', {
        companyId,
        reference: values.reference || null,
        lines: values.lines.map((l) => ({
          accountId: l.accountId,
          debit: Number(l.debit) || 0,
          credit: Number(l.credit) || 0,
          description: null,
        })),
      }),
    onSuccess: () => {
      reset({ reference: '', lines: [{ accountId: '', debit: '', credit: '' }, { accountId: '', debit: '', credit: '' }] });
      queryClient.invalidateQueries({ queryKey: ['journal-entries', companyId] });
    },
  });

  const postEntry = useMutation({
    mutationFn: (id: string) => apiClient.post(`/finance/journal-entries/${id}/post?companyId=${companyId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['journal-entries', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Journal Entries</h2>
      <p className="mb-3 text-xs text-text-muted">
        Every line needs exactly one of debit or credit, and total debit must equal total credit.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createEntry.mutate(values))} className="flex flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm">
            Reference (optional)
            <Controller
              control={control}
              name="reference"
              render={({ field }) => (
                <input className="w-96 rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
          </label>

          <div className="flex flex-col gap-2">
            {fields.map((field, index) => (
              <div key={field.id} className="flex items-end gap-2">
                <label className="flex flex-col gap-1 text-sm">
                  Account
                  <Controller
                    control={control}
                    name={`lines.${index}.accountId`}
                    render={({ field: lineField }) => (
                      <EntityCombobox
                        className="w-72"
                        value={lineField.value}
                        onChange={lineField.onChange}
                        options={accountOptions.options}
                        isLoading={accountOptions.isLoading}
                        onSearchChange={accountOptions.onSearchChange}
                        placeholder="Search accounts…"
                      />
                    )}
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  Debit
                  <Controller
                    control={control}
                    name={`lines.${index}.debit`}
                    render={({ field: lineField }) => (
                      <input className="w-28 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
                    )}
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  Credit
                  <Controller
                    control={control}
                    name={`lines.${index}.credit`}
                    render={({ field: lineField }) => (
                      <input className="w-28 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
                    )}
                  />
                </label>
                <Button type="button" variant="secondary" onClick={() => remove(index)} disabled={fields.length === 2}>
                  <Trash2 size={16} />
                </Button>
              </div>
            ))}
            {errors.lines?.root && <span className="text-xs text-danger">{errors.lines.root.message}</span>}
            {typeof errors.lines?.message === 'string' && <span className="text-xs text-danger">{errors.lines.message}</span>}
            <Button type="button" variant="secondary" onClick={() => append({ accountId: '', debit: '', credit: '' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add line
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Creating…' : 'Create journal entry'}
          </Button>
        </form>
      </Card>

      {entriesQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Date', render: (entry: JournalEntryDto) => new Date(entry.entryDate).toLocaleDateString() },
              { header: 'Reference', render: (entry: JournalEntryDto) => entry.reference ?? '—' },
              { header: 'Status', render: (entry: JournalEntryDto) => entry.status },
              { header: 'Total', render: (entry: JournalEntryDto) => entry.totalDebit.toLocaleString() },
              {
                header: '',
                render: (entry: JournalEntryDto) =>
                  entry.status === 'Draft' ? (
                    <Button variant="secondary" onClick={() => postEntry.mutate(entry.id)} disabled={postEntry.isPending}>
                      Post
                    </Button>
                  ) : null,
              },
            ]}
            rows={entriesQuery.data.data}
            isLoading={entriesQuery.isLoading}
            emptyMessage="No journal entries yet."
            rowKey={(entry) => entry.id}
          />
        </Card>
      )}
      {createEntry.isError && createEntry.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{createEntry.error.problem.title}</p>
      )}
    </div>
  );
}
