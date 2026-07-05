export interface DemoUser { email: string; name: string; roleKey: string; }

export const DEMO_USERS: DemoUser[] = [
  { email: 'alice@example.com', name: 'Alice Employee', roleKey: 'demo.alice' },
  { email: 'bob@example.com', name: 'Bob Manager', roleKey: 'demo.bob' },
  { email: 'carol@example.com', name: 'Carol DeptHead', roleKey: 'demo.carol' },
  { email: 'dan@example.com', name: 'Dan HR', roleKey: 'demo.dan' },
  { email: 'admin@example.com', name: 'Admin User', roleKey: 'demo.admin' },
];

export type ViewName = 'inbox' | 'documents' | 'sla' | 'detail' | 'doctypes' | 'workflows';
export const NAV_SECTIONS: ViewName[] = ['inbox', 'documents', 'sla', 'doctypes', 'workflows'];
