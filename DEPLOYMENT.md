# Deployment — Frontend on Vercel + Backend elsewhere

**النشر: الواجهة على Vercel + الخادم (.NET) على استضافة تانية.**

The frontend (`MyProject/wwwroot`) is a static site. The backend is an ASP.NET Core app.
**Vercel cannot run the .NET backend**, so it goes on a .NET-capable host, and the Vercel page
is pointed at it. CORS is already enabled on the backend, so cross-origin calls work.

Do the **backend first** (you need its URL for the frontend).

---

## Part A — Backend on Render (free, Docker)

A `Dockerfile` is included at the repo root. It runs on SQLite (schema + demo data created on boot).

1. Push this repo to GitHub (already at `github.com/mariamthabett/Approval-workflow-system`).
2. Go to **render.com → New → Web Service** and connect the repo.
3. Render detects the `Dockerfile` automatically. Settings:
   - **Runtime:** Docker
   - **Instance type:** Free
   - No build/start command needed — the Dockerfile handles it (it honors Render's `$PORT`).
4. **Create Web Service** and wait for the first deploy.
5. Copy the service URL, e.g. `https://approval-workflow-xxxx.onrender.com`.
6. Verify it's up: open `https://<your-url>/api/auth/login` — a `405`/JSON response means it's live
   (that route only accepts POST). The seeded demo login is `alice@example.com`, etc.

> Railway / Fly.io / Azure Container Apps work the same way — they all pick up the `Dockerfile`.

> **Note:** SQLite on a free host is *ephemeral* — data resets (and re-seeds) on each redeploy/restart.
> That's fine for a demo. For persistent data, set `Database__Provider=SqlServer` and a real
> `ConnectionStrings__Default` (env vars) pointing at a hosted SQL Server.

---

## Part B — Frontend on Vercel

1. Go to **vercel.com → Add New → Project** and import the same GitHub repo.
2. In **Configure Project**, set:
   - **Framework Preset:** Other
   - **Root Directory:** `MyProject/wwwroot`   ← *this is the key setting — it's why nothing showed before*
   - **Build Command:** (leave empty)
   - **Output Directory:** (leave empty)
3. **Deploy.** The login screen should now appear at your `*.vercel.app` URL.

### Point the frontend at your backend
The screens load, but they need the backend URL to log in / show data. Pick one:

- **Easiest (no redeploy):** on the login screen click **⚙️ Backend URL** and paste your Render URL, then Save.
  (Or open the site once as `https://<your-site>.vercel.app/?api=https://<your-backend>.onrender.com`.)
- **Permanent:** edit [`MyProject/wwwroot/config.js`](MyProject/wwwroot/config.js):
  ```js
  window.API_BASE = "https://approval-workflow-xxxx.onrender.com";
  ```
  Commit & push — Vercel redeploys and every visitor uses that backend automatically.

That's it — log in as `alice@example.com`, create & submit a leave request, then log in as
`bob@example.com` to approve it. The 🌐 button switches between العربية and English.

---

## Simpler alternative (one deployment, fully working)
Because the .NET app already serves the frontend from `wwwroot`, you can skip Vercel entirely and
just deploy the **backend** (Part A). Its URL then shows the screens *and* serves the API from one
origin — no `API_BASE`, no CORS to think about.

---

## Security notes for real use
- Change `Jwt:SigningKey` in `appsettings.json` (env var `Jwt__SigningKey`) to a long secret.
- Restrict CORS: set `Cors__AllowedOrigins__0=https://<your-site>.vercel.app` on the backend
  instead of the default allow-any-origin.
