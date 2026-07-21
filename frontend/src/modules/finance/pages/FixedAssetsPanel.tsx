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
import { useAccountOptions, useCostCenterOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  name: z.string().min(1, 'Name is required').max(200),
  assetAccountId: z.string().uuid('Pick the asset account'),
  accumulatedDepreciationAccountId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Account'),
  costCenterId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Cost Center'),
  acquisitionDate: z.string().min(1, 'Acquisition date is required'),
  acquisitionCost: z.string().refine((v) => v.trim() !== '' && Number(v) > 0, 'Acquisition cost must be greater than zero'),
  salvageValue: z.string().refine((v) => v.trim() !== '' && Number(v) >= 0, 'Salvage value must be zero or greater'),
  usefulLifeMonths: z.string().refine((v) => v.trim() !== '' && Number.isInteger(Number(v)) && Number(v) > 0, 'Useful life (months) must be a whole number greater than zero'),
});
type FormValues = z.infer<typeof schema>;

// Update deliberately excludes AssetAccountId/AccumulatedDepreciationAccountId/
// AcquisitionDate/AcquisitionCost/SalvageValue/UsefulLifeMonths — see
// FixedAsset.UpdateDetails's own doc comment for why those financial fields
// are not editable after creation.
const editSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  costCenterId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Cost Center'),
});
type EditFormValues = z.infer<typeof editSchema>;

const disposeSchema = z.object({
  disposedDate: z.string().min(1, 'Disposed date is required'),
});
type DisposeFormValues = z.infer<typeof disposeSchema>;

const scheduleSchema = z.object({
  asOfDate: z.string().min(1, 'As-of date is required'),
});
type ScheduleFormValues = z.infer<typeof scheduleSchema>;

const postDepreciationSchema = z.object({
  depreciationExpenseAccountId: z.string().uuid('Pick the depreciation expense account'),
  periodEnd: z.string().min(1, 'Period end date is required'),
});
type PostDepreciationFormValues = z.infer<typeof postDepreciationSchema>;

interface FixedAssetDto {
  id: string;
  code: string;
  name: string;
  assetAccountId: string;
  accumulatedDepreciationAccountId: string | null;
  costCenterId: string | null;
  acquisitionDate: string;
  acquisitionCost: number;
  salvageValue: number;
  usefulLifeMonths: number;
  isDisposed: boolean;
  disposedDate: string | null;
  isActive: boolean;
  createdAt: string;
}

interface DepreciationScheduleDto {
  fixedAssetId: string;
  monthlyDepreciationAmount: number;
  monthsElapsed: number;
  accumulatedDepreciation: number;
  bookValue: number;
}

interface JournalEntryLineDto {
  id: string;
  accountId: string;
  debit: number;
  credit: number;
  description: string | null;
  costCenterId: string | null;
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
 * Fixed Assets — M8g, Finance depth: fixed assets. Master data (Code/Name/
 * AssetAccountId/AccumulatedDepreciationAccountId/CostCenterId/
 * AcquisitionDate/AcquisitionCost/SalvageValue/UsefulLifeMonths) plus three
 * dedicated actions per row: "Dispose" (a genuine one-way business state
 * change — see FixedAsset.Dispose's own doc comment for why no gain/loss is
 * calculated), "View depreciation schedule" (a pure on-demand calculation —
 * see GetDepreciationScheduleQueryHandler's own doc comment: nothing is
 * persisted, no JournalEntry is ever created), and "Post Depreciation" (posts
 * one month of straight-line depreciation as a real Posted JournalEntry —
 * Debit the picked expense account, Credit the asset's accumulated-
 * depreciation account — see PostMonthlyDepreciationCommand's own doc
 * comment). Still no automated monthly depreciation *run*: each period is
 * posted one asset at a time, by hand, via that button.
 */
export function FixedAssetsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingAssetId, setEditingAssetId] = useState<string | null>(null);
  const [disposingAssetId, setDisposingAssetId] = useState<string | null>(null);
  const [scheduleAssetId, setScheduleAssetId] = useState<string | null>(null);
  const [postingDepreciationAssetId, setPostingDepreciationAssetId] = useState<string | null>(null);

  const accountOptions = useAccountOptions(companyId);
  const costCenterOptions = useCostCenterOptions(companyId);

  const fixedAssetsQuery = useQuery({
    queryKey: ['fixed-assets', companyId],
    queryFn: () => apiClient.get<PagedResult<FixedAssetDto>>(`/finance/fixed-assets?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      code: '', name: '', assetAccountId: '', accumulatedDepreciationAccountId: '', costCenterId: '',
      acquisitionDate: '', acquisitionCost: '', salvageValue: '', usefulLifeMonths: '',
    },
  });

  const createFixedAsset = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<FixedAssetDto>('/finance/fixed-assets', {
        companyId,
        code: values.code,
        name: values.name,
        assetAccountId: values.assetAccountId,
        accumulatedDepreciationAccountId: values.accumulatedDepreciationAccountId || null,
        costCenterId: values.costCenterId || null,
        acquisitionDate: new Date(values.acquisitionDate).toISOString(),
        acquisitionCost: Number(values.acquisitionCost),
        salvageValue: Number(values.salvageValue),
        usefulLifeMonths: Number(values.usefulLifeMonths),
      }),
    onSuccess: () => {
      reset({ code: '', name: '', assetAccountId: '', accumulatedDepreciationAccountId: '', costCenterId: '', acquisitionDate: '', acquisitionCost: '', salvageValue: '', usefulLifeMonths: '' });
      queryClient.invalidateQueries({ queryKey: ['fixed-assets', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — FixedAssetsController exposes this as a dedicated
  // POST .../{id}/deactivate action (never a DELETE), same convention as
  // every other M8 sub-slice. Independent of Dispose — see
  // FixedAsset.Deactivate's own doc comment for the distinction.
  const deactivateFixedAsset = useMutation({
    mutationFn: (fixedAssetId: string) => apiClient.post<FixedAssetDto>(`/finance/fixed-assets/${fixedAssetId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['fixed-assets', companyId] }),
  });

  if (!companyId) return null;

  const editingAsset = fixedAssetsQuery.data?.data.find((a) => a.id === editingAssetId) ?? null;
  const disposingAsset = fixedAssetsQuery.data?.data.find((a) => a.id === disposingAssetId) ?? null;
  const scheduleAsset = fixedAssetsQuery.data?.data.find((a) => a.id === scheduleAssetId) ?? null;
  const postingDepreciationAsset = fixedAssetsQuery.data?.data.find((a) => a.id === postingDepreciationAssetId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Fixed Assets</h2>
      <p className="mb-3 text-xs text-text-muted">
        Master data plus an on-demand straight-line depreciation calculation and a "Post Depreciation" action that
        posts one month of it as a real Posted JournalEntry to the GL. No automated monthly depreciation run — each
        period is still posted one asset at a time, by hand.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createFixedAsset.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-4">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="FA-100" {...field} />
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
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Delivery Van #3" {...field} />
              )}
            />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Asset account
            <Controller
              control={control}
              name="assetAccountId"
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
            {errors.assetAccountId && <span className="text-xs text-danger">{errors.assetAccountId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Accumulated depreciation account (optional)
            <Controller
              control={control}
              name="accumulatedDepreciationAccountId"
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
            {errors.accumulatedDepreciationAccountId && <span className="text-xs text-danger">{errors.accumulatedDepreciationAccountId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Cost center (optional)
            <Controller
              control={control}
              name="costCenterId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={costCenterOptions.options}
                  isLoading={costCenterOptions.isLoading}
                  onSearchChange={costCenterOptions.onSearchChange}
                  placeholder="Search cost centers…"
                />
              )}
            />
            {errors.costCenterId && <span className="text-xs text-danger">{errors.costCenterId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Acquisition date
            <Controller
              control={control}
              name="acquisitionDate"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.acquisitionDate && <span className="text-xs text-danger">{errors.acquisitionDate.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Acquisition cost
            <Controller
              control={control}
              name="acquisitionCost"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="24000.00" {...field} />
              )}
            />
            {errors.acquisitionCost && <span className="text-xs text-danger">{errors.acquisitionCost.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Salvage value
            <Controller
              control={control}
              name="salvageValue"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="4000.00" {...field} />
              )}
            />
            {errors.salvageValue && <span className="text-xs text-danger">{errors.salvageValue.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Useful life (months)
            <Controller
              control={control}
              name="usefulLifeMonths"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="60" {...field} />
              )}
            />
            {errors.usefulLifeMonths && <span className="text-xs text-danger">{errors.usefulLifeMonths.message}</span>}
          </label>
          <div className="col-span-4">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Registering…' : 'Register fixed asset'}</Button>
          </div>
        </form>
        {createFixedAsset.isError && createFixedAsset.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createFixedAsset.error.problem.title}</p>
        )}
      </Card>

      <Card className="mb-6">
        <DataTable
          columns={[
            { header: 'Code', render: (row: FixedAssetDto) => row.code },
            { header: 'Name', render: (row: FixedAssetDto) => row.name },
            { header: 'Acquired', render: (row: FixedAssetDto) => new Date(row.acquisitionDate).toLocaleDateString() },
            { header: 'Cost', render: (row: FixedAssetDto) => row.acquisitionCost.toLocaleString() },
            { header: 'Salvage', render: (row: FixedAssetDto) => row.salvageValue.toLocaleString() },
            { header: 'Life (mo)', render: (row: FixedAssetDto) => row.usefulLifeMonths },
            { header: 'Disposed', render: (row: FixedAssetDto) => (row.isDisposed ? new Date(row.disposedDate!).toLocaleDateString() : '—') },
            { header: 'Status', render: (row: FixedAssetDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: FixedAssetDto) => (
                <div className="flex flex-wrap items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setScheduleAssetId(row.id)}>
                    Depreciation schedule
                  </Button>
                  <Button type="button" variant="secondary" onClick={() => setEditingAssetId(row.id)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant="secondary"
                    disabled={row.isDisposed}
                    onClick={() => setDisposingAssetId(row.id)}
                  >
                    {row.isDisposed ? 'Disposed' : 'Dispose'}
                  </Button>
                  <Button
                    type="button"
                    variant="secondary"
                    disabled={row.isDisposed}
                    onClick={() => setPostingDepreciationAssetId(row.id)}
                  >
                    Post Depreciation
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateFixedAsset.isPending}
                    onClick={() => deactivateFixedAsset.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={fixedAssetsQuery.data?.data}
          isLoading={fixedAssetsQuery.isLoading}
          isError={fixedAssetsQuery.isError}
          errorMessage="Could not load fixed assets."
          emptyMessage="No fixed assets yet — register the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateFixedAsset.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that fixed asset.</p>
      )}

      {editingAsset && (
        <FixedAssetEditPanel companyId={companyId} fixedAsset={editingAsset} onClose={() => setEditingAssetId(null)} />
      )}

      {disposingAsset && (
        <FixedAssetDisposePanel companyId={companyId} fixedAsset={disposingAsset} onClose={() => setDisposingAssetId(null)} />
      )}

      {scheduleAsset && (
        <FixedAssetDepreciationSchedulePanel companyId={companyId} fixedAsset={scheduleAsset} onClose={() => setScheduleAssetId(null)} />
      )}

      {postingDepreciationAsset && (
        <FixedAssetPostDepreciationPanel
          companyId={companyId}
          fixedAsset={postingDepreciationAsset}
          onClose={() => setPostingDepreciationAssetId(null)}
        />
      )}
    </div>
  );
}

interface FixedAssetEditPanelProps {
  companyId: string;
  fixedAsset: FixedAssetDto;
  onClose: () => void;
}

function FixedAssetEditPanel({ companyId, fixedAsset, onClose }: FixedAssetEditPanelProps) {
  const queryClient = useQueryClient();
  const costCenterOptions = useCostCenterOptions(companyId);

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: {
      name: fixedAsset.name,
      costCenterId: fixedAsset.costCenterId ?? '',
    },
  });

  const updateFixedAsset = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<FixedAssetDto>(`/finance/fixed-assets/${fixedAsset.id}`, {
        companyId,
        name: values.name,
        costCenterId: values.costCenterId || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fixed-assets', companyId] });
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
        <h3 className="text-base font-semibold text-text">Edit fixed asset — {fixedAsset.code}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <p className="mb-3 text-xs text-text-muted">
        Only name and cost center are editable — acquisition cost, salvage value, useful life, and account references
        are fixed at creation (see FixedAsset.UpdateDetails's own doc comment).
      </p>
      <form onSubmit={handleSubmit((values) => updateFixedAsset.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
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
          Cost center (optional)
          <Controller
            control={control}
            name="costCenterId"
            render={({ field }) => (
              <EntityCombobox
                value={field.value}
                onChange={field.onChange}
                options={costCenterOptions.options}
                isLoading={costCenterOptions.isLoading}
                onSearchChange={costCenterOptions.onSearchChange}
                placeholder="Search cost centers…"
              />
            )}
          />
          {errors.costCenterId && <span className="text-xs text-danger">{errors.costCenterId.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateFixedAsset.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that fixed asset.</span>
          )}
        </div>
      </form>
    </Card>
  );
}

interface FixedAssetDisposePanelProps {
  companyId: string;
  fixedAsset: FixedAssetDto;
  onClose: () => void;
}

function FixedAssetDisposePanel({ companyId, fixedAsset, onClose }: FixedAssetDisposePanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, formState: { errors, isSubmitting } } = useForm<DisposeFormValues>({
    resolver: zodResolver(disposeSchema),
    defaultValues: { disposedDate: '' },
  });

  const disposeFixedAsset = useMutation({
    mutationFn: (values: DisposeFormValues) =>
      apiClient.post<FixedAssetDto>(`/finance/fixed-assets/${fixedAsset.id}/dispose`, {
        companyId,
        disposedDate: new Date(values.disposedDate).toISOString(),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fixed-assets', companyId] });
      onClose();
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Dispose — {fixedAsset.code}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <p className="mb-3 text-xs text-text-muted">
        Records the disposal date only — no gain/loss on disposal is calculated or posted to the GL (see
        FixedAsset.Dispose's own doc comment). This is a one-way business state change.
      </p>
      <form onSubmit={handleSubmit((values) => disposeFixedAsset.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Disposed date
          <Controller
            control={control}
            name="disposedDate"
            render={({ field }) => (
              <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.disposedDate && <span className="text-xs text-danger">{errors.disposedDate.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" variant="danger" disabled={isSubmitting}>{isSubmitting ? 'Recording…' : 'Record disposal'}</Button>
          {disposeFixedAsset.isError && (
            <span role="alert" className="text-sm text-danger">Could not record that disposal.</span>
          )}
        </div>
      </form>
    </Card>
  );
}

interface FixedAssetDepreciationSchedulePanelProps {
  companyId: string;
  fixedAsset: FixedAssetDto;
  onClose: () => void;
}

function FixedAssetDepreciationSchedulePanel({ companyId, fixedAsset, onClose }: FixedAssetDepreciationSchedulePanelProps) {
  const today = new Date().toISOString().slice(0, 10);

  const { control, watch } = useForm<ScheduleFormValues>({
    resolver: zodResolver(scheduleSchema),
    defaultValues: { asOfDate: today },
  });

  const asOfDate = watch('asOfDate');

  const scheduleQuery = useQuery({
    queryKey: ['fixed-asset-depreciation-schedule', companyId, fixedAsset.id, asOfDate],
    queryFn: () =>
      apiClient.get<DepreciationScheduleDto>(
        `/finance/fixed-assets/${fixedAsset.id}/depreciation-schedule?companyId=${companyId}&asOfDate=${new Date(asOfDate).toISOString()}`,
      ),
    enabled: Boolean(companyId && fixedAsset.id && asOfDate),
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Depreciation schedule — {fixedAsset.code}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <p className="mb-3 text-xs text-text-muted">
        Calculated on demand, straight-line, from this asset's own acquisition cost/salvage value/useful life. Nothing
        here is persisted and no journal entry is ever created — see GetDepreciationScheduleQueryHandler's own doc
        comment.
      </p>
      <form className="mb-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          As of date
          <Controller
            control={control}
            name="asOfDate"
            render={({ field }) => (
              <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
        </label>
      </form>

      {scheduleQuery.isLoading && <p role="status" className="text-text-muted">Loading…</p>}
      {scheduleQuery.isError && <p role="alert" className="text-danger">Could not calculate the depreciation schedule.</p>}
      {scheduleQuery.data && (
        <dl className="grid grid-cols-2 gap-4 text-sm sm:grid-cols-4">
          <div>
            <dt className="text-text-muted">Monthly depreciation</dt>
            <dd className="font-medium text-text">{scheduleQuery.data.monthlyDepreciationAmount.toLocaleString()}</dd>
          </div>
          <div>
            <dt className="text-text-muted">Months elapsed</dt>
            <dd className="font-medium text-text">{scheduleQuery.data.monthsElapsed}</dd>
          </div>
          <div>
            <dt className="text-text-muted">Accumulated depreciation</dt>
            <dd className="font-medium text-text">{scheduleQuery.data.accumulatedDepreciation.toLocaleString()}</dd>
          </div>
          <div>
            <dt className="text-text-muted">Book value</dt>
            <dd className="font-medium text-text">{scheduleQuery.data.bookValue.toLocaleString()}</dd>
          </div>
        </dl>
      )}
    </Card>
  );
}

interface FixedAssetPostDepreciationPanelProps {
  companyId: string;
  fixedAsset: FixedAssetDto;
  onClose: () => void;
}

/**
 * Posts one month of straight-line depreciation as a real Posted JournalEntry
 * (POST .../{id}/post-depreciation) — Debit the picked expense account,
 * Credit the asset's own AccumulatedDepreciationAccountId (fixed at creation,
 * not re-picked here). See PostMonthlyDepreciationCommand's own doc comment:
 * the asset must not be disposed and must already have an accumulated-
 * depreciation account set, and there is no duplicate/period-tracking guard —
 * posting the same period twice is possible and the user's own responsibility.
 */
function FixedAssetPostDepreciationPanel({ companyId, fixedAsset, onClose }: FixedAssetPostDepreciationPanelProps) {
  const queryClient = useQueryClient();
  const accountOptions = useAccountOptions(companyId);

  const { control, handleSubmit, formState: { errors, isSubmitting } } = useForm<PostDepreciationFormValues>({
    resolver: zodResolver(postDepreciationSchema),
    defaultValues: { depreciationExpenseAccountId: '', periodEnd: '' },
  });

  const postDepreciation = useMutation({
    mutationFn: (values: PostDepreciationFormValues) =>
      apiClient.post<JournalEntryDto>(`/finance/fixed-assets/${fixedAsset.id}/post-depreciation`, {
        companyId,
        depreciationExpenseAccountId: values.depreciationExpenseAccountId,
        periodEnd: new Date(values.periodEnd).toISOString(),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fixed-assets', companyId] });
      queryClient.invalidateQueries({ queryKey: ['journal-entries', companyId] });
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Post Depreciation — {fixedAsset.code}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <p className="mb-3 text-xs text-text-muted">
        Posts one month of straight-line depreciation as a real Posted JournalEntry — Debit the expense account below,
        Credit this asset's accumulated-depreciation account. Unlike the schedule above, this DOES write to the GL.
        There is no duplicate-posting guard, so make sure this period hasn't already been posted.
      </p>
      <form onSubmit={handleSubmit((values) => postDepreciation.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Depreciation expense account
          <Controller
            control={control}
            name="depreciationExpenseAccountId"
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
          {errors.depreciationExpenseAccountId && <span className="text-xs text-danger">{errors.depreciationExpenseAccountId.message}</span>}
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
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Posting…' : 'Post depreciation'}</Button>
          {postDepreciation.isError && postDepreciation.error instanceof ApiError && (
            <span role="alert" className="text-sm text-danger">{postDepreciation.error.problem.title}</span>
          )}
        </div>
      </form>
      {postDepreciation.isSuccess && (
        <dl className="mt-4 grid grid-cols-2 gap-4 border-t border-border pt-4 text-sm sm:grid-cols-4">
          <div>
            <dt className="text-text-muted">Journal entry</dt>
            <dd className="font-medium text-text">{postDepreciation.data.reference ?? postDepreciation.data.id.slice(0, 8)}</dd>
          </div>
          <div>
            <dt className="text-text-muted">Status</dt>
            <dd className="font-medium text-text">{postDepreciation.data.status}</dd>
          </div>
          <div>
            <dt className="text-text-muted">Entry date</dt>
            <dd className="font-medium text-text">{new Date(postDepreciation.data.entryDate).toLocaleDateString()}</dd>
          </div>
          <div>
            <dt className="text-text-muted">Amount</dt>
            <dd className="font-medium text-text">{postDepreciation.data.totalDebit.toLocaleString()}</dd>
          </div>
        </dl>
      )}
    </Card>
  );
}
