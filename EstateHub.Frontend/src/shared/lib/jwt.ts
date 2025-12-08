import { jwtDecode } from 'jwt-decode';

/**
 * JWT token payload interface
 */
export interface JwtPayload {
  exp?: number; // Expiration timestamp
  iat?: number; // Issued at timestamp
  [key: string]: unknown; // Allow other claims
}

/**
 * Parses and validates a JWT token
 * @param token - The JWT token string
 * @returns Decoded token payload or null if invalid
 */
export function parseJwtToken(token: string): JwtPayload | null {
  try {
    return jwtDecode<JwtPayload>(token);
  } catch {
    // Invalid token format
    return null;
  }
}

/**
 * Checks if a JWT token is expired
 * @param token - The JWT token string or decoded payload
 * @returns true if token is expired or invalid, false otherwise
 */
export function isTokenExpired(token: string | JwtPayload | null): boolean {
  if (!token) return true;

  let payload: JwtPayload;
  if (typeof token === 'string') {
    const decoded = parseJwtToken(token);
    if (!decoded) return true;
    payload = decoded;
  } else {
    payload = token;
  }

  // If no expiration claim, consider it expired for security
  if (!payload.exp) return true;

  // Check if token is expired (with 5 second buffer for clock skew)
  const currentTime = Math.floor(Date.now() / 1000);
  return payload.exp < currentTime - 5;
}

/**
 * Extracts roles from a JWT token
 * Supports multiple claim formats:
 * - http://schemas.microsoft.com/ws/2008/06/identity/claims/role
 * - role
 * - roles
 * @param token - The JWT token string or decoded payload
 * @returns Array of role strings
 */
export function getRolesFromToken(token: string | JwtPayload | null): string[] {
  if (!token) return [];

  let payload: JwtPayload;
  if (typeof token === 'string') {
    const decoded = parseJwtToken(token);
    if (!decoded) return [];
    payload = decoded;
  } else {
    payload = token;
  }

  // Try different claim formats
  const roleClaim =
    payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
    payload.role ||
    payload.roles;

  if (!roleClaim) return [];

  // Handle both array and single value
  if (Array.isArray(roleClaim)) {
    return roleClaim.filter((role): role is string => typeof role === 'string');
  }

  if (typeof roleClaim === 'string') {
    return [roleClaim];
  }

  return [];
}

/**
 * Checks if a token has a specific role
 * @param token - The JWT token string or decoded payload
 * @param role - The role to check for
 * @returns true if token has the role, false otherwise
 */
export function hasRole(token: string | JwtPayload | null, role: string): boolean {
  const roles = getRolesFromToken(token);
  return roles.includes(role);
}



