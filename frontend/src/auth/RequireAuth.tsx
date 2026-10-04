import { Navigate, Outlet, useLocation } from 'react-router-dom';
import type { UserRole } from '../api/types';
import { useAuth } from './useAuth';

export function RequireAuth({ roles }: { roles?: UserRole[] }) {
  const { user } = useAuth();
  const location = useLocation();

  if (!user) return <Navigate to="/logg-inn" replace state={{ from: location.pathname }} />;
  if (roles && !roles.includes(user.role)) return <Navigate to="/" replace />;
  return <Outlet />;
}
