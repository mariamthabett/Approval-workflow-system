'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { fmtDate, fromNow } from '@/lib/format';
import type { PendingApproval } from '@/lib/types';
import { useApp } from '../AppProvider';
import { EmptyState, Loader, Page } from '../ui';

export default function SlaBreaches() {
  const { t, lang, empName, go, refreshTick } = useApp();
  const [loading, setLoading] = useState(true);
  const [rows, setRows] = useState<PendingApproval[]>([]);
  const [error, setError] = useState('');
  const [localTick, setLocalTick] = useState(0);

  useEffect(() => {
    let alive = true;
    setLoading(true); setError('');
    api<PendingApproval[]>('GET', '/api/dashboard/sla-breaches')
      .then((r) => { if (alive) setRows(r); })
      .catch((e) => { if (alive) setError((e as Error).message); })
      .finally(() => { if (alive) setLoading(false); });
    return () => { alive = false; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [refreshTick, localTick]);

  const actions = <button className="btn btn-outline btn-sm" onClick={() => setLocalTick((x) => x + 1)}>↻ {t('refresh')}</button>;

  return (
    <Page title={t('nav.sla')} subtitle={t('sub.sla')} actions={actions}>
      {loading ? <Loader />
        : error ? <EmptyState ico="⚠️" title={t('couldNotLoad')} sub={error} />
          : rows.length === 0 ? <EmptyState ico="✅" title={t('allWithinSla')} sub={t('allWithinSlaSub')} />
            : rows.map((p) => (
              <div key={p.instanceId} className="row-card overdue" onClick={() => go('detail', { id: p.instanceId, from: 'sla', code: p.documentTypeCode })}>
                <div className="doc-icon" style={{ background: 'var(--red-bg)', color: 'var(--red)' }}>⏰</div>
                <div className="row-main">
                  <div className="row-title">
                    {p.documentTypeCode} #{p.documentId}
                    <span className="tag">{t('stage')} {p.stageOrder} · {p.stageName}</span>
                    <span className="pill overdue">{t('overdue')} {fromNow(p.dueAtUtc, lang)}</span>
                  </div>
                  <div className="row-meta">
                    <span>{t('from')} <b>{empName(p.initiatorEmployeeId)}</b></span>
                    <span>{t('dueWas')} <b>{fmtDate(p.dueAtUtc, lang)}</b></span>
                  </div>
                </div>
                <div className="row-actions"><button className="btn btn-outline btn-sm">{t('view')} <span className="flip-x">→</span></button></div>
              </div>
            ))}
    </Page>
  );
}
