# Sona Web App

React and Vite frontend for Sona, a Spotify playlist ordering workspace.

## Current State

The current development UI connects through the backend development OAuth
routes, lists the authenticated user's playlists, paginates results, and links
displayed Spotify content back to Spotify. Selecting a playlist loads tracks
into a page-scoped editor foundation with drag-and-drop reordering, moved-row
indication, unsaved-change notice, and reset for the loaded track page.

## MVP Direction

The editor will let a user select one playlist, inspect available track
metadata, reorder tracks manually or with simple rules, lock positions, review
the change, and save it back to Spotify. Analysis-based signals remain
optional; the core editor must work without audio-analysis data or playback.

Spotify playlist content remains transient editor data. Authentication tokens
are managed by the backend and must not be stored in the browser.

## Development

Run the backend first, then start the Vite development server:

```bash
pnpm install
pnpm dev
```

In development, Vite listens on `http://127.0.0.1:5173` and proxies
`/spotify` requests to `http://127.0.0.1:5000`. See the project
[README](../README.md) and
[Spotify integration notes](../docs/spotify.md) for backend setup and API
constraints.

## Checks

```bash
pnpm check
```

## Frontend Technical Debt

- The current editor state covers the loaded track page, not the full playlist.
  Before preview/save work, load all playlist items into one editor state and
  make pagination a view concern.
- Track row identity is currently frontend-generated from original position and
  available item data. Before save work, introduce an explicit editor row model
  for duplicate tracks, local tracks, unsupported items, and unavailable items.
- Selecting a different playlist can discard unsaved page edits. Add a guard
  before full-playlist editing.
- UI state is intentionally local React state for now. If editor interactions
  become hard to reason about, move only the editor session state into a small
  store rather than lifting all API state globally.
- Frontend tests are intentionally deferred until editor state stabilizes. Add
  Vitest coverage before save/reorder work.
