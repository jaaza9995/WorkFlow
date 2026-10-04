import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { api } from '../api';
import { errorMessage } from '../api/client';
import type { Task, TaskStatus } from '../api/types';
import { canManage, isStaff } from '../auth/roles';
import { useAuth } from '../auth/useAuth';
import { ProjectFormModal } from '../components/ProjectFormModal';
import { TaskFormModal } from '../components/TaskFormModal';
import { TimeEntryModal } from '../components/TimeEntryModal';
import {
  Badge,
  EmptyState,
  ErrorNote,
  Field,
  Loading,
  PageHeader,
  ProgressTrack,
  ProjectStatusBadge,
  TaskStatusBadge,
} from '../components/ui';
import {
  formatDate,
  formatDateTime,
  formatHours,
  fullName,
  isOverdue,
  priorityLabel,
  roleLabel,
  taskStatuses,
  taskStatusLabel,
} from '../labels';
import { useLoad } from '../useLoad';

export function ProjectDetailPage() {
  const { id } = useParams();
  const projectId = Number(id);
  const { user } = useAuth();
  const navigate = useNavigate();
  const manage = user ? canManage(user.role) : false;
  const staff = user ? isStaff(user.role) : false;

  const project = useLoad(() => api.projects.get(projectId), `project-${projectId}`);
  const tasks = useLoad(() => api.tasks.list({ projectId }), `tasks-${projectId}`);
  const comments = useLoad(() => api.comments.list({ projectId }), `comments-${projectId}`);
  const members = useLoad(
    async () => (staff ? await api.projects.users(projectId) : []),
    `members-${projectId}-${staff}`,
  );
  const hours = useLoad(
    async () => (manage ? await api.timeEntries.projectSummary(projectId) : null),
    `hours-${projectId}-${manage}`,
  );
  const allUsers = useLoad(async () => (manage ? await api.users.list() : []), `all-users-${manage}`);

  const [editing, setEditing] = useState(false);
  const [addingTask, setAddingTask] = useState(false);
  const [timeTask, setTimeTask] = useState<Task | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [commentText, setCommentText] = useState('');
  const [commentTarget, setCommentTarget] = useState('');
  const [memberToAdd, setMemberToAdd] = useState('');

  if (!user) return null;

  if (!project.data) {
    return (
      <>
        <PageHeader title="Prosjekt" />
        {project.error ? (
          <>
            <ErrorNote message={project.error} />
            <Link to="/prosjekter">Tilbake til prosjekter</Link>
          </>
        ) : (
          <Loading />
        )}
      </>
    );
  }

  const p = project.data;
  const memberList = members.data ?? [];
  const taskList = tasks.data ?? [];
  const commentList = comments.data ?? [];

  const candidates = (allUsers.data ?? []).filter(
    (u) =>
      u.role !== 'Customer' &&
      u.companyId === user.companyId &&
      !memberList.some((m) => m.id === u.id),
  );

  async function run(action: () => Promise<unknown>) {
    setActionError(null);
    try {
      await action();
    } catch (e) {
      setActionError(errorMessage(e));
    }
  }

  function refreshTasks() {
    project.reload();
    tasks.reload();
    hours.reload();
  }

  const canChangeStatus = (t: Task) =>
    manage || (user?.role === 'Employee' && t.assignedToUserId === user.id);

  const taskTitle = (taskId: number | null) =>
    taskId === null ? null : (taskList.find((t) => t.id === taskId)?.title ?? null);

  function changeStatus(task: Task, status: TaskStatus) {
    return run(async () => {
      await api.tasks.setStatus(task.id, status);
      refreshTasks();
    });
  }

  function deleteTask(task: Task) {
    if (!window.confirm(`Slette oppgaven «${task.title}»?`)) return;
    return run(async () => {
      await api.tasks.remove(task.id);
      refreshTasks();
    });
  }

  function deleteProject() {
    if (!window.confirm(`Slette prosjektet «${p.name}»? Dette kan ikke angres.`)) return;
    return run(async () => {
      await api.projects.remove(p.id);
      navigate('/prosjekter');
    });
  }

  function addComment(e: FormEvent) {
    e.preventDefault();
    return run(async () => {
      await api.comments.create({
        content: commentText.trim(),
        projectId,
        taskId: commentTarget ? Number(commentTarget) : null,
      });
      setCommentText('');
      comments.reload();
    });
  }

  function deleteComment(commentId: number) {
    return run(async () => {
      await api.comments.remove(commentId);
      comments.reload();
    });
  }

  function addMember(e: FormEvent) {
    e.preventDefault();
    if (!memberToAdd) return;
    return run(async () => {
      await api.projects.addUser(projectId, Number(memberToAdd));
      setMemberToAdd('');
      members.reload();
    });
  }

  function removeMember(userId: number) {
    return run(async () => {
      await api.projects.removeUser(projectId, userId);
      members.reload();
    });
  }

  return (
    <>
      <p className="crumb">
        <Link to="/prosjekter">Prosjekter</Link>
      </p>

      <PageHeader
        title={p.name}
        subtitle={
          <>
            <ProjectStatusBadge status={p.status} /> {p.customerName}
          </>
        }
        actions={
          manage && (
            <>
              <button type="button" className="btn" onClick={() => setEditing(true)}>
                Rediger
              </button>
              <button type="button" className="btn btn-danger" onClick={deleteProject}>
                Slett
              </button>
            </>
          )
        }
      />

      <div className="progress-block">
        <ProgressTrack percent={p.progressPercent} label="Prosjektets fremdrift" />
        <p className="small">
          <strong>{p.progressPercent} %</strong> ferdig, {p.completedTasks} av {p.totalTasks} oppgaver
        </p>
      </div>

      <ErrorNote message={actionError} />

      <div className="detail-grid">
        <div className="detail-main">
          <section>
            <div className="section-head">
              <h2>Oppgaver</h2>
              {manage && (
                <button type="button" className="btn btn-primary" onClick={() => setAddingTask(true)}>
                  Ny oppgave
                </button>
              )}
            </div>

            <ErrorNote message={tasks.error} />
            {taskList.length === 0 && !tasks.loading ? (
              <EmptyState title="Ingen oppgaver ennå">
                {manage
                  ? 'Legg til ansatte under Team, og opprett så den første oppgaven.'
                  : 'Oppgaver vises her når de er opprettet.'}
              </EmptyState>
            ) : (
              <ul className="rows">
                {taskList.map((t) => (
                  <li key={t.id} className="task">
                    <div className="task-main">
                      <p className="task-title">
                        <span className="row-title">{t.title}</span>
                        {t.priority === 'High' && <Badge tone="warn">Høy prioritet</Badge>}
                        {isOverdue(t.dueDate, t.status) && <Badge tone="danger">Forfalt</Badge>}
                      </p>
                      <p className="muted small">
                        {t.assignedToUserName ?? 'Ikke tildelt'}, frist {formatDate(t.dueDate)}, prioritet{' '}
                        {priorityLabel[t.priority].toLowerCase()}
                      </p>
                      {t.description && <p className="small task-desc">{t.description}</p>}
                    </div>

                    <div className="task-side">
                      {canChangeStatus(t) ? (
                        <select
                          className="input input-small"
                          aria-label={`Status for ${t.title}`}
                          value={t.status}
                          onChange={(e) => changeStatus(t, e.target.value as TaskStatus)}
                        >
                          {taskStatuses.map((s) => (
                            <option key={s} value={s}>
                              {taskStatusLabel[s]}
                            </option>
                          ))}
                        </select>
                      ) : (
                        <TaskStatusBadge status={t.status} />
                      )}

                      <div className="task-actions">
                        {staff && (
                          <button type="button" className="btn btn-quiet" onClick={() => setTimeTask(t)}>
                            Før timer
                          </button>
                        )}
                        {manage && (
                          <button type="button" className="btn btn-quiet btn-danger-text" onClick={() => deleteTask(t)}>
                            Slett
                          </button>
                        )}
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <section>
            <h2>Kommentarer</h2>
            <ErrorNote message={comments.error} />

            {commentList.length === 0 && !comments.loading ? (
              <EmptyState title="Ingen kommentarer ennå">Skriv den første under.</EmptyState>
            ) : (
              <ul className="rows">
                {commentList.map((c) => (
                  <li key={c.id} className="comment">
                    <div className="comment-head">
                      <strong>{c.userName}</strong>
                      <span className="muted small">{formatDateTime(c.createdAt)}</span>
                      {taskTitle(c.taskId) && <Badge>Oppgave: {taskTitle(c.taskId)}</Badge>}
                    </div>
                    <p className="comment-body">{c.content}</p>
                    {(manage || c.userId === user.id) && (
                      <button
                        type="button"
                        className="btn btn-quiet btn-danger-text"
                        onClick={() => deleteComment(c.id)}
                      >
                        Slett
                      </button>
                    )}
                  </li>
                ))}
              </ul>
            )}

            <form onSubmit={addComment} className="form comment-form">
              <Field label="Ny kommentar">
                <textarea
                  className="input"
                  rows={3}
                  required
                  maxLength={2000}
                  value={commentText}
                  onChange={(e) => setCommentText(e.target.value)}
                />
              </Field>
              <div className="form-row form-row-end">
                <Field label="Gjelder">
                  <select
                    className="input"
                    value={commentTarget}
                    onChange={(e) => setCommentTarget(e.target.value)}
                  >
                    <option value="">Hele prosjektet</option>
                    {taskList.map((t) => (
                      <option key={t.id} value={t.id}>
                        {t.title}
                      </option>
                    ))}
                  </select>
                </Field>
                <button type="submit" className="btn btn-primary">
                  Legg til kommentar
                </button>
              </div>
            </form>
          </section>
        </div>

        <aside className="detail-side">
          <section>
            <h2>Om prosjektet</h2>
            <dl className="facts">
              <div>
                <dt>Kunde</dt>
                <dd>{p.customerName}</dd>
              </div>
              <div>
                <dt>Ansvarlig</dt>
                <dd>{p.responsibleManagerName}</dd>
              </div>
              <div>
                <dt>Start</dt>
                <dd>{formatDate(p.startDate)}</dd>
              </div>
              <div>
                <dt>Slutt</dt>
                <dd>{formatDate(p.endDate)}</dd>
              </div>
            </dl>
            {p.description && <p className="small">{p.description}</p>}
          </section>

          {staff && (
            <section>
              <h2>Team</h2>
              <ErrorNote message={members.error} />
              {memberList.length === 0 ? (
                <p className="muted small">Ingen ansatte er lagt til ennå.</p>
              ) : (
                <ul className="rows rows-tight">
                  {memberList.map((m) => (
                    <li key={m.id} className="member">
                      <div>
                        <p>{fullName(m)}</p>
                        <p className="muted small">{roleLabel[m.role]}</p>
                      </div>
                      {manage && (
                        <button type="button" className="btn btn-quiet btn-danger-text" onClick={() => removeMember(m.id)}>
                          Fjern
                        </button>
                      )}
                    </li>
                  ))}
                </ul>
              )}

              {manage && (
                <form onSubmit={addMember} className="member-form">
                  <select
                    className="input"
                    aria-label="Velg bruker å legge til"
                    value={memberToAdd}
                    onChange={(e) => setMemberToAdd(e.target.value)}
                  >
                    <option value="">Legg til person</option>
                    {candidates.map((u) => (
                      <option key={u.id} value={u.id}>
                        {fullName(u)}
                      </option>
                    ))}
                  </select>
                  <button type="submit" className="btn" disabled={!memberToAdd}>
                    Legg til
                  </button>
                </form>
              )}
              {manage && allUsers.data && candidates.length === 0 && (
                <p className="muted small">
                  Ingen flere å legge til. <Link to="/brukere">Se brukere</Link>
                </p>
              )}
            </section>
          )}

          {manage && hours.data && (
            <section>
              <h2>Timer</h2>
              <p className="figure-large">{formatHours(hours.data.totalHours)}</p>
              {hours.data.byUser.length === 0 ? (
                <p className="muted small">Ingen timer registrert ennå.</p>
              ) : (
                <ul className="rows rows-tight">
                  {hours.data.byUser.map((u) => (
                    <li key={u.userId} className="member">
                      <span>{u.userName}</span>
                      <span>{formatHours(u.hours)}</span>
                    </li>
                  ))}
                </ul>
              )}
            </section>
          )}
        </aside>
      </div>

      {editing && (
        <ProjectFormModal
          project={p}
          onClose={() => setEditing(false)}
          onSaved={() => {
            setEditing(false);
            project.reload();
          }}
        />
      )}

      {addingTask && (
        <TaskFormModal
          projectId={projectId}
          members={memberList}
          onClose={() => setAddingTask(false)}
          onSaved={() => {
            setAddingTask(false);
            refreshTasks();
          }}
        />
      )}

      {timeTask && (
        <TimeEntryModal
          taskId={timeTask.id}
          taskLabel={timeTask.title}
          onClose={() => setTimeTask(null)}
          onSaved={() => {
            setTimeTask(null);
            hours.reload();
          }}
        />
      )}
    </>
  );
}
