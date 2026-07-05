/* ============================================================
   FlowApprove — single-page frontend for the approval engine
   Vanilla JS, no build step. Bilingual (English / العربية) with RTL.
   ============================================================ */

const DEMO_USERS = [
    { email: 'alice@example.com', name: 'Alice Employee',  roleKey: 'demo.alice' },
    { email: 'bob@example.com',   name: 'Bob Manager',     roleKey: 'demo.bob' },
    { email: 'carol@example.com', name: 'Carol DeptHead',  roleKey: 'demo.carol' },
    { email: 'dan@example.com',   name: 'Dan HR',          roleKey: 'demo.dan' },
    { email: 'admin@example.com', name: 'Admin User',      roleKey: 'demo.admin' },
];

// Shared password for every seeded demo account (see DataSeeder.DefaultPassword). Lets the
// one-click demo buttons sign in without typing credentials.
const DEMO_PASSWORD = 'Password123!';

const STORE_KEY = 'flowapprove.session';
const LANG_KEY = 'flowapprove.lang';
const API_BASE_KEY = 'flowapprove.apiBase';

// Base URL for the API. Empty = same origin (the .NET app serves this page).
// Precedence: localStorage override (set via ⚙️ button or ?api=) > window.API_BASE (config.js) > "".
function currentApiBase() {
    let b = null;
    try { b = localStorage.getItem(API_BASE_KEY); } catch { /* ignore */ }
    if (b == null && typeof window !== 'undefined' && typeof window.API_BASE === 'string') b = window.API_BASE;
    return (b || '').replace(/\/+$/, '');
}

/* ============================================================
   i18n dictionary
   ============================================================ */
const I18N = {
    en: {
        tagline: 'Approval Workflow System',
        signInAs: 'Sign in as', signIn: 'Sign In', orPickDemo: 'or pick a demo user',
        loginFooter: 'Generic, config-driven approval engine · JWT auth · Leave-request sample module',
        signOut: 'Sign out',
        errNoEmployee: 'No active employee with that email.', errEnterEmail: 'Enter an email address.',

        emailLabel: 'Email', passwordLabel: 'Password', createAccount: 'Create account',
        errEnterPassword: 'Enter your password.', errInvalidCreds: 'Invalid email or password.',
        registerTitle: 'Create your account', fullNameLabel: 'Full name', departmentLabel: 'Department',
        phFullName: 'e.g. Sara Ahmed', pwHint: 'At least 8 characters', registerBtn: 'Create account',
        changePassword: 'Change password', currentPassword: 'Current password', newPassword: 'New password',
        tRegistered: 'Welcome!', tRegisteredBody: 'Your account has been created.',
        tRegisterFailed: 'Could not create account', tFillAllFields: 'Please fill in all fields.',
        tPwTooShort: 'Password must be at least 8 characters.',
        tPasswordChanged: 'Password changed', tPasswordChangedBody: 'Use your new password next time you sign in.',
        tCouldNotChangePw: 'Could not change password',
        'nav.activity': 'Activity', 'sub.activity': 'A log of who did what and when',
        actScopeMe: 'My activity', actScopeAll: 'All users',
        noActivityYet: 'No activity yet', noActivityYetSub: 'Actions you take will appear here.', actIp: 'IP',
        'activity.Login': 'signed in', 'activity.LoginFailed': 'failed sign-in attempt',
        'activity.Register': 'registered an account', 'activity.PasswordChanged': 'changed password',
        'activity.AdminCreatedUser': 'created a user', 'activity.DocumentSubmitted': 'submitted a document',
        'activity.ApprovalDecision': 'made an approval decision',

        'demo.alice': 'Employee · initiates leave', 'demo.bob': 'Manager · Engineering',
        'demo.carol': 'Department Head', 'demo.dan': 'HR Officer · final approver',
        'demo.admin': 'Workflow Administrator',

        'role.Employee': 'Employee', 'role.Manager': 'Manager', 'role.DeptHead': 'Department Head',
        'role.HR': 'HR Officer', 'role.WorkflowAdmin': 'Workflow Admin', noRole: 'No role',

        'nav.inbox': 'Inbox', 'nav.documents': 'My Documents', 'nav.sla': 'SLA Breaches',
        'nav.admin': 'Administration', 'nav.doctypes': 'Document Types', 'nav.workflows': 'Workflows',

        'sub.inbox': 'Approvals waiting on your decision',
        'sub.documents': 'Leave requests you have created',
        'sub.sla': 'Approvals that have passed their due time',
        'sub.doctypes': 'Kinds of documents the engine can route',
        'sub.workflows': 'Design and version approval templates',

        refresh: 'Refresh', newLeave: 'New Leave Request', newDocType: 'New Document Type',
        newWorkflow: 'New Workflow', approve: 'Approve', reject: 'Reject', cancel: 'Cancel',
        save: 'Save', saveChanges: 'Save changes', create: 'Create', edit: 'Edit', submitBtn: 'Submit',
        view: 'View', del: 'Delete', rename: 'Rename', activate: 'Activate', comment: 'Comment',
        resubmit: 'Resubmit', addStageBtn: 'Add stage',

        inboxZero: 'Inbox zero', inboxZeroSub: 'Nothing is waiting on your approval right now.',
        from: 'From', waiting: 'Waiting', due: 'Due', overdue: 'Overdue', cycle: 'Cycle', stage: 'Stage',

        draftsTitle: 'Drafts — not yet submitted', inApprovalTitle: 'In approval & completed',
        nothingSubmitted: 'Nothing submitted yet',
        nothingSubmittedSub: 'Create a leave request and submit it to start an approval.',
        days: 'day(s)', created: 'Created', completed: 'Completed', leaveReq: 'Leave Request',

        'status.Draft': 'Draft', 'status.Pending': 'Pending', 'status.Approved': 'Approved',
        'status.Rejected': 'Rejected', 'status.Cancelled': 'Cancelled', 'status.Submitted': 'Submitted',

        allWithinSla: 'All within SLA', allWithinSlaSub: 'No approvals are currently overdue.',
        dueWas: 'Due was', couldNotLoad: 'Could not load',

        approvalDetail: 'Approval Detail', approvalProgress: 'Approval progress',
        activityHistory: 'Activity history', events: '{n} event(s)', noActivity: 'No activity yet.',
        summary: 'Summary', lStatus: 'Status', lDocument: 'Document', lInitiator: 'Initiator',
        lCurrentStage: 'Current stage', actions: 'Actions', cancelApproval: 'Cancel approval',
        backTo: 'Back to {x}', wfVersionBy: 'Workflow v{v} · initiated by {name}',
        couldNotLoadApproval: 'Could not load approval',

        awaitingDecision: 'Awaiting decision', dueWhen: 'due {when}', notStarted: 'Not started',
        current: 'current', actedBy: '{status} by {name} · {when}',
        'kind.Role': 'Role', 'kind.Department': 'Department', 'kind.User': 'User',

        'act.Submit': 'submitted the document', 'act.Approve': 'approved', 'act.Reject': 'rejected',
        'act.Comment': 'commented', 'act.Resubmit': 'resubmitted the document', 'act.Cancel': 'cancelled the approval',

        mApproveBody: 'Add an optional note for the record, then confirm your approval.',
        commentOptional: 'Comment (optional)', phLooksGood: 'Looks good…',
        mRejectBody: 'Rejection sends the document back to its initiator. A reason is required.',
        reasonForReject: 'Reason for rejection *', phExplain: 'Please explain…',
        addComment: 'Add comment', commentStar: 'Comment *', phWriteNote: 'Write a note…', postComment: 'Post comment',
        mCancelBody: 'This stops the approval process for the document.',
        reasonOptional: 'Reason (optional)', phWhyCancel: 'Why are you cancelling?', keepIt: 'Keep it',

        fromDate: 'From date *', toDate: 'To date *', reasonStar: 'Reason *', phVacation: 'e.g. Family vacation',
        createDraft: 'Create draft', editLeaveTitle: 'Edit Leave Request #{id}',

        newInvoice: 'New Invoice', invoiceTitle: 'New Invoice',
        amountStar: 'Amount *', currencyStar: 'Currency *', vendorStar: 'Vendor / Payee *',
        invDescription: 'Description', invoiceDateStar: 'Invoice date *', dueDateStar: 'Due date *',
        phVendor: 'e.g. ACME Supplies', phInvDesc: 'What is this invoice for?', createSubmitInvoice: 'Create & submit',
        invoiceDetails: 'Invoice details', lAmount: 'Amount', lVendor: 'Vendor', lDescription: 'Description',
        lInvoiceDate: 'Invoice date', lDueDate: 'Due date',
        printLabel: 'Print', invoicePrintTitle: 'INVOICE', invoiceNoLabel: 'Invoice No.',
        lStatus: 'Status', printedOn: 'Printed on {when}', printBrandTag: 'Approval Workflow System',
        tInvoiceSubmitted: 'Invoice submitted', tInvoiceSubmittedBody: 'Invoice #{id} entered the approval workflow.',
        tCouldNotSubmitInvoice: 'Could not submit invoice',
        tInvMissingFields: 'Amount and vendor are required.',
        tInvInvalidAmount: 'Amount must be greater than zero.',
        tInvInvalidDates: 'Due date cannot be before the invoice date.',
        codeStar: 'Code *', codeHint: '(stable identifier, e.g. PurchaseOrder)', displayName: 'Display name *',
        documentTypeStar: 'Document type *', workflowNameStar: 'Workflow name *',
        wfHint: "You'll add approval stages after creating it. New workflows start as a draft.",
        renameWorkflow: 'Rename Workflow', nameStar: 'Name *',
        addStageTitle: 'Add Stage', editStageTitle: 'Edit Stage', stageNameStar: 'Stage name *',
        approverTypeStar: 'Approver type *', optRole: 'Role — anyone with a role',
        optDept: 'Department — the department manager', optUser: 'User — a specific person',
        roleStar: 'Role *', departmentStar: 'Department *', employeeStar: 'Employee *',
        slaHours: 'SLA hours', slaHint: '(optional — for overdue tracking)', phSla: 'e.g. 48',
        deleteStageQ: 'Delete stage?',
        deleteStageBody: 'This removes the stage from the workflow. Remaining stages are renumbered.',

        allWorkflows: 'All workflows', approvalStages: 'Approval stages ({n})',
        noStagesYet: 'No stages yet. Add the first approval step below.',
        version: 'version {v}', active: 'Active', draft: 'Draft', inactive: 'Inactive',
        loadingMetrics: 'Loading metrics…', needAStage: 'Add at least one stage',
        kRole: 'Role', kDept: 'Department', kUser: 'User', slaShort: 'SLA {n}h',
        noDocTypes: 'No document types', noDocTypesSub: 'Create one to start building workflows.',
        codeColon: 'code', noWorkflows: 'No workflows yet',
        noWorkflowsSub: 'Create a workflow, add stages, then activate it.',
        mTotal: 'Total', mPending: 'Pending', mApproved: 'Approved', mRejected: 'Rejected', mAvgCycle: 'Avg. cycle',
        loading: 'Loading…',

        tDone: 'Done', tWentWrong: 'Something went wrong', tApprovalRecorded: 'Approval recorded.',
        tDocRejected: 'Document rejected.', tCommentPosted: 'Comment posted.', tApprovalCancelled: 'Approval cancelled.',
        tResubmitted: 'Document resubmitted — approval restarted.', tCouldNotResubmit: 'Could not resubmit',
        tReasonReq: 'Reason required', tReasonReqBody: 'Please provide a reason for rejection.',
        tEmptyComment: 'Empty comment', tEmptyCommentBody: 'Please write something first.',
        tDraftCreated: 'Draft created', tDraftCreatedBody: 'Leave request #{id} saved. Submit it to start approval.',
        tSaved: 'Saved', tLeaveUpdated: 'Leave request updated.',
        tSubmitted: 'Submitted', tSubmittedBody: 'Leave request #{id} entered the approval workflow.',
        tCouldNotSubmit: 'Could not submit',
        tMissingFields: 'Missing fields', tMissingFieldsBody: 'From, To and Reason are all required.',
        tInvalidDates: 'Invalid dates', tInvalidDatesBody: 'The end date cannot be before the start date.',
        tCreated: 'Created', tDocTypeAdded: 'Document type “{name}” added.',
        tMissingCodeName: 'Both code and name are required.',
        tCreateDocTypeFirst: 'Create a document type first.',
        tMissingName: 'Missing name', tGiveWfName: 'Give the workflow a name.',
        tWfCreatedBody: 'Workflow “{name}” created as a draft.',
        tRenamed: 'Renamed', tNameRequired: 'Name is required.',
        tActivated: 'Activated', tActivatedBody: 'This is now the active workflow for its document type.',
        tCouldNotActivate: 'Could not activate',
        tStageNameReq: 'Stage name is required.', tStageAdded: 'Stage added', tStageUpdated: 'Stage updated',
        tStageDeleted: 'Stage deleted', tCouldNotReorder: 'Could not reorder',
        tSessionExpired: 'Your session has expired. Please sign in again.',
        reqFailed: 'Request failed ({n})',
        langButton: '🌐 العربية',
        backendBtn: '⚙️ Backend URL',
        backendTitle: 'Backend API URL',
        backendHelp: 'Enter the URL of your hosted .NET backend, e.g. https://your-app.onrender.com. Leave empty to use the same site that serves this page.',
        backendPh: 'https://your-backend.onrender.com',
        backendSaved: 'Backend URL saved',
        backendCleared: 'Using same-origin backend',
        errBackendUnreachable: 'Cannot reach the backend. Check the Backend URL (⚙️ below) and that the server is running.',
    },
    ar: {
        tagline: 'نظام سير عمل الموافقات',
        signInAs: 'تسجيل الدخول باسم', signIn: 'تسجيل الدخول', orPickDemo: 'أو اختر مستخدمًا تجريبيًا',
        loginFooter: 'محرك موافقات عام قابل للتهيئة · مصادقة JWT · وحدة طلب إجازة تجريبية',
        signOut: 'تسجيل الخروج',
        errNoEmployee: 'لا يوجد موظف نشط بهذا البريد الإلكتروني.', errEnterEmail: 'أدخل عنوان بريد إلكتروني.',

        emailLabel: 'البريد الإلكتروني', passwordLabel: 'كلمة المرور', createAccount: 'إنشاء حساب',
        errEnterPassword: 'أدخل كلمة المرور.', errInvalidCreds: 'البريد الإلكتروني أو كلمة المرور غير صحيحة.',
        registerTitle: 'أنشئ حسابك', fullNameLabel: 'الاسم الكامل', departmentLabel: 'القسم',
        phFullName: 'مثال: سارة أحمد', pwHint: '٨ أحرف على الأقل', registerBtn: 'إنشاء الحساب',
        changePassword: 'تغيير كلمة المرور', currentPassword: 'كلمة المرور الحالية', newPassword: 'كلمة المرور الجديدة',
        tRegistered: 'أهلًا بك!', tRegisteredBody: 'تم إنشاء حسابك.',
        tRegisterFailed: 'تعذّر إنشاء الحساب', tFillAllFields: 'يرجى ملء جميع الحقول.',
        tPwTooShort: 'يجب ألا تقل كلمة المرور عن ٨ أحرف.',
        tPasswordChanged: 'تم تغيير كلمة المرور', tPasswordChangedBody: 'استخدم كلمة المرور الجديدة في تسجيل الدخول القادم.',
        tCouldNotChangePw: 'تعذّر تغيير كلمة المرور',
        'nav.activity': 'النشاط', 'sub.activity': 'سجل بمن فعل ماذا ومتى',
        actScopeMe: 'نشاطي', actScopeAll: 'كل المستخدمين',
        noActivityYet: 'لا يوجد نشاط بعد', noActivityYetSub: 'ستظهر هنا الإجراءات التي تقوم بها.', actIp: 'IP',
        'activity.Login': 'سجّل الدخول', 'activity.LoginFailed': 'محاولة دخول فاشلة',
        'activity.Register': 'أنشأ حسابًا', 'activity.PasswordChanged': 'غيّر كلمة المرور',
        'activity.AdminCreatedUser': 'أنشأ مستخدمًا', 'activity.DocumentSubmitted': 'أرسل مستندًا',
        'activity.ApprovalDecision': 'اتخذ قرار موافقة',

        'demo.alice': 'موظفة · تبدأ طلبات الإجازة', 'demo.bob': 'مدير · الهندسة',
        'demo.carol': 'رئيس القسم', 'demo.dan': 'موظف موارد بشرية · الموافق النهائي',
        'demo.admin': 'مسؤول سير العمل',

        'role.Employee': 'موظف', 'role.Manager': 'مدير', 'role.DeptHead': 'رئيس قسم',
        'role.HR': 'موظف موارد بشرية', 'role.WorkflowAdmin': 'مسؤول سير العمل', noRole: 'بدون دور',

        'nav.inbox': 'الوارد', 'nav.documents': 'مستنداتي', 'nav.sla': 'تجاوزات المهلة',
        'nav.admin': 'الإدارة', 'nav.doctypes': 'أنواع المستندات', 'nav.workflows': 'مسارات العمل',

        'sub.inbox': 'موافقات تنتظر قرارك',
        'sub.documents': 'طلبات الإجازة التي أنشأتها',
        'sub.sla': 'موافقات تجاوزت وقتها المحدد',
        'sub.doctypes': 'أنواع المستندات التي يمكن للمحرك توجيهها',
        'sub.workflows': 'صمّم وأصدر قوالب الموافقات',

        refresh: 'تحديث', newLeave: 'طلب إجازة جديد', newDocType: 'نوع مستند جديد',
        newWorkflow: 'مسار عمل جديد', approve: 'موافقة', reject: 'رفض', cancel: 'إلغاء',
        save: 'حفظ', saveChanges: 'حفظ التغييرات', create: 'إنشاء', edit: 'تعديل', submitBtn: 'إرسال',
        view: 'عرض', del: 'حذف', rename: 'إعادة تسمية', activate: 'تفعيل', comment: 'تعليق',
        resubmit: 'إعادة الإرسال', addStageBtn: 'إضافة مرحلة',

        inboxZero: 'لا يوجد وارد', inboxZeroSub: 'لا شيء ينتظر موافقتك الآن.',
        from: 'من', waiting: 'بالانتظار', due: 'الاستحقاق', overdue: 'متأخر', cycle: 'دورة', stage: 'مرحلة',

        draftsTitle: 'المسودات — لم تُرسل بعد', inApprovalTitle: 'قيد الموافقة والمكتملة',
        nothingSubmitted: 'لم يتم إرسال أي شيء بعد',
        nothingSubmittedSub: 'أنشئ طلب إجازة وأرسله لبدء الموافقة.',
        days: 'يوم', created: 'أُنشئ', completed: 'اكتمل', leaveReq: 'طلب إجازة',

        'status.Draft': 'مسودة', 'status.Pending': 'قيد الانتظار', 'status.Approved': 'تمت الموافقة',
        'status.Rejected': 'مرفوض', 'status.Cancelled': 'ملغى', 'status.Submitted': 'مُرسل',

        allWithinSla: 'الكل ضمن المهلة', allWithinSlaSub: 'لا توجد موافقات متأخرة حاليًا.',
        dueWas: 'كان مستحقًا', couldNotLoad: 'تعذّر التحميل',

        approvalDetail: 'تفاصيل الموافقة', approvalProgress: 'تقدّم الموافقة',
        activityHistory: 'سجل النشاط', events: '{n} حدث', noActivity: 'لا يوجد نشاط بعد.',
        summary: 'الملخص', lStatus: 'الحالة', lDocument: 'المستند', lInitiator: 'المُنشئ',
        lCurrentStage: 'المرحلة الحالية', actions: 'الإجراءات', cancelApproval: 'إلغاء الموافقة',
        backTo: 'الرجوع إلى {x}', wfVersionBy: 'مسار عمل نسخة {v} · بدأه {name}',
        couldNotLoadApproval: 'تعذّر تحميل الموافقة',

        awaitingDecision: 'بانتظار القرار', dueWhen: 'الاستحقاق {when}', notStarted: 'لم تبدأ',
        current: 'الحالية', actedBy: '{status} بواسطة {name} · {when}',
        'kind.Role': 'دور', 'kind.Department': 'قسم', 'kind.User': 'مستخدم',

        'act.Submit': 'أرسل المستند', 'act.Approve': 'وافق', 'act.Reject': 'رفض',
        'act.Comment': 'علّق', 'act.Resubmit': 'أعاد إرسال المستند', 'act.Cancel': 'ألغى الموافقة',

        mApproveBody: 'أضف ملاحظة اختيارية للسجل، ثم أكّد موافقتك.',
        commentOptional: 'تعليق (اختياري)', phLooksGood: 'يبدو جيدًا…',
        mRejectBody: 'الرفض يعيد المستند إلى مُنشئه. السبب مطلوب.',
        reasonForReject: 'سبب الرفض *', phExplain: 'يرجى التوضيح…',
        addComment: 'إضافة تعليق', commentStar: 'التعليق *', phWriteNote: 'اكتب ملاحظة…', postComment: 'نشر التعليق',
        mCancelBody: 'هذا يوقف عملية الموافقة للمستند.',
        reasonOptional: 'السبب (اختياري)', phWhyCancel: 'لماذا تُلغي؟', keepIt: 'احتفظ به',

        fromDate: 'تاريخ البداية *', toDate: 'تاريخ النهاية *', reasonStar: 'السبب *', phVacation: 'مثال: إجازة عائلية',
        createDraft: 'إنشاء مسودة', editLeaveTitle: 'تعديل طلب الإجازة رقم {id}',

        newInvoice: 'فاتورة جديدة', invoiceTitle: 'فاتورة جديدة',
        amountStar: 'المبلغ *', currencyStar: 'العملة *', vendorStar: 'الجهة / المستفيد *',
        invDescription: 'الوصف', invoiceDateStar: 'تاريخ الفاتورة *', dueDateStar: 'تاريخ الاستحقاق *',
        phVendor: 'مثال: شركة التوريدات', phInvDesc: 'الفاتورة عن ماذا؟', createSubmitInvoice: 'إنشاء وإرسال',
        invoiceDetails: 'تفاصيل الفاتورة', lAmount: 'المبلغ', lVendor: 'الجهة', lDescription: 'الوصف',
        lInvoiceDate: 'تاريخ الفاتورة', lDueDate: 'تاريخ الاستحقاق',
        printLabel: 'طباعة', invoicePrintTitle: 'فاتورة', invoiceNoLabel: 'رقم الفاتورة',
        lStatus: 'الحالة', printedOn: 'طُبعت في {when}', printBrandTag: 'نظام مسار الموافقات',
        tInvoiceSubmitted: 'تم إرسال الفاتورة', tInvoiceSubmittedBody: 'دخلت الفاتورة رقم {id} مسار الموافقة.',
        tCouldNotSubmitInvoice: 'تعذّر إرسال الفاتورة',
        tInvMissingFields: 'المبلغ والجهة مطلوبان.',
        tInvInvalidAmount: 'يجب أن يكون المبلغ أكبر من صفر.',
        tInvInvalidDates: 'لا يمكن أن يكون تاريخ الاستحقاق قبل تاريخ الفاتورة.',
        codeStar: 'الرمز *', codeHint: '(معرّف ثابت، مثل PurchaseOrder)', displayName: 'الاسم المعروض *',
        documentTypeStar: 'نوع المستند *', workflowNameStar: 'اسم مسار العمل *',
        wfHint: 'ستضيف مراحل الموافقة بعد إنشائه. تبدأ مسارات العمل الجديدة كمسودة.',
        renameWorkflow: 'إعادة تسمية مسار العمل', nameStar: 'الاسم *',
        addStageTitle: 'إضافة مرحلة', editStageTitle: 'تعديل مرحلة', stageNameStar: 'اسم المرحلة *',
        approverTypeStar: 'نوع الموافق *', optRole: 'دور — أي شخص لديه هذا الدور',
        optDept: 'قسم — مدير القسم', optUser: 'مستخدم — شخص محدد',
        roleStar: 'الدور *', departmentStar: 'القسم *', employeeStar: 'الموظف *',
        slaHours: 'ساعات المهلة', slaHint: '(اختياري — لتتبّع التأخير)', phSla: 'مثال: 48',
        deleteStageQ: 'حذف المرحلة؟',
        deleteStageBody: 'هذا يزيل المرحلة من مسار العمل. يُعاد ترقيم المراحل المتبقية.',

        allWorkflows: 'كل مسارات العمل', approvalStages: 'مراحل الموافقة ({n})',
        noStagesYet: 'لا توجد مراحل بعد. أضف أول خطوة موافقة أدناه.',
        version: 'الإصدار {v}', active: 'نشط', draft: 'مسودة', inactive: 'غير نشط',
        loadingMetrics: 'جارٍ تحميل المؤشرات…', needAStage: 'أضف مرحلة واحدة على الأقل',
        kRole: 'دور', kDept: 'قسم', kUser: 'مستخدم', slaShort: 'مهلة {n}س',
        noDocTypes: 'لا توجد أنواع مستندات', noDocTypesSub: 'أنشئ واحدًا لبدء بناء مسارات العمل.',
        codeColon: 'الرمز', noWorkflows: 'لا توجد مسارات عمل بعد',
        noWorkflowsSub: 'أنشئ مسار عمل، أضف مراحل، ثم فعّله.',
        mTotal: 'الإجمالي', mPending: 'قيد الانتظار', mApproved: 'موافق عليها', mRejected: 'مرفوضة', mAvgCycle: 'متوسط الدورة',
        loading: 'جارٍ التحميل…',

        tDone: 'تم', tWentWrong: 'حدث خطأ ما', tApprovalRecorded: 'تم تسجيل الموافقة.',
        tDocRejected: 'تم رفض المستند.', tCommentPosted: 'تم نشر التعليق.', tApprovalCancelled: 'تم إلغاء الموافقة.',
        tResubmitted: 'أُعيد إرسال المستند — استؤنفت الموافقة.', tCouldNotResubmit: 'تعذّر إعادة الإرسال',
        tReasonReq: 'السبب مطلوب', tReasonReqBody: 'يرجى تقديم سبب للرفض.',
        tEmptyComment: 'تعليق فارغ', tEmptyCommentBody: 'يرجى كتابة شيء أولًا.',
        tDraftCreated: 'تم إنشاء المسودة', tDraftCreatedBody: 'تم حفظ طلب الإجازة رقم {id}. أرسله لبدء الموافقة.',
        tSaved: 'تم الحفظ', tLeaveUpdated: 'تم تحديث طلب الإجازة.',
        tSubmitted: 'تم الإرسال', tSubmittedBody: 'دخل طلب الإجازة رقم {id} مسار الموافقة.',
        tCouldNotSubmit: 'تعذّر الإرسال',
        tMissingFields: 'حقول ناقصة', tMissingFieldsBody: 'تاريخ البداية والنهاية والسبب كلها مطلوبة.',
        tInvalidDates: 'تواريخ غير صالحة', tInvalidDatesBody: 'لا يمكن أن يكون تاريخ النهاية قبل تاريخ البداية.',
        tCreated: 'تم الإنشاء', tDocTypeAdded: 'تمت إضافة نوع المستند «{name}».',
        tMissingCodeName: 'الرمز والاسم كلاهما مطلوب.',
        tCreateDocTypeFirst: 'أنشئ نوع مستند أولًا.',
        tMissingName: 'الاسم ناقص', tGiveWfName: 'أعطِ مسار العمل اسمًا.',
        tWfCreatedBody: 'تم إنشاء مسار العمل «{name}» كمسودة.',
        tRenamed: 'تمت إعادة التسمية', tNameRequired: 'الاسم مطلوب.',
        tActivated: 'تم التفعيل', tActivatedBody: 'أصبح هذا هو مسار العمل النشط لنوع مستنده.',
        tCouldNotActivate: 'تعذّر التفعيل',
        tStageNameReq: 'اسم المرحلة مطلوب.', tStageAdded: 'تمت إضافة المرحلة', tStageUpdated: 'تم تحديث المرحلة',
        tStageDeleted: 'تم حذف المرحلة', tCouldNotReorder: 'تعذّرت إعادة الترتيب',
        tSessionExpired: 'انتهت جلستك. يرجى تسجيل الدخول مجددًا.',
        reqFailed: 'فشل الطلب ({n})',
        langButton: '🌐 English',
        backendBtn: '⚙️ رابط الخادم',
        backendTitle: 'رابط خادم الـ API',
        backendHelp: 'أدخل رابط خادم .NET المستضاف، مثل https://your-app.onrender.com. اتركه فارغًا لاستخدام نفس الموقع الذي يقدّم هذه الصفحة.',
        backendPh: 'https://your-backend.onrender.com',
        backendSaved: 'تم حفظ رابط الخادم',
        backendCleared: 'سيتم استخدام نفس المصدر',
        errBackendUnreachable: 'تعذّر الوصول إلى الخادم. تحقق من رابط الخادم (⚙️ بالأسفل) وأن الخادم يعمل.',
    },
};

// ---- global app state ----
const S = {
    token: null, me: null, roles: [], name: '', email: '',
    lang: 'en',
    empById: {}, roleById: {}, deptById: {},
    pending: [], actionable: new Set(),
    section: 'inbox', view: 'inbox', detailId: null, detailFrom: 'inbox',
    wfSelected: null, activityScope: 'me',
};

function t(key, p) {
    let s = (I18N[S.lang] && I18N[S.lang][key] != null) ? I18N[S.lang][key] : (I18N.en[key] != null ? I18N.en[key] : key);
    if (p) for (const k in p) s = s.split('{' + k + '}').join(p[k]);
    return s;
}
function tStatus(s) { return t('status.' + s); }
const loc = () => (S.lang === 'ar' ? 'ar' : 'en-US');

/* ============================================================
   API helper
   ============================================================ */
async function api(method, path, body) {
    const res = await fetch(currentApiBase() + path, {
        method,
        headers: {
            'Content-Type': 'application/json',
            ...(S.token ? { Authorization: `Bearer ${S.token}` } : {}),
        },
        body: body !== undefined ? JSON.stringify(body) : undefined,
    });
    if (res.status === 401) { logout(); throw new Error(t('tSessionExpired')); }
    const text = await res.text();
    const data = text ? JSON.parse(text) : null;
    if (!res.ok) {
        const err = new Error(data?.detail || data?.title || t('reqFailed', { n: res.status }));
        err.status = res.status;
        throw err;
    }
    return data;
}

/* ============================================================
   small utilities
   ============================================================ */
const $ = (sel) => document.querySelector(sel);
const el = (id) => document.getElementById(id);

function escapeHtml(s) {
    return String(s ?? '').replace(/[&<>"']/g, (c) =>
        ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

const AVATAR_COLORS = ['#5b5bf0', '#17a673', '#d9860b', '#e0484d', '#2f7de1', '#8a6bf2', '#0ea5a3', '#db5a9d'];
function avatarColor(seed) {
    let h = 0; for (const ch of String(seed)) h = (h * 31 + ch.charCodeAt(0)) >>> 0;
    return AVATAR_COLORS[h % AVATAR_COLORS.length];
}
function initials(name) {
    const p = String(name || '?').trim().split(/\s+/);
    return ((p[0]?.[0] || '') + (p[1]?.[0] || '')).toUpperCase() || '?';
}
function avatar(name, cls = '') {
    return `<span class="avatar ${cls}" style="background:${avatarColor(name)}">${escapeHtml(initials(name))}</span>`;
}

function empName(id) { return S.empById[id]?.fullName || `#${id}`; }

function fmtDate(iso) {
    if (!iso) return '—';
    const d = new Date(iso);
    return d.toLocaleDateString(loc(), { month: 'short', day: 'numeric', year: 'numeric' }) +
        ', ' + d.toLocaleTimeString(loc(), { hour: 'numeric', minute: '2-digit' });
}
function fmtDay(s) {
    if (!s) return '—';
    const d = new Date(s + (s.length === 10 ? 'T00:00:00' : ''));
    return d.toLocaleDateString(loc(), { weekday: 'short', month: 'short', day: 'numeric', year: 'numeric' });
}
function fromNow(iso) {
    if (!iso) return '';
    const diff = Date.now() - new Date(iso).getTime(), abs = Math.abs(diff);
    let n, u;
    if (abs < 3.6e6) { n = Math.max(1, Math.round(abs / 6e4)); u = S.lang === 'ar' ? 'د' : 'm'; }
    else if (abs < 8.64e7) { n = Math.round(abs / 3.6e6); u = S.lang === 'ar' ? 'س' : 'h'; }
    else { n = Math.round(abs / 8.64e7); u = S.lang === 'ar' ? 'ي' : 'd'; }
    const num = `${n}${u}`;
    return diff >= 0 ? (S.lang === 'ar' ? `منذ ${num}` : `${num} ago`) : (S.lang === 'ar' ? `خلال ${num}` : `in ${num}`);
}
function daysBetween(a, b) {
    const ms = new Date(b + 'T00:00:00') - new Date(a + 'T00:00:00');
    return Math.round(ms / 8.64e7) + 1;
}
function statusPill(status) {
    return `<span class="pill ${String(status || '').toLowerCase()}">${escapeHtml(tStatus(status))}</span>`;
}

/* ============================================================
   toasts + modal
   ============================================================ */
function toast(title, msg = '', type = 'success') {
    const icons = { success: '✅', error: '⚠️', info: 'ℹ️' };
    const node = document.createElement('div');
    node.className = `toast ${type}`;
    node.innerHTML = `<div class="ti">${icons[type] || 'ℹ️'}</div>
        <div class="tb"><b>${escapeHtml(title)}</b>${msg ? `<span>${escapeHtml(msg)}</span>` : ''}</div>`;
    el('toasts').appendChild(node);
    setTimeout(() => { node.style.opacity = '0'; node.style.transition = 'all .25s'; }, 3600);
    setTimeout(() => node.remove(), 3900);
}

let modalOnSubmit = null;
function openModal(title, bodyHtml, footHtml, onSubmit) {
    el('modal-title').textContent = title;
    el('modal-body').innerHTML = bodyHtml;
    el('modal-foot').innerHTML = footHtml;
    modalOnSubmit = onSubmit || null;
    el('modal-root').classList.remove('hidden');
    const first = el('modal-body').querySelector('input, select, textarea');
    if (first) setTimeout(() => first.focus(), 50);
}
function closeModal() { el('modal-root').classList.add('hidden'); modalOnSubmit = null; }
el('modal-root').addEventListener('click', (e) => { if (e.target.dataset.close !== undefined) closeModal(); });
document.addEventListener('keydown', (e) => { if (e.key === 'Escape') closeModal(); });

async function submitModal(btn) {
    if (!modalOnSubmit) return;
    btn.disabled = true;
    try { await modalOnSubmit(); }
    catch (err) { if (err.message !== 'validation') toast(t('tWentWrong'), err.message, 'error'); btn.disabled = false; }
}

/* ============================================================
   language
   ============================================================ */
function applyLang() {
    document.documentElement.lang = S.lang;
    document.documentElement.dir = S.lang === 'ar' ? 'rtl' : 'ltr';
    // static [data-i18n] text + placeholders
    document.querySelectorAll('[data-i18n]').forEach((n) => { n.textContent = t(n.dataset.i18n); });
    el('lang-login').textContent = t('langButton');
    el('lang-app').textContent = t('langButton');
}
function toggleLang() { setLang(S.lang === 'en' ? 'ar' : 'en'); }
function setLang(lang) {
    S.lang = lang;
    localStorage.setItem(LANG_KEY, lang);
    applyLang();
    renderDemoUsers();
    if (!el('app-view').classList.contains('hidden')) {
        renderUserChip();
        renderNav();
        (VIEWS[S.view] || renderInbox)();
    }
}

/* ============================================================
   auth
   ============================================================ */
async function doLogin(email, password) {
    el('login-error').textContent = '';
    try {
        const res = await api('POST', '/api/auth/login', { email, password });
        applySession(res, email);
        await enterApp();
    } catch (err) {
        el('login-error').textContent = err.status === 401 ? t('errInvalidCreds')
            : !err.status ? t('errBackendUnreachable')
            : err.message;
    }
}

// Persists the authenticated session returned by /login or /register.
function applySession(res, email) {
    S.token = res.token; S.me = res.employeeId; S.roles = res.roles || [];
    S.email = email;
    S.name = res.fullName || DEMO_USERS.find((u) => u.email === email)?.name || email.split('@')[0];
    localStorage.setItem(STORE_KEY, JSON.stringify(
        { token: S.token, me: S.me, roles: S.roles, name: S.name, email: S.email }));
}

// Self-service sign-up. Loads departments on demand to populate the picker.
async function openRegister() {
    let depts = [];
    try { depts = await api('GET', '/api/auth/departments'); }
    catch (err) { toast(t('tRegisterFailed'), err.status ? err.message : t('errBackendUnreachable'), 'error'); return; }

    const deptOptions = depts.map((d) => `<option value="${d.id}">${escapeHtml(d.name)}</option>`).join('');
    openModal(t('registerTitle'), `
        <label class="field"><span>${t('fullNameLabel')} *</span>
            <input id="r-name" type="text" placeholder="${t('phFullName')}" /></label>
        <label class="field"><span>${t('emailLabel')} *</span>
            <input id="r-email" type="email" placeholder="you@example.com" /></label>
        <label class="field"><span>${t('passwordLabel')} *</span>
            <input id="r-password" type="password" placeholder="••••••••" />
            <small class="muted">${t('pwHint')}</small></label>
        <label class="field"><span>${t('departmentLabel')} *</span>
            <select id="r-dept">${deptOptions}</select></label>`,
        `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
         <button class="btn btn-primary" onclick="submitModal(this)">${t('registerBtn')}</button>`,
        async () => {
            const fullName = el('r-name').value.trim();
            const email = el('r-email').value.trim();
            const password = el('r-password').value;
            const departmentId = Number(el('r-dept').value);
            if (!fullName || !email || !password || !departmentId) {
                toast(t('tRegisterFailed'), t('tFillAllFields'), 'error'); throw new Error('validation');
            }
            if (password.length < 8) { toast(t('tRegisterFailed'), t('tPwTooShort'), 'error'); throw new Error('validation'); }
            const res = await api('POST', '/api/auth/register', { fullName, email, password, departmentId });
            applySession(res, email);
            closeModal();
            toast(t('tRegistered'), t('tRegisteredBody'), 'success');
            await enterApp();
        });
}

// Change the signed-in user's own password.
function openChangePassword() {
    openModal(t('changePassword'), `
        <label class="field"><span>${t('currentPassword')} *</span>
            <input id="cp-current" type="password" placeholder="••••••••" autocomplete="current-password" /></label>
        <label class="field"><span>${t('newPassword')} *</span>
            <input id="cp-new" type="password" placeholder="••••••••" autocomplete="new-password" />
            <small class="muted">${t('pwHint')}</small></label>`,
        `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
         <button class="btn btn-primary" onclick="submitModal(this)">${t('save')}</button>`,
        async () => {
            const currentPassword = el('cp-current').value;
            const newPassword = el('cp-new').value;
            if (!currentPassword || !newPassword) { toast(t('tCouldNotChangePw'), t('tFillAllFields'), 'error'); throw new Error('validation'); }
            if (newPassword.length < 8) { toast(t('tCouldNotChangePw'), t('tPwTooShort'), 'error'); throw new Error('validation'); }
            await api('POST', '/api/auth/change-password', { currentPassword, newPassword });
            closeModal();
            toast(t('tPasswordChanged'), t('tPasswordChangedBody'), 'success');
        });
}

// Backend URL settings (for when the API is hosted separately from this static frontend).
function openBackendSettings() {
    openModal(t('backendTitle'), `
        <p class="muted" style="margin-top:0">${t('backendHelp')}</p>
        <label class="field"><span>${t('backendTitle')}</span>
            <input id="m-api" type="url" value="${escapeHtml(currentApiBase())}" placeholder="${t('backendPh')}" /></label>`,
        `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
         <button class="btn btn-primary" onclick="submitModal(this)">${t('save')}</button>`,
        async () => {
            const v = el('m-api').value.trim().replace(/\/+$/, '');
            if (v) localStorage.setItem(API_BASE_KEY, v); else localStorage.removeItem(API_BASE_KEY);
            closeModal();
            toast(v ? t('backendSaved') : t('backendCleared'), v, 'success');
        });
}

function logout() {
    localStorage.removeItem(STORE_KEY);
    S.token = null; S.me = null; S.roles = [];
    el('app-view').classList.add('hidden');
    el('login-view').classList.remove('hidden');
}
function isAdmin() { return S.roles.includes('WorkflowAdmin'); }

async function enterApp() {
    el('login-view').classList.add('hidden');
    el('app-view').classList.remove('hidden');
    renderUserChip();
    await loadReferenceData();
    await refreshPending();
    go('inbox');
}

async function loadReferenceData() {
    try {
        const [emps, roles, depts] = await Promise.all([
            api('GET', '/api/reference/employees'),
            api('GET', '/api/reference/roles'),
            api('GET', '/api/reference/departments'),
        ]);
        S.empById = Object.fromEntries(emps.map((e) => [e.id, e]));
        S.roleById = Object.fromEntries(roles.map((r) => [r.id, r]));
        S.deptById = Object.fromEntries(depts.map((d) => [d.id, d]));
    } catch { /* non-fatal */ }
}

async function refreshPending() {
    try {
        S.pending = await api('GET', '/api/dashboard/my-pending');
        S.actionable = new Set(S.pending.map((p) => p.instanceId));
    } catch { S.pending = []; S.actionable = new Set(); }
}

/* ============================================================
   shell: user chip + nav
   ============================================================ */
function renderUserChip() {
    const roleBadge = S.roles.map((r) => t('role.' + r)).join(' · ') || t('noRole');
    el('user-chip').innerHTML = `${avatar(S.name)}
        <div class="info"><b>${escapeHtml(S.name)}</b><small>${escapeHtml(roleBadge)}</small></div>`;
}

function renderNav() {
    const items = [
        { id: 'inbox', ico: '📥', badge: S.pending.length },
        { id: 'documents', ico: '📄' },
        { id: 'sla', ico: '⏰' },
        { id: 'activity', ico: '📋' },
    ];
    let html = items.map(navItem).join('');
    if (isAdmin()) {
        html += `<div class="nav-section">${t('nav.admin')}</div>`;
        html += navItem({ id: 'doctypes', ico: '🗂️' });
        html += navItem({ id: 'workflows', ico: '🔀' });
    }
    el('nav').innerHTML = html;
    el('nav').querySelectorAll('.nav-item').forEach((n) => n.addEventListener('click', () => go(n.dataset.view)));
}
function navItem(it) {
    const active = S.section === it.id ? 'active' : '';
    const badge = it.badge ? `<span class="badge-count">${it.badge}</span>` : '';
    return `<button class="nav-item ${active}" data-view="${it.id}">
        <span class="ico">${it.ico}</span><span>${t('nav.' + it.id)}</span>${badge}</button>`;
}

function setPage(title, subtitle, actionsHtml = '') {
    el('page-title').textContent = title;
    el('page-subtitle').textContent = subtitle || '';
    el('topbar-actions').innerHTML = actionsHtml;
}
const loader = () => `<div class="loader"><div class="spinner"></div>${t('loading')}</div>`;
function emptyState(ico, title, sub) {
    return `<div class="empty"><div class="ico">${ico}</div><h3>${escapeHtml(title)}</h3><p>${escapeHtml(sub)}</p></div>`;
}

/* ============================================================
   router
   ============================================================ */
const VIEWS = {
    inbox: renderInbox, documents: renderDocuments, sla: renderSla, activity: renderActivity,
    detail: renderDetail, doctypes: renderDocTypes, workflows: renderWorkflows,
};
const NAV_SECTIONS = new Set(['inbox', 'documents', 'sla', 'activity', 'doctypes', 'workflows']);

function go(view, opts = {}) {
    if (opts.id !== undefined) S.detailId = opts.id;
    if (opts.from) S.detailFrom = opts.from;
    if (NAV_SECTIONS.has(view)) S.section = view;
    S.view = view;
    renderNav();
    window.scrollTo(0, 0);
    VIEWS[view]();
}

/* ============================================================
   VIEW: Inbox
   ============================================================ */
async function renderInbox() {
    setPage(t('nav.inbox'), t('sub.inbox'), `<button class="btn btn-outline btn-sm" onclick="renderInbox()">↻ ${t('refresh')}</button>`);
    el('content').innerHTML = loader();
    await refreshPending();
    renderNav();
    el('content').innerHTML = S.pending.length
        ? S.pending.map(pendingRow).join('')
        : emptyState('🎉', t('inboxZero'), t('inboxZeroSub'));
}

function pendingRow(p) {
    const due = p.dueAtUtc ? `<span>${p.isOverdue ? '🔴' : '🕒'} ${t('due')} <b>${fromNow(p.dueAtUtc)}</b></span>` : '';
    return `<div class="row-card ${p.isOverdue ? 'overdue' : ''}" onclick="openDetail(${p.instanceId}, 'inbox')">
        <div class="doc-icon">📄</div>
        <div class="row-main">
            <div class="row-title">${escapeHtml(p.documentTypeCode)} #${escapeHtml(p.documentId)}
                <span class="tag">${t('stage')} ${p.stageOrder} · ${escapeHtml(p.stageName)}</span>
                ${p.isOverdue ? `<span class="pill overdue">${t('overdue')}</span>` : ''}</div>
            <div class="row-meta">
                <span>${t('from')} <b>${escapeHtml(empName(p.initiatorEmployeeId))}</b></span>
                <span>${t('waiting')} <b>${fromNow(p.enteredAtUtc)}</b></span>
                ${due}
                ${p.cycleNumber > 1 ? `<span>${t('cycle')} ${p.cycleNumber}</span>` : ''}</div>
        </div>
        <div class="row-actions" onclick="event.stopPropagation()">
            <button class="btn btn-success btn-sm" onclick="quickApprove(${p.instanceId})">${t('approve')}</button>
            <button class="btn btn-danger-outline btn-sm" onclick="promptReject(${p.instanceId})">${t('reject')}</button>
        </div>
    </div>`;
}

/* ============================================================
   VIEW: My Documents
   ============================================================ */
function draftKey() { return `flowapprove.drafts.${S.me}`; }
function getDraftIds() { try { return JSON.parse(localStorage.getItem(draftKey()) || '[]'); } catch { return []; } }
function saveDraftIds(ids) { localStorage.setItem(draftKey(), JSON.stringify([...new Set(ids)])); }

async function renderDocuments() {
    setPage(t('nav.documents'), t('sub.documents'),
        `<button class="btn btn-outline btn-sm" onclick="openCreateInvoice()">＋ ${t('newInvoice')}</button>
         <button class="btn btn-primary btn-sm" onclick="openCreateLeave()">＋ ${t('newLeave')}</button>`);
    el('content').innerHTML = loader();

    const draftIds = getDraftIds();
    const fetched = await Promise.allSettled(draftIds.map((id) => api('GET', `/api/leave-requests/${id}`)));
    const drafts = [], stillValid = [];
    fetched.forEach((r, i) => {
        if (r.status === 'fulfilled') { stillValid.push(draftIds[i]); if (r.value.status === 'Draft') drafts.push(r.value); }
    });
    saveDraftIds(stillValid);

    let submitted = [];
    try { submitted = await api('GET', '/api/dashboard/my-documents'); } catch { /* ignore */ }

    let html = '';
    if (drafts.length) html += `<div class="section-title">${t('draftsTitle')}</div>` + drafts.map(draftRow).join('');
    html += `<div class="section-title" style="margin-top:${drafts.length ? '26px' : '4px'}">${t('inApprovalTitle')}</div>`;
    html += submitted.length ? submitted.map(docRow).join('') : emptyState('📭', t('nothingSubmitted'), t('nothingSubmittedSub'));
    el('content').innerHTML = html;
}

function draftRow(l) {
    return `<div class="row-card" onclick="openEditLeave(${l.id})">
        <div class="doc-icon">📝</div>
        <div class="row-main">
            <div class="row-title">${t('leaveReq')} #${l.id} ${statusPill('Draft')}</div>
            <div class="row-meta">
                <span>${fmtDay(l.fromDate)} → ${fmtDay(l.toDate)}</span>
                <span><b>${daysBetween(l.fromDate, l.toDate)}</b> ${t('days')}</span>
                <span>${escapeHtml(l.reason)}</span></div>
        </div>
        <div class="row-actions" onclick="event.stopPropagation()">
            <button class="btn btn-outline btn-sm" onclick="openEditLeave(${l.id})">${t('edit')}</button>
            <button class="btn btn-primary btn-sm" onclick="submitLeave(${l.id})">${t('submitBtn')}</button>
        </div>
    </div>`;
}

function docRow(d) {
    const stage = d.currentStageName ? `<span class="tag">${t('stage')} ${d.currentStageOrder} · ${escapeHtml(d.currentStageName)}</span>` : '';
    return `<div class="row-card" onclick="openDetail(${d.instanceId}, 'documents')">
        <div class="doc-icon">📄</div>
        <div class="row-main">
            <div class="row-title">${escapeHtml(d.documentTypeCode)} #${escapeHtml(d.documentId)} ${statusPill(d.status)} ${d.status === 'Pending' ? stage : ''}</div>
            <div class="row-meta">
                <span>${t('created')} <b>${fmtDate(d.createdAtUtc)}</b></span>
                ${d.completedAtUtc ? `<span>${t('completed')} <b>${fmtDate(d.completedAtUtc)}</b></span>` : ''}
                ${d.cycleNumber > 1 ? `<span>${t('cycle')} ${d.cycleNumber}</span>` : ''}</div>
        </div>
        <div class="row-actions"><button class="btn btn-outline btn-sm">${t('view')} <span class="flip-x">→</span></button></div>
    </div>`;
}

/* ============================================================
   VIEW: SLA breaches
   ============================================================ */
async function renderSla() {
    setPage(t('nav.sla'), t('sub.sla'), `<button class="btn btn-outline btn-sm" onclick="renderSla()">↻ ${t('refresh')}</button>`);
    el('content').innerHTML = loader();
    let rows = [];
    try { rows = await api('GET', '/api/dashboard/sla-breaches'); }
    catch (err) { el('content').innerHTML = emptyState('⚠️', t('couldNotLoad'), err.message); return; }
    if (!rows.length) { el('content').innerHTML = emptyState('✅', t('allWithinSla'), t('allWithinSlaSub')); return; }
    el('content').innerHTML = rows.map((p) => `
        <div class="row-card overdue" onclick="openDetail(${p.instanceId}, 'sla')">
            <div class="doc-icon" style="background:var(--red-bg);color:var(--red)">⏰</div>
            <div class="row-main">
                <div class="row-title">${escapeHtml(p.documentTypeCode)} #${escapeHtml(p.documentId)}
                    <span class="tag">${t('stage')} ${p.stageOrder} · ${escapeHtml(p.stageName)}</span>
                    <span class="pill overdue">${t('overdue')} ${fromNow(p.dueAtUtc)}</span></div>
                <div class="row-meta">
                    <span>${t('from')} <b>${escapeHtml(empName(p.initiatorEmployeeId))}</b></span>
                    <span>${t('dueWas')} <b>${fmtDate(p.dueAtUtc)}</b></span></div>
            </div>
            <div class="row-actions"><button class="btn btn-outline btn-sm">${t('view')} <span class="flip-x">→</span></button></div>
        </div>`).join('');
}

/* ============================================================
   VIEW: Activity log
   ============================================================ */
const ACTIVITY_ICONS = {
    Login: '🔓', LoginFailed: '🚫', Register: '🆕', PasswordChanged: '🔑',
    AdminCreatedUser: '👤', DocumentSubmitted: '📤', ApprovalDecision: '⚖️',
};

function setActivityScope(scope) { S.activityScope = scope; renderActivity(); }

async function renderActivity() {
    const admin = isAdmin();
    if (!admin) S.activityScope = 'me';

    let actions = '';
    if (admin) {
        actions += `<button class="btn btn-sm ${S.activityScope === 'me' ? 'btn-primary' : 'btn-outline'}" onclick="setActivityScope('me')">${t('actScopeMe')}</button>
            <button class="btn btn-sm ${S.activityScope === 'all' ? 'btn-primary' : 'btn-outline'}" onclick="setActivityScope('all')">${t('actScopeAll')}</button>`;
    }
    actions += `<button class="btn btn-outline btn-sm" onclick="renderActivity()">↻ ${t('refresh')}</button>`;
    setPage(t('nav.activity'), t('sub.activity'), actions);
    el('content').innerHTML = loader();

    const path = admin && S.activityScope === 'all' ? '/api/activity' : '/api/activity/me';
    let rows = [];
    try { rows = await api('GET', path); }
    catch (err) { el('content').innerHTML = emptyState('⚠️', t('couldNotLoad'), err.message); return; }

    el('content').innerHTML = rows.length
        ? `<div class="card"><div class="card-pad">${rows.map(activityRow).join('')}</div></div>`
        : emptyState('📋', t('noActivityYet'), t('noActivityYetSub'));
}

function activityRow(a) {
    const who = a.actorName || a.actorEmail || (a.employeeId ? `#${a.employeeId}` : '—');
    const target = a.entityType
        ? ` <span class="tag">${escapeHtml(a.entityType)}${a.entityId ? ' #' + escapeHtml(a.entityId) : ''}</span>` : '';
    const ip = a.ipAddress ? ` · ${t('actIp')} ${escapeHtml(a.ipAddress)}` : '';
    return `<div class="feed-item">
        <div class="feed-ico">${ACTIVITY_ICONS[a.type] || '•'}</div>
        <div class="feed-body">
            <div class="t"><b>${escapeHtml(who)}</b> ${escapeHtml(t('activity.' + a.type))}${target}</div>
            ${a.description ? `<div class="c">${escapeHtml(a.description)}</div>` : ''}
            <div class="when">${fmtDate(a.createdAtUtc)} · ${fromNow(a.createdAtUtc)}${ip}</div>
        </div>
    </div>`;
}

/* ============================================================
   VIEW: Approval detail
   ============================================================ */
function openDetail(id, from) { go('detail', { id, from }); }

async function renderDetail() {
    setPage(t('approvalDetail'), '');
    el('topbar-actions').innerHTML = '';
    el('content').innerHTML = loader();

    let inst, history;
    try {
        [inst, history] = await Promise.all([
            api('GET', `/api/approvals/${S.detailId}`),
            api('GET', `/api/approvals/${S.detailId}/history`),
        ]);
    } catch (err) { el('content').innerHTML = emptyState('⚠️', t('couldNotLoadApproval'), err.message); return; }

    setPage(`${inst.documentTypeCode || ''} #${inst.documentId}`,
        t('wfVersionBy', { v: inst.workflowVersion, name: empName(inst.initiatorEmployeeId) }));

    const isInitiator = inst.initiatorEmployeeId === S.me;
    const canAct = S.actionable.has(inst.id);
    const stages = [...inst.stages].sort((a, b) => a.stageOrder - b.stageOrder);

    // Document-specific details for invoices, so an approver sees what they're approving.
    let docCard = '';
    if (inst.documentTypeCode === 'Invoice') {
        try { docCard = invoiceCard(await api('GET', `/api/invoices/${inst.documentId}`)); }
        catch { /* non-fatal — the approval flow still renders */ }
    }

    el('content').innerHTML = `
        <button class="back-link" onclick="go('${S.detailFrom}')"><span class="flip-x">←</span> ${t('backTo', { x: t('nav.' + S.detailFrom) })}</button>
        <div class="detail-grid">
            <div>
                ${docCard}
                <div class="card">
                    <div class="card-head"><h3>${t('approvalProgress')}</h3>${statusPill(inst.status)}</div>
                    <div class="card-pad"><div class="timeline">${stages.map((s) => stageTimelineItem(s, inst)).join('')}</div></div>
                </div>
                <div class="card">
                    <div class="card-head"><h3>${t('activityHistory')}</h3><span class="pill plain">${t('events', { n: history.length })}</span></div>
                    <div class="card-pad">${history.length ? history.map(feedItem).join('') : `<p class="muted">${t('noActivity')}</p>`}</div>
                </div>
            </div>
            <div>
                <div class="card card-pad">
                    <div class="section-title">${t('summary')}</div>
                    <div class="kv"><span class="k">${t('lStatus')}</span><span class="v">${statusPill(inst.status)}</span></div>
                    <div class="kv"><span class="k">${t('lDocument')}</span><span class="v">${escapeHtml(inst.documentTypeCode)} #${escapeHtml(inst.documentId)}</span></div>
                    <div class="kv"><span class="k">${t('lInitiator')}</span><span class="v">${escapeHtml(empName(inst.initiatorEmployeeId))}</span></div>
                    <div class="kv"><span class="k">${t('cycle')}</span><span class="v">${inst.cycleNumber}</span></div>
                    <div class="kv"><span class="k">${t('lCurrentStage')}</span><span class="v">${inst.currentStageName ? escapeHtml(inst.currentStageName) : '—'}</span></div>
                    <div class="kv"><span class="k">${t('created')}</span><span class="v">${fmtDate(inst.createdAtUtc)}</span></div>
                    ${inst.completedAtUtc ? `<div class="kv"><span class="k">${t('completed')}</span><span class="v">${fmtDate(inst.completedAtUtc)}</span></div>` : ''}
                    ${detailActions(inst, isInitiator, canAct)}
                </div>
            </div>
        </div>`;
}

function stageTimelineItem(s, inst) {
    const isCurrent = inst.status === 'Pending' && s.stageOrder === inst.currentStageOrder;
    const dotClass = s.status === 'Approved' ? 'approved' : s.status === 'Rejected' ? 'rejected' : isCurrent ? 'current' : 'pending';
    const dotIcon = s.status === 'Approved' ? '✓' : s.status === 'Rejected' ? '✕' : isCurrent ? '●' : '';
    let sub;
    if (s.status === 'Pending')
        sub = isCurrent ? `${t('awaitingDecision')}${s.dueAtUtc ? ` · ${t('dueWhen', { when: fromNow(s.dueAtUtc) })}` : ''}` : t('notStarted');
    else
        sub = t('actedBy', { status: tStatus(s.status), name: empName(s.actedByEmployeeId), when: fmtDate(s.actedAtUtc) });
    return `<div class="tl-item">
        <div class="tl-dot ${dotClass}">${dotIcon}</div>
        <div class="tl-body">
            <div class="tl-title">${escapeHtml(s.name)}
                <span class="pill plain">${t('kind.' + s.approverType)}</span>
                ${isCurrent ? `<span class="tag">${t('current')}</span>` : ''}</div>
            <div class="tl-sub">${sub}</div>
        </div>
    </div>`;
}

function feedItem(a) {
    const icons = { Submit: '📤', Approve: '✅', Reject: '❌', Comment: '💬', Resubmit: '🔁', Cancel: '🚫' };
    return `<div class="feed-item">
        <div class="feed-ico">${icons[a.actionType] || '•'}</div>
        <div class="feed-body">
            <div class="t"><b>${escapeHtml(empName(a.actedByEmployeeId))}</b> ${t('act.' + a.actionType)}
                ${a.fromStatus && a.toStatus && a.fromStatus !== a.toStatus ? `<span class="muted">· ${tStatus(a.fromStatus)} → ${tStatus(a.toStatus)}</span>` : ''}</div>
            ${a.comment ? `<div class="c">${escapeHtml(a.comment)}</div>` : ''}
            <div class="when">${fmtDate(a.createdAtUtc)} · ${fromNow(a.createdAtUtc)}</div>
        </div>
    </div>`;
}

function detailActions(inst, isInitiator, canAct) {
    const btns = [];
    if (inst.status === 'Pending' && canAct) {
        btns.push(`<button class="btn btn-success" onclick="quickApprove(${inst.id})">✓ ${t('approve')}</button>`);
        btns.push(`<button class="btn btn-danger-outline" onclick="promptReject(${inst.id})">✕ ${t('reject')}</button>`);
    }
    if (inst.status === 'Rejected' && isInitiator)
        btns.push(`<button class="btn btn-primary" onclick="resubmit(${inst.id})">🔁 ${t('resubmit')}</button>`);
    if (inst.status === 'Pending' && (isInitiator || isAdmin()))
        btns.push(`<button class="btn btn-danger-outline" onclick="promptCancel(${inst.id})">${t('cancelApproval')}</button>`);
    if (isInitiator || canAct)
        btns.push(`<button class="btn btn-outline" onclick="promptComment(${inst.id})">💬 ${t('comment')}</button>`);
    if (!btns.length) return '';
    return `<div class="section-title" style="margin-top:18px">${t('actions')}</div><div class="inline-actions">${btns.join('')}</div>`;
}

/* ============================================================
   approval actions
   ============================================================ */
async function quickApprove(id) {
    openModal(t('approve'), `
        <p class="muted" style="margin-top:0">${t('mApproveBody')}</p>
        <label class="field"><span>${t('commentOptional')}</span><textarea id="m-comment" placeholder="${t('phLooksGood')}"></textarea></label>`,
        `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
         <button class="btn btn-success" onclick="submitModal(this)">✓ ${t('approve')}</button>`,
        async () => {
            await api('POST', `/api/approvals/${id}/approve`, { comment: el('m-comment').value.trim() || null });
            closeModal(); await refreshApproval(id, t('tApprovalRecorded'));
        });
}

function promptReject(id) {
    openModal(t('reject'), `
        <p class="muted" style="margin-top:0">${t('mRejectBody')}</p>
        <label class="field"><span>${t('reasonForReject')}</span><textarea id="m-comment" placeholder="${t('phExplain')}"></textarea></label>`,
        `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
         <button class="btn btn-danger" onclick="submitModal(this)">${t('reject')}</button>`,
        async () => {
            const c = el('m-comment').value.trim();
            if (!c) { toast(t('tReasonReq'), t('tReasonReqBody'), 'error'); throw new Error('validation'); }
            await api('POST', `/api/approvals/${id}/reject`, { comment: c });
            closeModal(); await refreshApproval(id, t('tDocRejected'));
        });
}

function promptComment(id) {
    openModal(t('addComment'), `<label class="field"><span>${t('commentStar')}</span><textarea id="m-comment" placeholder="${t('phWriteNote')}"></textarea></label>`,
        `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
         <button class="btn btn-primary" onclick="submitModal(this)">${t('postComment')}</button>`,
        async () => {
            const c = el('m-comment').value.trim();
            if (!c) { toast(t('tEmptyComment'), t('tEmptyCommentBody'), 'error'); throw new Error('validation'); }
            await api('POST', `/api/approvals/${id}/comment`, { comment: c });
            closeModal(); await refreshApproval(id, t('tCommentPosted'));
        });
}

function promptCancel(id) {
    openModal(t('cancelApproval'), `
        <p class="muted" style="margin-top:0">${t('mCancelBody')}</p>
        <label class="field"><span>${t('reasonOptional')}</span><textarea id="m-comment" placeholder="${t('phWhyCancel')}"></textarea></label>`,
        `<button class="btn btn-ghost" data-close>${t('keepIt')}</button>
         <button class="btn btn-danger" onclick="submitModal(this)">${t('cancelApproval')}</button>`,
        async () => {
            await api('POST', `/api/approvals/${id}/cancel`, { comment: el('m-comment').value.trim() || null });
            closeModal(); await refreshApproval(id, t('tApprovalCancelled'));
        });
}

async function resubmit(id) {
    try { await api('POST', `/api/approvals/${id}/resubmit`); await refreshApproval(id, t('tResubmitted')); }
    catch (err) { toast(t('tCouldNotResubmit'), err.message, 'error'); }
}

async function refreshApproval(id, msg) {
    toast(t('tDone'), msg, 'success');
    await refreshPending();
    renderNav();
    if (S.view === 'detail') go('detail', { id, from: S.detailFrom });
    else (VIEWS[S.view] || renderInbox)();
}

/* ============================================================
   leave request create / edit / submit
   ============================================================ */
function leaveForm(l) {
    const today = new Date().toISOString().slice(0, 10);
    return `
        <div class="field-row">
            <label class="field"><span>${t('fromDate')}</span><input id="m-from" type="date" value="${l?.fromDate || today}" /></label>
            <label class="field"><span>${t('toDate')}</span><input id="m-to" type="date" value="${l?.toDate || today}" /></label>
        </div>
        <label class="field"><span>${t('reasonStar')}</span><textarea id="m-reason" placeholder="${t('phVacation')}">${escapeHtml(l?.reason || '')}</textarea></label>`;
}

function openCreateLeave() {
    openModal(t('newLeave'), leaveForm(),
        `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
         <button class="btn btn-primary" onclick="submitModal(this)">${t('createDraft')}</button>`,
        async () => {
            const body = readLeaveForm(); if (!body) throw new Error('validation');
            const created = await api('POST', '/api/leave-requests', body);
            saveDraftIds([...getDraftIds(), created.id]);
            closeModal(); toast(t('tDraftCreated'), t('tDraftCreatedBody', { id: created.id }), 'success'); renderDocuments();
        });
}

async function openEditLeave(id) {
    let l;
    try { l = await api('GET', `/api/leave-requests/${id}`); }
    catch (err) { toast(t('couldNotLoad'), err.message, 'error'); return; }
    openModal(t('editLeaveTitle', { id }), leaveForm(l),
        `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
         <button class="btn btn-primary" onclick="submitModal(this)">${t('saveChanges')}</button>`,
        async () => {
            const body = readLeaveForm(); if (!body) throw new Error('validation');
            await api('PUT', `/api/leave-requests/${id}`, body);
            closeModal(); toast(t('tSaved'), t('tLeaveUpdated'), 'success'); renderDocuments();
        });
}

function readLeaveForm() {
    const fromDate = el('m-from').value, toDate = el('m-to').value, reason = el('m-reason').value.trim();
    if (!fromDate || !toDate || !reason) { toast(t('tMissingFields'), t('tMissingFieldsBody'), 'error'); return null; }
    if (toDate < fromDate) { toast(t('tInvalidDates'), t('tInvalidDatesBody'), 'error'); return null; }
    return { fromDate, toDate, reason };
}

async function submitLeave(id) {
    try {
        await api('POST', `/api/leave-requests/${id}/submit`);
        saveDraftIds(getDraftIds().filter((x) => x !== id));
        toast(t('tSubmitted'), t('tSubmittedBody', { id }), 'success');
        await refreshPending(); renderDocuments();
    } catch (err) { toast(t('tCouldNotSubmit'), err.message, 'error'); }
}

/* ============================================================
   invoice create + submit
   ============================================================ */
const INVOICE_CURRENCIES = ['EGP', 'USD', 'EUR', 'SAR', 'AED'];

function fmtMoney(amount, currency) {
    const n = Number(amount);
    const s = Number.isFinite(n) ? n.toLocaleString(loc(), { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : amount;
    return `${escapeHtml(String(currency || ''))} ${s}`;
}

function invoiceForm() {
    const today = new Date().toISOString().slice(0, 10);
    const currencyOpts = INVOICE_CURRENCIES.map((c) => `<option value="${c}">${c}</option>`).join('');
    return `
        <div class="field-row">
            <label class="field"><span>${t('amountStar')}</span><input id="m-amount" type="number" min="0" step="0.01" placeholder="0.00" /></label>
            <label class="field"><span>${t('currencyStar')}</span><select id="m-currency">${currencyOpts}</select></label>
        </div>
        <label class="field"><span>${t('vendorStar')}</span><input id="m-vendor" type="text" placeholder="${t('phVendor')}" /></label>
        <label class="field"><span>${t('invDescription')}</span><textarea id="m-desc" placeholder="${t('phInvDesc')}"></textarea></label>
        <div class="field-row">
            <label class="field"><span>${t('invoiceDateStar')}</span><input id="m-invdate" type="date" value="${today}" /></label>
            <label class="field"><span>${t('dueDateStar')}</span><input id="m-duedate" type="date" value="${today}" /></label>
        </div>`;
}

function readInvoiceForm() {
    const amount = parseFloat(el('m-amount').value);
    const currency = el('m-currency').value;
    const vendor = el('m-vendor').value.trim();
    const description = el('m-desc').value.trim() || null;
    const invoiceDate = el('m-invdate').value, dueDate = el('m-duedate').value;
    if (!vendor || !el('m-amount').value || !invoiceDate || !dueDate) { toast(t('tWentWrong'), t('tInvMissingFields'), 'error'); return null; }
    if (!(amount > 0)) { toast(t('tWentWrong'), t('tInvInvalidAmount'), 'error'); return null; }
    if (dueDate < invoiceDate) { toast(t('tInvalidDates'), t('tInvInvalidDates'), 'error'); return null; }
    return { amount, currency, vendor, description, invoiceDate, dueDate };
}

// One step: create the invoice draft, then submit it into the approval engine.
function openCreateInvoice() {
    openModal(t('invoiceTitle'), invoiceForm(),
        `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
         <button class="btn btn-primary" onclick="submitModal(this)">${t('createSubmitInvoice')}</button>`,
        async () => {
            const body = readInvoiceForm(); if (!body) throw new Error('validation');
            const created = await api('POST', '/api/invoices', body);
            await api('POST', `/api/invoices/${created.id}/submit`);
            closeModal();
            toast(t('tInvoiceSubmitted'), t('tInvoiceSubmittedBody', { id: created.id }), 'success');
            await refreshPending(); renderDocuments();
        });
}

// Holds the invoice currently rendered in the detail view, so printInvoice() can format it
// without a second fetch (and stays synchronous, avoiding pop-up blockers on the print window).
let _printableInvoice = null;

function invoiceCard(inv) {
    _printableInvoice = inv;
    return `<div class="card card-pad">
        <div class="section-title" style="display:flex;align-items:center;justify-content:space-between;gap:8px">
            <span>${t('invoiceDetails')}</span>
            <button class="btn btn-outline btn-sm" onclick="printInvoice()">🖨 ${t('printLabel')}</button>
        </div>
        <div class="kv"><span class="k">${t('lAmount')}</span><span class="v"><b>${fmtMoney(inv.amount, inv.currency)}</b></span></div>
        <div class="kv"><span class="k">${t('lVendor')}</span><span class="v">${escapeHtml(inv.vendor)}</span></div>
        ${inv.description ? `<div class="kv"><span class="k">${t('lDescription')}</span><span class="v">${escapeHtml(inv.description)}</span></div>` : ''}
        <div class="kv"><span class="k">${t('lInvoiceDate')}</span><span class="v">${fmtDay(inv.invoiceDate)}</span></div>
        <div class="kv"><span class="k">${t('lDueDate')}</span><span class="v">${fmtDay(inv.dueDate)}</span></div>
    </div>`;
}

// Opens a clean, print-ready invoice document in a new window and triggers the browser print dialog.
// Fully self-contained (inline styles, no network), RTL-aware, so it works for both languages.
function printInvoice() {
    const inv = _printableInvoice;
    if (!inv) return;
    const rtl = S.lang === 'ar';
    const now = new Date().toLocaleDateString(loc(), { year: 'numeric', month: 'long', day: 'numeric' });
    const row = (k, v) => `<tr><td class="k">${k}</td><td class="v">${v}</td></tr>`;
    const doc = `<!doctype html>
<html lang="${S.lang}" dir="${rtl ? 'rtl' : 'ltr'}">
<head>
<meta charset="utf-8" />
<title>${t('invoicePrintTitle')} #${inv.id}</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: 'Segoe UI', system-ui, -apple-system, Arial, sans-serif; color: #1f2733; background: #f4f5f7; padding: 32px; -webkit-print-color-adjust: exact; print-color-adjust: exact; }
  .sheet { max-width: 720px; margin: 0 auto; background: #fff; border: 1px solid #e6e8ec; border-radius: 14px; padding: 40px; }
  .head { display: flex; justify-content: space-between; align-items: flex-start; gap: 16px; border-bottom: 2px solid #eef0f3; padding-bottom: 24px; margin-bottom: 28px; }
  .brand { display: flex; align-items: center; gap: 12px; }
  .mark { width: 44px; height: 44px; border-radius: 12px; background: #5b5bf0; color: #fff; display: flex; align-items: center; justify-content: center; font-size: 22px; font-weight: 700; }
  .bname { font-size: 19px; font-weight: 700; }
  .btag { font-size: 12px; color: #7a828e; margin-top: 2px; }
  .doc { text-align: end; }
  .doctitle { font-size: 26px; font-weight: 800; letter-spacing: 2px; color: #5b5bf0; }
  .docno { font-size: 13px; color: #4a5361; margin-top: 4px; }
  .status { display: inline-block; margin-top: 8px; font-size: 12px; font-weight: 600; padding: 3px 12px; border-radius: 999px; background: #eef0ff; color: #4646c8; }
  .amount { background: #f7f8fb; border: 1px solid #edeff3; border-radius: 12px; padding: 22px 24px; margin-bottom: 26px; }
  .amount-label { font-size: 12px; text-transform: uppercase; letter-spacing: 1px; color: #7a828e; }
  .amount-value { font-size: 32px; font-weight: 800; margin-top: 6px; }
  .meta { width: 100%; border-collapse: collapse; }
  .meta td { padding: 13px 4px; border-bottom: 1px solid #eef0f3; vertical-align: top; font-size: 14px; }
  .meta td.k { color: #7a828e; width: 38%; font-weight: 600; }
  .meta td.v { text-align: end; }
  .foot { margin-top: 30px; padding-top: 18px; border-top: 1px solid #eef0f3; font-size: 12px; color: #9aa1ab; text-align: center; }
  @media print { body { background: #fff; padding: 0; } .sheet { border: none; border-radius: 0; max-width: none; padding: 8px; } }
</style>
</head>
<body>
  <div class="sheet">
    <header class="head">
      <div class="brand">
        <span class="mark">✓</span>
        <div><div class="bname">FlowApprove</div><div class="btag">${escapeHtml(t('printBrandTag'))}</div></div>
      </div>
      <div class="doc">
        <div class="doctitle">${t('invoicePrintTitle')}</div>
        <div class="docno">${t('invoiceNoLabel')} #${inv.id}</div>
        <span class="status">${escapeHtml(tStatus(inv.status))}</span>
      </div>
    </header>
    <div class="amount">
      <div class="amount-label">${t('lAmount')}</div>
      <div class="amount-value">${fmtMoney(inv.amount, inv.currency)}</div>
    </div>
    <table class="meta">
      ${row(t('lVendor'), escapeHtml(inv.vendor))}
      ${inv.description ? row(t('lDescription'), escapeHtml(inv.description)) : ''}
      ${row(t('lInvoiceDate'), fmtDay(inv.invoiceDate))}
      ${row(t('lDueDate'), fmtDay(inv.dueDate))}
      ${row(t('lStatus'), escapeHtml(tStatus(inv.status)))}
    </table>
    <footer class="foot">${escapeHtml(t('printedOn', { when: now }))}</footer>
  </div>
  <script>window.addEventListener('load', function () { window.focus(); window.print(); });<\/script>
</body>
</html>`;
    const w = window.open('', '_blank', 'width=820,height=1000');
    if (!w) { toast(t('tWentWrong'), t('printLabel'), 'error'); return; }
    w.document.open();
    w.document.write(doc);
    w.document.close();
}

/* ============================================================
   ADMIN: Document Types
   ============================================================ */
async function renderDocTypes() {
    setPage(t('nav.doctypes'), t('sub.doctypes'), `<button class="btn btn-primary btn-sm" onclick="openCreateDocType()">＋ ${t('newDocType')}</button>`);
    el('content').innerHTML = loader();
    let types = [];
    try { types = await api('GET', '/api/document-types'); }
    catch (err) { el('content').innerHTML = emptyState('⚠️', t('couldNotLoad'), err.message); return; }
    if (!types.length) { el('content').innerHTML = emptyState('🗂️', t('noDocTypes'), t('noDocTypesSub')); return; }
    el('content').innerHTML = `<div class="card"><div class="card-pad">` +
        types.map((ty) => `<div class="stage-item" style="margin-bottom:10px">
            <div class="doc-icon">🗂️</div>
            <div class="stage-info"><b>${escapeHtml(ty.name)}</b><div class="sub">${t('codeColon')}: <code>${escapeHtml(ty.code)}</code></div></div>
            ${ty.isActive ? `<span class="pill approved">${t('active')}</span>` : `<span class="pill cancelled">${t('inactive')}</span>`}
        </div>`).join('') + `</div></div>`;
}

function openCreateDocType() {
    openModal(t('newDocType'), `
        <label class="field"><span>${t('codeStar')} <small class="muted">${t('codeHint')}</small></span><input id="m-code" placeholder="PurchaseOrder" /></label>
        <label class="field"><span>${t('displayName')}</span><input id="m-name" placeholder="Purchase Order" /></label>`,
        `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
         <button class="btn btn-primary" onclick="submitModal(this)">${t('create')}</button>`,
        async () => {
            const code = el('m-code').value.trim(), name = el('m-name').value.trim();
            if (!code || !name) { toast(t('tMissingFields'), t('tMissingCodeName'), 'error'); throw new Error('validation'); }
            await api('POST', '/api/document-types', { code, name });
            closeModal(); toast(t('tCreated'), t('tDocTypeAdded', { name }), 'success'); renderDocTypes();
        });
}

/* ============================================================
   ADMIN: Workflows + stage builder
   ============================================================ */
async function renderWorkflows() {
    setPage(t('nav.workflows'), t('sub.workflows'), `<button class="btn btn-primary btn-sm" onclick="openCreateWorkflow()">＋ ${t('newWorkflow')}</button>`);
    el('content').innerHTML = loader();

    let workflows = [], types = [];
    try { [workflows, types] = await Promise.all([api('GET', '/api/workflows'), api('GET', '/api/document-types')]); }
    catch (err) { el('content').innerHTML = emptyState('⚠️', t('couldNotLoad'), err.message); return; }
    if (!workflows.length) { el('content').innerHTML = emptyState('🔀', t('noWorkflows'), t('noWorkflowsSub')); return; }

    const typeName = Object.fromEntries(types.map((ty) => [ty.id, ty.name]));
    if (!S.wfSelected || !workflows.some((w) => w.id === S.wfSelected)) S.wfSelected = workflows[0].id;
    const selected = workflows.find((w) => w.id === S.wfSelected);

    const list = workflows.map((w) => `
        <div class="wf-list-item ${w.id === S.wfSelected ? 'active' : ''}" onclick="selectWorkflow(${w.id})">
            <div class="doc-icon" style="width:34px;height:34px;font-size:15px">🔀</div>
            <div class="info"><b>${escapeHtml(w.name)}</b><small>${escapeHtml(typeName[w.documentTypeId] || '#' + w.documentTypeId)} · v${w.version}</small></div>
            ${w.isActive ? `<span class="pill approved">${t('active')}</span>` : ''}
        </div>`).join('');

    el('content').innerHTML = `<div class="wf-layout">
        <div><div class="section-title">${t('allWorkflows')}</div>${list}</div>
        <div id="wf-detail">${workflowDetail(selected, typeName)}</div>
    </div>`;
    loadWorkflowMetrics(selected.id);
}

function selectWorkflow(id) { S.wfSelected = id; renderWorkflows(); }

function workflowDetail(w, typeName) {
    const stages = [...w.stages].sort((a, b) => a.stageOrder - b.stageOrder);
    const stageHtml = stages.length ? stages.map((s, i) => stageItem(w, s, i, stages.length)).join('') : `<p class="muted">${t('noStagesYet')}</p>`;
    return `
        <div class="card">
            <div class="card-head">
                <div><h3>${escapeHtml(w.name)}</h3>
                    <div class="muted" style="font-size:12.5px">${escapeHtml(typeName[w.documentTypeId] || '#' + w.documentTypeId)} · ${t('version', { v: w.version })}</div></div>
                <div style="display:flex;gap:8px;align-items:center">
                    ${w.isActive ? `<span class="pill approved">${t('active')}</span>` : `<span class="pill cancelled">${t('draft')}</span>`}
                    <button class="btn btn-outline btn-sm" onclick="openRenameWorkflow(${w.id})">${t('rename')}</button>
                    ${w.isActive ? '' : `<button class="btn btn-primary btn-sm" onclick="activateWorkflow(${w.id})" ${stages.length ? '' : `disabled title="${t('needAStage')}"`}>${t('activate')}</button>`}
                </div>
            </div>
            <div class="card-pad">
                <div id="wf-metrics" class="stat-grid" style="margin-bottom:20px"><div class="muted">${t('loadingMetrics')}</div></div>
                <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:12px">
                    <div class="section-title" style="margin:0">${t('approvalStages', { n: stages.length })}</div>
                    <button class="btn btn-outline btn-sm" onclick="openAddStage(${w.id})">＋ ${t('addStageBtn')}</button>
                </div>
                ${stageHtml}
            </div>
        </div>`;
}

function stageApproverText(s) {
    if (s.approverType === 'Role') return `${t('kRole')}: ${S.roleById[s.approverRoleId]?.name || '#' + s.approverRoleId}`;
    if (s.approverType === 'Department') return `${t('kDept')}: ${S.deptById[s.approverDepartmentId]?.name || '#' + s.approverDepartmentId}`;
    if (s.approverType === 'User') return `${t('kUser')}: ${empName(s.approverEmployeeId)}`;
    return s.approverType;
}

function stageItem(w, s, idx, total) {
    return `<div class="stage-item">
        <div class="reorder-btns">
            <button onclick="moveStage(${w.id}, ${idx}, -1)" ${idx === 0 ? 'disabled' : ''}>▲</button>
            <button onclick="moveStage(${w.id}, ${idx}, 1)" ${idx === total - 1 ? 'disabled' : ''}>▼</button>
        </div>
        <div class="stage-num">${s.stageOrder}</div>
        <div class="stage-info"><b>${escapeHtml(s.name)}</b>
            <div class="sub">${escapeHtml(stageApproverText(s))}${s.slaHours ? ` · ${t('slaShort', { n: s.slaHours })}` : ''}</div></div>
        <div class="stage-actions">
            <button class="btn btn-ghost btn-icon" onclick="openEditStage(${w.id}, ${s.id})" title="${t('edit')}">✏️</button>
            <button class="btn btn-ghost btn-icon" onclick="deleteStage(${w.id}, ${s.id})" title="${t('del')}">🗑️</button>
        </div>
    </div>`;
}

async function loadWorkflowMetrics(id) {
    try {
        const m = await api('GET', `/api/dashboard/workflows/${id}/metrics`);
        const box = el('wf-metrics'); if (!box) return;
        const avg = m.averageCycleTimeHours != null ? `${m.averageCycleTimeHours.toFixed(1)}<small>h</small>` : '—';
        box.innerHTML =
            statTile(t('mTotal'), m.total, '') +
            statTile(t('mPending'), m.pending, 'accent-amber') +
            statTile(t('mApproved'), m.approved, 'accent-green') +
            statTile(t('mRejected'), m.rejected, 'accent-red') +
            statTile(t('mAvgCycle'), avg, '');
    } catch { const box = el('wf-metrics'); if (box) box.innerHTML = ''; }
}
function statTile(k, v, cls) { return `<div class="stat ${cls}"><div class="k">${k}</div><div class="v">${v}</div></div>`; }

/* ---- workflow / stage mutations ---- */
function openCreateWorkflow() {
    api('GET', '/api/document-types').then((types) => {
        if (!types.length) { toast(t('tMissingName'), t('tCreateDocTypeFirst'), 'error'); return; }
        const opts = types.map((ty) => `<option value="${ty.id}">${escapeHtml(ty.name)} (${escapeHtml(ty.code)})</option>`).join('');
        openModal(t('newWorkflow'), `
            <label class="field"><span>${t('documentTypeStar')}</span><select id="m-doctype">${opts}</select></label>
            <label class="field"><span>${t('workflowNameStar')}</span><input id="m-name" placeholder="Standard Leave Approval" /></label>
            <p class="muted" style="font-size:12.5px">${t('wfHint')}</p>`,
            `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
             <button class="btn btn-primary" onclick="submitModal(this)">${t('create')}</button>`,
            async () => {
                const documentTypeId = Number(el('m-doctype').value), name = el('m-name').value.trim();
                if (!name) { toast(t('tMissingName'), t('tGiveWfName'), 'error'); throw new Error('validation'); }
                const wf = await api('POST', '/api/workflows', { documentTypeId, name });
                S.wfSelected = wf.id;
                closeModal(); toast(t('tCreated'), t('tWfCreatedBody', { name }), 'success'); renderWorkflows();
            });
    });
}

function openRenameWorkflow(id) {
    api('GET', `/api/workflows/${id}`).then((w) => {
        openModal(t('renameWorkflow'), `<label class="field"><span>${t('nameStar')}</span><input id="m-name" value="${escapeHtml(w.name)}" /></label>`,
            `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
             <button class="btn btn-primary" onclick="submitModal(this)">${t('save')}</button>`,
            async () => {
                const name = el('m-name').value.trim();
                if (!name) { toast(t('tMissingName'), t('tNameRequired'), 'error'); throw new Error('validation'); }
                await api('PUT', `/api/workflows/${id}`, { name });
                closeModal(); toast(t('tRenamed'), '', 'success'); renderWorkflows();
            });
    });
}

async function activateWorkflow(id) {
    try { await api('POST', `/api/workflows/${id}/activate`); toast(t('tActivated'), t('tActivatedBody'), 'success'); renderWorkflows(); }
    catch (err) { toast(t('tCouldNotActivate'), err.message, 'error'); }
}

function stageFormHtml(s) {
    const type = s?.approverType || 'Role';
    const roleOpts = Object.values(S.roleById).map((r) => `<option value="${r.id}" ${s?.approverRoleId === r.id ? 'selected' : ''}>${escapeHtml(r.name)}</option>`).join('');
    const deptOpts = Object.values(S.deptById).map((d) => `<option value="${d.id}" ${s?.approverDepartmentId === d.id ? 'selected' : ''}>${escapeHtml(d.name)}</option>`).join('');
    const empOpts = Object.values(S.empById).map((e) => `<option value="${e.id}" ${s?.approverEmployeeId === e.id ? 'selected' : ''}>${escapeHtml(e.fullName)}</option>`).join('');
    return `
        <label class="field"><span>${t('stageNameStar')}</span><input id="m-name" value="${escapeHtml(s?.name || '')}" placeholder="Manager Approval" /></label>
        <label class="field"><span>${t('approverTypeStar')}</span>
            <select id="m-atype" onchange="onApproverTypeChange()">
                <option value="Role" ${type === 'Role' ? 'selected' : ''}>${t('optRole')}</option>
                <option value="Department" ${type === 'Department' ? 'selected' : ''}>${t('optDept')}</option>
                <option value="User" ${type === 'User' ? 'selected' : ''}>${t('optUser')}</option>
            </select></label>
        <label class="field" id="f-role"><span>${t('roleStar')}</span><select id="m-role">${roleOpts}</select></label>
        <label class="field" id="f-dept"><span>${t('departmentStar')}</span><select id="m-dept">${deptOpts}</select></label>
        <label class="field" id="f-emp"><span>${t('employeeStar')}</span><select id="m-emp">${empOpts}</select></label>
        <label class="field"><span>${t('slaHours')} <small class="muted">${t('slaHint')}</small></span>
            <input id="m-sla" type="number" min="1" value="${s?.slaHours ?? ''}" placeholder="${t('phSla')}" /></label>`;
}
function onApproverTypeChange() {
    const ty = el('m-atype').value;
    el('f-role').style.display = ty === 'Role' ? '' : 'none';
    el('f-dept').style.display = ty === 'Department' ? '' : 'none';
    el('f-emp').style.display = ty === 'User' ? '' : 'none';
}
function readStageForm() {
    const name = el('m-name').value.trim();
    if (!name) { toast(t('tMissingName'), t('tStageNameReq'), 'error'); return null; }
    const approverType = el('m-atype').value, slaRaw = el('m-sla').value.trim();
    return {
        approverType, name,
        roleId: approverType === 'Role' ? Number(el('m-role').value) : null,
        departmentId: approverType === 'Department' ? Number(el('m-dept').value) : null,
        employeeId: approverType === 'User' ? Number(el('m-emp').value) : null,
        slaHours: slaRaw ? Number(slaRaw) : null,
    };
}

function openAddStage(wfId) {
    openModal(t('addStageTitle'), stageFormHtml(),
        `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
         <button class="btn btn-primary" onclick="submitModal(this)">${t('addStageBtn')}</button>`,
        async () => {
            const body = readStageForm(); if (!body) throw new Error('validation');
            await api('POST', `/api/workflows/${wfId}/stages`, body);
            closeModal(); toast(t('tStageAdded'), '', 'success'); renderWorkflows();
        });
    onApproverTypeChange();
}

function openEditStage(wfId, stageId) {
    api('GET', `/api/workflows/${wfId}`).then((w) => {
        const s = w.stages.find((x) => x.id === stageId);
        openModal(t('editStageTitle'), stageFormHtml(s),
            `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
             <button class="btn btn-primary" onclick="submitModal(this)">${t('save')}</button>`,
            async () => {
                const body = readStageForm(); if (!body) throw new Error('validation');
                await api('PUT', `/api/workflows/${wfId}/stages/${stageId}`, body);
                closeModal(); toast(t('tStageUpdated'), '', 'success'); renderWorkflows();
            });
        onApproverTypeChange();
    });
}

function deleteStage(wfId, stageId) {
    openModal(t('deleteStageQ'), `<p style="margin:0">${t('deleteStageBody')}</p>`,
        `<button class="btn btn-ghost" data-close>${t('cancel')}</button>
         <button class="btn btn-danger" onclick="submitModal(this)">${t('del')}</button>`,
        async () => {
            await api('DELETE', `/api/workflows/${wfId}/stages/${stageId}`);
            closeModal(); toast(t('tStageDeleted'), '', 'success'); renderWorkflows();
        });
}

async function moveStage(wfId, idx, dir) {
    try {
        const w = await api('GET', `/api/workflows/${wfId}`);
        const stages = [...w.stages].sort((a, b) => a.stageOrder - b.stageOrder);
        const j = idx + dir;
        if (j < 0 || j >= stages.length) return;
        [stages[idx], stages[j]] = [stages[j], stages[idx]];
        await api('PUT', `/api/workflows/${wfId}/stages/reorder`, { orderedStageIds: stages.map((s) => s.id) });
        renderWorkflows();
    } catch (err) { toast(t('tCouldNotReorder'), err.message, 'error'); }
}

/* ============================================================
   login wiring + boot
   ============================================================ */
function renderDemoUsers() {
    el('demo-users').innerHTML = DEMO_USERS.map((u) => `
        <button class="demo-user" onclick="doLogin('${u.email}', DEMO_PASSWORD)">
            ${avatar(u.name)}
            <div class="demo-user-info"><b>${escapeHtml(u.name)}</b><small>${escapeHtml(t(u.roleKey))}</small></div>
            <span class="muted flip-x">→</span>
        </button>`).join('');
}

el('login-btn').addEventListener('click', () => {
    const email = el('login-email').value.trim();
    const password = el('login-password').value;
    if (!email) { el('login-error').textContent = t('errEnterEmail'); return; }
    if (!password) { el('login-error').textContent = t('errEnterPassword'); return; }
    doLogin(email, password);
});
el('login-email').addEventListener('keydown', (e) => { if (e.key === 'Enter') el('login-password').focus(); });
el('login-password').addEventListener('keydown', (e) => { if (e.key === 'Enter') el('login-btn').click(); });
el('logout-btn').addEventListener('click', logout);

async function boot() {
    // One-time backend override via ?api=<url> (empty value clears it).
    try {
        const q = new URLSearchParams(location.search).get('api');
        if (q !== null) {
            if (q) localStorage.setItem(API_BASE_KEY, q.replace(/\/+$/, ''));
            else localStorage.removeItem(API_BASE_KEY);
        }
    } catch { /* ignore */ }

    S.lang = localStorage.getItem(LANG_KEY) || 'en';
    applyLang();
    renderDemoUsers();
    const saved = localStorage.getItem(STORE_KEY);
    if (saved) {
        try {
            const s = JSON.parse(saved);
            S.token = s.token; S.me = s.me; S.roles = s.roles || []; S.name = s.name; S.email = s.email;
            renderUserChip();
            el('login-view').classList.add('hidden');
            el('app-view').classList.remove('hidden');
            await loadReferenceData();
            await refreshPending();
            go('inbox');
            return;
        } catch { logout(); }
    }
}
boot();
