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
import { usePurchaseOrderOptions, useSupplierOptions, useUserOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const SCORE_OPTIONS = [1, 2, 3, 4, 5] as const;

// Scores stay strings in the form schema (same convention as FeatureFlagsPage's
// rolloutPercentage) — a <select> still yields a string value, and
// z.coerce.number() has a mismatched input/output type zodResolver can't
// reconcile with useForm's single type parameter. Converted to a real number
// with Number(...) only at the mutation call site.
const schema = z.object({
  supplierId: z.string().uuid('Pick a supplier'),
  purchaseOrderId: z.string().optional().or(z.literal('')),
  qualityScore: z.string().refine((v) => Number.isInteger(Number(v)) && Number(v) >= 1 && Number(v) <= 5, 'Must be 1-5'),
  deliveryScore: z.string().refine((v) => Number.isInteger(Number(v)) && Number(v) >= 1 && Number(v) <= 5, 'Must be 1-5'),
  ratedByUserId: z.string().uuid('Pick who is recording this rating'),
  comments: z.string().max(2000).optional(),
});
type FormValues = z.infer<typeof schema>;

interface SupplierRatingDto {
  id: string;
  supplierId: string;
  purchaseOrderId: string | null;
  qualityScore: number;
  deliveryScore: number;
  comments: string | null;
  ratedByUserId: string;
  ratedAt: string;
}

interface SupplierScorecardLineDto {
  supplierId: string;
  orderCount: number;
  totalOrderValue: number;
  averageOrderValue: number;
  fullyReceivedCount: number;
  fullyReceivedRate: number;
  averageQualityScore: number | null;
  averageDeliveryScore: number | null;
  ratingCount: number | null;
}

/**
 * SupplierRating — a manually-recorded performance rating (quality/delivery,
 * 1-5, optional comments), a genuine judgment a procurement person enters
 * after a Purchase Order is fully received, or as a general periodic rating
 * with no PO reference. Deliberately separate from GetSupplierScorecardReport
 * (Reports/Contracts/SupplierScorecardReportDto), which is purely derived
 * from PurchaseOrder history — this panel both records new ratings and shows
 * the scorecard's averaged AverageQualityScore/AverageDeliveryScore/RatingCount
 * for whichever supplier is currently selected, since no other panel displays
 * the scorecard report yet.
 */
export function SupplierRatingsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const supplierOptions = useSupplierOptions(companyId);
  const purchaseOrderOptions = usePurchaseOrderOptions(companyId);
  const userOptions = useUserOptions(companyId);

  const { control, register, handleSubmit, reset, watch, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { supplierId: '', purchaseOrderId: '', qualityScore: '5', deliveryScore: '5', ratedByUserId: '', comments: '' },
  });
  const selectedSupplierId = watch('supplierId');

  const ratingsQuery = useQuery({
    queryKey: ['supplier-ratings', companyId, selectedSupplierId],
    queryFn: () =>
      apiClient.get<PagedResult<SupplierRatingDto>>(
        `/procurement/supplier-ratings?companyId=${companyId}&supplierId=${selectedSupplierId}&page=1&pageSize=25`,
      ),
    enabled: Boolean(companyId && selectedSupplierId),
  });

  const scorecardQuery = useQuery({
    queryKey: ['supplier-scorecard', companyId],
    queryFn: () => apiClient.get<SupplierScorecardLineDto[]>(`/procurement/reports/supplier-scorecard?companyId=${companyId}`),
    enabled: Boolean(companyId && selectedSupplierId),
  });
  const scorecardLine = scorecardQuery.data?.find((line) => line.supplierId === selectedSupplierId) ?? null;

  const createRating = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<SupplierRatingDto>('/procurement/supplier-ratings', {
        companyId,
        supplierId: values.supplierId,
        purchaseOrderId: values.purchaseOrderId || null,
        qualityScore: Number(values.qualityScore),
        deliveryScore: Number(values.deliveryScore),
        ratedByUserId: values.ratedByUserId,
        comments: values.comments || null,
      }),
    onSuccess: () => {
      reset({ supplierId: selectedSupplierId, purchaseOrderId: '', qualityScore: '5', deliveryScore: '5', ratedByUserId: '', comments: '' });
      queryClient.invalidateQueries({ queryKey: ['supplier-ratings', companyId, selectedSupplierId] });
      queryClient.invalidateQueries({ queryKey: ['supplier-scorecard', companyId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Supplier Ratings</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createRating.mutate(values))} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <label className="flex flex-col gap-1 text-sm">
              Supplier
              <Controller
                control={control}
                name="supplierId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={supplierOptions.options}
                    isLoading={supplierOptions.isLoading}
                    onSearchChange={supplierOptions.onSearchChange}
                    placeholder="Search suppliers…"
                  />
                )}
              />
              {errors.supplierId && <span className="text-xs text-danger">{errors.supplierId.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Purchase order (optional — must be fully received)
              <Controller
                control={control}
                name="purchaseOrderId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value ?? ''}
                    onChange={field.onChange}
                    options={purchaseOrderOptions.options}
                    isLoading={purchaseOrderOptions.isLoading}
                    placeholder="No specific order — general rating"
                  />
                )}
              />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Quality score (1-5)
              <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('qualityScore')}>
                {SCORE_OPTIONS.map((score) => (
                  <option key={score} value={score}>{score}</option>
                ))}
              </select>
              {errors.qualityScore && <span className="text-xs text-danger">{errors.qualityScore.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Delivery score (1-5)
              <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('deliveryScore')}>
                {SCORE_OPTIONS.map((score) => (
                  <option key={score} value={score}>{score}</option>
                ))}
              </select>
              {errors.deliveryScore && <span className="text-xs text-danger">{errors.deliveryScore.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Rated by
              <Controller
                control={control}
                name="ratedByUserId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={userOptions.options}
                    isLoading={userOptions.isLoading}
                    onSearchChange={userOptions.onSearchChange}
                    placeholder="Search users…"
                  />
                )}
              />
              {errors.ratedByUserId && <span className="text-xs text-danger">{errors.ratedByUserId.message}</span>}
            </label>
          </div>
          <label className="flex flex-col gap-1 text-sm">
            Comments (optional)
            <textarea className="rounded-md border border-border bg-surface px-2 py-1.5" rows={3} {...register('comments')} />
          </label>
          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Recording…' : 'Record rating'}
          </Button>
        </form>
        {createRating.isError && createRating.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createRating.error.problem.title}</p>
        )}
      </Card>

      {selectedSupplierId && scorecardLine && (
        <Card className="mb-6">
          <h3 className="mb-2 text-sm font-semibold text-text">Scorecard summary</h3>
          <dl className="grid grid-cols-2 gap-2 text-sm sm:grid-cols-4">
            <div>
              <dt className="text-text-muted">Orders</dt>
              <dd>{scorecardLine.orderCount}</dd>
            </div>
            <div>
              <dt className="text-text-muted">Fully received rate</dt>
              <dd>{scorecardLine.fullyReceivedRate.toFixed(1)}%</dd>
            </div>
            <div>
              <dt className="text-text-muted">Avg. quality score</dt>
              <dd>{scorecardLine.averageQualityScore !== null ? scorecardLine.averageQualityScore.toFixed(1) : 'No ratings yet'}</dd>
            </div>
            <div>
              <dt className="text-text-muted">Avg. delivery score</dt>
              <dd>{scorecardLine.averageDeliveryScore !== null ? scorecardLine.averageDeliveryScore.toFixed(1) : 'No ratings yet'}</dd>
            </div>
          </dl>
        </Card>
      )}

      {selectedSupplierId && ratingsQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Rated', render: (r: SupplierRatingDto) => new Date(r.ratedAt).toLocaleDateString() },
              { header: 'Quality', render: (r: SupplierRatingDto) => r.qualityScore },
              { header: 'Delivery', render: (r: SupplierRatingDto) => r.deliveryScore },
              { header: 'Purchase order', render: (r: SupplierRatingDto) => (r.purchaseOrderId ? r.purchaseOrderId.slice(0, 8) + '…' : 'General') },
              { header: 'Comments', render: (r: SupplierRatingDto) => r.comments ?? '—' },
            ]}
            rows={ratingsQuery.data.data}
            isLoading={ratingsQuery.isLoading}
            isError={ratingsQuery.isError}
            errorMessage="Could not load supplier ratings."
            emptyMessage="No ratings recorded for this supplier yet."
            rowKey={(r) => r.id}
          />
        </Card>
      )}
    </div>
  );
}
