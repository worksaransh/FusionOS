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
import { useCrmAccountOptions, useLeadOptions } from '../../../shared/api/entityOptions';
import { useDebouncedValue } from '../../../shared/hooks/useDebouncedValue';
import type { PagedResult } from '../../../shared/api/types';

const SEARCH_DEBOUNCE_MS = 250;

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  email: z.string().email('Must be a valid email').max(320).or(z.literal('')),
  phone: z.string().max(50).or(z.literal('')),
  title: z.string().max(100).or(z.literal('')),
  accountId: z.string(),
  leadId: z.string(),
});
type FormValues = z.infer<typeof schema>;

const editSchema = schema;
type EditFormValues = z.infer<typeof editSchema>;

interface ContactDto {
  id: string;
  name: string;
  email: string | null;
  phone: string | null;
  title: string | null;
  accountId: string | null;
  leadId: string | null;
  isActive: boolean;
  createdAt: string;
}

/**
 * Contacts — CRM depth pass (2026-07-20). A named individual, usually belonging to an
 * Account but capturable straight off a Lead before an Account exists (see Contact.cs).
 * Rendered as a sibling panel under LeadsPage, same stacking pattern as
 * OpportunitiesPanel/CrmAccountsPanel/ActivitiesPanel.
 */
export function ContactsPanel() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [editingContactId, setEditingContactId] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);

  const accountOptions = useCrmAccountOptions(companyId);
  const leadOptions = useLeadOptions(companyId);

  const contactsQuery = useQuery({
    queryKey: ['contacts', companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId, page: '1', pageSize: '50' });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<ContactDto>>(`/crm/contacts?${params.toString()}`);
    },
    enabled: Boolean(companyId),
  });

  const { control, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', email: '', phone: '', title: '', accountId: '', leadId: '' },
  });

  const createContact = useMutation({
    mutationFn: (values: FormValues) =>
      apiClient.post<ContactDto>('/crm/contacts', {
        companyId,
        name: values.name,
        email: values.email || null,
        phone: values.phone || null,
        title: values.title || null,
        accountId: values.accountId || null,
        leadId: values.leadId || null,
      }),
    onSuccess: () => {
      reset({ name: '', email: '', phone: '', title: '', accountId: '', leadId: '' });
      queryClient.invalidateQueries({ queryKey: ['contacts', companyId] });
      queryClient.invalidateQueries({ queryKey: ['contact-options', companyId] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof FormValues, { message: messages[0] });
        }
      }
    },
  });

  // Soft-deactivate only — ContactsController exposes this as a dedicated
  // POST .../{id}/deactivate action (never a DELETE), same convention as CostCentersController.
  const deactivateContact = useMutation({
    mutationFn: (contactId: string) => apiClient.post<ContactDto>(`/crm/contacts/${contactId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['contacts', companyId] }),
  });

  if (!companyId) return null;

  const editingContact = contactsQuery.data?.data.find((c) => c.id === editingContactId) ?? null;

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Contacts</h2>
      <p className="mb-3 text-xs text-text-muted">
        A named individual — usually linked to an Account, but can be linked straight to a Lead before an Account exists.
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createContact.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
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
            Email (optional)
            <Controller
              control={control}
              name="email"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.email && <span className="text-xs text-danger">{errors.email.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Phone (optional)
            <Controller
              control={control}
              name="phone"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
              )}
            />
            {errors.phone && <span className="text-xs text-danger">{errors.phone.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Title (optional)
            <Controller
              control={control}
              name="title"
              render={({ field }) => (
                <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="VP Sales…" {...field} />
              )}
            />
            {errors.title && <span className="text-xs text-danger">{errors.title.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Account (optional)
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
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Lead (optional — before an Account exists)
            <Controller
              control={control}
              name="leadId"
              render={({ field }) => (
                <EntityCombobox
                  value={field.value}
                  onChange={field.onChange}
                  options={leadOptions.options}
                  isLoading={leadOptions.isLoading}
                  onSearchChange={leadOptions.onSearchChange}
                  placeholder="Search leads…"
                />
              )}
            />
          </label>
          <div className="col-span-full">
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create contact'}</Button>
          </div>
        </form>
        {createContact.isError && createContact.error instanceof ApiError && (
          <p role="alert" className="mt-2 text-sm text-danger">{createContact.error.problem.title}</p>
        )}
      </Card>

      <Card>
        <label className="mb-3 flex flex-col gap-1 text-sm sm:w-72">
          Search
          <input
            className="rounded-md border border-border bg-surface px-2 py-1.5"
            placeholder="Search by name or email…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </label>
        <DataTable
          columns={[
            { header: 'Name', render: (row: ContactDto) => row.name },
            { header: 'Contact', render: (row: ContactDto) => row.email ?? row.phone ?? '—' },
            { header: 'Title', render: (row: ContactDto) => row.title ?? '—' },
            { header: 'Linked to', render: (row: ContactDto) => (row.accountId ? 'Account' : row.leadId ? 'Lead' : '—') },
            { header: 'Status', render: (row: ContactDto) => (row.isActive ? 'Active' : 'Inactive') },
            {
              header: 'Actions',
              render: (row: ContactDto) => (
                <div className="flex items-center gap-2">
                  <Button type="button" variant="secondary" onClick={() => setEditingContactId(row.id)}>
                    Edit
                  </Button>
                  <Button
                    type="button"
                    variant="danger"
                    disabled={!row.isActive || deactivateContact.isPending}
                    onClick={() => deactivateContact.mutate(row.id)}
                  >
                    {row.isActive ? 'Deactivate' : 'Deactivated'}
                  </Button>
                </div>
              ),
            },
          ]}
          rows={contactsQuery.data?.data}
          isLoading={contactsQuery.isLoading}
          isError={contactsQuery.isError}
          errorMessage="Could not load contacts."
          emptyMessage="No contacts yet — create the first one above."
          rowKey={(row) => row.id}
        />
      </Card>
      {deactivateContact.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that contact.</p>
      )}

      {editingContact && (
        <ContactEditPanel
          companyId={companyId}
          contact={editingContact}
          onClose={() => setEditingContactId(null)}
        />
      )}
    </div>
  );
}

interface ContactEditPanelProps {
  companyId: string;
  contact: ContactDto;
  onClose: () => void;
}

function ContactEditPanel({ companyId, contact, onClose }: ContactEditPanelProps) {
  const queryClient = useQueryClient();
  const accountOptions = useCrmAccountOptions(companyId);
  const leadOptions = useLeadOptions(companyId);

  const { control, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    values: {
      name: contact.name,
      email: contact.email ?? '',
      phone: contact.phone ?? '',
      title: contact.title ?? '',
      accountId: contact.accountId ?? '',
      leadId: contact.leadId ?? '',
    },
  });

  const updateContact = useMutation({
    mutationFn: (values: EditFormValues) =>
      apiClient.put<ContactDto>(`/crm/contacts/${contact.id}`, {
        companyId,
        name: values.name,
        email: values.email || null,
        phone: values.phone || null,
        title: values.title || null,
        accountId: values.accountId || null,
        leadId: values.leadId || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contacts', companyId] });
      onClose();
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof EditFormValues, { message: messages[0] });
        }
      }
    },
  });

  return (
    <Card className="mt-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-base font-semibold text-text">Edit contact — {contact.name}</h3>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateContact.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-3">
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
          Email (optional)
          <Controller
            control={control}
            name="email"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.email && <span className="text-xs text-danger">{errors.email.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Phone (optional)
          <Controller
            control={control}
            name="phone"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.phone && <span className="text-xs text-danger">{errors.phone.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Title (optional)
          <Controller
            control={control}
            name="title"
            render={({ field }) => (
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...field} />
            )}
          />
          {errors.title && <span className="text-xs text-danger">{errors.title.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Account (optional)
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
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Lead (optional)
          <Controller
            control={control}
            name="leadId"
            render={({ field }) => (
              <EntityCombobox
                value={field.value}
                onChange={field.onChange}
                options={leadOptions.options}
                isLoading={leadOptions.isLoading}
                onSearchChange={leadOptions.onSearchChange}
                placeholder="Search leads…"
              />
            )}
          />
        </label>
        <div className="col-span-full flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving…' : 'Save changes'}</Button>
          {updateContact.isError && (
            <span role="alert" className="text-sm text-danger">Could not save that contact.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
