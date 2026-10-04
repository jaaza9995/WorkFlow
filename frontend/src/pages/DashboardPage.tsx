import { Link } from 'react-router-dom';
import { api } from '../api';
import { canManage } from '../auth/roles';
import { useAuth } from '../auth/useAuth';
import { EmptyState, ErrorNote, Loading, PageHeader, ProgressTrack, TaskStatusBadge } from '../components/ui';
import { formatDate, formatHours, isOverdue } from '../labels';
import { useLoad } from '../useLoad';

function Figure({ label, value, warn }: { label: string; value: string | number; warn?: boolean }) {
  return (
    <div className={warn ? 'figure figure-warn' : 'figure'}>
      <dt>{label}</dt>
      <dd>{value}</dd>
    </div>
  );
}

export function DashboardPage() {
  const { user } = useAuth();
  const { data, error, loading } = useLoad(() => api.dashboard(), 'dashboard');

  if (!user) return null;

  return (
    <>
      <PageHeader title={`Hei, ${user.firstName}`} subtitle="Dette er status akkurat nå." />

      {loading && <Loading />}
      <ErrorNote message={error} />

      {data && (
        <>
          <dl className="figures">
            <Figure label="Aktive prosjekter" value={data.activeProjects} />
            <Figure
              label={user.role === 'Employee' ? 'Mine åpne oppgaver' : 'Åpne oppgaver'}
              value={data.openTasks}
            />
            <Figure label="Forfalte oppgaver" value={data.overdueTasks} warn={data.overdueTasks > 0} />
            <Figure label="Frist denne uken" value={data.tasksThisWeek} />
            {data.hoursThisWeek !== null && (
              <Figure label="Timer denne uken" value={formatHours(data.hoursThisWeek)} />
            )}
          </dl>

          <div className="columns">
            <section>
              <h2>Fremdrift</h2>
              {data.projectProgress.length === 0 ? (
                <EmptyState title="Ingen aktive prosjekter ennå">
                  {canManage(user.role) ? (
                    <Link to="/prosjekter">Gå til prosjekter for å opprette det første</Link>
                  ) : (
                    'Når et prosjekt er aktivt, dukker det opp her.'
                  )}
                </EmptyState>
              ) : (
                <ul className="rows">
                  {data.projectProgress.map((p) => (
                    <li key={p.id} className="row-progress">
                      <div className="row-progress-head">
                        <Link to={`/prosjekter/${p.id}`} className="row-title">
                          {p.name}
                        </Link>
                        <span className="figure-small">{p.progressPercent} %</span>
                      </div>
                      <ProgressTrack percent={p.progressPercent} label={`Fremdrift for ${p.name}`} />
                      <p className="muted small">
                        {p.customerName}, {p.completedTasks} av {p.totalTasks} oppgaver ferdig
                      </p>
                    </li>
                  ))}
                </ul>
              )}
            </section>

            <section>
              <h2>{user.role === 'Employee' ? 'Mine neste oppgaver' : 'Neste oppgaver'}</h2>
              {data.upcomingTasks.length === 0 ? (
                <EmptyState title="Ingen åpne oppgaver">Her vises oppgavene med nærmest frist.</EmptyState>
              ) : (
                <ul className="rows">
                  {data.upcomingTasks.map((t) => (
                    <li key={t.id} className="row-task">
                      <div>
                        <Link to={`/prosjekter/${t.projectId}`} className="row-title">
                          {t.title}
                        </Link>
                        <p className="muted small">
                          {t.projectName}
                          {t.assignedToUserName ? `, ${t.assignedToUserName}` : ''}
                        </p>
                      </div>
                      <div className="row-task-side">
                        <TaskStatusBadge status={t.status} />
                        <span className={isOverdue(t.dueDate, t.status) ? 'due due-late' : 'due'}>
                          {t.dueDate ? formatDate(t.dueDate) : 'Ingen frist'}
                        </span>
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </section>
          </div>
        </>
      )}
    </>
  );
}
