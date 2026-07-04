# FlowApprove — Frontend

A **standalone static single-page app** (no build step, no framework) for the Approval Workflow API.
It's plain `index.html` + `styles.css` + `app.js`, bilingual (English / العربية) with RTL support.

It talks to the ASP.NET Core backend (in [`../MyProject`](../MyProject)) over HTTP. Because it's a
separate app, you tell it where the backend lives.

## Files
| File | Purpose |
|---|---|
| `index.html` | App shell (login, sidebar, screens, modal) |
| `styles.css` | Design system + RTL styles |
| `app.js` | API client, i18n, router, all screens |
| `config.js` | Sets `window.API_BASE` — the backend URL |
| `vercel.json` | Vercel static config |

## Point it at a backend
Set the backend base URL (leave empty only if the backend serves this page from the same origin):

- **Permanent:** edit `config.js` → `window.API_BASE = "https://your-backend.onrender.com";`
- **No redeploy:** click **⚙️ Backend URL** on the login screen, or open the site once with
  `?api=https://your-backend.onrender.com`.

## Run locally
It's just static files — serve the folder with anything:

```bash
# from the repo root
npx serve frontend           # or:  python -m http.server 5500 -d frontend
```

Then open the printed URL. For a locally-running backend (`dotnet run --project MyProject`,
listening on `http://localhost:5294`), click **⚙️ Backend URL** and enter `http://localhost:5294`
(CORS is enabled on the backend, so cross-origin calls work).

## Deploy to Vercel
New Project → import the repo → **Root Directory = `frontend`** → Framework Preset **Other** →
no build command → Deploy. Then set the backend URL as above. Full guide: [`../DEPLOYMENT.md`](../DEPLOYMENT.md).

## Demo users
`alice@example.com` (initiates leave) · `bob@example.com` (manager) · `carol@example.com` (dept head) ·
`dan@example.com` (HR) · `admin@example.com` (workflow admin).
