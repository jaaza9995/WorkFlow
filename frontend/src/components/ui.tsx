import type { ReactNode } from 'react';
import type { ProjectStatus, TaskStatus } from '../api/types';
import { projectStatusLabel, taskStatusLabel } from '../labels';

export type Tone = 'neutral' | 'brand' | 'ok' | 'warn' | 'danger';

export function Badge({ tone = 'neutral', children }: { tone?: Tone; children: ReactNode }) {
  return <span className={`badge badge-${tone}`}>{children}</span>;
}

const taskTone: Record<TaskStatus, Tone> = {
  NotStarted: 'neutral',
  InProgress: 'brand',
  InReview: 'warn',
  Done: 'ok',
};

const projectTone: Record<ProjectStatus, Tone> = {
  Planned: 'neutral',
  Active: 'brand',
  OnHold: 'warn',
  Completed: 'ok',
};

export function TaskStatusBadge({ status }: { status: TaskStatus }) {
  return <Badge tone={taskTone[status]}>{taskStatusLabel[status]}</Badge>;
}

export function ProjectStatusBadge({ status }: { status: ProjectStatus }) {
  return <Badge tone={projectTone[status]}>{projectStatusLabel[status]}</Badge>;
}

export function ProgressTrack({ percent, label }: { percent: number; label?: string }) {
  return (
    <div
      className="track"
      role="progressbar"
      aria-valuemin={0}
      aria-valuemax={100}
      aria-valuenow={percent}
      aria-label={label ?? 'Fremdrift'}
    >
      <div className="track-fill" style={{ width: `${percent}%` }} />
    </div>
  );
}

export function Field({
  label,
  hint,
  children,
}: {
  label: string;
  hint?: string;
  children: ReactNode;
}) {
  return (
    <label className="field">
      <span className="field-label">{label}</span>
      {children}
      {hint && <span className="field-hint">{hint}</span>}
    </label>
  );
}

export function ErrorNote({ message }: { message: string | null }) {
  if (!message) return null;
  return (
    <p className="note note-error" role="alert">
      {message}
    </p>
  );
}

export function Loading() {
  return <p className="muted">Laster …</p>;
}

export function EmptyState({ title, children }: { title: string; children?: ReactNode }) {
  return (
    <div className="empty">
      <p className="empty-title">{title}</p>
      {children && <div className="empty-body">{children}</div>}
    </div>
  );
}

export function PageHeader({
  title,
  subtitle,
  actions,
}: {
  title: string;
  subtitle?: ReactNode;
  actions?: ReactNode;
}) {
  return (
    <header className="page-header">
      <div>
        <h1>{title}</h1>
        {subtitle && <p className="muted">{subtitle}</p>}
      </div>
      {actions && <div className="page-actions">{actions}</div>}
    </header>
  );
}
