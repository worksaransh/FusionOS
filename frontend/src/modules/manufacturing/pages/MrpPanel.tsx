import { Controller, useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Trash2 } from 'lucide-react';
import { useMutation } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useProductOptions } from '../../../shared/api/entityOptions';

const demandLineSchema = z.object({
  productId: z.string().uuid('Pick a product'),
  quantity: z.string().refine((v) => Number(v) > 0, 'Quantity must be greater than zero'),
});

const schema = z.object({
  demandLines: z.array(demandLineSchema).min(1, 'At least one demand line is required'),
});
type FormValues = z.infer<typeof schema>;

interface MrpRequirementLineDto {
  productId: string;
  totalQuantityRequired: number;
}

interface MrpRequirementsResultDto {
  requirements: MrpRequirementLineDto[];
  warnings: string[];
}

/**
 * Material Requirements Planning — Phase 6. Lets a planner build a demand list (product +
 * quantity rows, same EntityCombobox/useFieldArray shape as BillsOfMaterialsPage's own
 * component-line editor) and run it against MrpController's POST mrp/run action, which
 * explodes every demand line's bill of materials and sums the leaf component totals
 * company-wide. Rendered as a sibling panel under BillsOfMaterialsPage, same composition
 * pattern as WorkOrdersPanel/WorkInProgressSummaryPanel.
 *
 * This is a GROSS requirements run only — it does not net against on-hand Inventory stock
 * or incoming Purchase Orders (Manufacturing has no reachable read access into either; see
 * GenerateMrpRequirementsQuery's doc comment for the full reasoning). The "Requirements"
 * column below is exactly what the demand would consume, not what still needs sourcing.
 */
export function MrpPanel() {
  const { companyId } = useActiveCompany();
  const productOptions = useProductOptions(companyId);

  const { control, handleSubmit, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { demandLines: [{ productId: '', quantity: '1' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'demandLines' });

  const runMrp = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<MrpRequirementsResultDto>('/manufacturing/mrp/run', {
        companyId,
        demandLines: values.demandLines.map((l) => ({ productId: l.productId, quantity: Number(l.quantity) })),
      }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Material Requirements Planning</h2>
      <p className="mb-4 text-sm text-text-muted">
        Build a demand list, then run a gross-requirements explosion across every demand
        line's bill of materials — Manufacturing, Phase 6.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => runMrp.mutate(values))} className="flex flex-col gap-4">
          <div className="flex flex-col gap-2">
            {fields.map((field, index) => (
              <div key={field.id} className="flex items-end gap-2">
                <label className="flex flex-col gap-1 text-sm">
                  Product
                  <Controller
                    control={control}
                    name={`demandLines.${index}.productId`}
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
                    name={`demandLines.${index}.quantity`}
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
            {errors.demandLines && typeof errors.demandLines.message === 'string' && (
              <span className="text-xs text-danger">{errors.demandLines.message}</span>
            )}
            <Button type="button" variant="secondary" onClick={() => append({ productId: '', quantity: '1' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add demand line
            </Button>
          </div>

          <Button type="submit" disabled={runMrp.isPending} className="w-fit">
            {runMrp.isPending ? 'Running…' : 'Run MRP'}
          </Button>
        </form>
        {runMrp.isError && runMrp.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{runMrp.error.problem.title}</p>
        )}
      </Card>

      {runMrp.data && (
        <Card>
          {runMrp.data.warnings.length > 0 && (
            <div className="mb-4 rounded-md border border-danger/40 bg-danger/5 p-3">
              {runMrp.data.warnings.map((warning, index) => (
                <p key={index} role="alert" className="text-xs text-danger">{warning}</p>
              ))}
            </div>
          )}

          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-text-muted">
                <th className="pb-2">Component product</th>
                <th className="pb-2">Total quantity required</th>
              </tr>
            </thead>
            <tbody>
              {runMrp.data.requirements.map((line) => (
                <tr key={line.productId} className="border-t border-border">
                  <td className="py-2">
                    {productOptions.options.find((p) => p.id === line.productId)?.label ?? line.productId}
                  </td>
                  <td className="py-2">{line.totalQuantityRequired}</td>
                </tr>
              ))}
              {runMrp.data.requirements.length === 0 && (
                <tr>
                  <td colSpan={2} className="py-4 text-center text-text-muted">
                    No requirements — every demand line either had no bill of materials or produced no leaf components.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </Card>
      )}
    </div>
  );
}
