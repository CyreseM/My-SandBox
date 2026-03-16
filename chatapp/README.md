# ChatApp — ASP.NET Core 8 BFF + React

A Telegram-style chat app with real-time messaging via SignalR, built on **ASP.NET Core 8** (backend) and **React 18** (frontend) with **SQLite** for zero-config storage.

## Features

- 🔐 Cookie-based authentication (register / login / logout)
- 💬 Direct messages, Group chats, Channels
- ⚡ Real-time via SignalR (messages, typing indicators, online presence, read receipts)
- 😄 Emoji reactions & message replies
- 📎 File & image uploads (local storage)
- 🔗 Invite links for groups/channels
- 🔔 Browser notifications when tab is hidden

## Prerequisites

| Tool | Version |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0+ |
| [Node.js](https://nodejs.org) | 18+ |
| npm | 9+ |

## Quick Start

### Option A — Two terminals (recommended for development)

**Terminal 1 — Backend:**
```bash
cd ChatApp.Api
dotnet restore
dotnet run
```
The API starts at `http://localhost:5000`. SQLite DB (`chatapp.db`) is created automatically on first run.

**Terminal 2 — Frontend:**
```bash
cd client
npm install
npm run dev
```
Open `http://localhost:5173` in your browser.

### Option B — Build frontend into backend (single server)

```bash
# Build frontend
cd client
npm install
npm run build

# Copy dist to backend wwwroot
cp -r dist/* ../ChatApp.Api/wwwroot/

# Run backend only
cd ../ChatApp.Api
dotnet run
```
Open `http://localhost:5000`.

## Project Structure

```
chatapp/
├── ChatApp.sln
├── ChatApp.Core/          ← Domain models (entities, enums)
├── ChatApp.Infrastructure/ ← AppDbContext (EF Core + SQLite)
├── ChatApp.Api/            ← ASP.NET Core 8 (controllers, hubs, services)
│   ├── Controllers/        ← Auth, Chats, Users, Statuses, Files
│   ├── Hubs/ChatHub.cs     ← SignalR hub
│   ├── Services/           ← Business logic
│   ├── DTOs/               ← Request/Response models
│   ├── Migrations/         ← EF Core SQLite migrations
│   └── Program.cs
└── client/                 ← React 18 + Vite frontend
    └── src/
        ├── components/     ← UI, chat, forms, shared
        ├── hooks/          ← SWR data hooks
        ← lib/              ← axios, signalR, mutations, toast
        ├── store/          ← Zustand realtime state
        └── pages/          ← Login, Register, JoinViaLink
```

## Configuration

Backend config lives in `ChatApp.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Data Source=chatapp.db"
  }
}
```

To change the SQLite file location, update the connection string.

## File Uploads

Files are stored in `ChatApp.Api/wwwroot/uploads/`. In production, replace `FileStorageService` with Azure Blob Storage or S3.

## Production Build

```bash
# Build frontend
cd client && npm run build && cp -r dist/* ../ChatApp.Api/wwwroot/

# Publish backend
cd ../ChatApp.Api && dotnet publish -c Release -o ./publish

# Run
cd publish && dotnet ChatApp.Api.dll
```
