import { API_CONFIG } from '../../config/api';
import { throwUserFriendlyError } from '../../lib/errorParser';

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
  phoneNumber?: string;
  country?: string;
  city?: string;
  address?: string;
  postalCode?: string;
  companyName?: string;
  website?: string;
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
      await throwUserFriendlyError(response);
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

    if (request.phoneNumber !== undefined) {
      formData.append('phoneNumber', request.phoneNumber || '');
    }
    if (request.country !== undefined) {
      formData.append('country', request.country || '');
    }
    if (request.city !== undefined) {
      formData.append('city', request.city || '');
    }
    if (request.address !== undefined) {
      formData.append('address', request.address || '');
    }
    if (request.postalCode !== undefined) {
      formData.append('postalCode', request.postalCode || '');
    }
    if (request.companyName !== undefined) {
      formData.append('companyName', request.companyName || '');
    }
    if (request.website !== undefined) {
      formData.append('website', request.website || '');
    }

    const response = await fetch(buildAuthUrl(`user/${id}`), {
      method: 'PATCH',
      headers: getAuthHeaders(false), // Don't set Content-Type for FormData
      credentials: 'include',
      body: formData,
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }
  },

  async deleteUser(id: string): Promise<void> {
    const response = await fetch(buildAuthUrl(`user/${id}`), {
      method: 'DELETE',
      headers: getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }
  },

  async getSession(): Promise<SessionResponse> {
    const response = await fetch(buildAuthUrl('session'), {
      method: 'GET',
      headers: getAuthHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      await throwUserFriendlyError(response);
    }

    return response.json();
  },
};



