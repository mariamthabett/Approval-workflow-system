'use client';

import { useState } from 'react';
import { ApiError } from '@/lib/api';
import { DEMO_USERS } from '@/lib/constants';
import { useApp } from './AppProvider';
import { Avatar } from './ui';
import { BackendModal } from './modals';

export default function LoginScreen() {
  const { t, lang, setLang, login, showModal } = useApp();
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  async function doLogin(em: string) {
    setError('');
    if (!em) { setError(t('errEnterEmail')); return; }
    setBusy(true);
    try {
      await login(em);
    } catch (e) {
      const err = e as ApiError;
      setError(err.status === 401 ? t('errNoEmployee') : !err.status ? t('errBackendUnreachable') : err.message);
      setBusy(false);
    }
  }

  return (
    <div className="login-screen">
      <button className="lang-toggle lang-toggle-fixed" onClick={() => setLang(lang === 'en' ? 'ar' : 'en')}>{t('langButton')}</button>
      <div className="login-card">
        <div className="login-brand">
          <div className="logo-mark">✓</div>
          <h1>FlowApprove</h1>
          <p>{t('tagline')}</p>
        </div>

        <label className="field">
          <span>{t('signInAs')}</span>
          <input type="email" value={email} autoComplete="off" placeholder="you@example.com"
            onChange={(e) => setEmail(e.target.value)}
            onKeyDown={(e) => { if (e.key === 'Enter') doLogin(email.trim()); }} />
        </label>
        <button className="btn btn-primary btn-block" disabled={busy} onClick={() => doLogin(email.trim())}>{t('signIn')}</button>
        <div className="login-error">{error}</div>

        <div className="demo-divider"><span>{t('orPickDemo')}</span></div>
        <div className="demo-users">
          {DEMO_USERS.map((u) => (
            <button key={u.email} className="demo-user" onClick={() => doLogin(u.email)}>
              <Avatar name={u.name} />
              <div className="demo-user-info"><b>{u.name}</b><small>{t(u.roleKey)}</small></div>
              <span className="muted flip-x">→</span>
            </button>
          ))}
        </div>

        <button className="link-btn" onClick={() => showModal(<BackendModal />)}>{t('backendBtn')}</button>
      </div>
      <footer className="login-footer">{t('loginFooter')}</footer>
    </div>
  );
}
