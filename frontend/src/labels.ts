import type { ProjectStatus, TaskPriority, TaskStatus, UserRole } from './api/types';

export const roleLabel: Record<UserRole, string> = {
  Administrator: 'Administrator',
  Manager: 'Manager',
  Employee: 'Ansatt',
  Customer: 'Kunde',
};

export const projectStatusLabel: Record<ProjectStatus, string> = {
  Planned: 'Planlagt',
  Active: 'Aktivt',
  OnHold: 'På vent',
  Completed: 'Fullført',
};

export const taskStatusLabel: Record<TaskStatus, string> = {
  NotStarted: 'Ikke startet',
  InProgress: 'Pågår',
  InReview: 'Til gjennomgang',
  Done: 'Ferdig',
};

export const priorityLabel: Record<TaskPriority, string> = {
  Low: 'Lav',
  Medium: 'Middels',
  High: 'Høy',
};

export const taskStatuses: TaskStatus[] = ['NotStarted', 'InProgress', 'InReview', 'Done'];
export const projectStatuses: ProjectStatus[] = ['Planned', 'Active', 'OnHold', 'Completed'];
export const priorities: TaskPriority[] = ['Low', 'Medium', 'High'];

// "2026-10-04" -> "04.10.2026"
export function formatDate(value: string | null | undefined): string {
  if (!value) return '–';
  const [year, month, day] = value.slice(0, 10).split('-');
  return `${day}.${month}.${year}`;
}

export function formatDateTime(value: string): string {
  return new Date(value).toLocaleString('nb-NO', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function formatHours(value: number): string {
  return `${value.toLocaleString('nb-NO', { maximumFractionDigits: 2 })} t`;
}

// Dagens dato som "YYYY-MM-DD" i brukerens egen tidssone
export function todayIso(): string {
  return new Date().toLocaleDateString('sv-SE');
}

export function isOverdue(dueDate: string | null, status: TaskStatus): boolean {
  return dueDate !== null && status !== 'Done' && dueDate.slice(0, 10) < todayIso();
}

export function fullName(user: { firstName: string; lastName: string }): string {
  return `${user.firstName} ${user.lastName}`;
}
