import { Link, Route, Routes } from 'react-router-dom';
import { RequireAuth } from './auth/RequireAuth';
import { Layout } from './components/Layout';
import { PageHeader } from './components/ui';
import { CustomersPage } from './pages/CustomersPage';
import { DashboardPage } from './pages/DashboardPage';
import { LoginPage } from './pages/LoginPage';
import { ProjectDetailPage } from './pages/ProjectDetailPage';
import { ProjectsPage } from './pages/ProjectsPage';
import { TimeEntriesPage } from './pages/TimeEntriesPage';
import { UsersPage } from './pages/UsersPage';

function NotFound() {
  return (
    <>
      <PageHeader title="Siden finnes ikke" />
      <p>
        <Link to="/">Gå til oversikten</Link>
      </p>
    </>
  );
}

export default function App() {
  return (
    <Routes>
      <Route path="/logg-inn" element={<LoginPage />} />

      <Route element={<RequireAuth />}>
        <Route element={<Layout />}>
          <Route index element={<DashboardPage />} />
          <Route path="prosjekter" element={<ProjectsPage />} />
          <Route path="prosjekter/:id" element={<ProjectDetailPage />} />

          <Route element={<RequireAuth roles={['Administrator', 'Manager', 'Employee']} />}>
            <Route path="timer" element={<TimeEntriesPage />} />
          </Route>

          <Route element={<RequireAuth roles={['Administrator', 'Manager']} />}>
            <Route path="kunder" element={<CustomersPage />} />
            <Route path="brukere" element={<UsersPage />} />
          </Route>

          <Route path="*" element={<NotFound />} />
        </Route>
      </Route>
    </Routes>
  );
}
