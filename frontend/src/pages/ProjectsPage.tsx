import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { api } from '../api';
import { canManage } from '../auth/roles';
import { useAuth } from '../auth/useAuth';
import { ProjectFormModal } from '../components/ProjectFormModal';
import {
  EmptyState,
  ErrorNote,
  Loading,
  PageHeader,
  ProgressTrack,
  ProjectStatusBadge,
} from '../components/ui';
import { formatDate, projectStatuses, projectStatusLabel } from '../labels';
import { useLoad } from '../useLoad';

export function ProjectsPage() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [search, setSearch] = useState('');
  const [debounced, setDebounced] = useState('');
  const [status, setStatus] = useState('');
  const [creating, setCreating] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(search), 250);
    return () => clearTimeout(timer);
  }, [search]);

  const projects = useLoad(
    () => api.projects.list({ search: debounced, status }),
    `projects-${debounced}-${status}`,
  );

  if (!user) return null;
  const manage = canManage(user.role);
  const filtering = debounced !== '' || status !== '';

  return (
    <>
      <PageHeader
        title="Prosjekter"
        actions={
          manage && (
            <button type="button" className="btn btn-primary" onClick={() => setCreating(true)}>
              Nytt prosjekt
            </button>
          )
        }
      />

      <div className="filters">
        <input
          className="input"
          type="search"
          placeholder="Søk i prosjekter"
          aria-label="Søk i prosjekter"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <select
          className="input"
          aria-label="Filtrer på status"
          value={status}
          onChange={(e) => setStatus(e.target.value)}
        >
          <option value="">Alle statuser</option>
          {projectStatuses.map((s) => (
            <option key={s} value={s}>
              {projectStatusLabel[s]}
            </option>
          ))}
        </select>
      </div>

      <ErrorNote message={projects.error} />
      {projects.loading && <Loading />}

      {projects.data && projects.data.length === 0 && (
        <EmptyState title={filtering ? 'Ingen prosjekter passer til søket' : 'Ingen prosjekter ennå'}>
          {filtering
            ? 'Prøv et annet søk eller fjern filteret.'
            : manage
              ? 'Opprett det første prosjektet for å komme i gang.'
              : 'Du er ikke tilknyttet noen prosjekter ennå.'}
        </EmptyState>
      )}

      {projects.data && projects.data.length > 0 && (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Prosjekt</th>
                <th>Kunde</th>
                <th>Ansvarlig</th>
                <th>Status</th>
                <th>Slutt</th>
                <th>Fremdrift</th>
              </tr>
            </thead>
            <tbody>
              {projects.data.map((p) => (
                <tr key={p.id}>
                  <td>
                    <Link to={`/prosjekter/${p.id}`} className="row-title">
                      {p.name}
                    </Link>
                  </td>
                  <td>{p.customerName}</td>
                  <td>{p.responsibleManagerName}</td>
                  <td>
                    <ProjectStatusBadge status={p.status} />
                  </td>
                  <td>{formatDate(p.endDate)}</td>
                  <td className="cell-progress">
                    <ProgressTrack percent={p.progressPercent} label={`Fremdrift for ${p.name}`} />
                    <span className="small">{p.progressPercent} %</span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {creating && (
        <ProjectFormModal
          onClose={() => setCreating(false)}
          onSaved={(created) => {
            setCreating(false);
            if (created) navigate(`/prosjekter/${created.id}`);
            else projects.reload();
          }}
        />
      )}
    </>
  );
}
