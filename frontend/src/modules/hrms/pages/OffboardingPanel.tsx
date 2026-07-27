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
import { useEmployeeOptions } from '../../../shared/api/entityOptions';
import type { PagedResult } from '../../../shared/api/types';

const schema = z.object({
  employeeId: z.string().uuid('Pick an employee'),
  taskTitles: z.string().min(1, 'At least one task title is required'),
});
type FormValues = z.infer<typeof schema>;

interface WorkflowTaskDto {
  id: string;
  title: string;
  isCompleted: boolean;
  completedAt: string | null;
  notes: string | null;
}

interface EmployeeWorkflowDto {
  id: string;
  employeeId: string;
  kind: string;
  status: string;
  startedAt: string;
  completedAt: string | null;
  tasks: WorkflowTaskDto[];
}

/**
 * Offboarding checklists for departing employees (HRMS org-chart/Onboarding-Offboarding depth
 * pass, 2026-07-23). Backend-shared with OnboardingPanel (same EmployeeWorkflow entity, see
 * EmployeeWorkflow.cs's own doc comment) but this panel only ever talks to
 * api/v1/hrms/offboarding. Starting or completing an offboarding workflow never deactivates the
 * employee by itself — use the Deactivate action in the Employees table above for that,
 * explicitly and separately (see CreateOffboardingWorkflowCommand's own doc comment on the
 * backend for why this is deliberate).
 */
export function OffboardingPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [notesByTask, setNotesByTask] = useState<Record<string, string>>({});

  const employeeOptions = useEmployeeOptions(companyId);

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { employeeId: '', taskTitles: '' },
  });

  const workflowsQuery = useQuery({
    queryKey: ['offboarding-workflows', companyId],
    queryFn: () => apiClient.get<PagedResult<EmployeeWorkflowDto>>(`/hrms/offboarding?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const createWorkflow = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<EmployeeWorkflowDto>('/hrms/offboarding', {
        companyId,
        employeeId: values.employeeId,
        taskTitles: values.taskTitles.split(',').map((t) => t.trim()).filter(Boolean),
      }),
    onSuccess: () => {
      reset({ employeeId: '', taskTitles: '' });
      queryClient.invalidateQueries({ queryKey: ['offboarding-workflows', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  const completeTask = useMutation({
    mutationFn: ({ workflowId, taskId, notes }: { workflowId: string; taskId: string; notes: string }) =>
      apiClient.post<EmployeeWorkflowDto>(`/hrms/offboarding/${workflowId}/tasks/${taskId}/complete`, { companyId, notes: notes || null }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['offboarding-workflows', companyId] }),
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-1 text-lg font-semibold text-text">Offboarding</h2>
      <p className="mb-3 text-sm text-text-muted">
        Checklist-style offboarding workflows — enter task titles separated by commas. Starting or
        finishing a workflow here does not deactivate the employee; use Deactivate in the table
        above for that, separately.
      </p>

      <Card className="mb-6">
        <form noValidate onSubmit={handleSubmit((values) => createWorkflow.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Employee
            <Controller
              control={control}
              name="employeeId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={employeeOptions.options}
                  isLoading={employeeOptions.isLoading}
                  onSearchChange={employeeOptions.onSearchChange}
                  placeholder="Search employees…"
                />
              )}
            />
            {errors.employeeId && <span className="text-xs text-danger">{errors.employeeId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Task titles (comma-separated)
            <Controller
              control={control}
              name="taskTitles"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Return equipment, Revoke access, Exit interview" {...field} />
              )}
            />
            {errors.taskTitles && <span className="text-xs text-danger">{errors.taskTitles.message}</span>}
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Starting…' : 'Start offboarding'}</Button>
          </div>
        </form>
        {createWorkflow.isError && createWorkflow.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createWorkflow.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <DataTable
          columns={[
            { header: 'Employee', render: (row: EmployeeWorkflowDto) => employeeOptions.options.find((e) => e.id === row.employeeId)?.label ?? row.employeeId },
            { header: 'Started', render: (row: EmployeeWorkflowDto) => new Date(row.startedAt).toLocaleDateString() },
            { header: 'Status', render: (row: EmployeeWorkflowDto) => row.status },
            {
              header: 'Tasks',
              render: (row: EmployeeWorkflowDto) => (
                <ul className="flex flex-col gap-1">
                  {row.tasks.map((task) => (
                    <li key={task.id} className="flex items-center gap-2">
                      <span className={task.isCompleted ? 'text-text-muted line-through' : 'text-text'}>{task.title}</span>
                      {!task.isCompleted && (
                        <>
                          <input
                            className="w-32 rounded-md border border-border bg-surface px-1 py-0.5 text-xs"
                            placeholder="Notes (optional)"
                            value={notesByTask[task.id] ?? ''}
                            onChange={(e) => setNotesByTask((prev) => ({ ...prev, [task.id]: e.target.value }))}
                          />
                          <Button
                            type="button"
                            variant="secondary"
                            disabled={completeTask.isPending}
                            onClick={() => completeTask.mutate({ workflowId: row.id, taskId: task.id, notes: notesByTask[task.id] ?? '' })}
                          >
                            Complete
                          </Button>
                        </>
                      )}
                    </li>
                  ))}
                </ul>
              ),
            },
          ]}
          rows={workflowsQuery.data?.data}
          isLoading={workflowsQuery.isLoading}
          isError={workflowsQuery.isError}
          errorMessage="Could not load offboarding workflows."
          emptyMessage="No offboarding workflows yet — start the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {completeTask.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not complete that task.</p>
      )}
    </div>
  );
}
