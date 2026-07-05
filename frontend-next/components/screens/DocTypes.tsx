'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import type { DocumentType } from '@/lib/types';
import { useApp } from '../AppProvider';
import { EmptyState, Loader, Page } from '../ui';
import { DocTypeModal } from '../modals';

export default function DocTypes() {
  const { t, showModal, refreshTick } = useApp();
  const [loading, setLoading] = useState(true);
  const [types, setTypes] = useState<DocumentType[]>([]);
  const [error, setError] = useState('');

  useEffect(() => {
    let alive = true;
    setLoading(true); setError('');
    api<DocumentType[]>('GET', '/api/document-types')
      .then((r) => { if (alive) setTypes(r); })
      .catch((e) => { if (alive) setError((e as Error).message); })
      .finally(() => { if (alive) setLoading(false); });
    return () => { alive = false; };
  }, [refreshTick]);

  const actions = <button className="btn btn-primary btn-sm" onClick={() => showModal(<DocTypeModal />)}>＋ {t('newDocType')}</button>;

  return (
    <Page title={t('nav.doctypes')} subtitle={t('sub.doctypes')} actions={actions}>
      {loading ? <Loader />
        : error ? <EmptyState ico="⚠️" title={t('couldNotLoad')} sub={error} />
          : types.length === 0 ? <EmptyState ico="🗂️" title={t('noDocTypes')} sub={t('noDocTypesSub')} />
            : <div className="card"><div className="card-pad">
              {types.map((ty) => (
                <div key={ty.id} className="stage-item" style={{ marginBottom: 10 }}>
                  <div className="doc-icon">🗂️</div>
                  <div className="stage-info"><b>{ty.name}</b><div className="sub">{t('codeColon')}: <code>{ty.code}</code></div></div>
                  {ty.isActive ? <span className="pill approved">{t('active')}</span> : <span className="pill cancelled">{t('inactive')}</span>}
                </div>
              ))}
            </div></div>}
    </Page>
  );
}
