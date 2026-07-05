// Draft leave-request ids are tracked client-side (the API has no "list my leave requests" endpoint;
// approval instances only exist after submit). Keyed per employee.

const key = (me: number) => `flowapprove.drafts.${me}`;

export function getDraftIds(me: number): number[] {
  try { return JSON.parse(localStorage.getItem(key(me)) || '[]'); } catch { return []; }
}
export function saveDraftIds(me: number, ids: number[]): void {
  localStorage.setItem(key(me), JSON.stringify([...new Set(ids)]));
}
export function addDraftId(me: number, id: number): void {
  saveDraftIds(me, [...getDraftIds(me), id]);
}
export function removeDraftId(me: number, id: number): void {
  saveDraftIds(me, getDraftIds(me).filter((x) => x !== id));
}
