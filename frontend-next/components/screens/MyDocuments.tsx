'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { getDraftIds, removeDraftId, saveDraftIds } from '@/lib/drafts';
import { daysBetween, fmtDate, fmtDay } from '@/lib/format';
import type { LeaveRequest, MyDocument } from '@/lib/types';
import { useApp } from '../AppProvider';
import { EmptyState, Loader, Page, StatusPill } from '../ui';
import { LeaveModal } from '../modals';

export default function MyDocuments() {
  const { t, lang, session, go, showModal, toast, refreshPending, bumpRefresh, refreshTick } = useApp();
  const [loading, setLoading] = useState(true);
  const [drafts, setDrafts] = useState<LeaveRequest[]>([]);
  const [submitted, setSubmitted] = useState<MyDocument[]>([]);

  useEffect(() => {
    let alive = true;
    (async () => {
      if (!session) return;
      setLoading(true);
      const ids = getDraftIds(session.me);
      const results = await Promise.allSettled(ids.map((id) => api<LeaveRequest>('GET', `/api/leave-requests/${id}`)));
      const valid: number[] = [], ds: LeaveRequest[] = [];
      results.forEach((r, i) => {
        if (r.status === 'fulfilled') { valid.push(ids[i]); if (r.value.status === 'Draft') ds.push(r.value); }
      });
      saveDraftIds(session.me, valid);
      let subs: MyDocument[] = [];
      try { subs = await api<MyDocument[]>('GET', '/api/dashboard/my-documents'); } catch { /* ignore */ }
      if (alive) { setDrafts(ds); setSubmitted(subs); setLoading(false); }
    })();
    return () => { alive = false; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [refreshTick, session?.me]);

  async function submitLeave(id: number) {
    try {
      await api('POST', `/api/leave-requests/${id}/submit`);
      if (session) removeDraftId(session.me, id);
      toast(t('tSubmitted'), t('tSubmittedBody', { id }));
      await refreshPending(); bumpRefresh();
    } catch (e) { toast(t('tCouldNotSubmit'), (e as Error).message, 'error'); }
  }

  const actions = <button className="btn btn-primary btn-sm" onClick={() => showModal(<LeaveModal />)}>＋ {t('newLeave')}</button>;

  return (
    <Page title={t('nav.documents')} subtitle={t('sub.documents')} actions={actions}>
      {loading ? <Loader /> : <>
        {drafts.length > 0 && <>
          <div className="section-title">{t('draftsTitle')}</div>
          {drafts.map((l) => (
            <div key={l.id} className="row-card" onClick={() => showModal(<LeaveModal id={l.id} />)}>
              <div className="doc-icon">📝</div>
              <div className="row-main">
                <div className="row-title">{t('leaveReq')} #{l.id} <StatusPill status="Draft" /></div>
                <div className="row-meta">
                  <span>{fmtDay(l.fromDate, lang)} → {fmtDay(l.toDate, lang)}</span>
                  <span><b>{daysBetween(l.fromDate, l.toDate)}</b> {t('days')}</span>
                  <span>{l.reason}</span>
                </div>
              </div>
              <div className="row-actions" onClick={(e) => e.stopPropagation()}>
                <button className="btn btn-outline btn-sm" onClick={() => showModal(<LeaveModal id={l.id} />)}>{t('edit')}</button>
                <button className="btn btn-primary btn-sm" onClick={() => submitLeave(l.id)}>{t('submitBtn')}</button>
              </div>
            </div>
          ))}
        </>}

        <div className="section-title" style={{ marginTop: drafts.length ? 26 : 4 }}>{t('inApprovalTitle')}</div>
        {submitted.length === 0
          ? <EmptyState ico="📭" title={t('nothingSubmitted')} sub={t('nothingSubmittedSub')} />
          : submitted.map((d) => (
            <div key={d.instanceId} className="row-card" onClick={() => go('detail', { id: d.instanceId, from: 'documents', code: d.documentTypeCode })}>
              <div className="doc-icon">📄</div>
              <div className="row-main">
                <div className="row-title">
                  {d.documentTypeCode} #{d.documentId} <StatusPill status={d.status} />
                  {d.status === 'Pending' && d.currentStageName
                    ? <span className="tag">{t('stage')} {d.currentStageOrder} · {d.currentStageName}</span> : null}
                </div>
                <div className="row-meta">
                  <span>{t('created')} <b>{fmtDate(d.createdAtUtc, lang)}</b></span>
                  {d.completedAtUtc ? <span>{t('completed')} <b>{fmtDate(d.completedAtUtc, lang)}</b></span> : null}
                  {d.cycleNumber > 1 ? <span>{t('cycle')} {d.cycleNumber}</span> : null}
                </div>
              </div>
              <div className="row-actions"><button className="btn btn-outline btn-sm">{t('view')} <span className="flip-x">→</span></button></div>
            </div>
          ))}
      </>}
    </Page>
  );
}
