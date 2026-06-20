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
                         │feature services+ports│
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

The backend now follows the Clean Architecture dependency rule. Controllers
depend on Application feature services, Application defines ports and DTOs,
and Infrastructure implements those ports for Spotify.

`Sona.Api` is still the composition root, so it references Infrastructure only
to register adapters. DDD-style domain behavior is intentionally deferred until
playlist ordering rules exist.

### Folder structure

```
backend/
├── Sona.slnx
├── Sona.Api/
│   ├── Controllers/
│   │   └── SpotifyController.cs
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
│   │   ├── ISpotifyConnectionGateway.cs
│   │   ├── ISpotifyPlaylistGateway.cs
│   │   └── ISpotifyProfileGateway.cs
│   ├── Spotify/
│   │   └── SpotifyAccountService.cs
│   ├── Auth/
│   │   └── SpotifyConnectionService.cs
│   ├── DTOs/
│   │   ├── CurrentUserProfileDto.cs
│   │   ├── ImageDto.cs
│   │   ├── PagedResponseDto.cs
│   │   ├── PlaylistDto.cs
│   │   └── PlaylistItemDto.cs
│   └── Configuration/
│       └── SpotifyOptions.cs
└── Sona.Infrastructure/
    ├── Spotify/
    │   ├── Api/
    │   │   ├── SpotifyClient.cs
    │   │   ├── SpotifyPlaylistGateway.cs
    │   │   └── SpotifyProfileGateway.cs
    │   ├── Authorization/
    │   │   ├── DevelopmentSpotifyTokenStore.cs
    │   │   ├── SpotifyAuthClient.cs
    │   │   └── SpotifyAuthorizationService.cs
    │   └── Models/
    │       ├── SpotifyCurrentUser.cs
    │       ├── SpotifyPagedResponse.cs
    │       ├── SpotifyPlaylist.cs
    │       └── SpotifyPlaylistItem.cs
    ├── Persistence/
    │   ├── SonaDbContext.cs
    │   └── Repositories/
    └── DependencyInjection.cs
```

### Layer responsibilities

| Layer | Responsibility |
|---|---|
| `Sona.Domain` | Core entities, value objects, invariants, ordering rules |
| `Sona.Application` | Feature-focused services, ports, DTOs, orchestration |
| `Sona.Infrastructure` | Spotify clients, optional analysis providers, EF Core, repositories |
| `Sona.Api` | Controllers, session middleware, HTTP configuration, dependency wiring |

### Project references

```
Sona.Api → Sona.Application
Sona.Api → Sona.Infrastructure (composition root only)
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

### Application Feature Services

The app is still young, so Application uses feature-focused services instead of
per-action use-case classes. These services orchestrate work without embedding
Infrastructure details:

```
SpotifyConnectionService
SpotifyAccountService
```

Feature services depend on Application ports such as
`ISpotifyConnectionGateway`, `ISpotifyProfileGateway`, and
`ISpotifyPlaylistGateway`. Infrastructure implements those ports with Spotify
adapters. A `NullAnalysisProvider` can be added later when optional analysis is
introduced.

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
