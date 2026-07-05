'use client';

import { AppProvider, useApp } from './AppProvider';
import LoginScreen from './LoginScreen';
import Sidebar from './Sidebar';
import Inbox from './screens/Inbox';
import MyDocuments from './screens/MyDocuments';
import SlaBreaches from './screens/SlaBreaches';
import ApprovalDetail from './screens/ApprovalDetail';
import DocTypes from './screens/DocTypes';
import Workflows from './screens/Workflows';

const SCREENS = {
  inbox: Inbox,
  documents: MyDocuments,
  sla: SlaBreaches,
  detail: ApprovalDetail,
  doctypes: DocTypes,
  workflows: Workflows,
} as const;

function Root() {
  const { session, view } = useApp();
  if (!session) return <LoginScreen />;
  const Screen = SCREENS[view.name];
  return (
    <div className="app-shell">
      <Sidebar />
      <main className="main"><Screen /></main>
    </div>
  );
}

export default function App() {
  return (
    <AppProvider>
      <Root />
    </AppProvider>
  );
}
