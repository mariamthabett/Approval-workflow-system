/* ============================================================
   FlowApprove — runtime configuration
   ============================================================
   When the frontend is hosted SEPARATELY from the .NET backend
   (e.g. this page on Vercel, the API on Render/Railway/Azure),
   set API_BASE to your backend's base URL, for example:

       window.API_BASE = "https://your-app.onrender.com";

   Leave it as "" when the .NET app itself serves this page
   (same origin — local dev or a single combined deployment).

   You can also override this at runtime without redeploying:
     • append ?api=https://your-backend to the URL once, or
     • click the "⚙️ Backend URL" button on the login screen.
   ============================================================ */
window.API_BASE = "";
