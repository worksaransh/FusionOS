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

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage bills of materials.</p>;
  }

  return (
    <div>
      <h1 className="mb-1 text-xl font-semibold text-text">Bills of Materials</h1>
      <p className="mb-4 text-sm text-text-muted">What a manufactured product is made of — Manufacturing, Phase 3.</p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createBom.mutate(values))} className="flex flex-col gap-4">
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
            { header: 'Status', render: (row: BillOfMaterialsDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: BillOfMaterialsDto) => (
                <Button
                  type="button"
                  variant="danger"
                  disabled={!row.isActive || deactivateBom.isPending}
                  onClick={() => deactivateBom.mutate(row.id)}
                >
                  {row.isActive ? 'Deactivate' : 'Deactivated'}
                </Button>
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

      <RoutingOperationsPanel />
      <WorkOrdersPanel />
    </div>
  );
}
