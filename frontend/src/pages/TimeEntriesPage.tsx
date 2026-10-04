import { useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api';
import { errorMessage } from '../api/client';
import type { TimeEntry } from '../api/types';
import { canManage } from '../auth/roles';
import { useAuth } from '../auth/useAuth';
import { TimeEntryModal } from '../components/TimeEntryModal';
import { EmptyState, ErrorNote, Loading, PageHeader } from '../components/ui';
import { formatDate, formatHours } from '../labels';
import { useLoad } from '../useLoad';

export function TimeEntriesPage() {
  const { user } = useAuth();
  const entries = useLoad(() => api.timeEntries.list({}), 'time-entries');
  const [adding, setAdding] = useState(false);
  const [editing, setEditing] = useState<TimeEntry | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  if (!user) return null;
  const manage = canManage(user.role);
  const list = entries.data ?? [];
  const total = list.reduce((sum, e) => sum + e.hours, 0);

  async function remove(entry: TimeEntry) {
    if (!window.confirm('Slette denne timeregistreringen?')) return;
    setActionError(null);
    try {
      await api.timeEntries.remove(entry.id);
      entries.reload();
    } catch (e) {
      setActionError(errorMessage(e));
    }
  }

  return (
    <>
      <PageHeader
        title="Timer"
        subtitle={manage ? 'Timer registrert av alle i bedriften.' : 'Dine registrerte timer.'}
        actions={
          <button type="button" className="btn btn-primary" onClick={() => setAdding(true)}>
            Registrer timer
          </button>
        }
      />

      <ErrorNote message={entries.error ?? actionError} />
      {entries.loading && <Loading />}

      {entries.data && list.length === 0 && (
        <EmptyState title="Ingen timer registrert ennå">
          Registrer timer på en oppgave du jobber med.
        </EmptyState>
      )}

      {list.length > 0 && (
        <>
          <p className="summary">
            Totalt <strong>{formatHours(total)}</strong> fordelt på {list.length} registreringer
          </p>
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Dato</th>
                  <th>Oppgave</th>
                  {manage && <th>Ansatt</th>}
                  <th>Beskrivelse</th>
                  <th className="num">Timer</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {list.map((e) => (
                  <tr key={e.id}>
                    <td>{formatDate(e.date)}</td>
                    <td>
                      <Link to={`/prosjekter/${e.projectId}`} className="row-title">
                        {e.taskTitle}
                      </Link>
                    </td>
                    {manage && <td>{e.userName}</td>}
                    <td>{e.description ?? '–'}</td>
                    <td className="num">{formatHours(e.hours)}</td>
                    <td className="cell-actions">
                      {e.userId === user.id && (
                        <>
                          <button type="button" className="btn btn-quiet" onClick={() => setEditing(e)}>
                            Rediger
                          </button>
                          <button type="button" className="btn btn-quiet btn-danger-text" onClick={() => remove(e)}>
                            Slett
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {adding && (
        <TimeEntryModal
          onClose={() => setAdding(false)}
          onSaved={() => {
            setAdding(false);
            entries.reload();
          }}
        />
      )}

      {editing && (
        <TimeEntryModal
          entry={editing}
          onClose={() => setEditing(null)}
          onSaved={() => {
            setEditing(null);
            entries.reload();
          }}
        />
      )}
    </>
  );
}
