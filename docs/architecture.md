# Architecture

[← Back to README](../README.md)

---

## Overview

```
┌─────────────────┐         ┌─────────────────┐
│   web-app       │  HTTP   │   Sona.Api      │
│   React + Vite  │ ──────► │   ASP.NET Core  │
│   :5173         │         │   :5000         │
└─────────────────┘         └────────┬────────┘
                                     │
                                     ▼
                         ┌───────────────────────┐
                         │  Sona.Application     │
                         │  use cases + ports    │
                         └───────────┬───────────┘
                                     │
                                     ▼
                         ┌───────────────────────┐
                         │     Sona.Domain       │
                         │ entities + rules      │
                         └───────────────────────┘

External adapters live in Sona.Infrastructure:
Spotify Web API, optional analysis providers, PostgreSQL, and encrypted token storage.
```

---

## Backend

### Folder structure

```
backend/
├── Sona.slnx
├── Sona.Api/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   └── PlaylistController.cs
│   ├── Middleware/
│   ├── Program.cs
│   └── appsettings.json
├── Sona.Domain/
│   ├── Entities/
│   │   ├── OrderingSession.cs
│   │   └── ReorderPlan.cs
│   ├── ValueObjects/
│   │   ├── TrackIdentity.cs
│   │   ├── TrackAnalysis.cs
│   │   └── TrackPosition.cs
│   ├── Services/
│   │   └── OrderingService.cs
│   └── Exceptions/
├── Sona.Application/
│   ├── Abstractions/
│   │   ├── ITrackAnalysisProvider.cs
│   │   ├── IPlaylistGateway.cs
│   │   ├── ISessionRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Playlists/
│   │   ├── GetPlaylists/
│   │   ├── GetPlaylistDetails/
│   │   └── ReorderPlaylist/
│   ├── Auth/
│   │   ├── StartLogin/
│   │   ├── CompleteLogin/
│   │   └── Logout/
│   └── DTOs/
│       ├── PlaylistDto.cs
│       ├── TrackDto.cs
│       └── TrackAnalysisDto.cs
└── Sona.Infrastructure/
    ├── TrackAnalysis/
    │   └── NullAnalysisProvider.cs
    ├── Spotify/
    │   ├── SpotifyAuthClient.cs
    │   └── SpotifyPlaylistGateway.cs
    ├── Persistence/
    │   ├── SonaDbContext.cs
    │   └── Repositories/
    └── DependencyInjection.cs
```

### Layer responsibilities

| Layer | Responsibility |
|---|---|
| `Sona.Domain` | Core entities, value objects, invariants, ordering rules |
| `Sona.Application` | Use cases, ports, DTOs, transaction boundaries |
| `Sona.Infrastructure` | Spotify clients, optional analysis providers, EF Core, repositories |
| `Sona.Api` | Controllers, session middleware, HTTP configuration, dependency wiring |

### Project references

```
Sona.Api → Sona.Application
Sona.Api → Sona.Infrastructure
Sona.Infrastructure → Sona.Application
Sona.Infrastructure → Sona.Domain
Sona.Application → Sona.Domain
```

`Sona.Domain` has no dependencies on other projects or external packages. `Sona.Application` depends only on the domain and defines interfaces for infrastructure concerns. `Sona.Infrastructure` implements those interfaces.

### Domain rules

The domain layer owns behavior that should stay valid regardless of the UI or external APIs:

- Track order must contain the same tracks as the source playlist when saving a reorder.
- Locked tracks keep their fixed positions during auto-sort.
- Compatibility signals handle missing analysis explicitly and are optional.
- Provider identities are represented without making the domain a copy of Spotify response models.

### Application use cases

Application handlers orchestrate work without embedding infrastructure details:

```
StartSpotifyLogin
CompleteSpotifyLogin
GetCurrentUserPlaylists
GetPlaylistForEditing
PreviewPlaylistReorder
SavePlaylistReorder
```

Each use case depends on abstractions such as `IPlaylistGateway`,
`ITrackAnalysisProvider`, and `ISessionRepository`. A
`NullAnalysisProvider` keeps the ordering workflow functional without
analysis data.

---

## Frontend

### Folder structure

```
web-app/
├── src/
│   ├── api/
│   │   ├── auth.ts
│   │   └── playlists.ts
│   ├── components/
│   │   ├── PlaylistCard/
│   │   ├── TrackRow/
│   │   ├── LockToggle/
│   │   └── ChangePreview/
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
TanStack Query (server)             Zustand (local)
─────────────────────────           ───────────────────────
user playlists                      current track order
playlist items                      locked positions
optional analysis signals           loaded snapshot ID
```

---

## Database

### MVP tables

```sql
-- User sessions and Spotify tokens
user_sessions (
  id UUID PRIMARY KEY,
  spotify_id TEXT UNIQUE,
  access_token_encrypted TEXT,
  refresh_token_encrypted TEXT,
  token_expires_at TIMESTAMP,
  created_at TIMESTAMP
)
```

In the MVP, editor state is not persisted. The working order lives in the frontend and is saved to Spotify only when the user confirms.
The server stores credentials and session state, not Spotify playlist content
or change history. During a confirmed save it uses the loaded snapshot ID to
apply snapshot-aware reorder operations.

---

[API — Endpoints →](./api.md)
