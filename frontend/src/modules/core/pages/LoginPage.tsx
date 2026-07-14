import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate, useLocation } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { useAuthStore, type AuthSession } from '../../../shared/auth/authStore';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';
import { Button } from '../../../shared/ui/Button';

const schema = z.object({
  email: z.string().email('Enter a valid email'),
  password: z.string().min(1, 'Password is required'),
});
type FormValues = z.infer<typeof schema>;

/**
 * The one screen every other page (RequireAuth) now gates on — 07_SECURITY.md.
 * There is deliberately no "sign up" link here: creating a brand-new company's
 * first user is a distinct bootstrap action (POST /core/auth/register with no
 * signed-in caller yet) that belongs on the Companies page next to "Create
 * company," not folded into login. See README known-gaps for that follow-up.
 */
export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const setSession = useAuthStore((s) => s.setSession);
  const setCompanyId = useActiveCompany((s) => s.setCompanyId);
  const [serverError, setServerError] = useState<string | null>(null);

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  const login = useMutation({
    mutationFn: (values: FormValues) => apiClient.post<AuthSession>('/core/auth/login', values),
    onSuccess: (session) => {
      setSession(session);
      if (session.companyId) setCompanyId(session.companyId);
      const redirectTo = (location.state as { from?: string } | null)?.from ?? '/core';
      navigate(redirectTo, { replace: true });
    },
    onError: (error) => {
      setServerError(error instanceof ApiError ? error.problem.title : 'Could not sign in. Try again.');
    },
  });

  return (
    <div className="flex min-h-screen items-center justify-center bg-surface-muted">
      <div className="w-full max-w-sm rounded-lg border border-border bg-surface p-8 shadow-sm">
        <div className="mb-6 text-center">
          <div className="text-lg font-semibold">FusionOS</div>
          <p className="mt-1 text-sm text-text-muted">Sign in to continue</p>
        </div>

        <form onSubmit={handleSubmit((values) => login.mutate(values))} className="flex flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm">
            Email
            <input
              type="email"
              autoComplete="username"
              aria-invalid={Boolean(errors.email)}
              aria-describedby={errors.email ? 'login-email-error' : undefined}
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              {...register('email')}
            />
            {errors.email && (
              <span id="login-email-error" role="alert" className="text-xs text-danger">
                {errors.email.message}
              </span>
            )}
          </label>
          <label className="flex flex-col gap-1 text-sm">
            Password
            <input
              type="password"
              autoComplete="current-password"
              aria-invalid={Boolean(errors.password)}
              aria-describedby={errors.password ? 'login-password-error' : undefined}
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              {...register('password')}
            />
            {errors.password && (
              <span id="login-password-error" role="alert" className="text-xs text-danger">
                {errors.password.message}
              </span>
            )}
          </label>

          {serverError && <p role="alert" className="text-sm text-danger">{serverError}</p>}

          <Button type="submit" disabled={isSubmitting || login.isPending}>
            {isSubmitting || login.isPending ? 'Signing in…' : 'Sign in'}
          </Button>
        </form>
      </div>
    </div>
  );
}
