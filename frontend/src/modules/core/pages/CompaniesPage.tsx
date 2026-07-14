import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { PageHeader } from '../../../shared/ui/PageHeader';

/**
 * The one real, end-to-end vertical slice in this scaffold: create + list
 * Companies against FusionOS.Modules.Core.Api's CompaniesController
 * (08_API_STANDARDS.md). Every other module's page is a health-check
 * placeholder — see ModuleHealthPage.
 */

const createCompanySchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  legalName: z.string().min(1, 'Legal name is required').max(200),
  baseCurrency: z.string().length(3, 'Use a 3-letter ISO 4217 code').toUpperCase(),
  taxId: z.string().optional(),
});

type CreateCompanyForm = z.infer<typeof createCompanySchema>;

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

  return (
    <div>
      <PageHeader title="Companies" description="Multi-company setup — Core Platform, Phase 0" />

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
                <th className="py-2">Created</th>
              </tr>
            </thead>
            <tbody>
              {companiesQuery.data.data.map((company) => (
                <tr key={company.id} className="border-b border-border last:border-0">
                  <td className="py-2">{company.name}</td>
                  <td className="py-2">{company.legalName}</td>
                  <td className="py-2">{company.baseCurrency}</td>
                  <td className="py-2">{new Date(company.createdAt).toLocaleDateString()}</td>
                </tr>
              ))}
              {companiesQuery.data.data.length === 0 && (
                <tr>
                  <td colSpan={4} className="py-4 text-center text-text-muted">
                    No companies yet — create the first one above.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </Card>
    </div>
  );
}
