import type { Session } from './types';

export const SESSION_KEY = 'flowapprove.session';
export const API_BASE_KEY = 'flowapprove.apiBase';
export const LANG_KEY = 'flowapprove.lang';

// Base URL for the API. Precedence: localStorage override (⚙️ / ?api=) > NEXT_PUBLIC_API_BASE > "".
export function currentApiBase(): string {
  let b: string | null = null;
  if (typeof window !== 'undefined') { try { b = localStorage.getItem(API_BASE_KEY); } catch { /* ignore */ } }
  if (b == null) b = process.env.NEXT_PUBLIC_API_BASE ?? '';
  return b.replace(/\/+$/, '');
}
export function setApiBase(v: string | null): void {
  if (typeof window === 'undefined') return;
  const clean = (v ?? '').trim().replace(/\/+$/, '');
  if (clean) localStorage.setItem(API_BASE_KEY, clean);
  else localStorage.removeItem(API_BASE_KEY);
}

export function getSession(): Session | null {
  if (typeof window === 'undefined') return null;
  try { const s = localStorage.getItem(SESSION_KEY); return s ? (JSON.parse(s) as Session) : null; }
  catch { return null; }
}
export function saveSession(s: Session): void { localStorage.setItem(SESSION_KEY, JSON.stringify(s)); }
export function clearSession(): void { localStorage.removeItem(SESSION_KEY); }

export class ApiError extends Error {
  status?: number;
  constructor(message: string, status?: number) { super(message); this.status = status; this.name = 'ApiError'; }
}

export async function api<T = unknown>(method: string, path: string, body?: unknown): Promise<T> {
  const token = getSession()?.token;
  const res = await fetch(currentApiBase() + path, {
    method,
    headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });
  if (res.status === 401) {
    if (typeof window !== 'undefined') window.dispatchEvent(new Event('flowapprove:unauthorized'));
    throw new ApiError('Unauthorized', 401);
  }
  const text = await res.text();
  const data = text ? JSON.parse(text) : null;
  if (!res.ok) {
    const msg = (data && (data.detail || data.title)) || `Request failed (${res.status})`;
    throw new ApiError(msg, res.status);
  }
  return data as T;
}
