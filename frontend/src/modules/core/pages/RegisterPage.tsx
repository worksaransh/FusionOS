import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate, Link } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';

const schema = z
  .object({
    fullName: z.string().min(1, 'Full name is required').max(200),
    email: z.string().email('Enter a valid email'),
    password: z.string().min(10, 'Password must be at least 10 characters'),
    confirmPassword: z.string().min(1, 'Confirm your password'),
    companyName: z.string().min(1, 'Company name is required').max(200),
    companyLegalName: z.string().min(1, 'Legal name is required').max(200),
    baseCurrency: z.string().length(3, 'Use a 3-letter ISO 4217 code').toUpperCase(),
    taxId: z.string().optional(),
  })
  .refine((values) => values.password === values.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

type FormValues = z.infer<typeof schema>;

interface CompanyDto {
  id: string;
  name: string;
  legalName: string;
  taxId: string | null;
  baseCurrency: string;
  isActive: boolean;
  createdAt: string;
}

interface UserDto {
  id: string;
  email: string;
  fullName: string;
  isActive: boolean;
  createdAt: string;
}

/**
 * Public sign-up page (Phase H5, 2026-07-14 sprint) — the backend already
 * supported this via POST /core/auth/register; only the UI was missing.
 * RegisterUserCommand always needs a CompanyId, and RegisterUserCommandHandler
 * only allows an anonymous (not-yet-signed-in) caller to register when the
 * target company has zero users so far (the bootstrap case, Phase H3) — so
 * this page's only anonymous-accessible flow is "create a brand-new company
 * and become its first (Owner) user," not "join an existing company," which
 * requires an already-signed-in Owner with core.user.register to invite a
 * teammate from inside the app instead (not yet a dedicated UI — see RolesPage
 * follow-up). Submission is two calls in sequence: POST /core/companies (also
 * [AllowAnonymous], same bootstrap reasoning) to get a CompanyId, then POST
 * /core/auth/register with it. RegisterUserCommand returns a UserDto, not a
 * session/tokens (unlike LoginCommand's AuthResultDto), so success redirects
 * to /login rather than signing the user in directly.
 */
export function RegisterPage() {
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const registerCompanyAndOwner = useMutation({
    mutationFn: async (values: FormValues) => {
      const company = await apiClient.post<CompanyDto>('/core/companies', {
        name: values.companyName,
        legalName: values.companyLegalName,
        baseCurrency: values.baseCurrency,
        taxId: values.taxId,
      });

      return apiClient.post<UserDto>('/core/auth/register', {
        email: values.email,
        fullName: values.fullName,
        password: values.password,
        companyId: company.id,
      });
    },
    onSuccess: () => {
      navigate('/login', { replace: true, state: { registered: true } });
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          const formField = field.toLowerCase() === 'email' ? 'email' : undefined;
          if (formField) setError(formField, { message: messages[0] });
        }
        setServerError(error.problem.title);
      } else {
        setServerError(error instanceof ApiError ? error.problem.title : 'Could not create your account. Try again.');
      }
    },
  });

  return (
    <div className="flex min-h-screen items-center justify-center bg-surface-muted py-10">
      <div className="w-full max-w-lg rounded-lg border border-border bg-surface p-8 shadow-sm">
        <div className="mb-6 text-center">
          <div className="text-lg font-semibold">FusionOS</div>
          <p className="mt-1 text-sm text-text-muted">Create your company and get started</p>
        </div>

        <form
          onSubmit={handleSubmit((values) => registerCompanyAndOwner.mutate(values))}
          className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        >
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Full name
            <input
              autoComplete="name"
              aria-invalid={Boolean(errors.fullName)}
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              {...register('fullName')}
            />
            {errors.fullName && <span role="alert" className="text-xs text-danger">{errors.fullName.message}</span>}
          </label>

          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            Email
            <input
              type="email"
              autoComplete="username"
              aria-invalid={Boolean(errors.email)}
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              {...register('email')}
            />
            {errors.email && <span role="alert" className="text-xs text-danger">{errors.email.message}</span>}
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Password
            <input
              type="password"
              autoComplete="new-password"
              aria-invalid={Boolean(errors.password)}
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              {...register('password')}
            />
            {errors.password && <span role="alert" className="text-xs text-danger">{errors.password.message}</span>}
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Confirm password
            <input
              type="password"
              autoComplete="new-password"
              aria-invalid={Boolean(errors.confirmPassword)}
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              {...register('confirmPassword')}
            />
            {errors.confirmPassword && (
              <span role="alert" className="text-xs text-danger">{errors.confirmPassword.message}</span>
            )}
          </label>

          <div className="sm:col-span-2 mt-2 border-t border-border pt-4 text-sm font-medium text-text">
            Your company
          </div>

          <label className="flex flex-col gap-1 text-sm">
            Company name
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('companyName')} />
            {errors.companyName && <span role="alert" className="text-xs text-danger">{errors.companyName.message}</span>}
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Legal name
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('companyLegalName')} />
            {errors.companyLegalName && (
              <span role="alert" className="text-xs text-danger">{errors.companyLegalName.message}</span>
            )}
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Base currency
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="USD" {...register('baseCurrency')} />
            {errors.baseCurrency && <span role="alert" className="text-xs text-danger">{errors.baseCurrency.message}</span>}
          </label>

          <label className="flex flex-col gap-1 text-sm">
            Tax ID (optional)
            <input className="rounded-md border border-border bg-surface px-2 py-1.5" {...register('taxId')} />
          </label>

          {serverError && <p role="alert" className="sm:col-span-2 text-sm text-danger">{serverError}</p>}

          <div className="sm:col-span-2">
            <Button type="submit" disabled={isSubmitting || registerCompanyAndOwner.isPending}>
              {isSubmitting || registerCompanyAndOwner.isPending ? 'Creating your account…' : 'Create account'}
            </Button>
          </div>
        </form>

        <p className="mt-6 text-center text-sm text-text-muted">
          Already have an account?{' '}
          <Link to="/login" className="font-medium text-primary underline underline-offset-2">
            Sign in
          </Link>
        </p>
      </div>
    </div>
  );
}
