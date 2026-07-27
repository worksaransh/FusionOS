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
import { useWorkCenterOptions } from '../../../shared/api/entityOptions';

const schema = z.object({
  code: z.string().min(1, 'Code is required').max(50),
  name: z.string().min(1, 'Name is required').max(200),
  workCenterId: z.string().uuid('Pick a work center'),
});
type FormValues = z.infer<typeof schema>;

interface MachineDto {
  id: string;
  code: string;
  name: string;
  workCenterId: string;
  isActive: boolean;
}

/**
 * Machines — Machine Scheduling support (Phase 6, 2026-07-23) master data: a piece of
 * equipment belonging to exactly one WorkCenter. Since ListMachinesByWorkCenterQuery
 * requires a WorkCenterId (there's no company-wide machine list endpoint), the list is
 * scoped to a work center picked at the top of the panel — same "pick the parent first"
 * shape as ZonesPanel/BinsPanel under a Warehouse.
 */
export function MachinesPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const workCenterOptions = useWorkCenterOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { code: '', name: '', workCenterId: '' },
  });

  const listWorkCenterId = useForm<{ workCenterId: string }>({ defaultValues: { workCenterId: '' } });
  const filterWorkCenterId = listWorkCenterId.watch('workCenterId');

  const machinesQuery = useQuery({
    queryKey: ['machines', companyId, filterWorkCenterId],
    queryFn: () => apiClient.get<MachineDto[]>(`/manufacturing/machines?companyId=${companyId}&workCenterId=${filterWorkCenterId}`),
    enabled: Boolean(companyId && filterWorkCenterId),
  });

  const createMachine = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<MachineDto>('/manufacturing/machines', {
        companyId,
        code: values.code,
        name: values.name,
        workCenterId: values.workCenterId,
      }),
    onSuccess: () => {
      reset({ code: '', name: '', workCenterId: '' });
      queryClient.invalidateQueries({ queryKey: ['machines', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const deactivateMachine = useMutation({
    mutationFn: (machineId: string) => apiClient.post<MachineDto>(`/manufacturing/machines/${machineId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['machines', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Machines</h2>
      <p className="mb-3 text-sm text-text-muted">Equipment belonging to a work center, scheduled onto work order operations — Machine Scheduling, Phase 6.</p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createMachine.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            Code
            <Controller
              control={control}
              name="code"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="M-100" {...field} />
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
            Work center
            <Controller
              control={control}
              name="workCenterId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={workCenterOptions.options}
                  isLoading={workCenterOptions.isLoading}
                  placeholder="Search work centers…"
                />
              )}
            />
            {errors.workCenterId && <span className="text-xs text-danger">{errors.workCenterId.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create machine'}</Button>
          </div>
        </form>
        {createMachine.isError && createMachine.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createMachine.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <label className="mb-3 flex max-w-sm flex-col gap-1 text-sm">
          Filter by work center
          <Controller
            control={listWorkCenterId.control}
            name="workCenterId"
            render={({ field }) => (
              <EntityCombobox
                value={field.value}
                onChange={field.onChange}
                options={workCenterOptions.options}
                isLoading={workCenterOptions.isLoading}
                placeholder="Pick a work center to list its machines…"
              />
            )}
          />
        </label>

        {!filterWorkCenterId && <p className="text-sm text-text-muted">Pick a work center above to see its machines.</p>}

        {filterWorkCenterId && (
          <DataTable
            columns={[
              { header: 'Code', render: (row: MachineDto) => row.code },
              { header: 'Name', render: (row: MachineDto) => row.name },
              { header: 'Status', render: (row: MachineDto) => (row.isActive ? 'Active' : 'Inactive') },
              {
                header: 'Actions',
                render: (row: MachineDto) => (
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateMachine.isPending}
                    onClick={() => deactivateMachine.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                ),
              },
            ]}
            rows={machinesQuery.data}
            isLoading={machinesQuery.isLoading}
            isError={machinesQuery.isError}
            errorMessage="Could not load machines."
            emptyMessage="No machines yet for this work center — create one above."
            rowKey={(row) => row.id}
          />
        )}
      </Card>
      {deactivateMachine.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that machine.</p>
      )}
    </div>
  );
}
