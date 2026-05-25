# Tech stack

[← Back to README](../README.md)

---

## Summary

| Layer | Technology | Reason |
|---|---|---|
| Backend | .NET 10, ASP.NET Core | Current target runtime and mature web API stack |
| Architecture | Clean Architecture | Keeps domain rules independent from Spotify, EF Core, and HTTP |
| Database | PostgreSQL | Session and token persistence |
| ORM | Entity Framework Core | Typed data access |
| Frontend | Node.js 24+, React, TypeScript, Vite | Modern SPA toolchain with fast local feedback |
| Server state | TanStack Query | Caching and backend request management |
| Local state | Zustand | Editor session state (track order, locks) |
| Drag and drop | dnd-kit | Most solid DnD library for React |
| Styles | Tailwind CSS | Utility-first, consistent, fast |
| Auth | Spotify OAuth2 | Server-side Authorization Code Flow with refresh tokens |
| Infra | Docker + docker-compose | Local PostgreSQL and deployment |

---

## Technical decisions

### Why .NET 10
.NET 10 is the backend target for the MVP so the codebase starts on the current platform version instead of beginning with an immediate runtime upgrade. ASP.NET Core fits the API shape well: OAuth callbacks, session middleware, typed configuration, dependency injection, and clean project boundaries.

### Why Node.js 24+
The frontend targets Node.js 24+ to align local development and CI on a current runtime with modern package-manager and build-tool support. Keeping the minimum version explicit avoids debugging differences caused by older Node releases.

### Why Vite and not Next.js
The app is fully behind a login wall. With no public indexable content there's no need for SSR or SEO. Next.js would add complexity without providing anything useful. Vite is lighter and faster to set up for an authenticated SPA.

### Why full Clean Architecture for the MVP
The MVP depends on external services whose availability and policies can change. A separate domain layer keeps playlist ordering, locking, and compatibility rules independent from Spotify-specific DTOs, EF Core models, and HTTP controllers. The extra project is worth it because the core feature is domain behavior, not CRUD.

### Why Zustand and not Redux or Context API
- **Context API**: causes unnecessary re-renders for state that changes frequently, like drag and drop and lock toggles.
- **Redux**: too much boilerplate for a solo project. Redux Toolkit improves it but it's still more than needed.
- **Zustand**: minimal boilerplate, selective re-renders, entire store setup in one file.

### Separation of concerns in the frontend
- **TanStack Query**: server data (playlists, tracks, available audio analysis from the backend)
- **Zustand**: local editor session state (current track order, locked tracks)

They are complementary, not alternatives.

### Why PostgreSQL and Entity Framework Core
The MVP only needs durable server-side state for sessions and encrypted token
metadata. Working playlist order and snapshot IDs remain transient so Spotify
content is retained only for the immediate editing workflow. EF Core gives
typed migrations and repository implementations without leaking persistence
concerns into the domain layer.

### Why dnd-kit
Playlist editing depends on precise, accessible drag and drop behavior. dnd-kit provides composable primitives for sortable lists without forcing a large UI framework or owning the app's state model.

### Why Tailwind CSS
The frontend needs a compact editor UI with repeated rows, badges, controls, and responsive spacing. Tailwind keeps styling local to components and avoids introducing a heavier design system before the product patterns are proven.

### Why Docker Compose
Docker Compose is used for local infrastructure only. It gives contributors a repeatable PostgreSQL setup without requiring a machine-level database install.

### Why not the Spotify Playback SDK
The app organizes playlists; it does not provide playback. Playback scopes,
account requirements, and playback UI are outside the MVP.

---

[Architecture →](./architecture.md)
