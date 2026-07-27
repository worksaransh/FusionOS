import { Controller, useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Trash2 } from 'lucide-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import type { PagedResult } from '../../../shared/api/types';

const INSPECTION_TYPES = ['IncomingGoods', 'Production', 'Final'] as const;

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  inspectionType: z.enum(INSPECTION_TYPES),
  characteristicNames: z.array(z.object({ value: z.string().min(1, 'Characteristic cannot be blank').max(200) })).min(1, 'At least one characteristic is required'),
});
type FormValues = z.infer<typeof schema>;

export interface InspectionPlanCharacteristicDto {
  id: string;
  characteristicName: string;
  sequenceNumber: number;
}

export interface InspectionPlanDto {
  id: string;
  name: string;
  inspectionType: string;
  isActive: boolean;
  characteristics: InspectionPlanCharacteristicDto[];
}

/**
 * Inspection Plans — reusable checklist templates for a given InspectionType, so opening an
 * inspection of that kind doesn't require hand-typing the same characteristic list every
 * time. Purely a convenience default: InspectionsPage's create form below can pick one of
 * these to pre-fill its characteristic list, still editable before submit — see
 * CreateInspectionCommand's InspectionPlanId doc comment on the backend. Rendered as a
 * sibling panel under InspectionsPage.
 */
export function InspectionPlansPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();

  const plansQuery = useQuery({
    queryKey: ['inspection-plans', companyId],
    queryFn: () => apiClient.get<PagedResult<InspectionPlanDto>>(`/quality/inspection-plans?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', inspectionType: 'IncomingGoods', characteristicNames: [{ value: '' }] },
  });
  const { fields, append, remove } = useFieldArray({ control, name: 'characteristicNames' });

  const createPlan = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<InspectionPlanDto>('/quality/inspection-plans', {
        companyId,
        name: values.name,
        inspectionType: values.inspectionType,
        characteristicNames: values.characteristicNames.map((c) => c.value),
      }),
    onSuccess: () => {
      reset({ name: '', inspectionType: 'IncomingGoods', characteristicNames: [{ value: '' }] });
      queryClient.invalidateQueries({ queryKey: ['inspection-plans', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const deactivatePlan = useMutation({
    mutationFn: (id: string) => apiClient.post<InspectionPlanDto>(`/quality/inspection-plans/${id}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['inspection-plans', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Inspection Plans</h2>
      <p className="mb-3 text-sm text-text-muted">
        Reusable checklist templates — pick one when creating an inspection below to pre-fill its characteristics.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createPlan.mutate(values))} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <label className="flex flex-col gap-1 text-sm">
              Name
              <Controller
                control={control}
                name="name"
                render={({ field }) => (
                  <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Incoming Goods — Fasteners" {...field} />
                )}
              />
              {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
            </label>
            <label className="flex flex-col gap-1 text-sm">
              Inspection type
              <Controller
                control={control}
                name="inspectionType"
                render={({ field }) => (
                  <select className="rounded-md border border-border bg-surface px-2 py-1.5" {...field}>
                    {INSPECTION_TYPES.map((type) => (
                      <option key={type} value={type}>{type}</option>
                    ))}
                  </select>
                )}
              />
            </label>
          </div>

          <div className="flex flex-col gap-2">
            {fields.map((field, index) => (
              <div key={field.id} className="flex items-end gap-2">
                <label className="flex flex-col gap-1 text-sm">
                  Characteristic {index + 1}
                  <Controller
                    control={control}
                    name={`characteristicNames.${index}.value`}
                    render={({ field: lineField }) => (
                      <input className="w-72 rounded-md border border-border bg-surface px-2 py-1.5" placeholder="e.g. Dimensions within tolerance" {...lineField} />
                    )}
                  />
                </label>
                <Button type="button" variant="secondary" onClick={() => remove(index)} disabled={fields.length === 1}>
                  <Trash2 size={16} />
                </Button>
              </div>
            ))}
            {errors.characteristicNames && typeof errors.characteristicNames.message === 'string' && (
              <span className="text-xs text-danger">{errors.characteristicNames.message}</span>
            )}
            <Button type="button" variant="secondary" onClick={() => append({ value: '' })} className="w-fit">
              <Plus size={16} className="mr-1" /> Add characteristic
            </Button>
          </div>

          <Button type="submit" disabled={isSubmitting} className="w-fit">
            {isSubmitting ? 'Creating…' : 'Create plan'}
          </Button>
        </form>
        {createPlan.isError && createPlan.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createPlan.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Name', render: (row: InspectionPlanDto) => row.name },
            { header: 'Type', render: (row: InspectionPlanDto) => row.inspectionType },
            { header: 'Characteristics', render: (row: InspectionPlanDto) => row.characteristics.length },
            { header: 'Status', render: (row: InspectionPlanDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: InspectionPlanDto) =>
                row.isActive ? (
                  <Button type="button" variant="secondary" disabled={deactivatePlan.isPending} onClick={() => deactivatePlan.mutate(row.id)}>
                    Deactivate
                  </Button>
                ) : null,
            },
          ]}
          rows={plansQuery.data?.data}
          isLoading={plansQuery.isLoading}
          isError={plansQuery.isError}
          errorMessage="Could not load inspection plans."
          emptyMessage="No inspection plans yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivatePlan.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that inspection plan.</p>
      )}
    </div>
  );
}
