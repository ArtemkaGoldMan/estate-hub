import { useState, useEffect, useCallback } from 'react';
import { adminApi, type PagedUsersResponse, type UserStatsResponse } from './adminApi';

export const useAdminUsers = (
  page: number = 1,
  pageSize: number = 20,
  includeDeleted: boolean = false
) => {
  const [data, setData] = useState<PagedUsersResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchUsers = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await adminApi.getUsers(page, pageSize, includeDeleted);
      setData(result);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch users'));
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, includeDeleted]);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  return { data, loading, error, refetch: fetchUsers };
};

export const useAdminUserStats = () => {
  const [data, setData] = useState<UserStatsResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchStats = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await adminApi.getUserStats();
      setData(result);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch user stats'));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchStats();
  }, [fetchStats]);

  return { data, loading, error, refetch: fetchStats };
};

export const useAdminUserActions = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const assignRole = useCallback(async (userId: string, role: string) => {
    setLoading(true);
    setError(null);
    try {
      await adminApi.assignRole(userId, role);
    } catch (err) {
      const error = err instanceof Error ? err : new Error('Failed to assign role');
      setError(error);
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  const removeRole = useCallback(async (userId: string, role: string) => {
    setLoading(true);
    setError(null);
    try {
      await adminApi.removeRole(userId, role);
    } catch (err) {
      const error = err instanceof Error ? err : new Error('Failed to remove role');
      setError(error);
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  const suspendUser = useCallback(async (userId: string, reason: string) => {
    setLoading(true);
    setError(null);
    try {
      await adminApi.suspendUser(userId, reason);
    } catch (err) {
      const error = err instanceof Error ? err : new Error('Failed to suspend user');
      setError(error);
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  const activateUser = useCallback(async (userId: string) => {
    setLoading(true);
    setError(null);
    try {
      await adminApi.activateUser(userId);
    } catch (err) {
      const error = err instanceof Error ? err : new Error('Failed to activate user');
      setError(error);
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  const deleteUser = useCallback(async (userId: string) => {
    setLoading(true);
    setError(null);
    try {
      await adminApi.deleteUser(userId);
    } catch (err) {
      const error = err instanceof Error ? err : new Error('Failed to delete user');
      setError(error);
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    assignRole,
    removeRole,
    suspendUser,
    activateUser,
    deleteUser,
    loading,
    error,
  };
};

