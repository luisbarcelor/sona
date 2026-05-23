# Spotify — Integration notes

[← Back to README](../README.md)

---

## Credentials

Create your app at [developer.spotify.com](https://developer.spotify.com) and get your:

- `Client ID`
- `Client Secret`

Configure the **Redirect URI** in the dashboard:
```
https://localhost:7001/spotify/callback        # development
https://<production-host>/spotify/callback     # production
```

---

## OAuth2 — Authorization Code Flow

The flow Sona uses to authenticate users:

```
1. User opens /spotify/connect
2. Backend redirects to: https://accounts.spotify.com/authorize
   with client_id, redirect_uri, scope, state
3. User authorizes on Spotify
4. Spotify redirects to /spotify/callback with ?code=...
5. Backend exchanges the code for access_token + refresh_token
6. Current development implementation stores one connection in memory
7. Production implementation must store encrypted tokens for the authenticated app user
```

### Current scope

```
playlist-read-private
```

Additional future features must request scopes only when implemented:

```
playlist-read-collaborative
playlist-modify-public
playlist-modify-private
```

---

## Audio Features

Spotify exposes audio feature endpoints, but they are currently marked as deprecated in the Web API reference. Sona must treat Spotify audio features as an optional provider, not as a guaranteed dependency.

```
GET https://api.spotify.com/v1/audio-features/{id}
```

### Fields used in Sona

| Field | Type | Description |
|---|---|---|
| `tempo` | float | Track BPM |
| `key` | int | Musical key (0=C, 1=C#, 2=D... 11=B) |
| `mode` | int | 1=Major, 0=Minor |
| `energy` | float | Intensity and activity (0.0–1.0) |
| `valence` | float | Musical positivity (0.0–1.0) |
| `danceability` | float | How suitable for dancing (0.0–1.0) |

### Key to musical notation mapping

```
0=C  1=C#  2=D  3=D#  4=E  5=F
6=F# 7=G   8=G# 9=A  10=A# 11=B
```

---

## Preview URL

Each track may include a `preview_url` with a 30-second MP3 clip:

```json
{
  "preview_url": "https://p.scdn.co/mp3-preview/..."
}
```

Many tracks return `preview_url: null`. The app must handle this case by disabling the preview button when no URL is available.

---

## Development vs production mode

Spotify apps start in development mode. Access is limited to allowlisted users configured in the Spotify dashboard.

Before public release, check the current Spotify quota mode and policy requirements in the official dashboard and documentation.

---

## Spotify endpoints used

| Endpoint | Use |
|---|---|
| `GET /me/playlists` | List user playlists |
| `GET /playlists/{id}/items` | Future: get playlist items |
| `PUT /playlists/{id}/items` | Future: reorder playlist items |

---

[← API — Endpoints](./api.md)
