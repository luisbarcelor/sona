# Sona Web App

React and Vite frontend for Sona, a Spotify playlist ordering workspace.

## Current State

The current development UI connects through the backend development OAuth
routes, lists the authenticated user's playlists, paginates results, and links
displayed Spotify content back to Spotify. Selecting a playlist loads tracks
through the backend editor endpoint into full-playlist editor state with
page-sized rendering, drag-and-drop reordering on the visible page, moved-row
indication, unsaved-change notice, reset, and guards for actions that would
discard local edits.

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
