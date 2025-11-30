import { API_CONFIG } from '../../config/api';
import { throwUserFriendlyError } from '../../lib/errorParser';
import type {
  UserRegistrationRequest,
  LoginRequest,
  ConfirmEmailRequest,
  AuthenticationResponse,
} from './types';

const AUTH_BASE_URL = API_CONFIG.authorizationApiUrl;

// Helper to ensure proper URL construction with trailing slash
const buildAuthUrl = (endpoint: string): string => {
  const base = AUTH_BASE_URL.endsWith('/') ? AUTH_BASE_URL : `${AUTH_BASE_URL}/`;
  const path = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
  return `${base}${path}`;
};

export const authApi = {
  async register(
    request: UserRegistrationRequest
  ): Promise<AuthenticationResponse | null> {
    const response = await fetch(buildAuthUrl('user-registration'), {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include', // Important for cookies
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }

    // Check if response has content
    const contentType = response.headers.get('content-type');
    if (!contentType || !contentType.includes('application/json')) {
      return null; // No JSON response means email confirmation required
    }

    const text = await response.text();
    if (!text || text.trim() === '' || text === '{}') {
      return null; // Empty response means email confirmation required
    }

    try {
      const data = JSON.parse(text);
      // If response has data, return it (user auto-logged in)
      // Otherwise return null (email confirmation required)
      return data && Object.keys(data).length > 0 ? data : null;
    } catch {
      return null;
    }
  },

  async login(request: LoginRequest): Promise<AuthenticationResponse> {
    const response = await fetch(buildAuthUrl('login'), {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include', // Important for cookies
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }

    return response.json();
  },

  async confirmEmail(request: ConfirmEmailRequest): Promise<AuthenticationResponse> {
    const response = await fetch(buildAuthUrl('confirm-email'), {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }

    return response.json();
  },

  async logout(accessToken?: string): Promise<void> {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };

    // Include JWT token in Authorization header if provided
    // Note: Logout endpoint now allows anonymous access, so token is optional
    if (accessToken) {
      headers['Authorization'] = `Bearer ${accessToken}`;
    }

    const response = await fetch(buildAuthUrl('logout'), {
      method: 'POST',
      headers,
      credentials: 'include',
    });

    // Don't throw error for 401 - token might be expired, but logout should still work
    // The backend will use refresh token from cookies
    if (!response.ok && response.status !== 401) {
      await throwUserFriendlyError(response);
    }
    // For 401, we silently succeed since we're logging out anyway
  },

  async refreshToken(): Promise<AuthenticationResponse> {
    const response = await fetch(buildAuthUrl('refresh-access-token'), {
      method: 'POST',
      credentials: 'include',
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }

    return response.json();
  },

  async forgotPassword(email: string, returnUrl?: string): Promise<void> {
    const response = await fetch(buildAuthUrl('forgot-password'), {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify({ email, returnUrl: returnUrl || '' }),
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }
  },

  async resetPassword(
    userId: string,
    token: string,
    password: string,
    confirmPassword: string
  ): Promise<void> {
    const response = await fetch(buildAuthUrl('reset-password'), {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify({
        userId,
        token,
        password,
        confirmPassword,
      }),
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }
  },

  async manageAccountState(
    email: string,
    actionType: string,
    returnUrl?: string
  ): Promise<void> {
    const response = await fetch(buildAuthUrl('manage-account-state'), {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify({
        email,
        actionType,
        returnUrl: returnUrl || '',
      }),
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }
  },

  async confirmAccountAction(
    userId: string,
    token: string,
    actionType: string
  ): Promise<void> {
    const response = await fetch(buildAuthUrl('confirm-account-action'), {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify({
        userId,
        token,
        actionType,
      }),
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }
  },
};


