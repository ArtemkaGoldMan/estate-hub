import { API_CONFIG } from '../../config/api';

const AUTH_BASE_URL = API_CONFIG.authorizationApiUrl;

// Helper to ensure proper URL construction with trailing slash
const buildAuthUrl = (endpoint: string): string => {
  const base = AUTH_BASE_URL.endsWith('/') ? AUTH_BASE_URL : `${AUTH_BASE_URL}/`;
  const path = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
  return `${base}${path}`;
};

export interface GetUserResponse {
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
  avatar: string; // URL or base64 string
}

export interface UserUpdateRequest {
  displayName?: string;
  avatar?: File;
}

export interface SessionResponse {
  id: string;
  userId: string;
  accessToken: string;
  refreshToken: string;
  expirationDate: string;
}

const getAuthHeaders = (includeContentType = true): HeadersInit => {
  const token = localStorage.getItem('estatehub_access_token');
  const headers: HeadersInit = {};
  
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  
  // Only set Content-Type if not multipart/form-data (for file uploads)
  if (includeContentType) {
    headers['Content-Type'] = 'application/json';
  }
  
  return headers;
};

export const userApi = {
  async getUser(id: string): Promise<GetUserResponse> {
    const response = await fetch(buildAuthUrl(`user/${id}`), {
      method: 'GET',
      headers: getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Failed to fetch user' }));
      throw new Error(error.message || 'Failed to fetch user');
    }

    return response.json();
  },

  async updateUser(id: string, request: UserUpdateRequest): Promise<void> {
    const formData = new FormData();
    
    if (request.displayName !== undefined) {
      formData.append('displayName', request.displayName);
    }
    
    if (request.avatar) {
      formData.append('avatar', request.avatar);
    }

    const response = await fetch(buildAuthUrl(`user/${id}`), {
      method: 'PATCH',
      headers: getAuthHeaders(false), // Don't set Content-Type for FormData
      credentials: 'include',
      body: formData,
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Failed to update user' }));
      throw new Error(error.message || 'Failed to update user');
    }
  },

  async deleteUser(id: string): Promise<void> {
    const response = await fetch(buildAuthUrl(`user/${id}`), {
      method: 'DELETE',
      headers: getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Failed to delete user' }));
      throw new Error(error.message || 'Failed to delete user');
    }
  },

  async getSession(): Promise<SessionResponse> {
    const response = await fetch(buildAuthUrl('session'), {
      method: 'GET',
      headers: getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Failed to fetch session' }));
      throw new Error(error.message || 'Failed to fetch session');
    }

    return response.json();
  },
};



