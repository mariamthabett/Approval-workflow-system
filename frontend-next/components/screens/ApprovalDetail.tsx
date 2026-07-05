'use client';

import { useEffect, useState, type ReactNode } from 'react';
import { api } from '@/lib/api';
import { fmtDate, fromNow } from '@/lib/format';
import type { ApprovalAction, ApprovalInstance, StageInstance } from '@/lib/types';
import { useApp } from '../AppProvider';
import { EmptyState, Loader, Page, StatusPill } from '../ui';
import { ApproveModal, CancelModal, CommentModal, RejectModal } from '../modals';

const ACTION_ICON: Record<string, string> = {
  Submit: '📤', Approve: '✅', Reject: '❌', Comment: '💬', Resubmit: '🔁', Cancel: '🚫',
};

export default function ApprovalDetail() {
  const { t, lang, session, actionable, isAdmin, empName, go, showModal, toast, refreshPending, bumpRefresh, view, refreshTick } = useApp();
  const id = view.detailId;
  const [loading, setLoading] = useState(true);
  const [inst, setInst] = useState<ApprovalInstance | null>(null);
  const [history, setHistory] = useState<ApprovalAction[]>([]);
  const [error, setError] = useState('');

  useEffect(() => {
    if (id == null) return;
    let alive = true;
    setLoading(true); setError('');
    Promise.all([
      api<ApprovalInstance>('GET', `/api/approvals/${id}`),
      api<ApprovalAction[]>('GET', `/api/approvals/${id}/history`),
    ])
      .then(([i, h]) => { if (alive) { setInst(i); setHistory(h); } })
      .catch((e) => { if (alive) setError((e as Error).message); })
      .finally(() => { if (alive) setLoading(false); });
    return () => { alive = false; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, refreshTick]);

  async function resubmit() {
    if (id == null) return;
    try {
      await api('POST', `/api/approvals/${id}/resubmit`);
      toast(t('tDone'), t('tResubmitted')); await refreshPending(); bumpRefresh();
    } catch (e) { toast(t('tCouldNotResubmit'), (e as Error).message, 'error'); }
  }

  if (loading) return <Page title={t('approvalDetail')}><Loader /></Page>;
  if (error || !inst) return <Page title={t('approvalDetail')}><EmptyState ico="⚠️" title={t('couldNotLoadApproval')} sub={error} /></Page>;

  const isInitiator = inst.initiatorEmployeeId === session?.me;
  const canAct = actionable.has(inst.id);
  const stages = [...inst.stages].sort((a, b) => a.stageOrder - b.stageOrder);
  const docLabel = view.detailCode ? `${view.detailCode} #${inst.documentId}` : `#${inst.documentId}`;

  const actionButtons: ReactNode[] = [];
  if (inst.status === 'Pending' && canAct) {
    actionButtons.push(<button key="ap" className="btn btn-success" onClick={() => showModal(<ApproveModal id={inst.id} />)}>✓ {t('approve')}</button>);
    actionButtons.push(<button key="rj" className="btn btn-danger-outline" onClick={() => showModal(<RejectModal id={inst.id} />)}>✕ {t('reject')}</button>);
  }
  if (inst.status === 'Rejected' && isInitiator)
    actionButtons.push(<button key="rs" className="btn btn-primary" onClick={resubmit}>🔁 {t('resubmit')}</button>);
  if (inst.status === 'Pending' && (isInitiator || isAdmin))
    actionButtons.push(<button key="cn" className="btn btn-danger-outline" onClick={() => showModal(<CancelModal id={inst.id} />)}>{t('cancelApproval')}</button>);
  if (isInitiator || canAct)
    actionButtons.push(<button key="cm" className="btn btn-outline" onClick={() => showModal(<CommentModal id={inst.id} />)}>💬 {t('comment')}</button>);

  return (
    <Page title={docLabel}
      subtitle={t('wfVersionBy', { v: inst.workflowVersion, name: empName(inst.initiatorEmployeeId) })}>
      <button className="back-link" onClick={() => go(view.detailFrom)}>
        <span className="flip-x">←</span> {t('backTo', { x: t('nav.' + view.detailFrom) })}
      </button>

      <div className="detail-grid">
        <div>
          <div className="card">
            <div className="card-head"><h3>{t('approvalProgress')}</h3><StatusPill status={inst.status} /></div>
            <div className="card-pad"><div className="timeline">{stages.map((s) => <TimelineItem key={s.id} s={s} inst={inst} />)}</div></div>
          </div>
          <div className="card">
            <div className="card-head"><h3>{t('activityHistory')}</h3><span className="pill plain">{t('events', { n: history.length })}</span></div>
            <div className="card-pad">
              {history.length ? history.map((a) => <FeedItem key={a.id} a={a} />) : <p className="muted">{t('noActivity')}</p>}
            </div>
          </div>
        </div>
        <div>
          <div className="card card-pad">
            <div className="section-title">{t('summary')}</div>
            <div className="kv"><span className="k">{t('lStatus')}</span><span className="v"><StatusPill status={inst.status} /></span></div>
            <div className="kv"><span className="k">{t('lDocument')}</span><span className="v">{docLabel}</span></div>
            <div className="kv"><span className="k">{t('lInitiator')}</span><span className="v">{empName(inst.initiatorEmployeeId)}</span></div>
            <div className="kv"><span className="k">{t('cycle')}</span><span className="v">{inst.cycleNumber}</span></div>
            <div className="kv"><span className="k">{t('lCurrentStage')}</span><span className="v">{inst.currentStageName || '—'}</span></div>
            <div className="kv"><span className="k">{t('created')}</span><span className="v">{fmtDate(inst.createdAtUtc, lang)}</span></div>
            {inst.completedAtUtc ? <div className="kv"><span className="k">{t('completed')}</span><span className="v">{fmtDate(inst.completedAtUtc, lang)}</span></div> : null}
            {actionButtons.length > 0 && <>
              <div className="section-title" style={{ marginTop: 18 }}>{t('actions')}</div>
              <div className="inline-actions">{actionButtons}</div>
            </>}
          </div>
        </div>
      </div>
    </Page>
  );

  function TimelineItem({ s, inst }: { s: StageInstance; inst: ApprovalInstance }) {
    const isCurrent = inst.status === 'Pending' && s.stageOrder === inst.currentStageOrder;
    const dotClass = s.status === 'Approved' ? 'approved' : s.status === 'Rejected' ? 'rejected' : isCurrent ? 'current' : 'pending';
    const dotIcon = s.status === 'Approved' ? '✓' : s.status === 'Rejected' ? '✕' : isCurrent ? '●' : '';
    let sub: string;
    if (s.status === 'Pending')
      sub = isCurrent ? `${t('awaitingDecision')}${s.dueAtUtc ? ` · ${t('dueWhen', { when: fromNow(s.dueAtUtc, lang) })}` : ''}` : t('notStarted');
    else
      sub = t('actedBy', { status: t('status.' + s.status), name: empName(s.actedByEmployeeId), when: fmtDate(s.actedAtUtc, lang) });
    return (
      <div className="tl-item">
        <div className={`tl-dot ${dotClass}`}>{dotIcon}</div>
        <div className="tl-body">
          <div className="tl-title">{s.name}
            <span className="pill plain">{t('kind.' + s.approverType)}</span>
            {isCurrent ? <span className="tag">{t('current')}</span> : null}</div>
          <div className="tl-sub">{sub}</div>
        </div>
      </div>
    );
  }

  function FeedItem({ a }: { a: ApprovalAction }) {
    return (
      <div className="feed-item">
        <div className="feed-ico">{ACTION_ICON[a.actionType] || '•'}</div>
        <div className="feed-body">
          <div className="t"><b>{empName(a.actedByEmployeeId)}</b> {t('act.' + a.actionType)}
            {a.fromStatus && a.toStatus && a.fromStatus !== a.toStatus
              ? <span className="muted"> · {t('status.' + a.fromStatus)} → {t('status.' + a.toStatus)}</span> : null}</div>
          {a.comment ? <div className="c">{a.comment}</div> : null}
          <div className="when">{fmtDate(a.createdAtUtc, lang)} · {fromNow(a.createdAtUtc, lang)}</div>
        </div>
      </div>
    );
  }
}
