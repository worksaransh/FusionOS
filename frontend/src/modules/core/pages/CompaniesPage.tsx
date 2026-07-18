import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { PageHeader } from '../../../shared/ui/PageHeader';

/**
 * The one real, end-to-end vertical slice in this scaffold: create + list
 * Companies against FusionOS.Modules.Core.Api's CompaniesController
 * (08_API_STANDARDS.md). Every other module's page is a health-check
 * placeholder — see ModuleHealthPage. Deactivate was added in Phase I
 * (2026-07-14 sprint) — apiClient has no delete() verb, so it's a POST to an
 * explicit action path (POST /core/companies/{id}/deactivate) rather than a
 * DELETE, matching DeactivateCompanyCommand on the backend. Edit was added in
 * the same Phase I sprint — PUT /core/companies/{id} against
 * UpdateCompanyCommand, which only ever updates Name/LegalName/TaxId
 * (see UpdateCompanyCommandHandler.cs); BaseCurrency is immutable after
 * creation and is intentionally not part of the edit form.
 */

const createCompanySchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  legalName: z.string().min(1, 'Legal name is required').max(200),
  baseCurrency: z.string().length(3, 'Use a 3-letter ISO 4217 code').toUpperCase(),
  taxId: z.string().optional(),
});

type CreateCompanyForm = z.infer<typeof createCompanySchema>;

// Update does NOT touch BaseCurrency — UpdateCompanyCommand only carries
// Name/LegalName/TaxId (see UpdateCompanyCommandHandler.cs, which only ever
// calls company.UpdateDetails(name, legalName, taxId)); currency is
// business-key-adjacent and immutable after creation.
const updateCompanySchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  legalName: z.string().min(1, 'Legal name is required').max(200),
  taxId: z.string().optional(),
});

type UpdateCompanyForm = z.infer<typeof updateCompanySchema>;

interface CompanyDto {
  id: string;
  name: string;
  legalName: string;
  taxId: string | null;
  baseCurrency: string;
  isActive: boolean;
  createdAt: string;
}

interface PagedResult<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export function CompaniesPage() {
  const queryClient = useQueryClient();
  const [editingCompanyId, setEditingCompanyId] = useState<string | null>(null);

  const companiesQuery = useQuery({
    queryKey: ['companies'],
    queryFn: () => apiClient.get<PagedResult<CompanyDto>>('/core/companies?page=1&pageSize=25'),
  });

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<CreateCompanyForm>({ resolver: zodResolver(createCompanySchema) });

  const createCompany = useMutation({
    mutationFn: (values: CreateCompanyForm) => apiClient.post<CompanyDto>('/core/companies', values),
    onSuccess: () => {
      reset();
      queryClient.invalidateQueries({ queryKey: ['companies'] });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof CreateCompanyForm, { message: messages[0] });
        }
      }
    },
  });

  const deactivateCompany = useMutation({
    mutationFn: (id: string) => apiClient.post<CompanyDto>(`/core/companies/${id}/deactivate`, {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['companies'] });
    },
  });

  const editingCompany = companiesQuery.data?.data.find((c) => c.id === editingCompanyId);

  return (
    <div>
      <PageHeader title="Companies" description="Multi-company setup — Core Platform, Phase 0" />

      <p className="mb-6 text-sm text-text-muted">
        Manage who can do what:{' '}
        <Link to="/core/roles" className="font-medium text-primary underline underline-offset-2">
          Roles &amp; permissions
        </Link>
        {' · '}
        <Link to="/core/audit-log" className="font-medium text-primary underline underline-offset-2">
          Audit log
        </Link>
        {' · '}
        <Link to="/core/settings" className="font-medium text-primary underline underline-offset-2">
          Settings
        </Link>
      </p>

      <Card className="mb-6">
        <form onSubmit={handleSubmit((values) => createCompany.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            Name
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
            {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Legal name
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('legalName')} />
            {errors.legalName && <span className="text-xs text-danger">{errors.legalName.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Base currency
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="USD" {...register('baseCurrency')} />
            {errors.baseCurrency && <span className="text-xs text-danger">{errors.baseCurrency.message}</span>}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Tax ID (optional)
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('taxId')} />
          </label>
          <div className="col-span-2">
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Creating…' : 'Create company'}
            </Button>
          </div>
        </form>
      </Card>

      <Card>
        {companiesQuery.isLoading && <p className="text-text-muted">Loading companies…</p>}
        {companiesQuery.isError && (
          <p className="text-danger">Could not load companies. Is the backend running on localhost:5000?</p>
        )}
        {companiesQuery.data && (
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-border text-text-muted">
                <th className="py-2">Name</th>
                <th className="py-2">Legal name</th>
                <th className="py-2">Currency</th>
                <th className="py-2">Status</th>
                <th className="py-2">Created</th>
                <th className="py-2"></th>
              </tr>
            </thead>
            <tbody>
              {companiesQuery.data.data.map((company) => (
                <tr key={company.id} className="border-b border-border last:border-0">
                  <td className="py-2">{company.name}</td>
                  <td className="py-2">{company.legalName}</td>
                  <td className="py-2">{company.baseCurrency}</td>
                  <td className="py-2">{company.isActive ? 'Active' : 'Inactive'}</td>
                  <td className="py-2">{new Date(company.createdAt).toLocaleDateString()}</td>
                  <td className="py-2">
                    <div className="flex items-center gap-2">
                      <Button variant="secondary" onClick={() => setEditingCompanyId(company.id)}>
                        Edit
                      </Button>
                      {company.isActive && (
                        <Button
                          variant="danger"
                          disabled={deactivateCompany.isPending}
                          onClick={() => deactivateCompany.mutate(company.id)}
                        >
                          {deactivateCompany.isPending ? 'Deactivating…' : 'Deactivate'}
                        </Button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
              {companiesQuery.data.data.length === 0 && (
                <tr>
                  <td colSpan={6} className="py-4 text-center text-text-muted">
                    No companies yet — create the first one above.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
        {deactivateCompany.isError && (
          <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that company.</p>
        )}
      </Card>

      {editingCompany && (
        <CompanyEditForm
          key={editingCompany.id}
          company={editingCompany}
          onClose={() => setEditingCompanyId(null)}
        />
      )}
    </div>
  );
}

interface CompanyEditFormProps {
  company: CompanyDto;
  onClose: () => void;
}

function CompanyEditForm({ company, onClose }: CompanyEditFormProps) {
  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<UpdateCompanyForm>({
    resolver: zodResolver(updateCompanySchema),
    defaultValues: {
      name: company.name,
      legalName: company.legalName,
      taxId: company.taxId ?? '',
    },
  });

  const updateCompany = useMutation({
    mutationFn: (values: UpdateCompanyForm) => apiClient.put<CompanyDto>(`/core/companies/${company.id}`, values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['companies'] });
      onClose();
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof UpdateCompanyForm, { message: messages[0] });
        }
      }
    },
  });

  return (
    <Card className="mt-8">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-text">Edit company — {company.name}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>
      <form onSubmit={handleSubmit((values) => updateCompany.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-sm">
          Name
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('name')} />
          {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Legal name
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('legalName')} />
          {errors.legalName && <span className="text-xs text-danger">{errors.legalName.message}</span>}
        </label>
        <label className="flex flex-col gap-1 text-sm">
          Tax ID (optional)
          <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('taxId')} />
        </label>
        <div className="col-span-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Saving…' : 'Save changes'}
          </Button>
          {updateCompany.isError && (
            <span role="alert" className="text-sm text-danger">Could not update that company.</span>
          )}
        </div>
      </form>
    </Card>
  );
}
