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

const schema = z.object({
  productId: z.string().uuid('Must be a valid Product id'),
  historyDays: z.coerce.number().int().min(1).max(3650),
  forecastDays: z.coerce.number().int().min(1).max(365),
});
type FormInput = z.input<typeof schema>;
type FormValues = z.output<typeof schema>;

interface ForecastPointDto {
  date: string;
  projectedQuantity: number;
}

interface DemandForecastDto {
  productId: string;
  isForecastable: boolean;
  insufficientDataNote: string | null;
  projectedDailyDemandTrend: number | null;
  movingAverageDailyDemand: number | null;
  projectedPoints: ForecastPointDto[];
}

interface RecommendationDto {
  id: string;
  type: string;
  referenceId: string;
  summary: string;
  modelVersion: string;
  status: string;
  decidedAt: string | null;
}

/**
 * Demand forecasting — Phase 8 "AI & Integrations". Displays the deterministic
 * linear-regression/moving-average forecast GetDemandForecastQuery computes
 * from real DemandHistoryEntry rows (fed by Sales' DispatchLineDispatched
 * events), and a button to turn it into a recorded "ReorderSuggestion"
 * Recommendation. This is a genuinely computed statistical projection, not a
 * real ML model — see DemandHistoryEntry/GetDemandForecastQuery's own doc
 * comments for why. Product id is a plain id input, not an EntityCombobox —
 * same convention as RecommendationsPage's ReferenceId field: a deliberately
 * opaque, never-validated cross-module reference. Rendered as a sibling panel
 * under RecommendationsPage, the same composition pattern quality/inventory
 * panels use.
 */
export function ForecastsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [lookup, setLookup] = useState<{ productId: string; historyDays: number; forecastDays: number } | null>(null);

  const { control, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormInput, unknown, FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { productId: '', historyDays: 90, forecastDays: 30 },
  });

  const forecastQuery = useQuery({
    queryKey: ['ai-forecast', companyId, lookup?.productId, lookup?.historyDays, lookup?.forecastDays],
    queryFn: () =>
      apiClient.get<DemandForecastDto>(
        `/ai/forecasts/${lookup!.productId}?companyId=${companyId}&historyDays=${lookup!.historyDays}&forecastDays=${lookup!.forecastDays}`,
      ),
    enabled: Boolean(companyId && lookup),
  });

  const generateReorderRecommendation = useMutation({
    mutationFn: () =>
      apiClient.post<RecommendationDto | undefined>(`/ai/forecasts/${lookup!.productId}/generate-reorder-recommendation`, {
        companyId,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recommendations', companyId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-1 text-lg font-semibold text-text">Demand Forecast</h2>
      <p className="mb-3 text-sm text-text-muted">
        A genuine, deliberately simple statistical forecast (linear regression + moving average) computed from real
        dispatch history — not a real ML model. See a product's projected daily demand and optionally record a
        reorder recommendation from it.
      </p>

      <Card className="mb-6">
        <form noValidate
          onSubmit={handleSubmit((values) => setLookup(values))}
          className="grid grid-cols-1 gap-4 sm:grid-cols-3"
        >
          <label className="flex flex-col gap-1 text-sm sm:col-span-1">
            Product id
            <Controller
              control={control}
              name="productId"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="00000000-0000-0000-0000-000000000000" {...field} />
              )}
            />
            {errors.productId && <span className="text-xs text-danger">{errors.productId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            History days
            <Controller
              control={control}
              name="historyDays"
              render={({ field }) => (
                <input type="number" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} value={field.value as number} />
              )}
            />
            {errors.historyDays && <span className="text-xs text-danger">{errors.historyDays.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Forecast days
            <Controller
              control={control}
              name="forecastDays"
              render={({ field }) => (
                <input type="number" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} value={field.value as number} />
              )}
            />
            {errors.forecastDays && <span className="text-xs text-danger">{errors.forecastDays.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Loading…' : 'Get forecast'}</Button>
          </div>
        </form>
      </Card>

      {forecastQuery.isLoading && <p role="status" className="text-text-muted">Loading forecast…</p>}
      {forecastQuery.isError && forecastQuery.error instanceof ApiError && (
        <p role="alert" className="text-sm text-danger">{forecastQuery.error.problem.title}</p>
      )}

      {forecastQuery.data && !forecastQuery.data.isForecastable && (
        <Card>
          <p className="text-sm text-text-muted">{forecastQuery.data.insufficientDataNote}</p>
        </Card>
      )}

      {forecastQuery.data && forecastQuery.data.isForecastable && (
        <>
          <Card className="mb-4">
            <dl className="grid grid-cols-1 gap-x-6 gap-y-2 text-sm sm:grid-cols-2">
              <div>
                <dt className="text-text-muted">Projected daily demand (linear trend)</dt>
                <dd className="font-medium text-text">{forecastQuery.data.projectedDailyDemandTrend?.toFixed(2)} / day</dd>
              </div>
              <div>
                <dt className="text-text-muted">Moving average daily demand</dt>
                <dd className="font-medium text-text">{forecastQuery.data.movingAverageDailyDemand?.toFixed(2)} / day</dd>
              </div>
            </dl>
            <div className="mt-4">
              <Button
                type="button"
                variant="secondary"
                disabled={generateReorderRecommendation.isPending}
                onClick={() => generateReorderRecommendation.mutate()}
              >
                {generateReorderRecommendation.isPending ? 'Generating…' : 'Generate reorder recommendation'}
              </Button>
              {generateReorderRecommendation.isSuccess && (
                <p className="mt-2 text-sm text-text-muted">
                  {generateReorderRecommendation.data
                    ? 'Recommendation recorded — see the Recommendations list above.'
                    : 'Demand is too low/flat right now — no recommendation was recorded.'}
                </p>
              )}
              {generateReorderRecommendation.isError && (
                <p role="alert" className="mt-2 text-sm text-danger">Could not generate a recommendation.</p>
              )}
            </div>
          </Card>

          <Card>
            <DataTable
              columns={[
                { header: 'Date', render: (row: ForecastPointDto) => new Date(row.date).toLocaleDateString() },
                { header: 'Projected quantity', render: (row: ForecastPointDto) => row.projectedQuantity.toFixed(2) },
              ]}
              rows={forecastQuery.data.projectedPoints}
              isLoading={false}
              emptyMessage="No projected points."
              rowKey={(row) => row.date}
            />
          </Card>
        </>
      )}
    </div>
  );
}
