import { request } from './client';
import type {
  AuthResponse,
  Customer,
  Dashboard,
  Project,
  ProjectComment,
  ProjectHours,
  ProjectStatus,
  Task,
  TaskPriority,
  TaskStatus,
  TimeEntry,
  User,
  UserRole,
} from './types';

function qs(params: object): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params) as [string, unknown][]) {
    if (value !== undefined && value !== null && value !== '') search.set(key, String(value));
  }
  const text = search.toString();
  return text ? `?${text}` : '';
}

export interface RegisterCompanyInput {
  companyName: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface ProjectInput {
  name: string;
  description: string | null;
  startDate: string;
  endDate: string | null;
  status: ProjectStatus;
  customerId: number;
  responsibleManagerId: number;
}

export interface TaskInput {
  title: string;
  description: string | null;
  priority: TaskPriority;
  dueDate: string | null;
  projectId: number;
  assignedToUserId: number | null;
}

export interface TimeEntryInput {
  date: string;
  hours: number;
  description: string | null;
}

export interface CustomerInput {
  name: string;
  email: string;
  phone: string | null;
  companyId: number;
}

export interface UserInput {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: UserRole;
  companyId: number;
  customerId: number | null;
}

export const api = {
  auth: {
    login: (email: string, password: string) =>
      request<AuthResponse>('POST', '/auth/login', { email, password }),
    registerCompany: (input: RegisterCompanyInput) =>
      request<AuthResponse>('POST', '/auth/register-company', input),
  },

  dashboard: () => request<Dashboard>('GET', '/dashboard'),

  projects: {
    list: (filter: { search?: string; status?: string }) =>
      request<Project[]>('GET', `/projects${qs(filter)}`),
    get: (id: number) => request<Project>('GET', `/projects/${id}`),
    create: (input: ProjectInput) => request<Project>('POST', '/projects', input),
    update: (id: number, input: ProjectInput) => request<void>('PUT', `/projects/${id}`, input),
    remove: (id: number) => request<void>('DELETE', `/projects/${id}`),
    users: (id: number) => request<User[]>('GET', `/projects/${id}/users`),
    addUser: (id: number, userId: number) =>
      request<void>('POST', `/projects/${id}/users/${userId}`),
    removeUser: (id: number, userId: number) =>
      request<void>('DELETE', `/projects/${id}/users/${userId}`),
  },

  tasks: {
    list: (filter: { projectId?: number; status?: string; assignedToUserId?: number }) =>
      request<Task[]>('GET', `/tasks${qs(filter)}`),
    create: (input: TaskInput) => request<Task>('POST', '/tasks', input),
    setStatus: (id: number, status: TaskStatus) =>
      request<void>('PUT', `/tasks/${id}/status`, { status }),
    remove: (id: number) => request<void>('DELETE', `/tasks/${id}`),
  },

  comments: {
    list: (filter: { projectId?: number; taskId?: number }) =>
      request<ProjectComment[]>('GET', `/comments${qs(filter)}`),
    create: (input: { content: string; projectId: number; taskId: number | null }) =>
      request<ProjectComment>('POST', '/comments', input),
    remove: (id: number) => request<void>('DELETE', `/comments/${id}`),
  },

  timeEntries: {
    list: (filter: { projectId?: number; taskId?: number; userId?: number }) =>
      request<TimeEntry[]>('GET', `/timeentries${qs(filter)}`),
    create: (input: TimeEntryInput & { taskId: number }) =>
      request<TimeEntry>('POST', '/timeentries', input),
    update: (id: number, input: TimeEntryInput) =>
      request<void>('PUT', `/timeentries/${id}`, input),
    remove: (id: number) => request<void>('DELETE', `/timeentries/${id}`),
    projectSummary: (projectId: number) =>
      request<ProjectHours>('GET', `/timeentries/project/${projectId}/summary`),
  },

  customers: {
    list: () => request<Customer[]>('GET', '/customers'),
    create: (input: CustomerInput) => request<Customer>('POST', '/customers', input),
  },

  users: {
    list: () => request<User[]>('GET', '/users'),
    create: (input: UserInput) => request<User>('POST', '/users', input),
  },
};
