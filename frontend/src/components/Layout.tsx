import { NavLink, Outlet } from 'react-router-dom';
import { canManage, isStaff } from '../auth/roles';
import { useAuth } from '../auth/useAuth';
import { fullName, roleLabel } from '../labels';

export function Layout() {
  const { user, logout } = useAuth();
  if (!user) return null;

  const links = [
    { to: '/', label: 'Oversikt', end: true, show: true },
    { to: '/prosjekter', label: 'Prosjekter', end: false, show: true },
    { to: '/timer', label: 'Timer', end: false, show: isStaff(user.role) },
    { to: '/kunder', label: 'Kunder', end: false, show: canManage(user.role) },
    { to: '/brukere', label: 'Brukere', end: false, show: canManage(user.role) },
  ].filter((link) => link.show);

  return (
    <div className="shell">
      <aside className="sidebar">
        <div className="brand">WorkFlow</div>

        <nav aria-label="Hovedmeny" className="nav">
          {links.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              end={link.end}
              className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
            >
              {link.label}
            </NavLink>
          ))}
        </nav>

        <div className="whoami">
          <p className="whoami-name">{fullName(user)}</p>
          <p className="muted small">{roleLabel[user.role]}</p>
          <button type="button" className="btn btn-quiet" onClick={logout}>
            Logg ut
          </button>
        </div>
      </aside>

      <main className="main">
        <Outlet />
      </main>
    </div>
  );
}
