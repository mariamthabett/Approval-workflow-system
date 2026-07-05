'use client';

import type { ViewName } from '@/lib/constants';
import { useApp } from './AppProvider';
import { Avatar } from './ui';

const NAV: { id: ViewName; ico: string }[] = [
  { id: 'inbox', ico: '📥' },
  { id: 'documents', ico: '📄' },
  { id: 'sla', ico: '⏰' },
];

export default function Sidebar() {
  const { t, lang, setLang, session, isAdmin, pending, view, go, logout } = useApp();
  const roleBadge = (session?.roles || []).map((r) => t('role.' + r)).join(' · ') || t('noRole');

  function NavItem({ id, ico, badge = 0 }: { id: ViewName; ico: string; badge?: number }) {
    return (
      <button className={`nav-item ${view.section === id ? 'active' : ''}`} onClick={() => go(id)}>
        <span className="ico">{ico}</span><span>{t('nav.' + id)}</span>
        {badge ? <span className="badge-count">{badge}</span> : null}
      </button>
    );
  }

  return (
    <aside className="sidebar">
      <div className="sidebar-brand"><div className="logo-mark">✓</div><span>FlowApprove</span></div>
      <nav className="nav">
        {NAV.map((it) => <NavItem key={it.id} id={it.id} ico={it.ico} badge={it.id === 'inbox' ? pending.length : 0} />)}
        {isAdmin && <>
          <div className="nav-section">{t('nav.admin')}</div>
          <NavItem id="doctypes" ico="🗂️" />
          <NavItem id="workflows" ico="🔀" />
        </>}
      </nav>
      <div className="sidebar-foot">
        <div className="user-chip">
          {session && <Avatar name={session.name} />}
          <div className="info"><b>{session?.name}</b><small>{roleBadge}</small></div>
        </div>
        <button className="btn btn-outline btn-sm btn-block lang-toggle" onClick={() => setLang(lang === 'en' ? 'ar' : 'en')}>{t('langButton')}</button>
        <button className="btn btn-ghost btn-sm btn-block" style={{ marginTop: 8 }} onClick={logout}>{t('signOut')}</button>
      </div>
    </aside>
  );
}
