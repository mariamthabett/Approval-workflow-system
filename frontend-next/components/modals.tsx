'use client';

import { useEffect, useState } from 'react';
import { api, currentApiBase, setApiBase } from '@/lib/api';
import { addDraftId } from '@/lib/drafts';
import type { DocumentType, LeaveRequest, Workflow, WorkflowStage } from '@/lib/types';
import { useApp } from './AppProvider';
import { Modal } from './ui';

// ---------- approval action modals ----------
export function ApproveModal({ id }: { id: number }) {
  const { t, closeModal, toast, refreshPending, bumpRefresh } = useApp();
  const [comment, setComment] = useState('');
  const [busy, setBusy] = useState(false);
  async function submit() {
    setBusy(true);
    try {
      await api('POST', `/api/approvals/${id}/approve`, { comment: comment.trim() || null });
      closeModal(); toast(t('tDone'), t('tApprovalRecorded')); await refreshPending(); bumpRefresh();
    } catch (e) { toast(t('tWentWrong'), (e as Error).message, 'error'); setBusy(false); }
  }
  return (
    <Modal title={t('approve')} footer={<>
      <button className="btn btn-ghost" onClick={closeModal}>{t('cancel')}</button>
      <button className="btn btn-success" disabled={busy} onClick={submit}>✓ {t('approve')}</button>
    </>}>
      <p className="muted" style={{ marginTop: 0 }}>{t('mApproveBody')}</p>
      <label className="field"><span>{t('commentOptional')}</span>
        <textarea value={comment} onChange={(e) => setComment(e.target.value)} placeholder={t('phLooksGood')} /></label>
    </Modal>
  );
}

export function RejectModal({ id }: { id: number }) {
  const { t, closeModal, toast, refreshPending, bumpRefresh } = useApp();
  const [comment, setComment] = useState('');
  const [busy, setBusy] = useState(false);
  async function submit() {
    const c = comment.trim();
    if (!c) { toast(t('tReasonReq'), t('tReasonReqBody'), 'error'); return; }
    setBusy(true);
    try {
      await api('POST', `/api/approvals/${id}/reject`, { comment: c });
      closeModal(); toast(t('tDone'), t('tDocRejected')); await refreshPending(); bumpRefresh();
    } catch (e) { toast(t('tWentWrong'), (e as Error).message, 'error'); setBusy(false); }
  }
  return (
    <Modal title={t('reject')} footer={<>
      <button className="btn btn-ghost" onClick={closeModal}>{t('cancel')}</button>
      <button className="btn btn-danger" disabled={busy} onClick={submit}>{t('reject')}</button>
    </>}>
      <p className="muted" style={{ marginTop: 0 }}>{t('mRejectBody')}</p>
      <label className="field"><span>{t('reasonForReject')}</span>
        <textarea value={comment} onChange={(e) => setComment(e.target.value)} placeholder={t('phExplain')} /></label>
    </Modal>
  );
}

export function CommentModal({ id }: { id: number }) {
  const { t, closeModal, toast, bumpRefresh } = useApp();
  const [comment, setComment] = useState('');
  const [busy, setBusy] = useState(false);
  async function submit() {
    const c = comment.trim();
    if (!c) { toast(t('tEmptyComment'), t('tEmptyCommentBody'), 'error'); return; }
    setBusy(true);
    try {
      await api('POST', `/api/approvals/${id}/comment`, { comment: c });
      closeModal(); toast(t('tDone'), t('tCommentPosted')); bumpRefresh();
    } catch (e) { toast(t('tWentWrong'), (e as Error).message, 'error'); setBusy(false); }
  }
  return (
    <Modal title={t('addComment')} footer={<>
      <button className="btn btn-ghost" onClick={closeModal}>{t('cancel')}</button>
      <button className="btn btn-primary" disabled={busy} onClick={submit}>{t('postComment')}</button>
    </>}>
      <label className="field"><span>{t('commentStar')}</span>
        <textarea value={comment} onChange={(e) => setComment(e.target.value)} placeholder={t('phWriteNote')} /></label>
    </Modal>
  );
}

export function CancelModal({ id }: { id: number }) {
  const { t, closeModal, toast, refreshPending, bumpRefresh } = useApp();
  const [comment, setComment] = useState('');
  const [busy, setBusy] = useState(false);
  async function submit() {
    setBusy(true);
    try {
      await api('POST', `/api/approvals/${id}/cancel`, { comment: comment.trim() || null });
      closeModal(); toast(t('tDone'), t('tApprovalCancelled')); await refreshPending(); bumpRefresh();
    } catch (e) { toast(t('tWentWrong'), (e as Error).message, 'error'); setBusy(false); }
  }
  return (
    <Modal title={t('cancelApproval')} footer={<>
      <button className="btn btn-ghost" onClick={closeModal}>{t('keepIt')}</button>
      <button className="btn btn-danger" disabled={busy} onClick={submit}>{t('cancelApproval')}</button>
    </>}>
      <p className="muted" style={{ marginTop: 0 }}>{t('mCancelBody')}</p>
      <label className="field"><span>{t('reasonOptional')}</span>
        <textarea value={comment} onChange={(e) => setComment(e.target.value)} placeholder={t('phWhyCancel')} /></label>
    </Modal>
  );
}

// ---------- leave request create / edit ----------
export function LeaveModal({ id }: { id?: number }) {
  const { t, session, closeModal, toast, bumpRefresh } = useApp();
  const today = new Date().toISOString().slice(0, 10);
  const [fromDate, setFromDate] = useState(today);
  const [toDate, setToDate] = useState(today);
  const [reason, setReason] = useState('');
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    if (id === undefined) return;
    (async () => {
      try {
        const l = await api<LeaveRequest>('GET', `/api/leave-requests/${id}`);
        setFromDate(l.fromDate); setToDate(l.toDate); setReason(l.reason);
      } catch (e) { toast(t('couldNotLoad'), (e as Error).message, 'error'); }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  async function submit() {
    const r = reason.trim();
    if (!fromDate || !toDate || !r) { toast(t('tMissingFields'), t('tMissingFieldsBody'), 'error'); return; }
    if (toDate < fromDate) { toast(t('tInvalidDates'), t('tInvalidDatesBody'), 'error'); return; }
    setBusy(true);
    try {
      if (id === undefined) {
        const created = await api<LeaveRequest>('POST', '/api/leave-requests', { fromDate, toDate, reason: r });
        if (session) addDraftId(session.me, created.id);
        closeModal(); toast(t('tDraftCreated'), t('tDraftCreatedBody', { id: created.id }));
      } else {
        await api('PUT', `/api/leave-requests/${id}`, { fromDate, toDate, reason: r });
        closeModal(); toast(t('tSaved'), t('tLeaveUpdated'));
      }
      bumpRefresh();
    } catch (e) { toast(t('tWentWrong'), (e as Error).message, 'error'); setBusy(false); }
  }

  return (
    <Modal title={id === undefined ? t('newLeave') : t('editLeaveTitle', { id })} footer={<>
      <button className="btn btn-ghost" onClick={closeModal}>{t('cancel')}</button>
      <button className="btn btn-primary" disabled={busy} onClick={submit}>{id === undefined ? t('createDraft') : t('saveChanges')}</button>
    </>}>
      <div className="field-row">
        <label className="field"><span>{t('fromDate')}</span><input type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} /></label>
        <label className="field"><span>{t('toDate')}</span><input type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} /></label>
      </div>
      <label className="field"><span>{t('reasonStar')}</span>
        <textarea value={reason} onChange={(e) => setReason(e.target.value)} placeholder={t('phVacation')} /></label>
    </Modal>
  );
}

// ---------- admin: document type ----------
export function DocTypeModal() {
  const { t, closeModal, toast, bumpRefresh } = useApp();
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [busy, setBusy] = useState(false);
  async function submit() {
    if (!code.trim() || !name.trim()) { toast(t('tMissingFields'), t('tMissingCodeName'), 'error'); return; }
    setBusy(true);
    try {
      await api('POST', '/api/document-types', { code: code.trim(), name: name.trim() });
      closeModal(); toast(t('tCreated'), t('tDocTypeAdded', { name: name.trim() })); bumpRefresh();
    } catch (e) { toast(t('tWentWrong'), (e as Error).message, 'error'); setBusy(false); }
  }
  return (
    <Modal title={t('newDocType')} footer={<>
      <button className="btn btn-ghost" onClick={closeModal}>{t('cancel')}</button>
      <button className="btn btn-primary" disabled={busy} onClick={submit}>{t('create')}</button>
    </>}>
      <label className="field"><span>{t('codeStar')} <small className="muted">{t('codeHint')}</small></span>
        <input value={code} onChange={(e) => setCode(e.target.value)} placeholder="PurchaseOrder" /></label>
      <label className="field"><span>{t('displayName')}</span>
        <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Purchase Order" /></label>
    </Modal>
  );
}

// ---------- admin: workflow create / rename ----------
export function WorkflowModal({ onCreated }: { onCreated?: (id: number) => void }) {
  const { t, closeModal, toast, bumpRefresh } = useApp();
  const [types, setTypes] = useState<DocumentType[] | null>(null);
  const [docType, setDocType] = useState<number | ''>('');
  const [name, setName] = useState('');
  const [busy, setBusy] = useState(false);
  useEffect(() => {
    (async () => {
      try { const ts = await api<DocumentType[]>('GET', '/api/document-types'); setTypes(ts); if (ts[0]) setDocType(ts[0].id); }
      catch { setTypes([]); }
    })();
  }, []);
  async function submit() {
    if (!name.trim() || docType === '') { toast(t('tMissingName'), t('tGiveWfName'), 'error'); return; }
    setBusy(true);
    try {
      const wf = await api<Workflow>('POST', '/api/workflows', { documentTypeId: Number(docType), name: name.trim() });
      closeModal(); toast(t('tCreated'), t('tWfCreatedBody', { name: name.trim() }));
      onCreated?.(wf.id); bumpRefresh();
    } catch (e) { toast(t('tWentWrong'), (e as Error).message, 'error'); setBusy(false); }
  }
  const canSubmit = types !== null && types.length > 0;
  return (
    <Modal title={t('newWorkflow')} footer={<>
      <button className="btn btn-ghost" onClick={closeModal}>{t('cancel')}</button>
      <button className="btn btn-primary" disabled={busy || !canSubmit} onClick={submit}>{t('create')}</button>
    </>}>
      {types !== null && types.length === 0
        ? <p className="muted" style={{ marginTop: 0 }}>{t('tCreateDocTypeFirst')}</p>
        : <>
          <label className="field"><span>{t('documentTypeStar')}</span>
            <select value={docType} onChange={(e) => setDocType(Number(e.target.value))}>
              {(types || []).map((ty) => <option key={ty.id} value={ty.id}>{ty.name} ({ty.code})</option>)}
            </select></label>
          <label className="field"><span>{t('workflowNameStar')}</span>
            <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Standard Leave Approval" /></label>
          <p className="muted" style={{ fontSize: '12.5px' }}>{t('wfHint')}</p>
        </>}
    </Modal>
  );
}

export function RenameWorkflowModal({ id, currentName }: { id: number; currentName: string }) {
  const { t, closeModal, toast, bumpRefresh } = useApp();
  const [name, setName] = useState(currentName);
  const [busy, setBusy] = useState(false);
  async function submit() {
    if (!name.trim()) { toast(t('tMissingName'), t('tNameRequired'), 'error'); return; }
    setBusy(true);
    try {
      await api('PUT', `/api/workflows/${id}`, { name: name.trim() });
      closeModal(); toast(t('tRenamed')); bumpRefresh();
    } catch (e) { toast(t('tWentWrong'), (e as Error).message, 'error'); setBusy(false); }
  }
  return (
    <Modal title={t('renameWorkflow')} footer={<>
      <button className="btn btn-ghost" onClick={closeModal}>{t('cancel')}</button>
      <button className="btn btn-primary" disabled={busy} onClick={submit}>{t('save')}</button>
    </>}>
      <label className="field"><span>{t('nameStar')}</span><input value={name} onChange={(e) => setName(e.target.value)} /></label>
    </Modal>
  );
}

// ---------- admin: stage add / edit ----------
export function StageModal({ wfId, stage }: { wfId: number; stage?: WorkflowStage }) {
  const { t, roles, departments, employees, closeModal, toast, bumpRefresh } = useApp();
  const [name, setName] = useState(stage?.name || '');
  const [approverType, setApproverType] = useState(stage?.approverType || 'Role');
  const [roleId, setRoleId] = useState<number | ''>(stage?.approverRoleId ?? (roles[0]?.id ?? ''));
  const [deptId, setDeptId] = useState<number | ''>(stage?.approverDepartmentId ?? (departments[0]?.id ?? ''));
  const [empId, setEmpId] = useState<number | ''>(stage?.approverEmployeeId ?? (employees[0]?.id ?? ''));
  const [sla, setSla] = useState<string>(stage?.slaHours != null ? String(stage.slaHours) : '');
  const [busy, setBusy] = useState(false);

  async function submit() {
    if (!name.trim()) { toast(t('tMissingName'), t('tStageNameReq'), 'error'); return; }
    const body = {
      approverType, name: name.trim(),
      roleId: approverType === 'Role' ? Number(roleId) : null,
      departmentId: approverType === 'Department' ? Number(deptId) : null,
      employeeId: approverType === 'User' ? Number(empId) : null,
      slaHours: sla.trim() ? Number(sla) : null,
    };
    setBusy(true);
    try {
      if (stage) { await api('PUT', `/api/workflows/${wfId}/stages/${stage.id}`, body); toast(t('tStageUpdated')); }
      else { await api('POST', `/api/workflows/${wfId}/stages`, body); toast(t('tStageAdded')); }
      closeModal(); bumpRefresh();
    } catch (e) { toast(t('tWentWrong'), (e as Error).message, 'error'); setBusy(false); }
  }

  return (
    <Modal title={stage ? t('editStageTitle') : t('addStageTitle')} footer={<>
      <button className="btn btn-ghost" onClick={closeModal}>{t('cancel')}</button>
      <button className="btn btn-primary" disabled={busy} onClick={submit}>{stage ? t('save') : t('addStageBtn')}</button>
    </>}>
      <label className="field"><span>{t('stageNameStar')}</span>
        <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Manager Approval" /></label>
      <label className="field"><span>{t('approverTypeStar')}</span>
        <select value={approverType} onChange={(e) => setApproverType(e.target.value)}>
          <option value="Role">{t('optRole')}</option>
          <option value="Department">{t('optDept')}</option>
          <option value="User">{t('optUser')}</option>
        </select></label>
      {approverType === 'Role' && (
        <label className="field"><span>{t('roleStar')}</span>
          <select value={roleId} onChange={(e) => setRoleId(Number(e.target.value))}>
            {roles.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
          </select></label>)}
      {approverType === 'Department' && (
        <label className="field"><span>{t('departmentStar')}</span>
          <select value={deptId} onChange={(e) => setDeptId(Number(e.target.value))}>
            {departments.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
          </select></label>)}
      {approverType === 'User' && (
        <label className="field"><span>{t('employeeStar')}</span>
          <select value={empId} onChange={(e) => setEmpId(Number(e.target.value))}>
            {employees.map((em) => <option key={em.id} value={em.id}>{em.fullName}</option>)}
          </select></label>)}
      <label className="field"><span>{t('slaHours')} <small className="muted">{t('slaHint')}</small></span>
        <input type="number" min={1} value={sla} onChange={(e) => setSla(e.target.value)} placeholder={t('phSla')} /></label>
    </Modal>
  );
}

export function DeleteStageModal({ wfId, stageId }: { wfId: number; stageId: number }) {
  const { t, closeModal, toast, bumpRefresh } = useApp();
  const [busy, setBusy] = useState(false);
  async function submit() {
    setBusy(true);
    try { await api('DELETE', `/api/workflows/${wfId}/stages/${stageId}`); closeModal(); toast(t('tStageDeleted')); bumpRefresh(); }
    catch (e) { toast(t('tWentWrong'), (e as Error).message, 'error'); setBusy(false); }
  }
  return (
    <Modal title={t('deleteStageQ')} footer={<>
      <button className="btn btn-ghost" onClick={closeModal}>{t('cancel')}</button>
      <button className="btn btn-danger" disabled={busy} onClick={submit}>{t('del')}</button>
    </>}>
      <p style={{ margin: 0 }}>{t('deleteStageBody')}</p>
    </Modal>
  );
}

// ---------- backend URL settings ----------
export function BackendModal() {
  const { t, closeModal, toast } = useApp();
  const [url, setUrl] = useState(currentApiBase());
  function submit() {
    const v = url.trim().replace(/\/+$/, '');
    setApiBase(v || null);
    closeModal();
    toast(v ? t('backendSaved') : t('backendCleared'), v);
  }
  return (
    <Modal title={t('backendTitle')} footer={<>
      <button className="btn btn-ghost" onClick={closeModal}>{t('cancel')}</button>
      <button className="btn btn-primary" onClick={submit}>{t('save')}</button>
    </>}>
      <p className="muted" style={{ marginTop: 0 }}>{t('backendHelp')}</p>
      <label className="field"><span>{t('backendTitle')}</span>
        <input type="url" value={url} onChange={(e) => setUrl(e.target.value)} placeholder={t('backendPh')} /></label>
    </Modal>
  );
}
