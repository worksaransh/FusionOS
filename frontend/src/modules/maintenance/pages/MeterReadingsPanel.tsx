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
import { useAssetOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  assetId: z.string().uuid('Pick an asset'),
  readingValue: z.string().refine((v) => /^\d+(\.\d+)?$/.test(v) && Number(v) >= 0, 'Must be a non-negative number'),
  readingDate: z.string().min(1, 'Reading date is required'),
  unit: z.string().min(1, 'Unit is required').max(50),
});
type FormValues = z.infer<typeof schema>;

interface MeterReadingDto {
  id: string;
  assetId: string;
  readingValue: number;
  readingDate: string;
  unit: string;
  createdAt: string;
}

/**
 * Meter/Usage Readings — independent capture of an Asset's meter (hours run,
 * cycles, mileage, whatever unit that machine uses), separate from
 * MaintenanceSchedule's calendar-based due dates (see
 * MaintenanceScheduleFrequency.cs's doc comment for why usage-based scheduling
 * itself stays out of scope for this slice — this panel only records/lists
 * readings). Rendered as a sibling panel under AssetsPage, same pattern as
 * MaintenanceRequestsPanel/MaintenanceSchedulesPanel. A reading lower than the
 * asset's latest recorded one is rejected server-side (real meters are expected
 * to only increase) — surfaced here as a normal field-level error, same as any
 * other validation failure.
 */
export function MeterReadingsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [historyAssetId, setHistoryAssetId] = useState('');

  const assetOptions = useAssetOptions(companyId);
  const historyAssetOptions = useAssetOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { assetId: '', readingValue: '', readingDate: '', unit: 'hours' },
  });

  const readingsQuery = useQuery({
    queryKey: ['meter-readings', companyId, historyAssetId],
    queryFn: () => apiClient.get<PagedResult<MeterReadingDto>>(`/maintenance/meter-readings?companyId=${companyId}&assetId=${historyAssetId}&page=1&pageSize=50`),
    enabled: Boolean(companyId && historyAssetId),
  });

  const recordReading = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<MeterReadingDto>('/maintenance/meter-readings', {
        companyId,
        assetId: values.assetId,
        readingValue: Number(values.readingValue),
        readingDate: values.readingDate ? new Date(values.readingDate).toISOString() : null,
        unit: values.unit,
      }),
    onSuccess: (result) => {
      reset({ assetId: result.assetId, readingValue: '', readingDate: '', unit: result.unit });
      setHistoryAssetId(result.assetId);
      queryClient.invalidateQueries({ queryKey: ['meter-readings', companyId, result.assetId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Meter / Usage Readings</h2>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => recordReading.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-4">
          <label className="flex flex-col gap-1 text-sm">
            Asset
            <Controller
              control={control}
              name="assetId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={assetOptions.options}
                  isLoading={assetOptions.isLoading}
                  onSearchChange={assetOptions.onSearchChange}
                  placeholder="Search assets…"
                />
              )}
            />
            {errors.assetId && <span className="text-xs text-danger">{errors.assetId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Reading value
            <Controller
              control={control}
              name="readingValue"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="1234.5" {...field} />
              )}
            />
            {errors.readingValue && <span className="text-xs text-danger">{errors.readingValue.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Unit
            <Controller
              control={control}
              name="unit"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="hours" {...field} />
              )}
            />
            {errors.unit && <span className="text-xs text-danger">{errors.unit.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Reading date
            <Controller
              control={control}
              name="readingDate"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.readingDate && <span className="text-xs text-danger">{errors.readingDate.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Recording…' : 'Record reading'}</Button>
          </div>
        </form>
        {recordReading.isError && recordReading.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{recordReading.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <label className="mb-3 flex flex-col gap-1 text-sm sm:w-72">
          View history for asset
          <EntityCombobox
            value={historyAssetId}
            onChange={setHistoryAssetId}
            options={historyAssetOptions.options}
            isLoading={historyAssetOptions.isLoading}
            onSearchChange={historyAssetOptions.onSearchChange}
            placeholder="Search assets…"
          />
        </label>
        <DataTable
          columns={[
            { header: 'Reading date', render: (row: MeterReadingDto) => new Date(row.readingDate).toLocaleDateString() },
            { header: 'Value', render: (row: MeterReadingDto) => row.readingValue.toLocaleString() },
            { header: 'Unit', render: (row: MeterReadingDto) => row.unit },
            { header: 'Recorded', render: (row: MeterReadingDto) => new Date(row.createdAt).toLocaleString() },
          ]}
          rows={readingsQuery.data?.data}
          isLoading={readingsQuery.isLoading}
          isError={readingsQuery.isError}
          errorMessage="Could not load meter readings."
          emptyMessage={historyAssetId ? 'No meter readings recorded for this asset yet.' : 'Pick an asset above to view its meter reading history.'}
          rowKey={(row) => row.id}
        />
      </Card>
    </div>
  );
}
