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

const CALIBRATION_RESULTS = ['Pass', 'Fail', 'Adjusted'] as const;

const schema = z.object({
  equipmentReferenceId: z.string().uuid('Must be a valid equipment id (e.g. a Maintenance Asset id)'),
  equipmentName: z.string().min(1, 'Equipment name is required').max(200),
  equipmentIdentifier: z.string().max(100).or(z.literal('')),
  calibrationStandard: z.string().max(200).or(z.literal('')),
  lastCalibratedDate: z.string().min(1, 'Last calibrated date is required'),
  nextCalibrationDueDate: z.string().min(1, 'Next calibration due date is required'),
  result: z.enum(CALIBRATION_RESULTS),
  performedByUserId: z.string().uuid('Must be a valid user id').or(z.literal('')),
  performedByExternalVendor: z.string().max(200).or(z.literal('')),
  notes: z.string().max(1000).or(z.literal('')),
});
type FormValues = z.infer<typeof schema>;

interface CalibrationRecordDto {
  id: string;
  equipmentReferenceId: string;
  equipmentName: string;
  equipmentIdentifier: string | null;
  calibrationStandard: string | null;
  lastCalibratedDate: string;
  nextCalibrationDueDate: string;
  result: string;
  performedByUserId: string | null;
  performedByExternalVendor: string | null;
  notes: string | null;
  isOverdue: boolean;
}

/**
 * Calibration Records — tracks calibration state for measurement/test equipment (test gauges,
 * scales, calipers, thermometers). One row per piece of equipment holding its current
 * calibration state (see CalibrationRecord's backend doc comment for the current-state-vs-
 * history scope decision) — this panel only covers listing + registering new equipment;
 * recording a follow-up calibration event against existing equipment
 * (POST /quality/calibrations/{id}/calibrate) is a reasonable, separately-scoped follow-up,
 * same as the backend's own history-log note. Rendered as a sibling panel under
 * InspectionsPage, same pattern as CertificatesPanel.
 */
export function CalibrationRecordsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const calibrationsQuery = useQuery({
    queryKey: ['calibration-records', companyId],
    queryFn: () => apiClient.get<PagedResult<CalibrationRecordDto>>(`/quality/calibrations?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      equipmentReferenceId: '',
      equipmentName: '',
      equipmentIdentifier: '',
      calibrationStandard: '',
      lastCalibratedDate: '',
      nextCalibrationDueDate: '',
      result: 'Pass',
      performedByUserId: '',
      performedByExternalVendor: '',
      notes: '',
    },
  });

  const createCalibrationRecord = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<CalibrationRecordDto>('/quality/calibrations', {
        companyId,
        equipmentReferenceId: values.equipmentReferenceId,
        equipmentName: values.equipmentName,
        equipmentIdentifier: values.equipmentIdentifier || null,
        calibrationStandard: values.calibrationStandard || null,
        lastCalibratedDate: new Date(values.lastCalibratedDate).toISOString(),
        nextCalibrationDueDate: new Date(values.nextCalibrationDueDate).toISOString(),
        result: values.result,
        performedByUserId: values.performedByUserId || null,
        performedByExternalVendor: values.performedByExternalVendor || null,
        notes: values.notes || null,
      }),
    onSuccess: () => {
      reset();
      queryClient.invalidateQueries({ queryKey: ['calibration-records', companyId] });
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
      <h2 className="mb-3 text-lg font-semibold text-text">Calibration Records</h2>
      <p className="mb-3 text-sm text-text-muted">
        Track calibration state for measurement/test equipment (gauges, scales, calipers,
        thermometers). Equipment reference id is an opaque cross-module id — it may point at a
        Maintenance Asset, but is never validated here.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createCalibrationRecord.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Equipment reference id
            <Controller
              control={control}
              name="equipmentReferenceId"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="00000000-0000-0000-0000-000000000000" {...field} />
              )}
            />
            {errors.equipmentReferenceId && <span className="text-xs text-danger">{errors.equipmentReferenceId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Equipment name
            <Controller
              control={control}
              name="equipmentName"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="e.g. Digital caliper #3" {...field} />
              )}
            />
            {errors.equipmentName && <span className="text-xs text-danger">{errors.equipmentName.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Equipment identifier (optional)
            <Controller
              control={control}
              name="equipmentIdentifier"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Asset tag / serial number" {...field} />
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Calibration standard (optional)
            <Controller
              control={control}
              name="calibrationStandard"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="e.g. ISO 17025" {...field} />
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Last calibrated date
            <Controller
              control={control}
              name="lastCalibratedDate"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.lastCalibratedDate && <span className="text-xs text-danger">{errors.lastCalibratedDate.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Next calibration due date
            <Controller
              control={control}
              name="nextCalibrationDueDate"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.nextCalibrationDueDate && <span className="text-xs text-danger">{errors.nextCalibrationDueDate.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Result
            <Controller
              control={control}
              name="result"
              render={({ field }) => (
                <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                  {CALIBRATION_RESULTS.map((result) => (
                    <option key={result} value={result}>{result}</option>
                  ))}
                </select>
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Performed by user id (optional)
            <Controller
              control={control}
              name="performedByUserId"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="00000000-0000-0000-0000-000000000000" {...field} />
              )}
            />
            {errors.performedByUserId && <span className="text-xs text-danger">{errors.performedByUserId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Performed by external vendor (optional)
            <Controller
              control={control}
              name="performedByExternalVendor"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="e.g. Acme Calibration Services" {...field} />
              )}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Notes (optional)
            <Controller
              control={control}
              name="notes"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Registering…' : 'Register equipment'}</Button>
          </div>
        </form>
        {createCalibrationRecord.isError && createCalibrationRecord.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createCalibrationRecord.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Equipment', render: (row: CalibrationRecordDto) => row.equipmentName },
            { header: 'Identifier', render: (row: CalibrationRecordDto) => row.equipmentIdentifier ?? '—' },
            { header: 'Standard', render: (row: CalibrationRecordDto) => row.calibrationStandard ?? '—' },
            { header: 'Last calibrated', render: (row: CalibrationRecordDto) => new Date(row.lastCalibratedDate).toLocaleDateString() },
            {
              header: 'Next due',
              render: (row: CalibrationRecordDto) => (
                <span className={row.isOverdue ? 'font-medium text-danger' : ''}>
                  {new Date(row.nextCalibrationDueDate).toLocaleDateString()}{row.isOverdue ? ' (overdue)' : ''}
                </span>
              ),
            },
            { header: 'Result', render: (row: CalibrationRecordDto) => row.result },
          ]}
          rows={calibrationsQuery.data?.data}
          isLoading={calibrationsQuery.isLoading}
          isError={calibrationsQuery.isError}
          errorMessage="Could not load calibration records."
          emptyMessage="No equipment registered for calibration tracking yet — register one above."
          rowKey={(row) => row.id}
        />
      </Card>
    </div>
  );
}
