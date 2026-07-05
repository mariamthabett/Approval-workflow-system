'use client';

import { useEffect, useState } from 'react';
import { fromNow } from '@/lib/format';
import type { PendingApproval } from '@/lib/types';
import { useApp } from '../AppProvider';
import { EmptyState, Loader, Page } from '../ui';
import { ApproveModal, RejectModal } from '../modals';

export default function Inbox() {
  const { t, lang, pending, refreshPending, empName, go, showModal, refreshTick } = useApp();
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let alive = true;
    setLoading(true);
    refreshPending().finally(() => { if (alive) setLoading(false); });
    return () => { alive = false; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [refreshTick]);

  const actions = <button className="btn btn-outline btn-sm" onClick={() => refreshPending()}>↻ {t('refresh')}</button>;

  return (
    <Page title={t('nav.inbox')} subtitle={t('sub.inbox')} actions={actions}>
      {loading ? <Loader />
        : pending.length === 0 ? <EmptyState ico="🎉" title={t('inboxZero')} sub={t('inboxZeroSub')} />
          : pending.map((p) => <Row key={p.instanceId} p={p} />)}
    </Page>
  );

  function Row({ p }: { p: PendingApproval }) {
    return (
      <div className={`row-card ${p.isOverdue ? 'overdue' : ''}`} onClick={() => go('detail', { id: p.instanceId, from: 'inbox', code: p.documentTypeCode })}>
        <div className="doc-icon">📄</div>
        <div className="row-main">
          <div className="row-title">
            {p.documentTypeCode} #{p.documentId}
            <span className="tag">{t('stage')} {p.stageOrder} · {p.stageName}</span>
            {p.isOverdue ? <span className="pill overdue">{t('overdue')}</span> : null}
          </div>
          <div className="row-meta">
            <span>{t('from')} <b>{empName(p.initiatorEmployeeId)}</b></span>
            <span>{t('waiting')} <b>{fromNow(p.enteredAtUtc, lang)}</b></span>
            {p.dueAtUtc ? <span>{p.isOverdue ? '🔴' : '🕒'} {t('due')} <b>{fromNow(p.dueAtUtc, lang)}</b></span> : null}
            {p.cycleNumber > 1 ? <span>{t('cycle')} {p.cycleNumber}</span> : null}
          </div>
        </div>
        <div className="row-actions" onClick={(e) => e.stopPropagation()}>
          <button className="btn btn-success btn-sm" onClick={() => showModal(<ApproveModal id={p.instanceId} />)}>{t('approve')}</button>
          <button className="btn btn-danger-outline btn-sm" onClick={() => showModal(<RejectModal id={p.instanceId} />)}>{t('reject')}</button>
        </div>
      </div>
    );
  }
}
