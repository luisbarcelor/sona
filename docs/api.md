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
Returns a playlist with all its tracks and their audio features.

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
      "previewUrl": "https://...",
      "audioFeatures": {
        "bpm": 128.0,
        "key": 5,
        "mode": 1,
        "energy": 0.85,
        "valence": 0.62,
        "danceability": 0.78
      }
    }
  ]
}
```

---

### `PUT /playlists/{id}/reorder`
Saves the new track order to Spotify.

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

**Response:** `200 OK`

---

## Notes

- All endpoints except `/auth/login` and `/auth/callback` require an active session.
- Spotify tokens are managed internally — the frontend never sees them directly.
- The `previewUrl` field can be `null` if Spotify has no preview available for that track.

---

[Spotify — Integration notes →](./spotify.md)
