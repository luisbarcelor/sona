# API — Endpoints

[← Back to README](../README.md)

---

## Base URL

```
https://localhost:7001
```

---

## Current Development Integration

The routes in this section exist only while the backend runs in the
`Development` environment. The current implementation keeps one Spotify
connection in memory to test the API integration.

### `GET /spotify/connect`
Starts the OAuth2 flow with Spotify. Redirects the user to Spotify's authorization screen.

**Response:** `302 Redirect` → Spotify authorization URL

---

### `GET /spotify/callback`
OAuth callback. Spotify redirects here with the authorization code. The backend exchanges it for an access token and refresh token.

**Query params:**
| Param | Type | Description |
|---|---|---|
| `code` | string | Spotify authorization code |
| `state` | string | State value for CSRF verification |

**Response:** `200 OK` → confirms the in-memory Spotify connection

---

## Playlists

### `GET /spotify/playlists`
Returns a page of playlists for the Spotify account connected during development.

**Query params:**
| Param | Type | Description |
|---|---|---|
| `limit` | integer | Page size, from 1 to 50 |
| `offset` | integer | Page offset, from 0 to 100000 |

**Response:**
```json
{
  "items": [
    {
      "id": "37i9dQZF1DX...",
      "name": "My playlist"
    }
  ],
  "limit": 20,
  "offset": 0,
  "total": 1
}
```

---

## Planned API

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

- The current `/spotify/*` routes are local-development integration tests, not production authentication.
- The production implementation will require an app session and encrypted per-user Spotify token persistence.
- Spotify tokens are managed server-side; the frontend must never receive the client secret.
- `previewUrl` and `audioFeatures` can be `null`.
- Reorder requests must contain each editable track exactly once.

---

[Spotify — Integration notes →](./spotify.md)
