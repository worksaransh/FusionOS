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

const FREQUENCIES = ['Monthly', 'Quarterly', 'Annual'] as const;

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
  reference: z.string().min(1, 'Reference is required').max(200),
  frequency: z.enum(FREQUENCIES),
  nextDueDate: z.string().min(1, 'Next due date is required'),
  lines: z.array(lineSchema).min(2, 'A recurring journal entry template needs at least two lines'),
});
type FormValues = z.infer<typeof schema>;

interface RecurringJournalEntryTemplateLineDto {
  id: string;
  accountId: string;
  debit: number;
  credit: number;
  costCenterId: string | null;
}

interface RecurringJournalEntryTemplateDto {
  id: string;
  reference: string;
  frequency: string;
  nextDueDate: string;
  isActive: boolean;
  totalDebit: number;
  totalCredit: number;
  lines: RecurringJournalEntryTemplateLineDto[];
}

/**
 * Recurring journal entry templates — new (a monthly rent/depreciation-style entry that should
 * repeat, confirmed absent before this feature). Creating a template never itself posts
 * anything; "Generate now" turns its current lines into a real JournalEntry (going through the
 * exact same account-existence/cost-center-existence/closed-period/balance validation
 * JournalEntriesPanel's own Create does — see GenerateJournalEntryFromTemplateCommandHandler's
 * class doc comment) and advances NextDueDate by one Frequency unit.
 */
export function RecurringJournalEntriesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const accountOptions = useAccountOptions(companyId);

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      reference: '',
      frequency: 'Monthly',
      nextDueDate: '',
      lines: [{ accountId: '', debit: '', credit: '' }, { accountId: '', debit: '', credit: '' }],
    },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  const templatesQuery = useQuery({
    queryKey: ['recurring-journal-entries', companyId],
    queryFn: () => apiClient.get<PagedResult<RecurringJournalEntryTemplateDto>>(`/finance/recurring-journal-entries?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const createTemplate = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<RecurringJournalEntryTemplateDto>('/finance/recurring-journal-entries', {
        companyId,
        reference: values.reference,
        frequency: values.frequency,
        nextDueDate: new Date(values.nextDueDate).toISOString(),
        lines: values.lines.map((l) => ({
          accountId: l.accountId,
          debit: Number(l.debit) || 0,
          credit: Number(l.credit) || 0,
        })),
      }),
    onSuccess: () => {
      reset({ reference: '', frequency: 'Monthly', nextDueDate: '', lines: [{ accountId: '', debit: '', credit: '' }, { accountId: '', debit: '', credit: '' }] });
      queryClient.invalidateQueries({ queryKey: ['recurring-journal-entries', companyId] });
    },
  });

  const generateEntry = useMutation({
    mutationFn: (id: string) => apiClient.post(`/finance/recurring-journal-entries/${id}/generate?companyId=${companyId}`, {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recurring-journal-entries', companyId] });
      queryClient.invalidateQueries({ queryKey: ['journal-entries', companyId] });
    },
  });

  const deactivateTemplate = useMutation({
    mutationFn: (id: string) => apiClient.post<RecurringJournalEntryTemplateDto>(`/finance/recurring-journal-entries/${id}/deactivate?companyId=${companyId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['recurring-journal-entries', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Recurring Journal Entries</h2>
      <p className="mb-3 text-xs text-text-muted">
        A reusable balanced set of lines that repeats on a schedule. Creating one does not post anything by
        itself — use "Generate now" to turn the current lines into a real journal entry and advance the next due
        date.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createTemplate.mutate(values))} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <label className="flex flex-col gap-1 text-sm">
              Reference
              <Controller
                control={control}
                name="reference"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Monthly rent" {...field} />
                )}
              />
              {errors.reference && <span className="text-xs text-danger">{errors.reference.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Frequency
              <Controller
                control={control}
                name="frequency"
                render={({ field }) => (
                  <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                    {FREQUENCIES.map((freq) => (
                      <option key={freq} value={freq}>{freq}</option>
                    ))}
                  </select>
                )}
              />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Next due date
              <Controller
                control={control}
                name="nextDueDate"
                render={({ field }) => (
                  <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
                )}
              />
              {errors.nextDueDate && <span className="text-xs text-danger">{errors.nextDueDate.message}</span>}
            </label>
          </div>

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
            {typeof errors.lines?.message === 'string' && <span className="text-xs text-danger">{errors.lines.message}</span>}
            <Button type="button" variant="secondary" onClick={() => append({ accountId: '', debit: '', credit: '' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add line
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Creating…' : 'Create template'}
          </Button>
        </form>
        {createTemplate.isError && createTemplate.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createTemplate.error.problem.title}</p>
        )}
      </Card>

      {templatesQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Reference', render: (row: RecurringJournalEntryTemplateDto) => row.reference },
              { header: 'Frequency', render: (row: RecurringJournalEntryTemplateDto) => row.frequency },
              { header: 'Next due', render: (row: RecurringJournalEntryTemplateDto) => new Date(row.nextDueDate).toLocaleDateString() },
              { header: 'Amount', render: (row: RecurringJournalEntryTemplateDto) => row.totalDebit.toLocaleString() },
              { header: 'Status', render: (row: RecurringJournalEntryTemplateDto) => (row.isActive ? 'Active' : 'Inactive') },
              {
                header: 'Actions',
                render: (row: RecurringJournalEntryTemplateDto) => (
                  <div className="flex items-center gap-2">
                    <Button
                      variant="secondary"
                      onClick={() => generateEntry.mutate(row.id)}
                      disabled={!row.isActive || generateEntry.isPending}
                    >
                      Generate now
                    </Button>
                    <Button
                      variant="danger"
                      onClick={() => deactivateTemplate.mutate(row.id)}
                      disabled={!row.isActive || deactivateTemplate.isPending}
                    >
                      {row.isActive ? 'Deactivate' : 'Deactivated'}
                    </Button>
                  </div>
                ),
              },
            ]}
            rows={templatesQuery.data.data}
            isLoading={templatesQuery.isLoading}
            emptyMessage="No recurring journal entry templates yet."
            rowKey={(row) => row.id}
          />
        </Card>
      )}
      {(generateEntry.isError || deactivateTemplate.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that recurring journal entry template.</p>
      )}
    </div>
  );
}
