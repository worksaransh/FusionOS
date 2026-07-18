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

const entrySchema = z.object({
  productId: z.string().uuid('Pick a product'),
  unitPrice: z.string().refine((v) => Number(v) >= 0, 'Unit price cannot be negative'),
});

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  entries: z.array(entrySchema).min(1, 'At least one entry is required'),
});
type FormValues = z.infer<typeof schema>;

interface PriceListEntryDto {
  id: string;
  productId: string;
  unitPrice: number;
}

export interface PriceListDto {
  id: string;
  name: string;
  isActive: boolean;
  entries: PriceListEntryDto[];
}

/**
 * PriceLists — the "multiple price lists per customer segment" half of the
 * pricing/discount engine (docs/IMPLEMENTATION_PLAN.md Phase 10 item 10).
 * A price list is assigned to individual Customers via a control on
 * CustomersPage's edit panel, not here (see CustomerEditPanel).
 */
export function PriceListsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, reset, register, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', entries: [{ productId: '', unitPrice: '0' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'entries' });

  const priceListsQuery = useQuery({
    queryKey: ['price-lists', companyId],
    queryFn: () => apiClient.get<PagedResult<PriceListDto>>(`/sales/price-lists?companyId=${companyId}&page=1&pageSize=25`),
    enabled: Boolean(companyId),
  });

  const createPriceList = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<PriceListDto>('/sales/price-lists', {
        companyId,
        name: values.name,
        entries: values.entries.map((e) => ({ productId: e.productId, unitPrice: Number(e.unitPrice) })),
      }),
    onSuccess: () => {
      reset({ name: '', entries: [{ productId: '', unitPrice: '0' }] });
      queryClient.invalidateQueries({ queryKey: ['price-lists', companyId] });
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Price Lists</h2>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createPriceList.mutate(values))} className="flex flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm">
            Name
            <input className="w-96 rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>

          <div className="flex flex-col gap-2">
            {fields.map((field, index) => (
              <div key={field.id} className="flex items-end gap-2">
                <label className="flex flex-col gap-1 text-sm">
                  Product
                  <Controller
                    control={control}
                    name={`entries.${index}.productId`}
                    render={({ field: entryField }) => (
                      <EntityCombobox
                        className="w-72"
                        value={entryField.value}
                        onChange={entryField.onChange}
                        options={productOptions.options}
                        isLoading={productOptions.isLoading}
                        onSearchChange={productOptions.onSearchChange}
                        placeholder="Search products…"
                      />
                    )}
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  Unit price
                  <Controller
                    control={control}
                    name={`entries.${index}.unitPrice`}
                    render={({ field: entryField }) => (
                      <input className="w-28 rounded-md border border-border bg-surface px-2 py-1.5" {...entryField} />
                    )}
                  />
                </label>
                <Button type="button" variant="secondary" onClick={() => remove(index)} disabled={fields.length === 1}>
                  <Trash2 size={16} />
                </Button>
              </div>
            ))}
            {errors.entries && typeof errors.entries.message === 'string' && (
              <span className="text-xs text-danger">{errors.entries.message}</span>
            )}
            <Button type="button" variant="secondary" onClick={() => append({ productId: '', unitPrice: '0' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add entry
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Creating…' : 'Create price list'}
          </Button>
        </form>
      </Card>

      {priceListsQuery.data && (
        <Card>
          <DataTable
            columns={[
              { header: 'Name', render: (pl: PriceListDto) => pl.name },
              { header: 'Entries', render: (pl: PriceListDto) => pl.entries.length },
              { header: 'Status', render: (pl: PriceListDto) => (pl.isActive ? 'Active' : 'Inactive') },
            ]}
            rows={priceListsQuery.data.data}
            isLoading={priceListsQuery.isLoading}
            emptyMessage="No price lists yet."
            rowKey={(pl) => pl.id}
          />
        </Card>
      )}
      {createPriceList.isError && createPriceList.error instanceof ApiError && (
        <p role="alert" className="mt-2 text-sm text-danger">{createPriceList.error.problem.title}</p>
      )}
    </div>
  );
}
