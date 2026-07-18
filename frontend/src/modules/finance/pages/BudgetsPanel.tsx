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
  name: z.string().min(1, 'Name is required').max(200),
  periodStart: z.string().min(1, 'Period start is required'),
  periodEnd: z.string().min(1, 'Period end is required'),
});
type FormValues = z.infer<typeof schema>;

const lineSchema = z.object({
  accountId: z.string().uuid('Pick an account'),
  costCenterId: z.string().refine((v) => v === '' || /^[0-9a-fA-F-]{36}$/.test(v), 'Must be blank or a valid Cost Center'),
  budgetedAmount: z.string().refine((v) => v.trim() !== '' && Number(v) >= 0, 'Budgeted amount must be zero or greater'),
  notes: z.string().max(1000).optional(),
});
type LineFormValues = z.infer<typeof lineSchema>;

// Update deliberately excludes AccountId/CostCenterId — a line's identity is
// immutable (see BudgetLine.UpdateAmount's own doc comment / UpdateBudgetLineAmountCommand.cs).
const editLineSchema = z.object({
  budgetedAmount: z.string().refine((v) => v.trim() !== '' && Number(v) >= 0, 'Budgeted amount must be zero or greater'),
  notes: z.string().max(1000).optional(),
});
type EditLineFormValues = z.infer<typeof editLineSchema>;

interface BudgetDto {
  id: string;
  name: string;
  periodStart: string;
  periodEnd: string;
  isActive: boolean;
  createdAt: string;
}

interface BudgetLineDto {
  id: string;
  budgetId: string;
  accountId: string;
  costCenterId: string | null;
  budgetedAmount: number;
  notes: string | null;
  createdAt: string;
}

interface BudgetVsActualLineDto {
  accountId: string;
  accountCode: string;
  accountName: string;
  costCenterId: string | null;
  budgetedAmount: number;
  actualAmount: number;
  varianceAmount: number;
}

/**
 * Budgets — M8f, Finance depth: budgeting. Budget CRUD at the top (same
 * shape as ExchangeRatesPanel/CostCentersPanel), then — once a budget is
 * picked via "Manage lines" — its BudgetLine CRUD (per-Account, optionally
 * per-CostCenter, budgeted amount) and a read-only actual-vs-budget report
 * pulled from GetBudgetVsActualQuery. See Budget.cs's own class doc comment
 * for the scope line (no version history, no approval workflow, no
 * automated variance alerts) and GetBudgetVsActualQueryHandler's doc
 * comment for why a line's CostCenterId is shown but not actually used to
 * filter the actual amount (JournalEntryLine has no CostCenterId yet).
 */
export function BudgetsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingBudgetId, setEditingBudgetId] = useState<string | null>(null);
  const [managingBudgetId, setManagingBudgetId] = useState<string | null>(null);

  const budgetsQuery = useQuery({
    queryKey: ['budgets', companyId],
    queryFn: () => apiClient.get<PagedResult<BudgetDto>>(`/finance/budgets?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', periodStart: '', periodEnd: '' },
  });

  const createBudget = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<BudgetDto>('/finance/budgets', {
        companyId,
        name: values.name,
        periodStart: new Date(values.periodStart).toISOString(),
        periodEnd: new Date(values.periodEnd).toISOString(),
      }),
    onSuccess: () => {
      reset({ name: '', periodStart: '', periodEnd: '' });
      queryClient.invalidateQueries({ queryKey: ['budgets', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — BudgetsController exposes this as a dedicated
  // POST .../{id}/deactivate action (never a DELETE), same convention as
  // every other M8 sub-slice.
  const deactivateBudget = useMutation({
    mutationFn: (budgetId: string) => apiClient.post<BudgetDto>(`/finance/budgets/${budgetId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['budgets', companyId] }),
  });

  if (!companyId) return null;

  const editingBudget = budgetsQuery.data?.data.find((b) => b.id === editingBudgetId) ?? null;
  const managingBudget = budgetsQuery.data?.data.find((b) => b.id === managingBudgetId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Budgets</h2>
      <p className="mb-3 text-xs text-text-muted">
        Master data for a named period broken into per-account (optionally per-cost-center) budget lines, plus a
        read-only actual-vs-budget report computed from posted journal entries. No version history, approval
        workflow, or automated variance alerting in this slice — see Budget.cs's class doc comment.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createBudget.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            Name
            <Controller
              control={control}
              name="name"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="FY2026 Operating Budget" {...field} />
              )}
            />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Period start
            <Controller
              control={control}
              name="periodStart"
              render={({ field }) => (
                <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.periodStart && <span className="text-xs text-danger">{errors.periodStart.message}</span>}
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
          <div className="col-span-3">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create budget'}</Button>
          </div>
        </form>
        {createBudget.isError && createBudget.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createBudget.error.problem.title}</p>
        )}
      </Card>

      <Card className="mb-6">
        <DataTable
          columns={[
            { header: 'Name', render: (row: BudgetDto) => row.name },
            { header: 'Period start', render: (row: BudgetDto) => new Date(row.periodStart).toLocaleDateString() },
            { header: 'Period end', render: (row: BudgetDto) => new Date(row.periodEnd).toLocaleDateString() },
            { header: 'Status', render: (row: BudgetDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: BudgetDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setManagingBudgetId(row.id)}>
                    Manage lines
                  </Button>
                  <Button type="button" variant="secondary" onClick={() => setEditingBudgetId(row.id)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateBudget.isPending}
                    onClick={() => deactivateBudget.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={budgetsQuery.data?.data}
          isLoading={budgetsQuery.isLoading}
          isError={budgetsQuery.isError}
          errorMessage="Could not load budgets."
          emptyMessage="No budgets yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateBudget.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that budget.</p>
      )}

      {editingBudget && (
        <BudgetEditPanel companyId={companyId} budget={editingBudget} onClose={() => setEditingBudgetId(null)} />
      )}

      {managingBudget && (
        <BudgetLinesSection companyId={companyId} budget={managingBudget} onClose={() => setManagingBudgetId(null)} />
      )}
    </div>
  );
}

interface BudgetEditPanelProps {
  companyId: string;
  budget: BudgetDto;
  onClose: () => void;
}

function BudgetEditPanel({ companyId, budget, onClose }: BudgetEditPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    values: {
      name: budget.name,
      periodStart: budget.periodStart.slice(0, 10),
      periodEnd: budget.periodEnd.slice(0, 10),
    },
  });

  const updateBudget = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.put<BudgetDto>(`/finance/budgets/${budget.id}`, {
        companyId,
        name: values.name,
        periodStart: new Date(values.periodStart).toISOString(),
        periodEnd: new Date(values.periodEnd).toISOString(),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['budgets', companyId] });
      onClose();
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Edit budget — {budget.name}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateBudget.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
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
          Period start
          <Controller
            control={control}
            name="periodStart"
            render={({ field }) => (
              <input type="date" className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.periodStart && <span className="text-xs text-danger">{errors.periodStart.message}</span>}
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
        <div className="col-span-3 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateBudget.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that budget.</span>
          )}
        </div>
      </form>
    </Card>
  );
}

interface BudgetLinesSectionProps {
  companyId: string;
  budget: BudgetDto;
  onClose: () => void;
}

function BudgetLinesSection({ companyId, budget, onClose }: BudgetLinesSectionProps) {
  const queryClient = useQueryClient();
  const [editingLineId, setEditingLineId] = useState<string | null>(null);
  const accountOptions = useAccountOptions(companyId);
  const costCenterOptions = useCostCenterOptions(companyId);

  const linesQuery = useQuery({
    queryKey: ['budget-lines', companyId, budget.id],
    queryFn: () => apiClient.get<PagedResult<BudgetLineDto>>(`/finance/budgets/${budget.id}/lines?companyId=${companyId}&page=1&pageSize=50`),
    enabled: Boolean(companyId && budget.id),
  });

  const vsActualQuery = useQuery({
    queryKey: ['budget-vs-actual', companyId, budget.id],
    queryFn: () => apiClient.get<BudgetVsActualLineDto[]>(`/finance/budgets/${budget.id}/vs-actual?companyId=${companyId}`),
    enabled: Boolean(companyId && budget.id),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<LineFormValues>({
    resolver: zodResolver(lineSchema),
    defaultValues: { accountId: '', costCenterId: '', budgetedAmount: '', notes: '' },
  });

  const createLine = useMutation({
    mutationFn: (values: LineFormValues) =>
      apiClient.post<BudgetLineDto>(`/finance/budgets/${budget.id}/lines`, {
        companyId,
        accountId: values.accountId,
        costCenterId: values.costCenterId || null,
        budgetedAmount: Number(values.budgetedAmount),
        notes: values.notes || null,
      }),
    onSuccess: () => {
      reset({ accountId: '', costCenterId: '', budgetedAmount: '', notes: '' });
      queryClient.invalidateQueries({ queryKey: ['budget-lines', companyId, budget.id] });
      queryClient.invalidateQueries({ queryKey: ['budget-vs-actual', companyId, budget.id] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof LineFormValues, { message: messages[0] });
        }
      }
    },
  });

  const editingLine = linesQuery.data?.data.find((l) => l.id === editingLineId) ?? null;

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Manage lines — {budget.name}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>

      <form onSubmit={handleSubmit((values) => createLine.mutate(values))} className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-4">
        <label className="flex flex-col gap-1 text-sm">
          Account
          <Controller
            control={control}
            name="accountId"
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
          {errors.accountId && <span className="text-xs text-danger">{errors.accountId.message}</span>}
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
          Budgeted amount
          <Controller
            control={control}
            name="budgetedAmount"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="0.00" {...field} />
            )}
          />
          {errors.budgetedAmount && <span className="text-xs text-danger">{errors.budgetedAmount.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Notes (optional)
          <Controller
            control={control}
            name="notes"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.notes && <span className="text-xs text-danger">{errors.notes.message}</span>}
        </label>
        <div className="col-span-4">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Adding…' : 'Add budget line'}</Button>
        </div>
      </form>
      {createLine.isError && createLine.error instanceof ApiError && (
        <p role="alert" className="mb-4 text-sm text-danger">{createLine.error.problem.title}</p>
      )}

      <DataTable
        columns={[
          {
            header: 'Account',
            render: (row: BudgetLineDto) => accountOptions.options.find((a) => a.id === row.accountId)?.label ?? row.accountId,
          },
          {
            header: 'Cost center',
            render: (row: BudgetLineDto) =>
              row.costCenterId ? costCenterOptions.options.find((c) => c.id === row.costCenterId)?.label ?? row.costCenterId : '—',
          },
          { header: 'Budgeted', render: (row: BudgetLineDto) => row.budgetedAmount.toLocaleString() },
          { header: 'Notes', render: (row: BudgetLineDto) => row.notes ?? '—' },
          {
            header: 'Actions',
            render: (row: BudgetLineDto) => (
              <Button type="button" variant="secondary" onClick={() => setEditingLineId(row.id)}>
                Edit
              </Button>
            ),
          },
        ]}
        rows={linesQuery.data?.data}
        isLoading={linesQuery.isLoading}
        isError={linesQuery.isError}
        errorMessage="Could not load budget lines."
        emptyMessage="No budget lines yet — add the first one above."
        rowKey={(row) => row.id}
      />

      {editingLine && (
        <BudgetLineEditPanel
          companyId={companyId}
          budgetId={budget.id}
          line={editingLine}
          onClose={() => setEditingLineId(null)}
        />
      )}

      <div className="mt-6">
        <h4 className="mb-2 text-sm font-semibold text-text">Actual vs. budget</h4>
        <p className="mb-3 text-xs text-text-muted">
          Actual amounts come from Posted journal entries only, summed per account over this budget's period — not
          filtered by cost center even when a line has one, since journal entry lines don't carry a cost center yet
          (see GetBudgetVsActualQueryHandler's doc comment).
        </p>
        <DataTable
          columns={[
            { header: 'Account', render: (row: BudgetVsActualLineDto) => `${row.accountCode} — ${row.accountName}` },
            {
              header: 'Cost center',
              render: (row: BudgetVsActualLineDto) =>
                row.costCenterId ? costCenterOptions.options.find((c) => c.id === row.costCenterId)?.label ?? row.costCenterId : '—',
            },
            { header: 'Budgeted', render: (row: BudgetVsActualLineDto) => row.budgetedAmount.toLocaleString() },
            { header: 'Actual', render: (row: BudgetVsActualLineDto) => row.actualAmount.toLocaleString() },
            { header: 'Variance', render: (row: BudgetVsActualLineDto) => row.varianceAmount.toLocaleString() },
          ]}
          rows={vsActualQuery.data}
          isLoading={vsActualQuery.isLoading}
          isError={vsActualQuery.isError}
          errorMessage="Could not load the actual-vs-budget report."
          emptyMessage="No budget lines to report on yet."
          rowKey={(row) => row.accountId + (row.costCenterId ?? '')}
        />
      </div>
    </Card>
  );
}

interface BudgetLineEditPanelProps {
  companyId: string;
  budgetId: string;
  line: BudgetLineDto;
  onClose: () => void;
}

function BudgetLineEditPanel({ companyId, budgetId, line, onClose }: BudgetLineEditPanelProps) {
  const queryClient = useQueryClient();

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditLineFormValues>({
    resolver: zodResolver(editLineSchema),
    values: {
      budgetedAmount: String(line.budgetedAmount),
      notes: line.notes ?? '',
    },
  });

  const updateLine = useMutation({
    mutationFn: (values: EditLineFormValues) =>
      apiClient.put<BudgetLineDto>(`/finance/budgets/${budgetId}/lines/${line.id}`, {
        companyId,
        budgetedAmount: Number(values.budgetedAmount),
        notes: values.notes || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['budget-lines', companyId, budgetId] });
      queryClient.invalidateQueries({ queryKey: ['budget-vs-actual', companyId, budgetId] });
      onClose();
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof EditLineFormValues, { message: messages[0] });
        }
      }
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h4 className="text-sm font-semibold text-text">Edit budget line</h4>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateLine.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Budgeted amount
          <Controller
            control={control}
            name="budgetedAmount"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.budgetedAmount && <span className="text-xs text-danger">{errors.budgetedAmount.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Notes
          <Controller
            control={control}
            name="notes"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.notes && <span className="text-xs text-danger">{errors.notes.message}</span>}
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateLine.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that budget line.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
