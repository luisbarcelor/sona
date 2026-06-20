# Technical Debt

This file tracks intentional shortcuts and follow-up work that should not be
lost while the MVP is built.

## Phase 1 Carryover

- Development Spotify auth is single-connection and in-memory.
- Restarting the backend clears the Spotify connection.
- The development session cookie uses `Secure = false` because the local
  frontend runs over HTTP.
- There is no production app-user/session model yet.
- Spotify tokens are not encrypted or persisted yet.
- Tests cover controller/service behavior with fake HTTP handlers, not a fully
  hosted ASP.NET integration pipeline.
- CI is not configured yet for backend tests and frontend checks.
- Application services are intentionally feature-focused rather than
  fine-grained use cases while the app is small.
- Application response DTOs currently carry JSON property attributes to
  preserve the existing development API contract.
- `SpotifyOptions` currently lives in Application so API composition and
  Infrastructure adapters can share one configuration object; revisit when
  production auth/session configuration is introduced.
- Playlist loading currently returns provider-shaped response DTOs directly to
  the frontend. Introduce editor-specific contracts when reorder/save behavior
  needs stronger invariants.
- Spotify image collections are normalized from `null` to empty lists at the
  adapter boundary because real playlist responses can be looser than the
  documented schema.

## Later Cleanup

- Replace the development token store with encrypted per-user persistence.
- Revisit whether feature services should split into smaller use cases once
  playlist loading/editing expands.
- Use production cookie settings: `HttpOnly`, `Secure`, appropriate `SameSite`,
  explicit expiration, and server-side invalidation.
- Add hosted integration tests around routing, cookies, and middleware when the
  API surface grows.
