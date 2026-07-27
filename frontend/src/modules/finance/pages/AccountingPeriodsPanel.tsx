import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  periodStart: z.string().min(1, 'Period start is required'),
  periodEnd: z.string().min(1, 'Period end is required'),
});
type FormValues = z.infer<typeof schema>;

interface AccountingPeriodDto {
  id: string;
  periodStart: string;
  periodEnd: string;
  status: string;
}

/**
 * General Ledger period control — new, additive to JournalEntry (which has zero period
 * control today: entries can be posted to any date, forever). Creating an AccountingPeriod is
 * entirely opt-in per company; leaving none defined (or none covering a given date) keeps
 * posting exactly as unrestricted as before this feature existed. Only Closing a period blocks
 * new journal entries from posting into it (see AccountingPeriod.cs's own class doc comment).
 */
export function AccountingPeriodsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const periodsQuery = useQuery({
    queryKey: ['accounting-periods', companyId],
    queryFn: () => apiClient.get<PagedResult<AccountingPeriodDto>>(`/finance/accounting-periods?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { periodStart: '', periodEnd: '' },
  });

  const createPeriod = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<AccountingPeriodDto>('/finance/accounting-periods', {
        companyId,
        periodStart: new Date(values.periodStart).toISOString(),
        periodEnd: new Date(values.periodEnd).toISOString(),
      }),
    onSuccess: () => {
      reset({ periodStart: '', periodEnd: '' });
      queryClient.invalidateQueries({ queryKey: ['accounting-periods', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const closePeriod = useMutation({
    mutationFn: (id: string) => apiClient.post<AccountingPeriodDto>(`/finance/accounting-periods/${id}/close?companyId=${companyId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['accounting-periods', companyId] }),
  });

  const reopenPeriod = useMutation({
    mutationFn: (id: string) => apiClient.post<AccountingPeriodDto>(`/finance/accounting-periods/${id}/reopen?companyId=${companyId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['accounting-periods', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Accounting Periods</h2>
      <p className="mb-3 text-xs text-text-muted">
        Optional period control. Leaving no period defined for a date behaves exactly as before this feature
        existed — new journal entries only get rejected from an explicitly Closed period.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createPeriod.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            Period start
            <Controller
              control={control}
              name="periodStart"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.periodStart && <span className="text-xs text-danger">{errors.periodStart.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Period end
            <Controller
              control={control}
              name="periodEnd"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.periodEnd && <span className="text-xs text-danger">{errors.periodEnd.message}</span>}
          </label>
          <div className="col-span-3">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create period'}</Button>
          </div>
        </form>
        {createPeriod.isError && createPeriod.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createPeriod.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Period start', render: (row: AccountingPeriodDto) => new Date(row.periodStart).toLocaleDateString() },
            { header: 'Period end', render: (row: AccountingPeriodDto) => new Date(row.periodEnd).toLocaleDateString() },
            { header: 'Status', render: (row: AccountingPeriodDto) => row.status },
            {
              header: 'Actions',
              render: (row: AccountingPeriodDto) =>
                row.status === 'Open' ? (
                  <Button variant="danger" onClick={() => closePeriod.mutate(row.id)} disabled={closePeriod.isPending}>
                    Close
                  </Button>
                ) : (
                  <Button variant="secondary" onClick={() => reopenPeriod.mutate(row.id)} disabled={reopenPeriod.isPending}>
                    Reopen
                  </Button>
                ),
            },
          ]}
          rows={periodsQuery.data?.data}
          isLoading={periodsQuery.isLoading}
          isError={periodsQuery.isError}
          errorMessage="Could not load accounting periods."
          emptyMessage="No accounting periods yet — posting is unrestricted until you create one."
          rowKey={(row) => row.id}
        />
      </Card>
      {(closePeriod.isError || reopenPeriod.isError) && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not update that accounting period.</p>
      )}
    </div>
  );
}
