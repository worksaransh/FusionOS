/**
 * Thin fetch wrapper shared by every module's API calls. Understands the
 * RFC 7807 Problem Details error envelope every FusionOS endpoint returns
 * (08_API_STANDARDS.md §6) so components can render one consistent error UI.
 * Also attaches the signed-in session's access token (07_SECURITY.md) and
 * silently rotates it via the refresh token on the first 401 a request hits.
 */
import { useAuthStore, type AuthSession } from '../auth/authStore';

export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  traceId?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  problem: ProblemDetails;

  constructor(problem: ProblemDetails) {
    super(problem.title);
    this.problem = problem;
  }
}

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api/v1';

interface AuthResultResponse {
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

function toSession(dto: AuthResultResponse): AuthSession {
  return { ...dto };
}

// Coalesces concurrent 401s onto a single in-flight refresh call rather than
// firing one refresh request per failed request.
let refreshInFlight: Promise<boolean> | null = null;

async function refreshAccessToken(): Promise<boolean> {
  const session = useAuthStore.getState().session;
  if (!session) return false;

  if (!refreshInFlight) {
    refreshInFlight = (async () => {
      try {
        const response = await fetch(`${BASE_URL}/core/auth/refresh`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ refreshToken: session.refreshToken }),
        });
        if (!response.ok) return false;

        const dto = (await response.json()) as AuthResultResponse;
        useAuthStore.getState().setSession(toSession(dto));
        return true;
      } catch {
        return false;
      } finally {
        refreshInFlight = null;
      }
    })();
  }

  return refreshInFlight;
}

async function request<T>(path: string, init?: RequestInit, isRetry = false): Promise<T> {
  const session = useAuthStore.getState().session;

  const response = await fetch(`${BASE_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(session ? { Authorization: `Bearer ${session.accessToken}` } : {}),
      ...(init?.headers ?? {}),
    },
  });

  if (response.status === 401 && !isRetry && session) {
    const refreshed = await refreshAccessToken();
    if (refreshed) {
      return request<T>(path, init, true);
    }
    // Refresh token is also expired/revoked — the session is truly over.
    // RequireAuth (app/RequireAuth.tsx) reacts to session becoming null and
    // redirects to /login.
    useAuthStore.getState().clearSession();
  }

  if (!response.ok) {
    const problem = (await response.json().catch(() => ({
      title: response.statusText,
      status: response.status,
    }))) as ProblemDetails;
    throw new ApiError(problem);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export const apiClient = {
  get: <T>(path: string) => request<T>(path, { method: 'GET' }),
  post: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'POST', body: JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'PUT', body: JSON.stringify(body) }),
};
