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

- `GET /spotify/playlists/{playlistId}/editor` currently returns a minimal
  editor payload with playlist ID, loaded snapshot ID, total, and items. Before
  save work, expand the contract to include fuller playlist metadata,
  authoritative editor row occurrences, and save-preparation fields so the
  frontend does not assemble save-critical data from separate response shapes.
- Full editor loading currently reconstructs the playlist by fetching all
  Spotify item pages sequentially. This keeps save preparation simple, but it
  can make large playlists slow to open and increases `429` exposure. Add a
  clearer loading state, consider an MVP size warning, and keep the full load
  atomic so partial playlists are never editable.
- Editor row identity now has a frontend occurrence model, but it is still
  session-local. Before save work, move the authoritative row occurrence
  contract into the editable-playlist response so duplicate tracks, local
  tracks, unsupported items, and unavailable items are represented consistently
  across preview and save.
- The current editor guards playlist selection, playlist-page navigation, track
  refresh, and disconnect while dirty, but provider reconnect/connection-loss
  flows can still clear state without an explicit confirmation because the
  session is no longer valid. Revisit this once production session handling is
  introduced.
- Drag and drop is still page-local even though state is full-playlist scoped.
  Add an explicit cross-page move/reposition affordance if the MVP needs moving
  tracks across distant pages without repeated adjacent drags.
- Backend tests cover the editor endpoint happy path, auth/error behavior,
  empty and single-page playlists, unsupported item mapping, and partial
  full-load failure. Phase 2 remains open because frontend editor state and
  dirty-guard behavior still lack automated coverage. Add Vitest coverage for
  `usePlaylistEditor` and the shared unsaved-change guard, and defer Playwright
  E2E coverage until the full MVP workflow includes preview/save.
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
