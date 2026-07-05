'use client';

import { useEffect, useState, type ReactNode } from 'react';
import { api } from '@/lib/api';
import type { DocumentType, Workflow, WorkflowMetrics, WorkflowStage } from '@/lib/types';
import { useApp } from '../AppProvider';
import { EmptyState, Loader, Page } from '../ui';
import { DeleteStageModal, RenameWorkflowModal, StageModal, WorkflowModal } from '../modals';

export default function Workflows() {
  const { t, roleById, deptById, empName, showModal, toast, bumpRefresh, refreshTick } = useApp();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [workflows, setWorkflows] = useState<Workflow[]>([]);
  const [typeName, setTypeName] = useState<Record<number, string>>({});
  const [selected, setSelected] = useState<number | null>(null);
  const [metrics, setMetrics] = useState<WorkflowMetrics | null>(null);

  useEffect(() => {
    let alive = true;
    setLoading(true); setError('');
    Promise.all([api<Workflow[]>('GET', '/api/workflows'), api<DocumentType[]>('GET', '/api/document-types')])
      .then(([ws, types]) => {
        if (!alive) return;
        setWorkflows(ws);
        setTypeName(Object.fromEntries(types.map((ty) => [ty.id, ty.name])));
      })
      .catch((e) => { if (alive) setError((e as Error).message); })
      .finally(() => { if (alive) setLoading(false); });
    return () => { alive = false; };
  }, [refreshTick]);

  useEffect(() => {
    if (workflows.length && (selected == null || !workflows.some((w) => w.id === selected))) setSelected(workflows[0].id);
  }, [workflows, selected]);

  useEffect(() => {
    if (selected == null) { setMetrics(null); return; }
    let alive = true;
    setMetrics(null);
    api<WorkflowMetrics>('GET', `/api/dashboard/workflows/${selected}/metrics`)
      .then((m) => { if (alive) setMetrics(m); })
      .catch(() => { /* ignore */ });
    return () => { alive = false; };
  }, [selected, refreshTick]);

  async function activate(id: number) {
    try { await api('POST', `/api/workflows/${id}/activate`); toast(t('tActivated'), t('tActivatedBody')); bumpRefresh(); }
    catch (e) { toast(t('tCouldNotActivate'), (e as Error).message, 'error'); }
  }

  async function move(wf: Workflow, idx: number, dir: number) {
    const stages = [...wf.stages].sort((a, b) => a.stageOrder - b.stageOrder);
    const j = idx + dir;
    if (j < 0 || j >= stages.length) return;
    [stages[idx], stages[j]] = [stages[j], stages[idx]];
    try { await api('PUT', `/api/workflows/${wf.id}/stages/reorder`, { orderedStageIds: stages.map((s) => s.id) }); bumpRefresh(); }
    catch (e) { toast(t('tCouldNotReorder'), (e as Error).message, 'error'); }
  }

  function approverText(s: WorkflowStage): string {
    if (s.approverType === 'Role') return `${t('kRole')}: ${roleById[s.approverRoleId ?? -1]?.name || '#' + s.approverRoleId}`;
    if (s.approverType === 'Department') return `${t('kDept')}: ${deptById[s.approverDepartmentId ?? -1]?.name || '#' + s.approverDepartmentId}`;
    if (s.approverType === 'User') return `${t('kUser')}: ${empName(s.approverEmployeeId)}`;
    return s.approverType;
  }

  const actions = <button className="btn btn-primary btn-sm" onClick={() => showModal(<WorkflowModal onCreated={setSelected} />)}>＋ {t('newWorkflow')}</button>;
  const wf = workflows.find((w) => w.id === selected) || null;

  return (
    <Page title={t('nav.workflows')} subtitle={t('sub.workflows')} actions={actions}>
      {loading ? <Loader />
        : error ? <EmptyState ico="⚠️" title={t('couldNotLoad')} sub={error} />
          : workflows.length === 0 ? <EmptyState ico="🔀" title={t('noWorkflows')} sub={t('noWorkflowsSub')} />
            : <div className="wf-layout">
              <div>
                <div className="section-title">{t('allWorkflows')}</div>
                {workflows.map((w) => (
                  <div key={w.id} className={`wf-list-item ${w.id === selected ? 'active' : ''}`} onClick={() => setSelected(w.id)}>
                    <div className="doc-icon" style={{ width: 34, height: 34, fontSize: 15 }}>🔀</div>
                    <div className="info"><b>{w.name}</b><small>{typeName[w.documentTypeId] || '#' + w.documentTypeId} · v{w.version}</small></div>
                    {w.isActive ? <span className="pill approved">{t('active')}</span> : null}
                  </div>
                ))}
              </div>
              <div>{wf && <WorkflowDetail wf={wf} />}</div>
            </div>}
    </Page>
  );

  function WorkflowDetail({ wf }: { wf: Workflow }) {
    const stages = [...wf.stages].sort((a, b) => a.stageOrder - b.stageOrder);
    return (
      <div className="card">
        <div className="card-head">
          <div>
            <h3>{wf.name}</h3>
            <div className="muted" style={{ fontSize: '12.5px' }}>{typeName[wf.documentTypeId] || '#' + wf.documentTypeId} · {t('version', { v: wf.version })}</div>
          </div>
          <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
            {wf.isActive ? <span className="pill approved">{t('active')}</span> : <span className="pill cancelled">{t('draft')}</span>}
            <button className="btn btn-outline btn-sm" onClick={() => showModal(<RenameWorkflowModal id={wf.id} currentName={wf.name} />)}>{t('rename')}</button>
            {!wf.isActive && (
              <button className="btn btn-primary btn-sm" disabled={stages.length === 0}
                title={stages.length === 0 ? t('needAStage') : undefined} onClick={() => activate(wf.id)}>{t('activate')}</button>)}
          </div>
        </div>
        <div className="card-pad">
          <div className="stat-grid" style={{ marginBottom: 20 }}>
            {metrics ? <>
              <StatTile k={t('mTotal')} v={metrics.total} />
              <StatTile k={t('mPending')} v={metrics.pending} cls="accent-amber" />
              <StatTile k={t('mApproved')} v={metrics.approved} cls="accent-green" />
              <StatTile k={t('mRejected')} v={metrics.rejected} cls="accent-red" />
              <StatTile k={t('mAvgCycle')} v={metrics.averageCycleTimeHours != null ? <>{metrics.averageCycleTimeHours.toFixed(1)}<small>h</small></> : '—'} />
            </> : <div className="muted">{t('loadingMetrics')}</div>}
          </div>

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
            <div className="section-title" style={{ margin: 0 }}>{t('approvalStages', { n: stages.length })}</div>
            <button className="btn btn-outline btn-sm" onClick={() => showModal(<StageModal wfId={wf.id} />)}>＋ {t('addStageBtn')}</button>
          </div>

          {stages.length === 0
            ? <p className="muted">{t('noStagesYet')}</p>
            : stages.map((s, idx) => (
              <div key={s.id} className="stage-item">
                <div className="reorder-btns">
                  <button onClick={() => move(wf, idx, -1)} disabled={idx === 0}>▲</button>
                  <button onClick={() => move(wf, idx, 1)} disabled={idx === stages.length - 1}>▼</button>
                </div>
                <div className="stage-num">{s.stageOrder}</div>
                <div className="stage-info">
                  <b>{s.name}</b>
                  <div className="sub">{approverText(s)}{s.slaHours ? ` · ${t('slaShort', { n: s.slaHours })}` : ''}</div>
                </div>
                <div className="stage-actions">
                  <button className="btn btn-ghost btn-icon" title={t('edit')} onClick={() => showModal(<StageModal wfId={wf.id} stage={s} />)}>✏️</button>
                  <button className="btn btn-ghost btn-icon" title={t('del')} onClick={() => showModal(<DeleteStageModal wfId={wf.id} stageId={s.id} />)}>🗑️</button>
                </div>
              </div>
            ))}
        </div>
      </div>
    );
  }
}

function StatTile({ k, v, cls = '' }: { k: string; v: ReactNode; cls?: string }) {
  return <div className={`stat ${cls}`}><div className="k">{k}</div><div className="v">{v}</div></div>;
}
