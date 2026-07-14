import { create } from 'zustand';
import { persist } from 'zustand/middleware';

/**
 * Holds the signed-in session (07_SECURITY.md). Access token lives here only
 * (attached as a Bearer header by shared/api/client.ts); the refresh token is
 * persisted too so a page reload doesn't force a fresh login — apiClient
 * silently exchanges it for a new access token on the first 401.
 */
export interface AuthSession {
  userId: string;
  email: string;
  fullName: string;
  companyId: string | null;
  branchId: string | null;
  permissions: string[];
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
}

interface AuthState {
  session: AuthSession | null;
  setSession: (session: AuthSession) => void;
  clearSession: () => void;
  hasPermission: (code: string) => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      session: null,
      setSession: (session) => set({ session }),
      clearSession: () => set({ session: null }),
      hasPermission: (code) => get().session?.permissions.includes(code) ?? false,
    }),
    { name: 'fusionos.auth-session' },
  ),
);
