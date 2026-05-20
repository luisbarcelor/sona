# Architecture

[← Back to README](../README.md)

---

## Overview

```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│   sona-web      │  HTTP   │   Sona.Api      │  HTTP   │  Spotify API    │
│   React + Vite  │ ──────► │   .NET 8        │ ──────► │                 │
│   :5173         │         │   :5000         │         │                 │
└─────────────────┘         └────────┬────────┘         └─────────────────┘
                                     │
                                     │
                            ┌────────▼────────┐
                            │   PostgreSQL     │
                            │   :5432          │
                            └─────────────────┘
```

---

## Backend

### Folder structure

```
Sona/
├── Sona.sln
├── Sona.Api/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   └── PlaylistController.cs
│   ├── Middleware/
│   ├── Program.cs
│   └── appsettings.json
├── Sona.Application/
│   ├── Interfaces/
│   │   ├── ISpotifyService.cs
│   │   └── IPlaylistRepository.cs
│   ├── Services/
│   │   └── PlaylistService.cs
│   └── DTOs/
│       ├── PlaylistDto.cs
│       ├── TrackDto.cs
│       └── AudioFeaturesDto.cs
└── Sona.Infrastructure/
    ├── Spotify/
    │   └── SpotifyClient.cs
    ├── Persistence/
    │   ├── SonaDbContext.cs
    │   └── Repositories/
    └── DependencyInjection.cs
```

### Layer responsibilities

| Layer | Responsibility |
|---|---|
| `Sona.Api` | Controllers, endpoints, middleware, HTTP configuration |
| `Sona.Application` | Business logic, interfaces, DTOs |
| `Sona.Infrastructure` | Spotify HTTP client, EF Core, repositories |

### Project references

```
Sona.Api → Sona.Application
Sona.Api → Sona.Infrastructure
Sona.Infrastructure → Sona.Application
```

`Sona.Application` has no external dependencies — only interfaces that the other layers implement.

---

## Frontend

### Folder structure

```
sona-web/
├── src/
│   ├── api/
│   │   ├── auth.ts
│   │   └── playlists.ts
│   ├── components/
│   │   ├── PlaylistCard/
│   │   ├── TrackRow/
│   │   └── AudioBadge/
│   ├── pages/
│   │   ├── Home.tsx
│   │   ├── Playlists.tsx
│   │   └── Editor.tsx
│   ├── store/
│   │   └── playlistStore.ts
│   ├── types/
│   │   └── spotify.ts
│   └── main.tsx
├── index.html
├── vite.config.ts
└── tailwind.config.ts
```

### Application state

```
TanStack Query (server)          Zustand (local)
─────────────────────────        ───────────────────────
user playlists                   current track order
playlist tracks                  locked tracks
audio features per track         selected track
```

---

## Database

### MVP tables

```sql
-- User sessions and Spotify tokens
users (
  id UUID PRIMARY KEY,
  spotify_id TEXT UNIQUE,
  access_token TEXT,
  refresh_token TEXT,
  token_expires_at TIMESTAMP,
  created_at TIMESTAMP
)
```

In the MVP the editor state is not persisted — the order is saved directly to Spotify on confirm. No history or versioning.

---

[API — Endpoints →](./api.md)
