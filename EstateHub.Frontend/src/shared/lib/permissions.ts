/**
 * Permission definitions matching the backend PermissionDefinitions.cs
 */
export const PERMISSIONS = {
  // System permissions
  SystemAdmin: 'SystemAdmin',
  UserManagement: 'UserManagement',
  ContentModeration: 'ContentModeration',
  RoleManagement: 'RoleManagement',
  
  // Business permissions
  CreateListings: 'CreateListings',
  ManageListings: 'ManageListings',
  ViewAnalytics: 'ViewAnalytics',
  
  // Report permissions
  CreateReports: 'CreateReports',
  ManageReports: 'ManageReports',
  ViewReports: 'ViewReports',
} as const;

/**
 * Role to permissions mapping (matching backend PermissionDefinitions.cs)
 */
const ROLE_PERMISSIONS: Record<string, string[]> = {
  Admin: [
    PERMISSIONS.SystemAdmin,
    PERMISSIONS.UserManagement,
    PERMISSIONS.ContentModeration,
    PERMISSIONS.RoleManagement,
    PERMISSIONS.CreateListings,
    PERMISSIONS.ManageListings,
    PERMISSIONS.ViewAnalytics,
    PERMISSIONS.CreateReports,
    PERMISSIONS.ManageReports,
    PERMISSIONS.ViewReports,
  ],
  User: [
    PERMISSIONS.CreateListings,
    PERMISSIONS.ManageListings,
    PERMISSIONS.CreateReports,
  ],
};

/**
 * Parse JWT token to extract roles
 */
export function parseJwtRoles(token: string): string[] {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    const decoded = JSON.parse(jsonPayload);
    
    // Extract roles from claims (could be 'role' or 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role')
    const roleClaim = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 
                     decoded.role || 
                     decoded.roles || 
                     [];
    
    return Array.isArray(roleClaim) ? roleClaim : [roleClaim].filter(Boolean);
  } catch {
    // Silently fail - return empty array if token parsing fails
    return [];
  }
}

/**
 * Get all permissions for given roles
 */
export function getPermissionsForRoles(roles: string[]): string[] {
  const allPermissions = new Set<string>();
  
  for (const role of roles) {
    const permissions = ROLE_PERMISSIONS[role] || [];
    permissions.forEach(perm => allPermissions.add(perm));
  }
  
  return Array.from(allPermissions);
}

/**
 * Check if user has a specific permission
 */
export function hasPermission(userRoles: string[], permission: string): boolean {
  const userPermissions = getPermissionsForRoles(userRoles);
  return userPermissions.includes(permission);
}

/**
 * Check if user has any of the specified permissions
 */
export function hasAnyPermission(userRoles: string[], ...permissions: string[]): boolean {
  return permissions.some(permission => hasPermission(userRoles, permission));
}

/**
 * Check if user has all of the specified permissions
 */
export function hasAllPermissions(userRoles: string[], ...permissions: string[]): boolean {
  return permissions.every(permission => hasPermission(userRoles, permission));
}

/**
 * Get user roles from token stored in localStorage
 */
export function getUserRoles(): string[] {
  const token = localStorage.getItem('estatehub_access_token');
  if (!token) {
    return [];
  }
  
  return parseJwtRoles(token);
}

/**
 * Get user permissions from token stored in localStorage
 */
export function getUserPermissions(): string[] {
  const roles = getUserRoles();
  return getPermissionsForRoles(roles);
}

