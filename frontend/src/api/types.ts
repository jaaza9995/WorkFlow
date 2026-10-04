export type UserRole = 'Administrator' | 'Manager' | 'Employee' | 'Customer';
export type ProjectStatus = 'Planned' | 'Active' | 'OnHold' | 'Completed';
export type TaskStatus = 'NotStarted' | 'InProgress' | 'InReview' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High';

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
  companyId: number;
  customerId: number | null;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: User;
}

export interface Customer {
  id: number;
  name: string;
  email: string;
  phone: string | null;
  companyId: number;
}

export interface Project {
  id: number;
  name: string;
  description: string | null;
  startDate: string;
  endDate: string | null;
  status: ProjectStatus;
  companyId: number;
  customerId: number;
  customerName: string;
  responsibleManagerId: number;
  responsibleManagerName: string;
  totalTasks: number;
  completedTasks: number;
  progressPercent: number;
}

export interface Task {
  id: number;
  title: string;
  description: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate: string | null;
  createdAt: string;
  projectId: number;
  projectName: string;
  assignedToUserId: number | null;
  assignedToUserName: string | null;
}

export interface ProjectComment {
  id: number;
  content: string;
  createdAt: string;
  userId: number;
  userName: string;
  projectId: number;
  taskId: number | null;
}

export interface TimeEntry {
  id: number;
  date: string;
  hours: number;
  description: string | null;
  userId: number;
  userName: string;
  taskId: number;
  taskTitle: string;
  projectId: number;
}

export interface ProjectHours {
  projectId: number;
  totalHours: number;
  byUser: { userId: number; userName: string; hours: number }[];
}

export interface Dashboard {
  role: UserRole;
  activeProjects: number;
  openTasks: number;
  overdueTasks: number;
  tasksThisWeek: number;
  hoursThisWeek: number | null;
  projectProgress: Project[];
  upcomingTasks: Task[];
}
