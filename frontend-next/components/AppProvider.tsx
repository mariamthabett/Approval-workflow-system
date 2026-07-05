'use client';

import { createContext, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { I18N, translate, type Lang } from '@/lib/i18n';
import { api, getSession, saveSession, clearSession, setApiBase, LANG_KEY } from '@/lib/api';
import { DEMO_USERS, NAV_SECTIONS, type ViewName } from '@/lib/constants';
import type { PendingApproval, RefDept, RefEmployee, RefRole, Session } from '@/lib/types';

export type ToastType = 'success' | 'error' | 'info';
interface ToastItem { id: number; title: string; msg: string; type: ToastType; }
interface ViewState { name: ViewName; section: ViewName; detailId: number | null; detailFrom: ViewName; detailCode: string | null; }

interface AppCtx {
  lang: Lang;
  t: (key: string, params?: Record<string, string | number>) => string;
  setLang: (l: Lang) => void;

  session: Session | null;
  ready: boolean;
  isAdmin: boolean;
  login: (email: string) => Promise<void>;
  logout: () => void;

  employees: RefEmployee[];
  roles: RefRole[];
  departments: RefDept[];
  empById: Record<number, RefEmployee>;
  roleById: Record<number, RefRole>;
  deptById: Record<number, RefDept>;
  empName: (id: number | null | undefined) => string;

  pending: PendingApproval[];
  actionable: Set<number>;
  refreshPending: () => Promise<void>;

  view: ViewState;
  go: (name: ViewName, opts?: { id?: number; from?: ViewName; code?: string }) => void;

  refreshTick: number;
  bumpRefresh: () => void;

  toast: (title: string, msg?: string, type?: ToastType) => void;
  showModal: (node: ReactNode) => void;
  closeModal: () => void;
}

const Ctx = createContext<AppCtx | null>(null);

export function useApp(): AppCtx {
  const v = useContext(Ctx);
  if (!v) throw new Error('useApp must be used within <AppProvider>');
  return v;
}

const INITIAL_VIEW: ViewState = { name: 'inbox', section: 'inbox', detailId: null, detailFrom: 'inbox', detailCode: null };

export function AppProvider({ children }: { children: ReactNode }) {
  const [lang, setLangState] = useState<Lang>('en');
  const [session, setSession] = useState<Session | null>(null);
  const [ready, setReady] = useState(false);
  const [employees, setEmployees] = useState<RefEmployee[]>([]);
  const [roles, setRoles] = useState<RefRole[]>([]);
  const [departments, setDepartments] = useState<RefDept[]>([]);
  const [pending, setPending] = useState<PendingApproval[]>([]);
  const [view, setView] = useState<ViewState>(INITIAL_VIEW);
  const [refreshTick, setRefreshTick] = useState(0);
  const [toasts, setToasts] = useState<ToastItem[]>([]);
  const [modal, setModal] = useState<ReactNode>(null);
  const toastId = useRef(0);

  const t = useMemo(() => (key: string, params?: Record<string, string | number>) => translate(lang, key, params), [lang]);

  const empById = useMemo(() => Object.fromEntries(employees.map((e) => [e.id, e])) as Record<number, RefEmployee>, [employees]);
  const roleById = useMemo(() => Object.fromEntries(roles.map((r) => [r.id, r])) as Record<number, RefRole>, [roles]);
  const deptById = useMemo(() => Object.fromEntries(departments.map((d) => [d.id, d])) as Record<number, RefDept>, [departments]);
  const empName = (id: number | null | undefined) => (id != null && empById[id]?.fullName) || `#${id}`;

  const actionable = useMemo(() => new Set(pending.map((p) => p.instanceId)), [pending]);
  const isAdmin = session?.roles.includes('WorkflowAdmin') ?? false;

  async function loadReference() {
    try {
      const [emps, rls, dps] = await Promise.all([
        api<RefEmployee[]>('GET', '/api/reference/employees'),
        api<RefRole[]>('GET', '/api/reference/roles'),
        api<RefDept[]>('GET', '/api/reference/departments'),
      ]);
      setEmployees(emps); setRoles(rls); setDepartments(dps);
    } catch { /* non-fatal */ }
  }

  async function refreshPending() {
    try { setPending(await api<PendingApproval[]>('GET', '/api/dashboard/my-pending')); }
    catch { setPending([]); }
  }

  async function login(email: string) {
    const res = await api<{ token: string; employeeId: number; roles: string[] }>('POST', '/api/auth/login', { email });
    const name = DEMO_USERS.find((u) => u.email === email)?.name || email.split('@')[0];
    const s: Session = { token: res.token, me: res.employeeId, roles: res.roles || [], name, email };
    saveSession(s);
    setSession(s);
    await loadReference();
    await refreshPending();
    setView(INITIAL_VIEW);
  }

  function logout() {
    clearSession();
    setSession(null);
    setEmployees([]); setRoles([]); setDepartments([]); setPending([]);
    setView(INITIAL_VIEW);
    setModal(null);
  }

  function setLang(l: Lang) {
    setLangState(l);
    try { localStorage.setItem(LANG_KEY, l); } catch { /* ignore */ }
  }

  function go(name: ViewName, opts: { id?: number; from?: ViewName; code?: string } = {}) {
    setView((prev) => ({
      name,
      section: NAV_SECTIONS.includes(name) ? name : prev.section,
      detailId: opts.id !== undefined ? opts.id : prev.detailId,
      detailFrom: opts.from ?? prev.detailFrom,
      detailCode: opts.code !== undefined ? opts.code : prev.detailCode,
    }));
    if (typeof window !== 'undefined') window.scrollTo(0, 0);
  }

  const bumpRefresh = () => setRefreshTick((x) => x + 1);

  function toast(title: string, msg = '', type: ToastType = 'success') {
    const id = ++toastId.current;
    setToasts((ts) => [...ts, { id, title, msg, type }]);
    setTimeout(() => setToasts((ts) => ts.filter((x) => x.id !== id)), 3900);
  }
  const showModal = (node: ReactNode) => setModal(node);
  const closeModal = () => setModal(null);

  // ---- boot: read persisted lang + session ----
  useEffect(() => {
    try {
      const q = new URLSearchParams(window.location.search).get('api');
      if (q !== null) setApiBase(q || null);
    } catch { /* ignore */ }
    try {
      const stored = localStorage.getItem(LANG_KEY);
      if (stored === 'ar' || stored === 'en') setLangState(stored);
    } catch { /* ignore */ }

    const s = getSession();
    if (s) {
      setSession(s);
      (async () => { await loadReference(); await refreshPending(); setReady(true); })();
    } else {
      setReady(true);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // keep <html lang/dir> in sync
  useEffect(() => {
    document.documentElement.lang = lang;
    document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';
  }, [lang]);

  // API 401 → sign out
  useEffect(() => {
    const h = () => logout();
    window.addEventListener('flowapprove:unauthorized', h);
    return () => window.removeEventListener('flowapprove:unauthorized', h);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const value: AppCtx = {
    lang, t, setLang,
    session, ready, isAdmin, login, logout,
    employees, roles, departments, empById, roleById, deptById, empName,
    pending, actionable, refreshPending,
    view, go, refreshTick, bumpRefresh,
    toast, showModal, closeModal,
  };

  const toastIcon: Record<ToastType, string> = { success: '✅', error: '⚠️', info: 'ℹ️' };

  return (
    <Ctx.Provider value={value}>
      {ready ? children : <div className="loader"><div className="spinner" />{I18N[lang].loading}</div>}

      <div className="toasts">
        {toasts.map((tt) => (
          <div key={tt.id} className={`toast ${tt.type}`}>
            <div className="ti">{toastIcon[tt.type]}</div>
            <div className="tb"><b>{tt.title}</b>{tt.msg ? <span>{tt.msg}</span> : null}</div>
          </div>
        ))}
      </div>

      {modal}
    </Ctx.Provider>
  );
}
