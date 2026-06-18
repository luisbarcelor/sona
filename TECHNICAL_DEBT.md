# Technical Debt

This file tracks intentional shortcuts and follow-up work that should not be
lost while the MVP is built.

## Phase 1

- Development Spotify auth is single-connection and in-memory.
- Restarting the backend clears the Spotify connection.
- The development session cookie uses `Secure = false` because the local
  frontend runs over HTTP.
- Auth and playlist listing currently flow from `SpotifyController` directly
  to infrastructure services.
- There is no production app-user/session model yet.
- Spotify tokens are not encrypted or persisted yet.
- Tests cover controller/service behavior with fake HTTP handlers, not a fully
  hosted ASP.NET integration pipeline.
- CI is not configured yet for backend tests and frontend checks.

## Later Cleanup

- Replace the development token store with encrypted per-user persistence.
- Introduce application-layer use cases once playlist loading/editing expands.
- Use production cookie settings: `HttpOnly`, `Secure`, appropriate `SameSite`,
  explicit expiration, and server-side invalidation.
- Add hosted integration tests around routing, cookies, and middleware when the
  API surface grows.
