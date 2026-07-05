import type { Lang } from './i18n';

const AVATAR_COLORS = ['#5b5bf0', '#17a673', '#d9860b', '#e0484d', '#2f7de1', '#8a6bf2', '#0ea5a3', '#db5a9d'];

export function avatarColor(seed: string): string {
  let h = 0;
  for (const ch of String(seed)) h = (h * 31 + ch.charCodeAt(0)) >>> 0;
  return AVATAR_COLORS[h % AVATAR_COLORS.length];
}

export function initials(name: string): string {
  const p = String(name || '?').trim().split(/\s+/);
  return ((p[0]?.[0] || '') + (p[1]?.[0] || '')).toUpperCase() || '?';
}

const loc = (lang: Lang): string => (lang === 'ar' ? 'ar' : 'en-US');

export function fmtDate(iso: string | null | undefined, lang: Lang = 'en'): string {
  if (!iso) return '—';
  const d = new Date(iso);
  return d.toLocaleDateString(loc(lang), { month: 'short', day: 'numeric', year: 'numeric' }) +
    ', ' + d.toLocaleTimeString(loc(lang), { hour: 'numeric', minute: '2-digit' });
}

export function fmtDay(s: string | null | undefined, lang: Lang = 'en'): string {
  if (!s) return '—';
  const d = new Date(s + (s.length === 10 ? 'T00:00:00' : ''));
  return d.toLocaleDateString(loc(lang), { weekday: 'short', month: 'short', day: 'numeric', year: 'numeric' });
}

export function fromNow(iso: string | null | undefined, lang: Lang = 'en'): string {
  if (!iso) return '';
  const diff = Date.now() - new Date(iso).getTime();
  const abs = Math.abs(diff);
  let n: number, u: string;
  if (abs < 3.6e6) { n = Math.max(1, Math.round(abs / 6e4)); u = lang === 'ar' ? 'د' : 'm'; }
  else if (abs < 8.64e7) { n = Math.round(abs / 3.6e6); u = lang === 'ar' ? 'س' : 'h'; }
  else { n = Math.round(abs / 8.64e7); u = lang === 'ar' ? 'ي' : 'd'; }
  const num = `${n}${u}`;
  return diff >= 0 ? (lang === 'ar' ? `منذ ${num}` : `${num} ago`) : (lang === 'ar' ? `خلال ${num}` : `in ${num}`);
}

export function daysBetween(a: string, b: string): number {
  const ms = new Date(b + 'T00:00:00').getTime() - new Date(a + 'T00:00:00').getTime();
  return Math.round(ms / 8.64e7) + 1;
}
