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
import { useProductOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const lineSchema = z.object({
  productId: z.string().uuid('Pick a product'),
  quantity: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
});

const decimalOptionalSchema = z.string().refine((v) => v === '' || !Number.isNaN(Number(v)), 'Must be a number');

const schema = z.object({
  packageNumber: z.string().min(1, 'Package number is required').max(50),
  weightKg: decimalOptionalSchema,
  lengthCm: decimalOptionalSchema,
  widthCm: decimalOptionalSchema,
  heightCm: decimalOptionalSchema,
  lines: z.array(lineSchema).min(1, 'At least one line is required'),
});
type FormValues = z.infer<typeof schema>;

interface PackageLineDto {
  id: string;
  productId: string;
  quantity: number;
}

interface PackageDto {
  id: string;
  pickListId: string;
  packageNumber: string;
  weightKg: number | null;
  lengthCm: number | null;
  widthCm: number | null;
  heightCm: number | null;
  lines: PackageLineDto[];
  createdAt: string;
}

/** Narrow shape PackagesPanel needs from a pick list — PickListsPanel's own PickListDto is a superset, so it's passed straight in. */
interface PickListForPackaging {
  id: string;
  status: 'Pending' | 'Assigned' | 'Picked' | 'Packed';
}

interface PackagesPanelProps {
  companyId: string;
  warehouseId: string;
  pickList: PickListForPackaging;
}

/**
 * Packages (2026-07-21 packing depth pass — docs follow-up to Phase M9's Picking+Packing slice).
 * PickList.Pack() only ever flips the pick list's own status flag to Packed; it never captured
 * WHICH items went into WHICH physical carton, nor any package weight/dimensions. This panel
 * records that detail as its own resource once a pick list has reached Packed — purely additive,
 * it does not change or replace the Pack button in PickListManagePanel above it.
 */
export function PackagesPanel({ companyId, warehouseId, pickList }: PackagesPanelProps) {
  const queryClient = useQueryClient();
  const productOptions = useProductOptions(companyId);
  const isPacked = pickList.status === 'Packed';

  const packagesQuery = useQuery({
    queryKey: ['packages', companyId, warehouseId, pickList.id],
    queryFn: () =>
      apiClient.get<PagedResult<PackageDto>>(
        `/warehouse/warehouses/${warehouseId}/pick-lists/${pickList.id}/packages?companyId=${companyId}&page=1&pageSize=25`,
      ),
    enabled: isPacked,
  });

  const defaultFormValues: FormValues = { packageNumber: '', weightKg: '', lengthCm: '', widthCm: '', heightCm: '', lines: [{ productId: '', quantity: '1' }] };

  const { control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: defaultFormValues,
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  const createPackage = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<PackageDto>(`/warehouse/warehouses/${warehouseId}/pick-lists/${pickList.id}/packages`, {
        companyId,
        packageNumber: values.packageNumber,
        weightKg: values.weightKg ? Number(values.weightKg) : null,
        lengthCm: values.lengthCm ? Number(values.lengthCm) : null,
        widthCm: values.widthCm ? Number(values.widthCm) : null,
        heightCm: values.heightCm ? Number(values.heightCm) : null,
        lines: values.lines.map((l) => ({ productId: l.productId, quantity: Number(l.quantity) })),
      }),
    onSuccess: () => {
      reset(defaultFormValues);
      queryClient.invalidateQueries({ queryKey: ['packages', companyId, warehouseId, pickList.id] });
    },
  });

  if (!isPacked) {
    return (
      <Card className="mt-4">
        <h3 className="mb-1 text-base font-semibold text-text">Packages</h3>
        <p className="text-sm text-text-muted">Packages can only be recorded once this pick list is fully packed.</p>
      </Card>
    );
  }

  return (
    <Card className="mt-4">
      <h3 className="mb-3 text-base font-semibold text-text">Packages</h3>

      <form noValidate onSubmit={handleSubmit((values) => createPackage.mutate(values))} className="flex flex-col gap-4">
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-5">
          <label className="flex flex-col gap-1 text-sm">
            Package number
            <Controller
              control={control}
              name="packageNumber"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="PKG-001" {...field} />
              )}
            />
            {errors.packageNumber && <span className="text-xs text-danger">{errors.packageNumber.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Weight (kg)
            <Controller
              control={control}
              name="weightKg"
              render={({ field }) => <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="optional" {...field} />}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Length (cm)
            <Controller
              control={control}
              name="lengthCm"
              render={({ field }) => <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="optional" {...field} />}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Width (cm)
            <Controller
              control={control}
              name="widthCm"
              render={({ field }) => <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="optional" {...field} />}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Height (cm)
            <Controller
              control={control}
              name="heightCm"
              render={({ field }) => <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="optional" {...field} />}
            />
          </label>
        </div>

        <div className="flex flex-col gap-2">
          {fields.map((field, index) => (
            <div key={field.id} className="flex items-end gap-2">
              <label className="flex flex-col gap-1 text-sm">
                Product
                <Controller
                  control={control}
                  name={`lines.${index}.productId`}
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
          <Button type="button" variant="secondary" onClick={() => append({ productId: '', quantity: '1' })} className="w-fit">
            <Plus size={16} className="mr-1" /> Add line
          </Button>
        </div>

        <Button type="submit" disabled={isSubmitting} className="w-fit">
          {isSubmitting ? 'Recording…' : 'Record package'}
        </Button>
        {createPackage.isError && createPackage.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createPackage.error.problem.title}</p>
        )}
      </form>

      {packagesQuery.data && (
        <div className="mt-4">
          <DataTable
            columns={[
              { header: 'Package #', render: (p: PackageDto) => p.packageNumber },
              { header: 'Lines', render: (p: PackageDto) => p.lines.length },
              { header: 'Weight (kg)', render: (p: PackageDto) => p.weightKg ?? '—' },
              {
                header: 'Dimensions L×W×H (cm)',
                render: (p: PackageDto) =>
                  p.lengthCm || p.widthCm || p.heightCm ? `${p.lengthCm ?? '—'}×${p.widthCm ?? '—'}×${p.heightCm ?? '—'}` : '—',
              },
            ]}
            rows={packagesQuery.data.data}
            isLoading={packagesQuery.isLoading}
            emptyMessage="No packages recorded yet for this pick list."
            rowKey={(p) => p.id}
          />
        </div>
      )}
    </Card>
  );
}
