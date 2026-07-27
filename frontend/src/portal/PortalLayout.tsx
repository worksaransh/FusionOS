import { Outlet, useNavigate } from 'react-router-dom';
import { usePortalAuthStore } from './portalAuthStore';
import { Button } from '../shared/ui/Button';

/**
 * A minimal standalone shell for every /portal/* route - deliberately NOT
 * the staff AppShell (no sidebar, no module nav, no company switcher): a
 * customer must never see the internal staff navigation (task requirement).
 * Just a thin header with the signed-in email and a sign-out button.
 */
export function PortalLayout() {
  const session = usePortalAuthStore((s) => s.session);
  const clearSession = usePortalAuthStore((s) => s.clearSession);
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-surface-muted">
      <header className="flex items-center justify-between border-b border-border bg-surface px-6 py-3">
        <div className="text-lg font-semibold">FusionOS Customer Portal</div>
        <div className="flex items-center gap-3 text-sm text-text-muted">
          {session && <span>{session.email}</span>}
          <Button
            variant="secondary"
            onClick={() => {
              clearSession();
              navigate('/portal/login', { replace: true });
            }}
          >
            Sign out
          </Button>
        </div>
      </header>
      <main className="mx-auto max-w-5xl p-6">
        <Outlet />
      </main>
    </div>
  );
}
