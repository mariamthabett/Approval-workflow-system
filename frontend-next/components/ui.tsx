'use client';

import { useEffect, type ReactNode } from 'react';
import { avatarColor, initials } from '@/lib/format';
import { useApp } from './AppProvider';

export function Avatar({ name, cls = '' }: { name: string; cls?: string }) {
  return <span className={`avatar ${cls}`} style={{ background: avatarColor(name) }}>{initials(name)}</span>;
}

export function StatusPill({ status }: { status: string }) {
  const { t } = useApp();
  return <span className={`pill ${status.toLowerCase()}`}>{t('status.' + status)}</span>;
}

export function Tag({ children }: { children: ReactNode }) {
  return <span className="tag">{children}</span>;
}

export function EmptyState({ ico, title, sub }: { ico: string; title: string; sub: string }) {
  return (
    <div className="empty">
      <div className="ico">{ico}</div>
      <h3>{title}</h3>
      <p>{sub}</p>
    </div>
  );
}

export function Loader() {
  const { t } = useApp();
  return <div className="loader"><div className="spinner" />{t('loading')}</div>;
}

export function Page({ title, subtitle, actions, children }: {
  title: string; subtitle?: string; actions?: ReactNode; children: ReactNode;
}) {
  return (
    <>
      <header className="topbar">
        <div>
          <h2>{title}</h2>
          {subtitle ? <p className="muted">{subtitle}</p> : null}
        </div>
        <div className="topbar-actions">{actions}</div>
      </header>
      <section className="content">{children}</section>
    </>
  );
}

export function Modal({ title, children, footer }: { title: string; children: ReactNode; footer: ReactNode }) {
  const { closeModal } = useApp();
  useEffect(() => {
    const h = (e: KeyboardEvent) => { if (e.key === 'Escape') closeModal(); };
    window.addEventListener('keydown', h);
    return () => window.removeEventListener('keydown', h);
  }, [closeModal]);

  return (
    <div className="modal-root">
      <div className="modal-backdrop" onClick={closeModal} />
      <div className="modal-card" role="dialog" aria-modal="true">
        <header className="modal-head">
          <h3>{title}</h3>
          <button className="modal-x" onClick={closeModal} aria-label="Close">×</button>
        </header>
        <div className="modal-body">{children}</div>
        <footer className="modal-foot">{footer}</footer>
      </div>
    </div>
  );
}
