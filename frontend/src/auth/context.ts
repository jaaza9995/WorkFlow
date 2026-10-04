import { createContext } from 'react';
import type { RegisterCompanyInput } from '../api';
import type { User } from '../api/types';

export interface AuthContextValue {
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  registerCompany: (input: RegisterCompanyInput) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
