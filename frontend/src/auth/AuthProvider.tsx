import { useCallback, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { api } from '../api';
import type { RegisterCompanyInput } from '../api';
import { clearAuth, loadAuth, saveAuth, setUnauthorizedHandler } from '../api/client';
import type { AuthResponse, User } from '../api/types';
import { AuthContext } from './context';
import type { AuthContextValue } from './context';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => loadAuth()?.user ?? null);

  const logout = useCallback(() => {
    clearAuth();
    setUser(null);
  }, []);

  // Hvis API-et sier at tokenet er utløpt, logger vi ut
  useEffect(() => {
    setUnauthorizedHandler(logout);
    return () => setUnauthorizedHandler(null);
  }, [logout]);

  const finish = useCallback((auth: AuthResponse) => {
    saveAuth(auth);
    setUser(auth.user);
  }, []);

  const login = useCallback(
    async (email: string, password: string) => {
      finish(await api.auth.login(email, password));
    },
    [finish],
  );

  const registerCompany = useCallback(
    async (input: RegisterCompanyInput) => {
      finish(await api.auth.registerCompany(input));
    },
    [finish],
  );

  const value = useMemo<AuthContextValue>(
    () => ({ user, login, registerCompany, logout }),
    [user, login, registerCompany, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
