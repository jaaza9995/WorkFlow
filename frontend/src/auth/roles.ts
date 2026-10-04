import type { UserRole } from '../api/types';

// Administrator og Manager kan administrere prosjekter, oppgaver og brukere
export const canManage = (role: UserRole) => role === 'Administrator' || role === 'Manager';

// Alle utenom kunder jobber i systemet (timer, oppgaver osv.)
export const isStaff = (role: UserRole) => role !== 'Customer';
