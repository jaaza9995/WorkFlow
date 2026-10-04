import type { AuthResponse } from './types';

const BASE_URL: string = import.meta.env.VITE_API_URL ?? 'http://localhost:5157/api';
const STORAGE_KEY = 'workflow.auth';

export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

// ---------- Lagring av innlogging ----------

export function saveAuth(auth: AuthResponse): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(auth));
}

export function loadAuth(): AuthResponse | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  try {
    const auth = JSON.parse(raw) as AuthResponse;
    if (new Date(auth.expiresAt).getTime() <= Date.now()) {
      clearAuth();
      return null;
    }
    return auth;
  } catch {
    clearAuth();
    return null;
  }
}

export function clearAuth(): void {
  localStorage.removeItem(STORAGE_KEY);
}

let unauthorizedHandler: (() => void) | null = null;

// Kalles når API-et svarer 401 med et token (utløpt eller ugyldig)
export function setUnauthorizedHandler(handler: (() => void) | null): void {
  unauthorizedHandler = handler;
}

// ---------- Feilmeldinger ----------

async function readError(res: Response): Promise<string> {
  const text = await res.text();

  if (text) {
    try {
      const json: unknown = JSON.parse(text);
      if (typeof json === 'string') return json;
      if (json && typeof json === 'object') {
        const obj = json as Record<string, unknown>;
        if (typeof obj.message === 'string') return obj.message;
        if (obj.errors && typeof obj.errors === 'object') {
          const first = Object.values(obj.errors as Record<string, unknown>)[0];
          if (Array.isArray(first) && typeof first[0] === 'string') return first[0];
        }
        if (typeof obj.title === 'string') return obj.title;
      }
    } catch {
      return text; // vanlig tekst, for eksempel fra BadRequest("...")
    }
  }

  switch (res.status) {
    case 401:
      return 'Du må logge inn.';
    case 403:
      return 'Du har ikke tilgang til dette.';
    case 404:
      return 'Fant ikke det du ba om.';
    default:
      return `Noe gikk galt (${res.status}).`;
  }
}

// ---------- Selve kallet ----------

export async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  const headers: Record<string, string> = {};
  if (body !== undefined) headers['Content-Type'] = 'application/json';

  const token = loadAuth()?.token;
  if (token) headers['Authorization'] = `Bearer ${token}`;

  let res: Response;
  try {
    res = await fetch(`${BASE_URL}${path}`, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  } catch {
    throw new ApiError(0, 'Får ikke kontakt med serveren. Sjekk at API-et kjører.');
  }

  if (res.status === 401 && token) unauthorizedHandler?.();
  if (!res.ok) throw new ApiError(res.status, await readError(res));
  if (res.status === 204) return undefined as T;

  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

export function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'Noe gikk galt.';
}
