import { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from 'react';
import { authApi, type AuthenticationResponse } from '../api/auth';

interface AuthContextType {
  user: AuthenticationResponse | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, confirmPassword: string) => Promise<AuthenticationResponse | null>;
  confirmEmail: (token: string, userId: string) => Promise<void>;
  logout: () => Promise<void>;
  setUser: (user: AuthenticationResponse | null) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const ACCESS_TOKEN_KEY = 'estatehub_access_token';
const USER_KEY = 'estatehub_user';

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUserState] = useState<AuthenticationResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Load user from localStorage on mount
  useEffect(() => {
    const storedUser = localStorage.getItem(USER_KEY);
    const storedToken = localStorage.getItem(ACCESS_TOKEN_KEY);

    if (storedUser && storedToken) {
      try {
        const userData = JSON.parse(storedUser);
        setUserState(userData);
      } catch {
        // Clear invalid stored data
        localStorage.removeItem(USER_KEY);
        localStorage.removeItem(ACCESS_TOKEN_KEY);
      }
    }

    setIsLoading(false);
  }, []);

  const setUser = useCallback((userData: AuthenticationResponse | null) => {
    setUserState(userData);
    if (userData) {
      localStorage.setItem(USER_KEY, JSON.stringify(userData));
      localStorage.setItem(ACCESS_TOKEN_KEY, userData.accessToken);
    } else {
      localStorage.removeItem(USER_KEY);
      localStorage.removeItem(ACCESS_TOKEN_KEY);
    }
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const response = await authApi.login({ email, password });
    setUser(response);
  }, [setUser]);

  const register = useCallback(async (email: string, password: string, confirmPassword: string): Promise<AuthenticationResponse | null> => {
    // Construct callback URL for email confirmation
    const callbackUrl = `${window.location.origin}/confirm-email`;
    const response = await authApi.register({ 
      email, 
      password, 
      confirmPassword, 
      callbackUrl 
    });
    // Only set user if response is not null (user is auto-logged in)
    // If null, email confirmation is required
    if (response) {
      setUser(response);
    }
    return response;
  }, [setUser]);

  const confirmEmail = useCallback(async (token: string, userId: string) => {
    const response = await authApi.confirmEmail({ token, userId });
    setUser(response);
  }, [setUser]);

  const logout = useCallback(async () => {
    // Get access token before clearing user state
    const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);
    
    try {
      // Try to logout on server if we have a token
      if (accessToken) {
        await authApi.logout(accessToken);
      }
    } catch (error) {
      // Even if logout fails (e.g., token expired), clear local state
      console.error('Logout error:', error);
    } finally {
      // Always clear local state, even if API call fails
      setUser(null);
    }
  }, [setUser]);

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user,
    isLoading,
    login,
    register,
    confirmEmail,
    logout,
    setUser,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};


