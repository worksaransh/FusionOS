import { useState } from 'react';
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
import { useProductOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';
import { RoutingOperationsPanel } from './RoutingOperationsPanel';
import { WorkOrdersPanel } from './WorkOrdersPanel';
import { MrpPanel } from './MrpPanel';
import { WorkInProgressSummaryPanel } from './WorkInProgressSummaryPanel';
import { WorkCentersPanel } from './WorkCentersPanel';
import { MachinesPanel } from './MachinesPanel';

const lineSchema = z.object({
  componentProductId: z.string().uuid('Pick a component'),
  quantity: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
});

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(20),
  name: z.string().min(1, 'Name is required').max(200),
  productId: z.string().uuid('Pick the product this BOM builds'),
  lines: z.array(lineSchema).min(1, 'At least one component line is required'),
});
type FormValues = z.infer<typeof schema>;

// BOM Versioning (Phase 6) — the "new version" form only needs a name and a fresh set of
// component lines; the product and code carry over from the BOM being superseded.
const newVersionSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  lines: z.array(lineSchema).min(1, 'At least one component line is required'),
});
type NewVersionFormValues = z.infer<typeof newVersionSchema>;

interface BomLineDto {
  id: string;
  componentProductId: string;
  quantity: number;
}

interface RoutingOperationDto {
  id: string;
  sequenceNumber: number;
  operationName: string;
  workCenter: string;
  standardMinutes: number;
}

interface BillOfMaterialsDto {
  id: string;
  code: string;
  name: string;
  productId: string;
  isActive: boolean;
  lines: BomLineDto[];
  operations: RoutingOperationDto[];
  version: number;
  isCurrentVersion: boolean;
  previousVersionId: string | null;
  isPrimary: boolean;
}

/**
 * Bills of Materials — Manufacturing's first real frontend slice (backend has
 * existed since the Manufacturing/CRM/Quality backend-only pass; this closes
 * the "frontend panel deferred" gap flagged in docs/PROJECT_TRACKER.md).
 * Top-level page for /manufacturing, same shape as AccountsPage/SuppliersPage:
 * a create form + list here, with WorkOrdersPanel rendered as a sibling
 * panel below it (mirrors AccountsPage composing 9 Finance sub-panels).
 */
export function BillsOfMaterialsPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', productId: '', lines: [{ componentProductId: '', quantity: '1' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  // BOM Versioning (Phase 6) — which row's "New version" form is currently open, if any.
  const [newVersionBomId, setNewVersionBomId] = useState<string | null>(null);
  const newVersionForm = useForm<NewVersionFormValues>({
    resolver: zodResolver(newVersionSchema),
    defaultValues: { name: '', lines: [{ componentProductId: '', quantity: '1' }] },
  });
  const newVersionLines = useFieldArray({ control: newVersionForm.control, name: 'lines' });

  // BOM Versioning (Phase 6) — which row's version history is currently shown, if any.
  const [historyBomId, setHistoryBomId] = useState<string | null>(null);

  // Multi-Level BOM explosion (Phase 6) — which row's explode panel is currently open, if any.
  const [explodingBomId, setExplodingBomId] = useState<string | null>(null);

  const bomsQuery = useQuery({
    queryKey: ['bills-of-materials', companyId],
    queryFn: () => apiClient.get<PagedResult<BillOfMaterialsDto>>(`/manufacturing/bills-of-materials?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createBom = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<BillOfMaterialsDto>('/manufacturing/bills-of-materials', {
        companyId,
        code: values.code,
        name: values.name,
        productId: values.productId,
        lines: values.lines.map((l) => ({ componentProductId: l.componentProductId, quantity: Number(l.quantity) })),
      }),
    onSuccess: () => {
      reset({ code: '', name: '', productId: '', lines: [{ componentProductId: '', quantity: '1' }] });
      queryClient.invalidateQueries({ queryKey: ['bills-of-materials', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — BillsOfMaterialsController exposes this as a
  // dedicated POST .../{id}/deactivate action, same convention as
  // AccountsController/CostCentersController.
  const deactivateBom = useMutation({
    mutationFn: (bomId: string) => apiClient.post<BillOfMaterialsDto>(`/manufacturing/bills-of-materials/${bomId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['bills-of-materials', companyId] }),
  });

  // BOM Versioning (Phase 6) — supersedes the open row with a new Version + 1 BOM via
  // BillsOfMaterialsController's POST .../{id}/new-version action.
  const createNewVersion = useMutation({
    mutationFn: (values: NewVersionFormValues) =>
      apiClient.post<BillOfMaterialsDto>(`/manufacturing/bills-of-materials/${newVersionBomId}/new-version`, {
        companyId,
        name: values.name,
        lines: values.lines.map((l) => ({ componentProductId: l.componentProductId, quantity: Number(l.quantity) })),
      }),
    onSuccess: () => {
      newVersionForm.reset({ name: '', lines: [{ componentProductId: '', quantity: '1' }] });
      setNewVersionBomId(null);
      queryClient.invalidateQueries({ queryKey: ['bills-of-materials', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          newVersionForm.setError(field as keyof NewVersionFormValues, { message: messages[0] });
        }
      }
    },
  });

  // Alternative BOM (Phase 6) — promotes a row to primary via POST .../{id}/set-primary,
  // clearing the flag on every other active BOM for that product server-side.
  const setPrimaryBom = useMutation({
    mutationFn: (bomId: string) => apiClient.post<BillOfMaterialsDto>(`/manufacturing/bills-of-materials/${bomId}/set-primary`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['bills-of-materials', companyId] }),
  });

  // BOM Versioning (Phase 6) — every version in the open row's version chain, newest first.
  const versionHistoryQuery = useQuery({
    queryKey: ['bill-of-materials-version-history', companyId, historyBomId],
    queryFn: () => apiClient.get<BillOfMaterialsDto[]>(`/manufacturing/bills-of-materials/${historyBomId}/version-history?companyId=${companyId}`),
    enabled: Boolean(companyId && historyBomId),
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage bills of materials.</p>;
  }

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Bills of Materials</h1>
      <p className="mb-4 text-sm text-text-muted">What a manufactured product is made of — Manufacturing, Phase 3.</p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createBom.mutate(values))} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <label className="flex flex-col gap-1 text-sm">
              Code
              <Controller
                control={control}
                name="code"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="BOM-100" {...field} />
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
              Product this BOM builds
              <Controller
                control={control}
                name="productId"
                render={({ field }) => (
                  <EntityCombobox
                    value={field.value}
                    onChange={field.onChange}
                    options={productOptions.options}
                    isLoading={productOptions.isLoading}
                    onSearchChange={productOptions.onSearchChange}
                    placeholder="Search products…"
                  />
                )}
              />
              {errors.productId && <span className="text-xs text-danger">{errors.productId.message}</span>}
            </label>
          </div>

          <div className="flex flex-col gap-2">
            {fields.map((field, index) => (
              <div key={field.id} className="flex items-end gap-2">
                <label className="flex flex-col gap-1 text-sm">
                  Component product
                  <Controller
                    control={control}
                    name={`lines.${index}.componentProductId`}
                    render={({ field: lineField }) => (
                      <EntityCombobox
                        className="w-72"
                        value={lineField.value}
                        onChange={lineField.onChange}
                        options={productOptions.options}
                        isLoading={productOptions.isLoading}
                        onSearchChange={productOptions.onSearchChange}
                        placeholder="Search products…"
                      />
                    )}
                  />
                  {errors.lines?.[index]?.componentProductId && (
                    <span className="text-xs text-danger">{errors.lines[index]?.componentProductId?.message}</span>
                  )}
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  Quantity
                  <Controller
                    control={control}
                    name={`lines.${index}.quantity`}
                    render={({ field: lineField }) => (
                      <input className="w-24 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
                    )}
                  />
                  {errors.lines?.[index]?.quantity && (
                    <span className="text-xs text-danger">{errors.lines[index]?.quantity?.message}</span>
                  )}
                </label>
                <Button type="button" variant="secondary" onClick={() => remove(index)} disabled={fields.length === 1}>
                  <Trash2 size={16} />
                </Button>
              </div>
            ))}
            {errors.lines && typeof errors.lines.message === 'string' && (
              <span className="text-xs text-danger">{errors.lines.message}</span>
            )}
            <Button type="button" variant="secondary" onClick={() => append({ componentProductId: '', quantity: '1' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add component
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Creating…' : 'Create bill of materials'}
          </Button>
        </form>
        {createBom.isError && createBom.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createBom.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Code', render: (row: BillOfMaterialsDto) => row.code },
            { header: 'Name', render: (row: BillOfMaterialsDto) => row.name },
            { header: 'Components', render: (row: BillOfMaterialsDto) => row.lines.length },
            { header: 'Operations', render: (row: BillOfMaterialsDto) => row.operations.length },
            {
              header: 'Version',
              render: (row: BillOfMaterialsDto) => (
                <span>
                  v{row.version}
                  {!row.isCurrentVersion && <span className="ml-1 text-xs text-text-muted">(superseded)</span>}
                </span>
              ),
            },
            {
              header: 'Primary',
              render: (row: BillOfMaterialsDto) => (
                <span
                  className={
                    row.isPrimary
                      ? 'rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary'
                      : 'text-xs text-text-muted'
                  }
                >
                  {row.isPrimary ? 'Primary' : 'Alternative'}
                </span>
              ),
            },
            { header: 'Status', render: (row: BillOfMaterialsDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: BillOfMaterialsDto) => (
                <div className="flex flex-wrap items-center gap-2">
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateBom.isPending}
                    onClick={() => deactivateBom.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                  <Button
                    type="button"
                    variant="secondary"
                    disabled={!row.isCurrentVersion}
                    onClick={() => {
                      setNewVersionBomId(row.id);
                      newVersionForm.reset({ name: row.name, lines: [{ componentProductId: '', quantity: '1' }] });
                    }}
                  >
                    New version
                  </Button>
                  <Button
                    type="button"
                    variant="secondary"
                    disabled={row.isPrimary || setPrimaryBom.isPending}
                    onClick={() => setPrimaryBom.mutate(row.id)}
                  >
                    {row.isPrimary ? 'Primary' : 'Set primary'}
                  </Button>
                  <Button
                    type="button"
                    variant="secondary"
                    onClick={() => setHistoryBomId(historyBomId === row.id ? null : row.id)}
                  >
                    {historyBomId === row.id ? 'Hide history' : 'Version history'}
                  </Button>
                  <Button
                    type="button"
                    variant="secondary"
                    onClick={() => setExplodingBomId(explodingBomId === row.id ? null : row.id)}
                  >
                    {explodingBomId === row.id ? 'Hide explosion' : 'Explode'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={bomsQuery.data?.data}
          isLoading={bomsQuery.isLoading}
          isError={bomsQuery.isError}
          errorMessage="Could not load bills of materials."
          emptyMessage="No bills of materials yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateBom.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that bill of materials.</p>
      )}
      {setPrimaryBom.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not set that bill of materials as primary.</p>
      )}

      {newVersionBomId && (
        <Card className="mt-6">
          <h3 className="mb-3 text-sm font-semibold text-text">New version</h3>
          <p className="mb-3 text-xs text-text-muted">
            Creates a new Version + 1 bill of materials with the component lines below, copying the current routing
            operations across, and marks the current version as superseded.
          </p>
          <form noValidate
            onSubmit={newVersionForm.handleSubmit((values) => createNewVersion.mutate(values))}
            className="flex flex-col gap-4"
          >
            <label className="flex flex-col gap-1 text-sm sm:max-w-md">
              Name
              <Controller
                control={newVersionForm.control}
                name="name"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
                )}
              />
              {newVersionForm.formState.errors.name && (
                <span className="text-xs text-danger">{newVersionForm.formState.errors.name.message}</span>
              )}
            </label>

            <div className="flex flex-col gap-2">
              {newVersionLines.fields.map((field, index) => (
                <div key={field.id} className="flex items-end gap-2">
                  <label className="flex flex-col gap-1 text-sm">
                    Component product
                    <Controller
                      control={newVersionForm.control}
                      name={`lines.${index}.componentProductId`}
                      render={({ field: lineField }) => (
                        <EntityCombobox
                          className="w-72"
                          value={lineField.value}
                          onChange={lineField.onChange}
                          options={productOptions.options}
                          isLoading={productOptions.isLoading}
                          onSearchChange={productOptions.onSearchChange}
                          placeholder="Search products…"
                        />
                      )}
                    />
                    {newVersionForm.formState.errors.lines?.[index]?.componentProductId && (
                      <span className="text-xs text-danger">
                        {newVersionForm.formState.errors.lines[index]?.componentProductId?.message}
                      </span>
                    )}
                  </label>
                  <label className="flex flex-col gap-1 text-sm">
                    Quantity
                    <Controller
                      control={newVersionForm.control}
                      name={`lines.${index}.quantity`}
                      render={({ field: lineField }) => (
                        <input className="w-24 rounded-md border border-border bg-surface px-2 py-1.5" {...lineField} />
                      )}
                    />
                    {newVersionForm.formState.errors.lines?.[index]?.quantity && (
                      <span className="text-xs text-danger">
                        {newVersionForm.formState.errors.lines[index]?.quantity?.message}
                      </span>
                    )}
                  </label>
                  <Button
                    type="button"
                    variant="secondary"
                    onClick={() => newVersionLines.remove(index)}
                    disabled={newVersionLines.fields.length === 1}
                  >
                    <Trash2 size={16} />
                  </Button>
                </div>
              ))}
              {newVersionForm.formState.errors.lines && typeof newVersionForm.formState.errors.lines.message === 'string' && (
                <span className="text-xs text-danger">{newVersionForm.formState.errors.lines.message}</span>
              )}
              <Button
                type="button"
                variant="secondary"
                onClick={() => newVersionLines.append({ componentProductId: '', quantity: '1' })}
                className="w-fit"
              >
                <Plus size={16} className="mr-1" /> Add component
              </Button>
            </div>

            <div className="flex gap-2">
              <Button type="submit" disabled={newVersionForm.formState.isSubmitting} className="w-fit">
                {newVersionForm.formState.isSubmitting ? 'Creating…' : 'Create new version'}
              </Button>
              <Button type="button" variant="secondary" className="w-fit" onClick={() => setNewVersionBomId(null)}>
                Cancel
              </Button>
            </div>
          </form>
          {createNewVersion.isError && createNewVersion.error instanceof ApiError && (
            <p role="alert" className="mt-2 text-sm text-danger">{createNewVersion.error.problem.title}</p>
          )}
        </Card>
      )}

      {historyBomId && (
        <Card className="mt-6">
          <h3 className="mb-3 text-sm font-semibold text-text">Version history</h3>
          <DataTable
            columns={[
              { header: 'Version', render: (row: BillOfMaterialsDto) => `v${row.version}` },
              { header: 'Name', render: (row: BillOfMaterialsDto) => row.name },
              { header: 'Current', render: (row: BillOfMaterialsDto) => (row.isCurrentVersion ? 'Yes' : 'No') },
              { header: 'Status', render: (row: BillOfMaterialsDto) => (row.isActive ? 'Active' : 'Inactive') },
            ]}
            rows={versionHistoryQuery.data}
            isLoading={versionHistoryQuery.isLoading}
            isError={versionHistoryQuery.isError}
            errorMessage="Could not load version history."
            emptyMessage="No version history."
            rowKey={(row) => row.id}
          />
        </Card>
      )}

      {explodingBomId && (
        <ExplodeBomPanel
          companyId={companyId}
          bomId={explodingBomId}
          productOptions={productOptions}
          onClose={() => setExplodingBomId(null)}
        />
      )}

      <RoutingOperationsPanel />
      <WorkOrdersPanel />
      <WorkInProgressSummaryPanel />
      <MrpPanel />
      <WorkCentersPanel />
      <MachinesPanel />
    </div>
  );
}

interface BomExplosionLineDto {
  productId: string;
  totalQuantityRequired: number;
}

interface BomExplosionResultDto {
  rootBillOfMaterialsId: string;
  requestedQuantity: number;
  leafRequirements: BomExplosionLineDto[];
  warnings: string[];
}

interface ExplodeBomPanelProps {
  companyId: string;
  bomId: string;
  productOptions: ReturnType<typeof useProductOptions>;
  onClose: () => void;
}

/**
 * Multi-Level BOM explosion (Phase 6) — prompts for a quantity to build and flattens the
 * bill of materials (recursing into any component's own primary BillOfMaterials) down to
 * its leaf, non-manufactured component requirements via BillsOfMaterialsController's POST
 * .../{id}/explode action. Same inline-panel-below-the-table shape as the "New version"
 * and "Version history" panels above.
 */
function ExplodeBomPanel({ companyId, bomId, productOptions, onClose }: ExplodeBomPanelProps) {
  const [quantity, setQuantity] = useState('1');

  const explode = useMutation({
    mutationFn: (qty: number) =>
      apiClient.post<BomExplosionResultDto>(`/manufacturing/bills-of-materials/${bomId}/explode`, {
        companyId,
        quantity: qty,
      }),
  });

  const parsedQuantity = Number(quantity);
  const quantityValid = Number.isFinite(parsedQuantity) && parsedQuantity > 0;

  return (
    <Card className="mt-6">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-lg font-semibold text-text">Explode bill of materials</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <p className="mb-3 text-xs text-text-muted">
        Shows every leaf (non-manufactured) component required to build the given quantity,
        recursing through any component's own bill of materials and summing repeated
        components across every level and branch.
      </p>

      <div className="flex items-end gap-2">
        <label className="flex flex-col gap-1 text-sm">
          Quantity to build
          <input
            className="w-32 rounded-md border border-border bg-surface px-2 py-1.5"
            value={quantity}
            onChange={(e) => setQuantity(e.target.value)}
          />
        </label>
        <Button
          type="button"
          disabled={!quantityValid || explode.isPending}
          onClick={() => explode.mutate(parsedQuantity)}
        >
          {explode.isPending ? 'Exploding…' : 'Explode'}
        </Button>
      </div>
      {!quantityValid && (
        <p className="mt-1 text-xs text-danger">Quantity must be a number greater than zero.</p>
      )}
      {explode.isError && explode.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{explode.error.problem.title}</p>
      )}

      {explode.data && (
        <>
          {explode.data.warnings.length > 0 && (
            <div className="mt-4 rounded-md border border-danger/40 bg-danger/5 p-3">
              {explode.data.warnings.map((warning, index) => (
                <p key={index} role="alert" className="text-xs text-danger">{warning}</p>
              ))}
            </div>
          )}

          <table className="mt-4 w-full text-sm">
            <thead>
              <tr className="text-left text-text-muted">
                <th className="pb-2">Component product</th>
                <th className="pb-2">Total quantity required</th>
              </tr>
            </thead>
            <tbody>
              {explode.data.leafRequirements.map((line) => (
                <tr key={line.productId} className="border-t border-border">
                  <td className="py-2">
                    {productOptions.options.find((p) => p.id === line.productId)?.label ?? line.productId}
                  </td>
                  <td className="py-2">{line.totalQuantityRequired}</td>
                </tr>
              ))}
              {explode.data.leafRequirements.length === 0 && (
                <tr>
                  <td colSpan={2} className="py-4 text-center text-text-muted">
                    No leaf component requirements.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </>
      )}
    </Card>
  );
}
