import { useContext } from 'react';
import { AuthContext } from './context';

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth må brukes inni AuthProvider');
  return ctx;
}
