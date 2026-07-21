import { useEffect } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { EntityCombobox } from '../../../shared/ui/EntityCombobox';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { useAccountOptions } from '../../../shared/api/entityOptions';

const optionalUuid = z.union([z.string().uuid(), z.literal('')]);

const schema = z.object({
  defaultArAccountId: optionalUuid,
  defaultSalesRevenueAccountId: optionalUuid,
  defaultApAccountId: optionalUuid,
  defaultPurchaseExpenseAccountId: optionalUuid,
});
type FormValues = z.infer<typeof schema>;

interface FinanceSettingsDto {
  companyId: string;
  defaultArAccountId: string | null;
  defaultSalesRevenueAccountId: string | null;
  defaultApAccountId: string | null;
  defaultPurchaseExpenseAccountId: string | null;
}

/**
 * Finance Settings — the default-account mapping (Phase 2 closeout,
 * 2026-07-18) InvoiceIssuedConsumer/CreditNoteIssuedConsumer/
 * PurchaseOrderGoodsReceiptCostedConsumer use to auto-post a GL entry
 * alongside the AR/AP subledger entry they already write. Leaving any of the
 * four blank keeps that consumer's subledger-only behavior — GL posting
 * turns on automatically for whichever pair (AR+Revenue, AP+Expense) gets
 * configured, no separate toggle needed.
 */
export function FinanceSettingsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const accountOptions = useAccountOptions(companyId);

  const settingsQuery = useQuery({
    queryKey: ['finance-settings', companyId],
    queryFn: () => apiClient.get<FinanceSettingsDto>(`/finance/settings?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { defaultArAccountId: '', defaultSalesRevenueAccountId: '', defaultApAccountId: '', defaultPurchaseExpenseAccountId: '' },
  });

  useEffect(() => {
    if (settingsQuery.data) {
      reset({
        defaultArAccountId: settingsQuery.data.defaultArAccountId ?? '',
        defaultSalesRevenueAccountId: settingsQuery.data.defaultSalesRevenueAccountId ?? '',
        defaultApAccountId: settingsQuery.data.defaultApAccountId ?? '',
        defaultPurchaseExpenseAccountId: settingsQuery.data.defaultPurchaseExpenseAccountId ?? '',
      });
    }
  }, [settingsQuery.data, reset]);

  const updateSettings = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.put<FinanceSettingsDto>('/finance/settings', {
        companyId,
        defaultArAccountId: values.defaultArAccountId || null,
        defaultSalesRevenueAccountId: values.defaultSalesRevenueAccountId || null,
        defaultApAccountId: values.defaultApAccountId || null,
        defaultPurchaseExpenseAccountId: values.defaultPurchaseExpenseAccountId || null,
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['finance-settings', companyId] }),
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  if (!companyId) return null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Finance Settings</h2>

      <Card>
        <form onSubmit={handleSubmit((values) => updateSettings.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Default AR account
            <Controller
              control={control}
              name="defaultArAccountId"
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
            {errors.defaultArAccountId && <span className="text-xs text-danger">{errors.defaultArAccountId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Default sales revenue account
            <Controller
              control={control}
              name="defaultSalesRevenueAccountId"
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
            {errors.defaultSalesRevenueAccountId && <span className="text-xs text-danger">{errors.defaultSalesRevenueAccountId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Default AP account
            <Controller
              control={control}
              name="defaultApAccountId"
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
            {errors.defaultApAccountId && <span className="text-xs text-danger">{errors.defaultApAccountId.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Default purchase expense account
            <Controller
              control={control}
              name="defaultPurchaseExpenseAccountId"
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
            {errors.defaultPurchaseExpenseAccountId && <span className="text-xs text-danger">{errors.defaultPurchaseExpenseAccountId.message}</span>}
          </label>

          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save settings'}</Button>
          </div>
        </form>
        {updateSettings.isError && updateSettings.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{updateSettings.error.problem.title}</p>
        )}
      </Card>
    </div>
  );
}
