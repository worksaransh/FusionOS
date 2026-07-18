import { useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { Menu, Moon, Sun, Search, LogOut, X, Bell, CheckSquare } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import { useTheme } from '../../shared/theme/ThemeProvider';
import { useActiveCompany } from '../../shared/company/useActiveCompany';
import { useAuthStore } from '../../shared/auth/authStore';
import { apiClient } from '../../shared/api/client';
import type { PagedResult } from '../../shared/api/types';
import { MODULES } from '../modules';

/**
 * Unread-notification count for the sidebar badge (Phase M7, 2026-07-15) —
 * a cheap page=1/pageSize=1 fetch just to read totalCount off the paged
 * envelope, same trick DashboardPage uses for "Open Sales Orders."
 */
function useUnreadNotificationCount(companyId: string) {
  const query = useQuery({
    queryKey: ['unread-notification-count', companyId],
    queryFn: () => apiClient.get<PagedResult<unknown>>(`/core/notifications?companyId=${companyId}&unreadOnly=true&page=1&pageSize=1`),
    enabled: Boolean(companyId),
    refetchInterval: 60_000,
  });
  return query.data?.totalCount ?? 0;
}

/**
 * Enterprise shell: persistent sidebar nav across every module, a global
 * search/command trigger, and a dark-mode toggle — 06_UI_UX_DESIGN_SYSTEM.md
 * §2, §3, §5. Full Cmd/Ctrl+K command palette is a follow-up; this wires the
 * visible entry point for it now so the shell shape doesn't change later.
 * Active company is pre-filled from the signed-in session's company_id claim
 * (07_SECURITY.md) but stays editable — useful for a user who belongs to more
 * than one company, or for testing.
 *
 * Below the `md` breakpoint the sidebar becomes an off-canvas drawer (fixed,
 * translated out of view, toggled by the header's hamburger button) instead
 * of a permanently docked column — the fixed w-64 sidebar previously ate
 * roughly half the viewport width on a phone-sized screen with no way to
 * collapse it (06_UI_UX_DESIGN_SYSTEM.md responsive breakpoints, flagged by
 * the audit's Frontend/UX pass). `md:static md:translate-x-0` restores the
 * original docked layout unchanged on tablet/desktop.
 */
export function AppShell() {
  const { theme, toggleTheme } = useTheme();
  const { companyId, setCompanyId } = useActiveCompany();
  const session = useAuthStore((s) => s.session);
  const clearSession = useAuthStore((s) => s.clearSession);
  const navigate = useNavigate();
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const unreadCount = useUnreadNotificationCount(companyId);

  const handleLogout = () => {
    clearSession();
    navigate('/login', { replace: true });
  };

  return (
    <div className="flex h-screen bg-surface-muted text-text">
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:left-4 focus:top-4 focus:z-50 focus:rounded-md focus:bg-primary focus:px-3 focus:py-2 focus:text-sm focus:text-primary-foreground"
      >
        Skip to main content
      </a>

      {isSidebarOpen && (
        <div
          className="fixed inset-0 z-20 bg-black/40 md:hidden"
          onClick={() => setIsSidebarOpen(false)}
          aria-hidden="true"
        />
      )}

      <aside
        className={`fixed inset-y-0 left-0 z-30 w-64 shrink-0 overflow-y-auto border-r border-border bg-surface p-4 transition-transform duration-200 md:static md:translate-x-0 ${
          isSidebarOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <div className="mb-6 flex items-center justify-between px-2">
          <span className="text-lg font-semibold">FusionOS</span>
          <button
            aria-label="Close navigation"
            onClick={() => setIsSidebarOpen(false)}
            className="rounded-md p-1 text-text-muted hover:bg-surface-muted md:hidden"
          >
            <X size={18} />
          </button>
        </div>
        <nav className="flex flex-col gap-1">
          <NavLink
            to="/dashboard"
            onClick={() => setIsSidebarOpen(false)}
            className={({ isActive }) =>
              `rounded-md px-2 py-1.5 text-sm ${
                isActive ? 'bg-primary text-primary-foreground' : 'text-text-muted hover:bg-surface-muted'
              }`
            }
          >
            Dashboard
          </NavLink>
          <NavLink
            to="/core/approvals"
            onClick={() => setIsSidebarOpen(false)}
            className={({ isActive }) =>
              `flex items-center gap-2 rounded-md px-2 py-1.5 text-sm ${
                isActive ? 'bg-primary text-primary-foreground' : 'text-text-muted hover:bg-surface-muted'
              }`
            }
          >
            <CheckSquare size={14} /> Approvals
          </NavLink>
          <NavLink
            to="/core/notifications"
            onClick={() => setIsSidebarOpen(false)}
            className={({ isActive }) =>
              `flex items-center gap-2 rounded-md px-2 py-1.5 text-sm ${
                isActive ? 'bg-primary text-primary-foreground' : 'text-text-muted hover:bg-surface-muted'
              }`
            }
          >
            <Bell size={14} /> Notifications
            {unreadCount > 0 && (
              <span className="ml-auto rounded-full bg-danger px-1.5 py-0.5 text-xs text-white">{unreadCount}</span>
            )}
          </NavLink>
          {MODULES.map((module) => (
            <NavLink
              key={module.name}
              to={`/${module.name}`}
              onClick={() => setIsSidebarOpen(false)}
              className={({ isActive }) =>
                `rounded-md px-2 py-1.5 text-sm ${
                  isActive ? 'bg-primary text-primary-foreground' : 'text-text-muted hover:bg-surface-muted'
                }`
              }
            >
              {module.label}
              {!module.implemented && <span className="ml-2 text-xs opacity-60">(scaffolded)</span>}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="flex flex-1 flex-col overflow-hidden">
        <header className="flex flex-wrap items-center justify-between gap-3 border-b border-border bg-surface px-4 py-3 sm:px-6">
          <div className="flex items-center gap-2">
            <button
              aria-label="Open navigation"
              onClick={() => setIsSidebarOpen(true)}
              className="rounded-md border border-border p-2 text-text-muted hover:bg-surface-muted md:hidden"
            >
              <Menu size={16} />
            </button>
            <button className="flex items-center gap-2 rounded-md border border-border px-3 py-1.5 text-sm text-text-muted">
              <Search size={16} />
              <span className="hidden sm:inline">
                Search… <kbd className="ml-2 rounded bg-surface-muted px-1.5 text-xs">⌘K</kbd>
              </span>
            </button>
          </div>
          <div className="flex items-center gap-3">
            <label className="flex items-center gap-2 text-xs text-text-muted">
              <span className="hidden sm:inline">Active company</span>
              <input
                value={companyId}
                onChange={(e) => setCompanyId(e.target.value)}
                placeholder="Company ID"
                className="w-36 rounded-md border border-border bg-surface px-2 py-1 text-text sm:w-72"
              />
            </label>
          </div>
          <div className="flex items-center gap-2">
            {session && <span className="hidden text-xs text-text-muted sm:inline">{session.email}</span>}
            <button
              aria-label="Toggle dark mode"
              onClick={toggleTheme}
              className="rounded-md border border-border p-2 text-text-muted hover:bg-surface-muted"
            >
              {theme === 'dark' ? <Sun size={16} /> : <Moon size={16} />}
            </button>
            <button
              aria-label="Sign out"
              onClick={handleLogout}
              className="flex items-center gap-1 rounded-md border border-border px-2 py-2 text-text-muted hover:bg-surface-muted"
            >
              <LogOut size={16} />
            </button>
          </div>
        </header>

        <main id="main-content" tabIndex={-1} className="flex-1 overflow-y-auto p-4 focus:outline-none sm:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
