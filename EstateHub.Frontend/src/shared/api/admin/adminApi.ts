import { API_CONFIG } from '../../config/api';
import { throwUserFriendlyError } from '../../lib/errorParser';

const AUTH_BASE_URL = API_CONFIG.authorizationApiUrl;

// Helper to ensure proper URL construction with trailing slash
const buildAuthUrl = (endpoint: string): string => {
  const base = AUTH_BASE_URL.endsWith('/') ? AUTH_BASE_URL : `${AUTH_BASE_URL}/`;
  const path = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
  return `${base}${path}`;
};

const getAuthHeaders = (): HeadersInit => {
  const token = localStorage.getItem('estatehub_access_token');
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
  };
  
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  
  return headers;
};

export interface AdminUser {
  id: string;
  email: string;
  userName: string;
  displayName: string;
  phoneNumber?: string | null;
  country?: string | null;
  city?: string | null;
  address?: string | null;
  postalCode?: string | null;
  companyName?: string | null;
  website?: string | null;
  lastActive?: string | null;
  isDeleted: boolean;
  deletedAt?: string | null;
  avatar: string;
  roles?: string[];
}

export interface PagedUsersResponse {
  items: AdminUser[];
  total: number;
  page: number;
  pageSize: number;
}

export interface UserStatsResponse {
  totalUsers: number;
  activeUsers: number;
  suspendedUsers: number;
  newUsersThisMonth: number;
}

export interface AssignRoleRequest {
  role: string;
}

export interface SuspendUserRequest {
  reason: string;
}

export const adminApi = {
  async getUsers(
    page: number = 1,
    pageSize: number = 20,
    includeDeleted: boolean = false
  ): Promise<PagedUsersResponse> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      includeDeleted: includeDeleted.toString(),
    });

    const response = await fetch(buildAuthUrl(`admin/users?${params}`), {
      method: 'GET',
      headers: getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }

    const data = await response.json();
    // Backend returns { items, total, page, pageSize } but we need to map it
    return {
      items: data.items || data.users || [],
      total: data.total || 0,
      page: data.page || page,
      pageSize: data.pageSize || pageSize,
    };
  },

  async getUserStats(): Promise<UserStatsResponse> {
    const response = await fetch(buildAuthUrl('admin/users/stats'), {
      method: 'GET',
      headers: getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }

    return response.json();
  },

  async getUser(id: string): Promise<AdminUser> {
    const response = await fetch(buildAuthUrl(`user/${id}`), {
      method: 'GET',
      headers: getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }

    return response.json();
  },

  async assignRole(userId: string, role: string): Promise<void> {
    const response = await fetch(buildAuthUrl(`admin/users/${userId}/roles`), {
      method: 'POST',
      headers: getAuthHeaders(),
      credentials: 'include',
      body: JSON.stringify({ role }),
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }
  },

  async removeRole(userId: string, role: string): Promise<void> {
    const response = await fetch(buildAuthUrl(`admin/users/${userId}/roles/${role}`), {
      method: 'DELETE',
      headers: getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }
  },

  async suspendUser(userId: string, reason: string): Promise<void> {
    const response = await fetch(buildAuthUrl(`admin/users/${userId}/suspend`), {
      method: 'POST',
      headers: getAuthHeaders(),
      credentials: 'include',
      body: JSON.stringify({ reason }),
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }
  },

  async activateUser(userId: string): Promise<void> {
    const response = await fetch(buildAuthUrl(`admin/users/${userId}/activate`), {
      method: 'POST',
      headers: getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }
  },

  async deleteUser(userId: string): Promise<void> {
    const response = await fetch(buildAuthUrl(`admin/users/${userId}`), {
      method: 'DELETE',
      headers: getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }
  },
};

