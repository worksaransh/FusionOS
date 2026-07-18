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
import { useUserOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  userId: z.string().uuid('Pick a salesperson'),
  ratePercentage: z.string().refine((v) => Number(v) >= 0 && Number(v) <= 100, 'Rate must be between 0 and 100'),
});
type FormValues = z.infer<typeof schema>;

interface SalesCommissionRateDto {
  id: string;
  userId: string;
  ratePercentage: number;
}

interface SalesCommissionSummaryLineDto {
  userId: string;
  totalInvoicedRevenue: number;
  ratePercentage: number;
  commissionAmount: number;
}

/**
 * Sales commissions — per-salesperson rate (%), applied to invoiced (not just
 * ordered) revenue, per docs/IMPLEMENTATION_PLAN.md Phase 10 item 11. Rates
 * are set/updated via an upsert (SetCommissionRateCommand); the summary
 * report is computed server-side from issued Invoices' SalesPersonId.
 */
export function CommissionsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const userOptions = useUserOptions(companyId);

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { userId: '', ratePercentage: '0' },
  });

  const ratesQuery = useQuery({
    queryKey: ['sales-commission-rates', companyId],
    queryFn: () => apiClient.get<PagedResult<SalesCommissionRateDto>>(`/sales/commissions/rates?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const summaryQuery = useQuery({
    queryKey: ['sales-commission-summary', companyId],
    queryFn: () => apiClient.get<SalesCommissionSummaryLineDto[]>(`/sales/commissions/summary-report?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  const setRate = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<SalesCommissionRateDto>('/sales/commissions/rates', {
        companyId,
        userId: values.userId,
        ratePercentage: Number(values.ratePercentage),
      }),
    onSuccess: () => {
      reset({ userId: '', ratePercentage: '0' });
      queryClient.invalidateQueries({ queryKey: ['sales-commission-rates', companyId] });
      queryClient.invalidateQueries({ queryKey: ['sales-commission-summary', companyId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Sales Commissions</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => setRate.mutate(values))} className="flex items-end gap-2">
          <label className="flex flex-col gap-1 text-sm">
            Salesperson
            <Controller
              control={control}
              name="userId"
              render={({ field }) => (
                <EntityCombobox
                  className="w-72"
                  value={field.value}
                  onChange={field.onChange}
                  options={userOptions.options}
                  isLoading={userOptions.isLoading}
                  onSearchChange={userOptions.onSearchChange}
                  placeholder="Search users…"
                />
              )}
            />
            {errors.userId && <span className="text-xs text-danger">{errors.userId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Rate %
            <Controller
              control={control}
              name="ratePercentage"
              render={({ field }) => (
                <input className="w-24 rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.ratePercentage && <span className="text-xs text-danger">{errors.ratePercentage.message}</span>}
          </label>
          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Saving…' : 'Set rate'}
          </Button>
        </form>
        {setRate.isError && setRate.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{setRate.error.problem.title}</p>
        )}
      </Card>

      {ratesQuery.data && (
        <Card className="mb-6">
          <DataTable
            columns={[
              { header: 'User ID', render: (rate: SalesCommissionRateDto) => rate.userId },
              { header: 'Rate %', render: (rate: SalesCommissionRateDto) => rate.ratePercentage.toString() },
            ]}
            rows={ratesQuery.data.data}
            isLoading={ratesQuery.isLoading}
            emptyMessage="No commission rates set yet."
            rowKey={(rate) => rate.id}
          />
        </Card>
      )}

      {summaryQuery.data && (
        <Card>
          <h3 className="mb-2 text-sm font-semibold text-text">Invoiced revenue summary</h3>
          <DataTable
            columns={[
              { header: 'User ID', render: (line: SalesCommissionSummaryLineDto) => line.userId },
              { header: 'Invoiced revenue', render: (line: SalesCommissionSummaryLineDto) => line.totalInvoicedRevenue.toLocaleString() },
              { header: 'Rate %', render: (line: SalesCommissionSummaryLineDto) => line.ratePercentage.toString() },
              { header: 'Commission', render: (line: SalesCommissionSummaryLineDto) => line.commissionAmount.toLocaleString() },
            ]}
            rows={summaryQuery.data}
            isLoading={summaryQuery.isLoading}
            emptyMessage="No invoiced revenue with a salesperson yet."
            rowKey={(line) => line.userId}
          />
        </Card>
      )}
    </div>
  );
}
