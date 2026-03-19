# 🎨 CollabPaint

Real-time collaborative MS Paint–style web app.  
**React 18** (pure JS) · **ASP.NET Core 8** · **SignalR** · **TanStack Query** · **Zustand**

---

## Quick Start

### 1 — Backend

**Requires:** [.NET 8 SDK](https://dotnet.microsoft.com/download)

```bash
cd backend/CollabPaint.API

# First run only – create the SQLite database
dotnet ef migrations add InitialCreate
dotnet ef database update

# Start the API  (https://localhost:7001)
dotnet run
```

> If you don't have the EF tools yet:
> ```bash
> dotnet tool install --global dotnet-ef
> ```

### 2 — Frontend

**Requires:** Node.js 18+

```bash
cd frontend

npm install
npm run dev        # http://localhost:5173
```

### 3 — Try it

1. Open **http://localhost:5173** in two different browser windows (or one normal + one incognito).
2. Register two separate accounts.
3. In window A, create a canvas and open it.
4. In window A's **Invite** tab, search for the user from window B and send an invite.
5. Window B sees a toast — click **Accept**.
6. Both users now paint on the same canvas in real time! 🎉

---

## Project layout

```
collabpaint/
├── backend/CollabPaint.API/     ASP.NET Core 8 Web API
│   ├── Controllers/             Auth, Users, Sessions REST endpoints
│   ├── Hubs/PaintHub.cs         SignalR hub (strokes, cursors, invites)
│   ├── Models/                  EF Core entities
│   ├── Services/                TokenService, SessionService
│   ├── Data/AppDbContext.cs     EF Core + Identity
│   ├── DTOs/Dtos.cs             Request / response records
│   └── Program.cs               DI, JWT, CORS, SignalR wiring
│
└── frontend/                    React 18 (Vite, plain JS)
    └── src/
        ├── api/                 Axios wrappers
        ├── components/
        │   ├── Canvas/          DrawingCanvas, Toolbar
        │   └── Invite/          UserSearchPanel, InviteNotification
        ├── hooks/               useCanvas (drawing engine), useSignalR
        ├── pages/               Login, Register, Dashboard, Canvas
        └── store/useAppStore.js Zustand (auth + tool state)
```

---

## Drawing tools

| Tool | Key | Notes |
|------|-----|-------|
| Pen | P | Smooth bezier freehand |
| Eraser | E | Destination-out composite |
| Line | L | Click-drag |
| Rectangle | R | Outline or filled |
| Ellipse | O | Outline or filled |
| Fill | F | BFS flood-fill |
| Text | T | Click to place, Enter to commit |

---

## Deployment

**Backend → Azure App Service**
```bash
dotnet publish -c Release -o ./publish
# Upload ./publish; set env vars for JWT secret + connection string
```

**Frontend → Vercel / Netlify**
```bash
npm run build
# Deploy ./dist; set VITE_API_BASE_URL to your backend HTTPS URL
```

For multi-instance production, swap in **Azure SignalR Service**:
```csharp
builder.Services.AddSignalR().AddAzureSignalR(connectionString);
```
