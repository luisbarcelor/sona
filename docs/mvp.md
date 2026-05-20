# MVP — Features and scope

[← Back to README](../README.md)

---

## Problem

Organizing a Spotify playlist is a blind process. You can't tell if there's a jarring BPM jump between tracks, clashing keys, or an energy drop at the wrong moment. Sona turns those technical data points into visible, actionable decisions.

---

## Main flow

```
Login with Spotify
→ View your playlists
→ Select a playlist
→ View tracks with technical data
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
- View tracks with technical data: BPM, key, energy, valence
- Visual compatibility indicator between adjacent tracks (BPM jump, key compatibility)
- Drag and drop reordering
- Lock tracks to a fixed position
- Auto-sort by BPM, key, or energy while respecting locks
- 30-second track preview (when available)
- Save new order back to Spotify

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

Does not require a Spotify Premium account.

---

[Tech stack →](./stack.md)
