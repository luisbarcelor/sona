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

## Phase 2 Carryover

- Playlist editor state is currently scoped to the loaded track page. Before
  save/reorder work, promote the editor to full-playlist state so dirty
  detection, reset, preview, and save operate on the complete playlist order.
- Track row identity is generated in the frontend from original position plus
  available Spotify item data. Before save/reorder work, introduce an explicit
  editor row contract that represents a specific playlist row occurrence,
  including duplicate tracks, local tracks, unsupported items, and unavailable
  items.
- The current editor disables track refresh and track pagination while the
  loaded page has unsaved local order changes, but selecting a different
  playlist can still discard local edits. Add a navigation/switch guard before
  the editor becomes full-playlist editing.
- Drag and drop uses `dnd-kit` without frontend automated tests yet. Add
  Vitest coverage for editor state after Phase 2 stabilizes, and defer
  Playwright E2E coverage until the full MVP workflow includes preview/save.
- Save is intentionally not implemented yet. Spotify reorder uses move
  operations with snapshot IDs, not a full-order replacement API, so Phase 4
  needs a dedicated reorder-plan algorithm and backend validation.

## Later Cleanup

- Replace the development token store with encrypted per-user persistence.
- Revisit whether feature services should split into smaller use cases once
  playlist loading/editing expands.
- Use production cookie settings: `HttpOnly`, `Secure`, appropriate `SameSite`,
  explicit expiration, and server-side invalidation.
- Add hosted integration tests around routing, cookies, and middleware when the
  API surface grows.
