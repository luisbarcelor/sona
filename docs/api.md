# API — Endpoints

[← Back to README](../README.md)

---

## Base URL

```
http://localhost:5000
```

---

## Authentication

### `GET /auth/login`
Starts the OAuth2 flow with Spotify. Redirects the user to Spotify's authorization screen.

**Response:** `302 Redirect` → Spotify authorization URL

---

### `GET /auth/callback`
OAuth callback. Spotify redirects here with the authorization code. The backend exchanges it for an access token and refresh token.

**Query params:**
| Param | Type | Description |
|---|---|---|
| `code` | string | Spotify authorization code |
| `state` | string | State value for CSRF verification |

**Response:** `302 Redirect` → frontend with active session

---

### `POST /auth/logout`
Ends the user session.

**Response:** `200 OK`

---

## Playlists

### `GET /playlists`
Returns all playlists for the authenticated user.

**Response:**
```json
[
  {
    "id": "37i9dQZF1DX...",
    "name": "My playlist",
    "imageUrl": "https://...",
    "totalTracks": 24
  }
]
```

---

### `GET /playlists/{id}`
Returns a playlist with all its tracks and any available audio analysis.

**Params:**
| Param | Type | Description |
|---|---|---|
| `id` | string | Spotify playlist ID |

**Response:**
```json
{
  "id": "37i9dQZF1DX...",
  "name": "My playlist",
  "tracks": [
    {
      "id": "4uLU6hMCjMI...",
      "name": "Track name",
      "artist": "Artist",
      "albumImageUrl": "https://...",
      "spotifyUri": "spotify:track:4uLU6hMCjMI...",
      "previewUrl": null,
      "audioFeatures": {
        "bpm": 128.0,
        "key": 5,
        "mode": 1,
        "energy": 0.85,
        "valence": 0.62,
        "danceability": 0.78,
        "source": "spotify"
      }
    }
  ]
}
```

`audioFeatures` is nullable when no configured analysis provider can supply data for the track.

---

### `PUT /playlists/{id}/reorder`
Saves the new track order to Spotify after validating the requested order against the playlist contents.

**Params:**
| Param | Type | Description |
|---|---|---|
| `id` | string | Spotify playlist ID |

**Body:**
```json
{
  "trackIds": [
    "4uLU6hMCjMI...",
    "3n3Ppam7vgaVa...",
    "..."
  ]
}
```

**Response:**
```json
{
  "snapshotId": "MTAsZjY..."
}
```

---

## Notes

- All endpoints except `/auth/login` and `/auth/callback` require an active session.
- Spotify tokens are managed internally — the frontend never sees them directly.
- `previewUrl` and `audioFeatures` can be `null`.
- Reorder requests must contain each editable track exactly once.

---

[Spotify — Integration notes →](./spotify.md)
