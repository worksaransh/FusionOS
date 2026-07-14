import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuthStore } from '../shared/auth/authStore';

/**
 * Gates every route rendered inside it on a signed-in session (07_SECURITY.md
 * — the backend now rejects unauthenticated requests by default via
 * AddAuthorization's FallbackPolicy, so the frontend must actually log in).
 * Remembers where the visitor was headed so LoginPage can send them back.
 */
export function RequireAuth() {
  const session = useAuthStore((s) => s.session);
  const location = useLocation();

  if (!session) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  return <Outlet />;
}
