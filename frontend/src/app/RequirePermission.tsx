import type { ReactNode } from 'react';
import { useAuthStore } from '../shared/auth/authStore';

interface RequirePermissionProps {
  /**
   * A single permission code, or a list of codes where holding ANY one of
   * them is enough (some pages read from more than one entity family — e.g.
   * a dashboard tile that shows both invoices and receivables).
   */
  permission: string | string[];
  children: ReactNode;
}

/**
 * Route-level RBAC gate that sits inside RequireAuth (which only checks
 * "is there a session at all"). Renders a "Not authorized" placeholder
 * instead of the page when the signed-in user lacks every listed permission
 * code — this is additive, defensive UI-layer gating on top of the backend's
 * IRequirePermission checks (07_SECURITY.md §2), which remain the real
 * security boundary; a user who somehow reached this component without the
 * permission still can't do anything the backend wouldn't already reject.
 * Users who DO hold the permission see children exactly as before — this
 * component is a no-op for them, so it never blocks existing route access.
 */
export function RequirePermission({ permission, children }: RequirePermissionProps) {
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const codes = Array.isArray(permission) ? permission : [permission];
  const isAllowed = codes.some((code) => hasPermission(code));

  if (!isAllowed) {
    return (
      <div className="p-6">
        <h1 className="text-lg font-semibold text-text">Not authorized</h1>
        <p className="mt-2 max-w-prose text-sm text-text-muted">
          Your account doesn't have permission to view this page. Ask a company administrator to grant you the{' '}
          <code className="rounded bg-surface-muted px-1 py-0.5 text-xs">{codes.join(', ')}</code> permission if you
          believe this is a mistake.
        </p>
      </div>
    );
  }

  return <>{children}</>;
}
