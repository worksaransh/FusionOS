import { create } from 'zustand';
import { persist } from 'zustand/middleware';

/**
 * Holds the CUSTOMER's own portal session - deliberately a completely
 * separate Zustand store from shared/auth/authStore's useAuthStore (staff
 * sessions), persisted under its own localStorage key so a customer's login
 * can never be confused with (or silently reuse) a staff session in the same
 * browser. Mirrors AuthSession's shape but only carries what a portal token
 * actually has - no companyId/branchId/permissions, since the backend
 * (PortalController) derives everything from the token's own customer_id
 * claim rather than trusting anything the client sends (see
 * backend PortalLoginCommandHandler's doc comment).
 */
export interface PortalSession {
  portalUserId: string;
  customerId: string;
  email: string;
  accessToken: string;
  accessTokenExpiresAt: string;
}

interface PortalAuthState {
  session: PortalSession | null;
  setSession: (session: PortalSession) => void;
  clearSession: () => void;
}

export const usePortalAuthStore = create<PortalAuthState>()(
  persist(
    (set) => ({
      session: null,
      setSession: (session) => set({ session }),
      clearSession: () => set({ session: null }),
    }),
    { name: 'fusionos.portal-session' },
  ),
);
