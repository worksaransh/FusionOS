import { render, screen } from '@testing-library/react';
import { afterEach, describe, expect, it } from 'vitest';
import { RequirePermission } from './RequirePermission';
import { useAuthStore, type AuthSession } from '../shared/auth/authStore';

function sessionWith(permissions: string[]): AuthSession {
  return {
    userId: 'u1',
    email: 'user@example.com',
    fullName: 'Test User',
    companyId: 'c1',
    branchId: null,
    permissions,
    accessToken: 'token',
    accessTokenExpiresAt: new Date().toISOString(),
    refreshToken: 'refresh',
    refreshTokenExpiresAt: new Date().toISOString(),
  };
}

describe('RequirePermission', () => {
  afterEach(() => {
    useAuthStore.setState({ session: null });
  });

  it('renders children when the signed-in user holds the required permission', () => {
    useAuthStore.setState({ session: sessionWith(['sales.customer.read']) });

    render(
      <RequirePermission permission="sales.customer.read">
        <p>Customer list</p>
      </RequirePermission>,
    );

    expect(screen.getByText('Customer list')).toBeInTheDocument();
    expect(screen.queryByText(/not authorized/i)).not.toBeInTheDocument();
  });

  it('renders a "Not authorized" placeholder instead of children when the permission is missing', () => {
    useAuthStore.setState({ session: sessionWith(['inventory.product.read']) });

    render(
      <RequirePermission permission="sales.customer.read">
        <p>Customer list</p>
      </RequirePermission>,
    );

    expect(screen.queryByText('Customer list')).not.toBeInTheDocument();
    expect(screen.getByText(/not authorized/i)).toBeInTheDocument();
  });

  it('allows access when the user holds ANY one of a list of acceptable permission codes', () => {
    useAuthStore.setState({ session: sessionWith(['finance.receivable.read']) });

    render(
      <RequirePermission permission={['finance.account.read', 'finance.receivable.read']}>
        <p>Finance page</p>
      </RequirePermission>,
    );

    expect(screen.getByText('Finance page')).toBeInTheDocument();
  });

  it('blocks a user with zero permissions (e.g. a freshly-invited Member role)', () => {
    useAuthStore.setState({ session: sessionWith([]) });

    render(
      <RequirePermission permission="core.company.read">
        <p>Companies</p>
      </RequirePermission>,
    );

    expect(screen.queryByText('Companies')).not.toBeInTheDocument();
    expect(screen.getByText(/not authorized/i)).toBeInTheDocument();
  });
});
