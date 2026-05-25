# API — Endpoints

[← Back to README](../README.md)

---

## Base URL

```
https://127.0.0.1:7001
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
Returns an editable playlist view with its snapshot ID, track items, and any
available optional analysis. The backend obtains playlist items from Spotify
using `GET /playlists/{playlist_id}/items` and handles pagination.

**Params:**
| Param | Type | Description |
|---|---|---|
| `id` | string | Spotify playlist ID |

**Response:**
```json
{
  "id": "37i9dQZF1DX...",
  "name": "My playlist",
  "snapshotId": "abc123",
  "items": [
    {
      "uri": "spotify:track:4uLU6hMCjMI...",
      "name": "Track name",
      "artist": "Artist",
      "albumImageUrl": "https://...",
      "spotifyUrl": "https://open.spotify.com/track/4uLU6hMCjMI...",
      "analysis": {
        "bpm": 128.0,
        "key": 5,
        "energy": 0.85,
        "source": "configured-provider"
      }
    }
  ]
}
```

`analysis` is nullable when no configured provider can supply data. Spotify
content shown in the editor must include Spotify attribution and a link back
to the applicable Spotify content.

---

### `PUT /playlists/{id}/reorder`
Validates the edited order and applies reorder operations to Spotify using
`PUT /playlists/{playlist_id}/items`. Reordering uses the loaded Spotify
`snapshot_id` and the returned snapshot ID from each subsequent operation.

**Params:**
| Param | Type | Description |
|---|---|---|
| `id` | string | Spotify playlist ID |

**Body:**
```json
{
  "snapshotId": "abc123",
  "itemUris": [
    "spotify:track:4uLU6hMCjMI...",
    "spotify:track:3n3Ppam7vgaVa..."
  ],
  "lockedPositions": [1, 7]
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
- Optional analysis can be `null`; the editor cannot depend on it.
- Reorder requests must contain each editable item exactly once and preserve locked positions.
- A stale snapshot or insufficient modification scope must produce an actionable save error.
- Spotify `429` responses are retried only with bounded exponential backoff that respects `Retry-After`.
- The backend does not persist Spotify playlist content or editor history for the MVP.

---

[Spotify — Integration notes →](./spotify.md)
