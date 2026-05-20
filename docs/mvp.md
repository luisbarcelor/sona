# MVP — Features and scope

[← Back to README](../README.md)

---

## Problem

Organizing a Spotify playlist can be a blind process. Sona makes ordering decisions easier by showing available track metadata, compatibility signals, and a controlled way to reorder tracks before saving changes.

---

## Main flow

```
Login with Spotify
→ View your playlists
→ Select a playlist
→ View tracks with metadata and available analysis
→ Reorder manually or auto-sort
→ Lock key tracks in place
→ Save back to Spotify
```

---

## What's included

### Authentication
- Login with Spotify via OAuth2
- Logout

### Playlists
- View all user playlists
- Select a playlist to edit

### Playlist editor
- View tracks with metadata and available technical analysis such as BPM, key, energy, and valence
- Visual compatibility indicator between adjacent tracks when enough analysis data is available
- Drag and drop reordering
- Lock tracks to a fixed position
- Auto-sort by available fields while respecting locks
- Track preview when Spotify provides one
- Save new order back to Spotify

### Technical foundation
- Full Clean Architecture backend with Domain, Application, Infrastructure, and API layers
- Domain model for playlists, tracks, audio features, locks, and ordering rules
- Provider abstraction for audio analysis data so the core app is not tied to one external API
- Session-based authentication with Spotify tokens kept server-side

---

## Out of scope for MVP

The following will **not** be included in this version:

- Multiple simultaneous playlists
- Version history
- Playlist sharing
- Collaboration between users
- AI integration
- Support for other platforms (Apple Music, Tidal, etc.)

---

## Target user

Spotify users who care about playlist coherence — not necessarily professional DJs, but anyone who wants their workout, dinner, or work playlist to flow well without jarring drops or energy spikes.

---

[Tech stack →](./stack.md)
