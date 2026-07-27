import { Navigate, Outlet } from 'react-router-dom';
import { usePortalAuthStore } from './portalAuthStore';

/**
 * Gates every route rendered inside it on a signed-in CUSTOMER portal session
 * - completely separate from the staff RequireAuth (app/RequireAuth.tsx),
 * which checks the unrelated useAuthStore. A customer with no portal session
 * is sent to /portal/login, never to the staff /login.
 */
export function PortalRequireAuth() {
  const session = usePortalAuthStore((s) => s.session);

  if (!session) {
    return <Navigate to="/portal/login" replace />;
  }

  return <Outlet />;
}
