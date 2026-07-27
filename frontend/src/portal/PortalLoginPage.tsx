import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { portalApiClient, PortalApiError } from './portalApiClient';
import { usePortalAuthStore, type PortalSession } from './portalAuthStore';
import { Button } from '../shared/ui/Button';

const schema = z.object({
  email: z.string().email('Enter a valid email'),
  password: z.string().min(1, 'Password is required'),
});
type FormValues = z.infer<typeof schema>;

interface PortalLoginResponse {
  portalUserId: string;
  customerId: string;
  email: string;
  accessToken: string;
  accessTokenExpiresAt: string;
}

/**
 * The Customer Portal's own sign-in screen - a customer's own separate
 * credentials, nothing to do with staff login (LoginPage,
 * shared/auth/authStore's useAuthStore). Deliberately rendered outside the
 * staff AppShell/RequireAuth tree entirely (see AppRoutes.tsx) so a customer
 * never sees the internal sidebar/navigation.
 */
export function PortalLoginPage() {
  const navigate = useNavigate();
  const setSession = usePortalAuthStore((s) => s.setSession);
  const [serverError, setServerError] = useState<string | null>(null);

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  const login = useMutation({
    mutationFn: (values: FormValues) => portalApiClient.post<PortalLoginResponse>('/sales/portal/auth/login', values),
    onSuccess: (result) => {
      const session: PortalSession = {
        portalUserId: result.portalUserId,
        customerId: result.customerId,
        email: result.email,
        accessToken: result.accessToken,
        accessTokenExpiresAt: result.accessTokenExpiresAt,
      };
      setSession(session);
      navigate('/portal', { replace: true });
    },
    onError: (error) => {
      setServerError(error instanceof PortalApiError ? error.problem.title : 'Could not sign in. Try again.');
    },
  });

  return (
    <div className="flex min-h-screen items-center justify-center bg-surface-muted">
      <div className="w-full max-w-sm rounded-lg border border-border bg-surface p-8 shadow-sm">
        <div className="mb-6 text-center">
          <div className="text-lg font-semibold">FusionOS Customer Portal</div>
          <p className="mt-1 text-sm text-text-muted">Sign in to view your quotations, orders, and invoices</p>
        </div>

        <form noValidate onSubmit={handleSubmit((values) => login.mutate(values))} className="flex flex-col gap-4">
          <label className="flex flex-col gap-1 text-sm">
            Email
            <input
              type="email"
              autoComplete="username"
              aria-invalid={Boolean(errors.email)}
              aria-describedby={errors.email ? 'portal-login-email-error' : undefined}
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              {...register('email')}
            />
            {errors.email && (
              <span id="portal-login-email-error" role="alert" className="text-xs text-danger">
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
              aria-describedby={errors.password ? 'portal-login-password-error' : undefined}
              className="rounded-md border border-border bg-surface px-2 py-1.5"
              {...register('password')}
            />
            {errors.password && (
              <span id="portal-login-password-error" role="alert" className="text-xs text-danger">
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
