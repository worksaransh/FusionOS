import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient, ApiError } from '../../../shared/api/client';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { CrudListPage } from '../../../shared/ui/CrudListPage';
import { DataTable } from '../../../shared/ui/DataTable';
import { useActiveCompany } from '../../../shared/company/useActiveCompany';

const createRoleSchema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
});
type CreateRoleFormValues = z.infer<typeof createRoleSchema>;

interface RoleDto {
  id: string;
  name: string;
  isSystemRole: boolean;
}

interface PermissionDto {
  module: string;
  code: string;
  description: string;
}

interface CompanyUserDto {
  userId: string;
  email: string;
  fullName: string;
  roleId: string;
  roleName: string;
  isActive: boolean;
}

/**
 * RBAC administration (2026-07-14 sprint audit, Phase H2). Every user in a
 * company was previously the auto-granted, all-permissions "Owner" — this
 * page is what lets an Owner create a lesser-privileged role, decide exactly
 * which permissions it carries (from the same catalog enforced server-side
 * via IRequirePermission), and reassign a teammate onto it. Requires the
 * "core.role.manage" permission on every read/write here — enforced by the
 * backend, not just hidden by this page.
 */
export function RolesPage() {
  const { companyId } = useActiveCompany();
  const queryClient = useQueryClient();
  const [selectedRoleId, setSelectedRoleId] = useState<string | null>(null);
  const [renamingRoleId, setRenamingRoleId] = useState<string | null>(null);
  const [renameDraft, setRenameDraft] = useState('');

  const rolesQuery = useQuery({
    queryKey: ['roles', companyId],
    queryFn: () => apiClient.get<RoleDto[]>(`/core/roles?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  const permissionsQuery = useQuery({
    queryKey: ['permissions'],
    queryFn: () => apiClient.get<PermissionDto[]>('/core/permissions'),
  });

  const usersQuery = useQuery({
    queryKey: ['company-users', companyId],
    queryFn: () => apiClient.get<CompanyUserDto[]>(`/core/users?companyId=${companyId}`),
    enabled: Boolean(companyId),
  });

  const { register, handleSubmit, reset, setError, formState: { errors, isSubmitting } } = useForm<CreateRoleFormValues>({
    resolver: zodResolver(createRoleSchema),
  });

  const createRole = useMutation({
    mutationFn: (values: CreateRoleFormValues) => apiClient.post<RoleDto>('/core/roles', { companyId, ...values }),
    onSuccess: (role) => {
      reset();
      queryClient.invalidateQueries({ queryKey: ['roles', companyId] });
      setSelectedRoleId(role.id);
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors) {
        for (const [field, messages] of Object.entries(error.problem.errors)) {
          setError(field as keyof CreateRoleFormValues, { message: messages[0] });
        }
      }
    },
  });

  const renameRole = useMutation({
    mutationFn: ({ roleId, name }: { roleId: string; name: string }) =>
      apiClient.put(`/core/roles/${roleId}`, { companyId, name }),
    onSuccess: () => {
      setRenamingRoleId(null);
      queryClient.invalidateQueries({ queryKey: ['roles', companyId] });
    },
  });

  if (!companyId) {
    return <p className="text-text-muted">Set an active Company ID in the header above to manage roles.</p>;
  }

  return (
    <div>
      <CrudListPage<RoleDto>
        title="Roles"
        description="Company-scoped roles — every teammate holds exactly one of these within this company."
        rows={rolesQuery.data}
        isLoading={rolesQuery.isLoading}
        isError={rolesQuery.isError}
        errorMessage="Could not load roles."
        emptyMessage="No custom roles yet — every user is on the auto-created Owner role until you create one."
        rowKey={(row) => row.id}
        columns={[
          {
            header: 'Name',
            render: (row) =>
              renamingRoleId === row.id ? (
                <div className="flex items-center gap-2">
                  <input
                    className="rounded-md border border-border bg-surface px-2 py-1 text-sm"
                    value={renameDraft}
                    onChange={(e) => setRenameDraft(e.target.value)}
                    autoFocus
                  />
                  <Button
                    disabled={renameRole.isPending || !renameDraft.trim()}
                    onClick={() => renameRole.mutate({ roleId: row.id, name: renameDraft.trim() })}
                  >
                    Save
                  </Button>
                  <Button variant="secondary" onClick={() => setRenamingRoleId(null)}>Cancel</Button>
                </div>
              ) : (
                row.name
              ),
          },
          { header: 'Type', render: (row) => (row.isSystemRole ? 'System' : 'Custom') },
          {
            header: '',
            render: (row) => (
              <div className="flex items-center gap-2">
                {!row.isSystemRole && renamingRoleId !== row.id && (
                  <Button
                    variant="secondary"
                    onClick={() => {
                      setRenamingRoleId(row.id);
                      setRenameDraft(row.name);
                    }}
                  >
                    Rename
                  </Button>
                )}
                <Button variant="secondary" onClick={() => setSelectedRoleId(row.id)}>
                  Edit permissions
                </Button>
              </div>
            ),
          },
        ]}
        form={
          <form onSubmit={handleSubmit((values) => createRole.mutate(values))} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <label className="flex flex-col gap-1 text-sm">
              Name
              <input className="rounded-md border border-border bg-surface px-2 py-1.5" placeholder="Manager" {...register('name')} />
              {errors.name && <span className="text-xs text-danger">{errors.name.message}</span>}
            </label>
            <div className="col-span-2">
              <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Creating…' : 'Create role'}</Button>
            </div>
          </form>
        }
      />

      {renameRole.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not rename that role.</p>
      )}

      {selectedRoleId && (
        <RolePermissionsEditor
          companyId={companyId}
          role={rolesQuery.data?.find((r) => r.id === selectedRoleId) ?? null}
          allPermissions={permissionsQuery.data}
          onClose={() => setSelectedRoleId(null)}
        />
      )}

      <CompanyUsersPanel companyId={companyId} users={usersQuery.data} roles={rolesQuery.data} />
    </div>
  );
}

interface RolePermissionsEditorProps {
  companyId: string;
  role: RoleDto | null;
  allPermissions: PermissionDto[] | undefined;
  onClose: () => void;
}

function RolePermissionsEditor({ companyId, role, allPermissions, onClose }: RolePermissionsEditorProps) {
  const queryClient = useQueryClient();
  const [pendingCodes, setPendingCodes] = useState<Set<string> | null>(null);

  const grantedQuery = useQuery({
    queryKey: ['role-permissions', role?.id, companyId],
    queryFn: () => apiClient.get<string[]>(`/core/roles/${role!.id}/permissions?companyId=${companyId}`),
    enabled: Boolean(role),
  });

  const codes = pendingCodes ?? new Set(grantedQuery.data ?? []);

  const savePermissions = useMutation({
    mutationFn: (permissionCodes: string[]) =>
      apiClient.put<string[]>(`/core/roles/${role!.id}/permissions`, { companyId, permissionCodes }),
    onSuccess: () => {
      setPendingCodes(null);
      queryClient.invalidateQueries({ queryKey: ['role-permissions', role?.id, companyId] });
    },
  });

  const grouped = useMemo(() => {
    const byModule = new Map<string, PermissionDto[]>();
    for (const p of allPermissions ?? []) {
      const list = byModule.get(p.module) ?? [];
      list.push(p);
      byModule.set(p.module, list);
    }
    return byModule;
  }, [allPermissions]);

  if (!role) return null;

  function toggle(code: string) {
    const next = new Set(codes);
    if (next.has(code)) next.delete(code);
    else next.add(code);
    setPendingCodes(next);
  }

  return (
    <Card className="mt-8">
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-text">Permissions — {role.name}</h2>
        <Button variant="secondary" onClick={onClose}>Close</Button>
      </div>

      {grantedQuery.isLoading && <p role="status" className="text-text-muted">Loading…</p>}

      {!grantedQuery.isLoading && (
        <div className="flex flex-col gap-4">
          {[...grouped.entries()].map(([module, permissions]) => (
            <fieldset key={module} className="rounded-md border border-border p-3">
              <legend className="px-1 text-sm font-medium capitalize text-text">{module}</legend>
              <div className="flex flex-col gap-1">
                {permissions.map((p) => (
                  <label key={p.code} className="flex items-start gap-2 text-sm">
                    <input
                      type="checkbox"
                      className="mt-0.5"
                      checked={codes.has(p.code)}
                      onChange={() => toggle(p.code)}
                    />
                    <span>
                      <span className="font-mono text-xs text-text-muted">{p.code}</span> — {p.description}
                    </span>
                  </label>
                ))}
              </div>
            </fieldset>
          ))}

          <div className="flex items-center gap-3">
            <Button
              disabled={savePermissions.isPending}
              onClick={() => savePermissions.mutate([...codes])}
            >
              {savePermissions.isPending ? 'Saving…' : 'Save permissions'}
            </Button>
            {savePermissions.isError && (
              <span role="alert" className="text-sm text-danger">Could not save permissions.</span>
            )}
          </div>
        </div>
      )}
    </Card>
  );
}

interface CompanyUsersPanelProps {
  companyId: string;
  users: CompanyUserDto[] | undefined;
  roles: RoleDto[] | undefined;
}

function CompanyUsersPanel({ companyId, users, roles }: CompanyUsersPanelProps) {
  const queryClient = useQueryClient();

  const assignRole = useMutation({
    mutationFn: ({ userId, roleId }: { userId: string; roleId: string }) =>
      apiClient.post(`/core/users/${userId}/role`, { companyId, roleId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['company-users', companyId] }),
  });

  const deactivateUser = useMutation({
    mutationFn: (userId: string) => apiClient.post(`/core/users/${userId}/deactivate`, { companyId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['company-users', companyId] }),
  });

  return (
    <div className="mt-8">
      <h2 className="mb-3 text-lg font-semibold text-text">Users in this company</h2>
      <Card>
        <DataTable<CompanyUserDto>
          columns={[
            { header: 'Name', render: (u) => u.fullName },
            { header: 'Email', render: (u) => u.email },
            {
              header: 'Role',
              render: (u) => (
                <select
                  className="rounded-md border border-border bg-surface px-2 py-1 text-sm"
                  value={u.roleId}
                  disabled={assignRole.isPending}
                  onChange={(e) => assignRole.mutate({ userId: u.userId, roleId: e.target.value })}
                >
                  {(roles ?? []).map((r) => (
                    <option key={r.id} value={r.id}>{r.name}</option>
                  ))}
                  {!(roles ?? []).some((r) => r.id === u.roleId) && (
                    <option value={u.roleId}>{u.roleName}</option>
                  )}
                </select>
              ),
            },
            { header: 'Status', render: (u) => (u.isActive ? 'Active' : 'Deactivated') },
            {
              header: '',
              render: (u) =>
                u.isActive ? (
                  <Button
                    variant="danger"
                    disabled={deactivateUser.isPending}
                    onClick={() => deactivateUser.mutate(u.userId)}
                  >
                    Deactivate
                  </Button>
                ) : null,
            },
          ]}
          rows={users}
          isLoading={users === undefined}
          emptyMessage="No users in this company yet."
          rowKey={(u) => u.userId}
        />
      </Card>
      {assignRole.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not change that user's role.</p>
      )}
      {deactivateUser.isError && (
        <p role="alert" className="mt-2 text-sm text-danger">Could not deactivate that user.</p>
      )}
    </div>
  );
}
