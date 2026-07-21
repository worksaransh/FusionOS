import { afterEach, describe, expect, it } from 'vitest';
import { useAuthStore, type AuthSession } from './authStore';

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

describe('authStore', () => {
  afterEach(() => {
    useAuthStore.setState({ session: null });
  });

  describe('hasPermission', () => {
    it('returns false when there is no session', () => {
      expect(useAuthStore.getState().hasPermission('sales.customer.read')).toBe(false);
    });

    it('returns true only for an exact permission code the session holds', () => {
      useAuthStore.setState({ session: sessionWith(['sales.customer.read']) });

      expect(useAuthStore.getState().hasPermission('sales.customer.read')).toBe(true);
      expect(useAuthStore.getState().hasPermission('sales.customer.create')).toBe(false);
    });
  });

  describe('hasPermissionPrefix', () => {
    it('returns false when there is no session', () => {
      expect(useAuthStore.getState().hasPermissionPrefix('sales')).toBe(false);
    });

    it('returns false when the session holds no permission under that module prefix', () => {
      useAuthStore.setState({ session: sessionWith(['inventory.product.read']) });

      expect(useAuthStore.getState().hasPermissionPrefix('sales')).toBe(false);
    });

    it('returns true when the session holds any permission whose code starts with "<prefix>."', () => {
      useAuthStore.setState({ session: sessionWith(['sales.invoice.issue']) });

      expect(useAuthStore.getState().hasPermissionPrefix('sales')).toBe(true);
    });

    it('does not treat one module as a substring match of another (e.g. "sale" vs "sales")', () => {
      useAuthStore.setState({ session: sessionWith(['sales.invoice.issue']) });

      expect(useAuthStore.getState().hasPermissionPrefix('sale')).toBe(false);
    });
  });
});
