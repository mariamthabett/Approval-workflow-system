# FlowApprove — Frontend (Next.js)

A **Next.js (App Router) + TypeScript + React** port of the approval-workflow frontend. Same screens,
same design, bilingual (English / العربية) with RTL. It's a client-side app that talks to the
ASP.NET Core backend (`../MyProject`) over REST + JWT.

> There's also a no-build vanilla version in [`../frontend`](../frontend). This folder is the Next.js one.

## Stack
- Next.js 15 (App Router), React 19, TypeScript
- No UI framework — the design system is hand-written CSS ([`app/globals.css`](app/globals.css))
- i18n dictionary auto-ported from the vanilla app ([`lib/i18n.ts`](lib/i18n.ts)) — 221 keys × 2 languages

## Point it at a backend
The backend URL is resolved in this order: `localStorage` override → `NEXT_PUBLIC_API_BASE` → same origin.

- **Build-time (Vercel):** set env var `NEXT_PUBLIC_API_BASE=https://your-app.onrender.com`.
- **Runtime (no rebuild):** click **⚙️ Backend URL** on the login screen, or open once with `?api=https://...`.

## Run locally
```bash
cd frontend-next
npm install
npm run dev            # http://localhost:3000
```
Run the backend too (`dotnet run --project ../MyProject` → http://localhost:5294), then set the
backend URL via the ⚙️ button (CORS is enabled server-side).

## Deploy to Vercel
New Project → import the repo → **Root Directory = `frontend-next`** → Framework Preset **Next.js**
(auto-detected) → add env var `NEXT_PUBLIC_API_BASE` → Deploy. Full guide: [`../DEPLOYMENT.md`](../DEPLOYMENT.md).

## Structure
```
app/            layout, page, globals.css
lib/            api client, i18n, types, formatters, drafts
components/      AppProvider (context), Sidebar, LoginScreen, ui atoms, modals
components/screens/   Inbox, MyDocuments, ApprovalDetail, SlaBreaches, DocTypes, Workflows
```
