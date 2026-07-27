import { User, Shield, Building } from 'lucide-react';
import { Card } from '../../../shared/ui/Card';
import { Button } from '../../../shared/ui/Button';
import { StatusBadge } from '../../../shared/ui/StatusBadge';
import { useAuthStore } from '../../../shared/auth/authStore';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';

export function UserProfilePage() {
  const session = useAuthStore((s) => s.session);
  const { companyId } = useActiveCompany();

  return (
    <div className="space-y-6 max-w-4xl">
      <div>
        <h1 className="text-xl font-bold tracking-tight text-text">User Profile & Account Settings</h1>
        <p className="text-sm text-text-muted">Manage your credentials, active organization, and personal security settings.</p>
      </div>

      {/* User Header Profile Card */}
      <Card className="flex flex-wrap items-center justify-between gap-4 p-6">
        <div className="flex items-center gap-4">
          <div className="flex h-16 w-16 items-center justify-center rounded-full bg-primary/10 text-primary text-2xl font-bold">
            {session?.email?.[0].toUpperCase() || 'U'}
          </div>
          <div>
            <h2 className="text-lg font-bold text-text">{session?.email || 'User Email'}</h2>
            <div className="mt-1 flex items-center gap-2 text-xs text-text-muted">
              <Shield size={14} className="text-emerald-500" />
              <span>Session Authenticated</span>
              <span>•</span>
              <StatusBadge status="Active" />
            </div>
          </div>
        </div>
        <Button type="button" variant="secondary">
          Edit Profile
        </Button>
      </Card>

      {/* Detailed Grid Settings */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
        <Card>
          <div className="flex items-center gap-2 mb-4 border-b border-border pb-3">
            <User size={18} className="text-primary" />
            <h3 className="font-semibold text-text">Account Information</h3>
          </div>
          <dl className="space-y-3 text-sm">
            <div>
              <dt className="text-xs text-text-muted">Email Address</dt>
              <dd className="font-medium text-text mt-0.5">{session?.email}</dd>
            </div>
            <div>
              <dt className="text-xs text-text-muted">User ID</dt>
              <dd className="font-mono text-xs text-text-muted mt-0.5">{session?.userId || 'N/A'}</dd>
            </div>
            <div>
              <dt className="text-xs text-text-muted">Role & Authorization</dt>
              <dd className="font-medium text-text mt-0.5">Enterprise Staff Administrator</dd>
            </div>
          </dl>
        </Card>

        <Card>
          <div className="flex items-center gap-2 mb-4 border-b border-border pb-3">
            <Building size={18} className="text-primary" />
            <h3 className="font-semibold text-text">Organization Context</h3>
          </div>
          <dl className="space-y-3 text-sm">
            <div>
              <dt className="text-xs text-text-muted">Active Company Scope</dt>
              <dd className="font-mono text-xs font-semibold text-primary mt-0.5">{companyId || 'No active company'}</dd>
            </div>
            <div>
              <dt className="text-xs text-text-muted">Tenant Scope</dt>
              <dd className="font-medium text-text mt-0.5">Primary Enterprise Workspace</dd>
            </div>
            <div>
              <dt className="text-xs text-text-muted">Permissions Granted</dt>
              <dd className="font-medium text-text mt-0.5">{session?.permissions?.length ?? 0} active RBAC policies</dd>
            </div>
          </dl>
        </Card>
      </div>
    </div>
  );
}
