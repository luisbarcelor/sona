# Spotify — Integration notes

[← Back to README](../README.md)

---

## Credentials

Create your app at [developer.spotify.com](https://developer.spotify.com) and get your:

- `Client ID`
- `Client Secret`

Configure the **Redirect URI** in the dashboard:
```
https://127.0.0.1:7001/spotify/callback        # development
https://<production-host>/spotify/callback     # production
```

---

## OAuth2 — Authorization Code Flow

Sona has a secure backend and uses Spotify's Authorization Code Flow. The
client secret remains in backend configuration and is never sent to the SPA.

```
1. User opens /spotify/connect
2. Backend redirects to: https://accounts.spotify.com/authorize
   with client_id, redirect_uri, scope, state
3. User authorizes on Spotify
4. Spotify redirects to /spotify/callback with ?code=...
5. Backend exchanges the code for access_token + refresh_token
6. Backend stores one development connection in memory
7. Backend sets an HTTP-only development session cookie
8. Backend redirects the browser back to http://127.0.0.1:5173
9. Production implementation must store encrypted tokens for the authenticated app user
```

Access tokens must be refreshed server-side when they expire. Logout clears
the local session and any stored Spotify tokens for that session.

The current development cookie is named `sona_spotify_session`. It contains a
random local session identifier only; Spotify access and refresh tokens are not
sent to the browser. The cookie is lost as a useful session pointer when the
backend restarts because the matching token record is in memory only.

### Scopes

The currently implemented development playlist listing needs:

```
playlist-read-private
```

The MVP editing flow additionally needs the modification scope that corresponds
to playlists the user will save:

```
playlist-modify-public
playlist-modify-private
```

Do not request `playlist-read-collaborative` until editing collaborative
playlists is an implemented feature.

---

## MVP Web API Operations

The non-deprecated Spotify playlist item endpoints used by the MVP are:

| Endpoint | Use | Scope |
|---|---|---|
| `GET /me/playlists` | List the authenticated user's playlists | `playlist-read-private` for private playlists |
| `GET /playlists/{playlist_id}/items` | Load editable playlist items | `playlist-read-private` for private playlists |
| `PUT /playlists/{playlist_id}/items` | Reorder playlist items | `playlist-modify-public` or `playlist-modify-private` |

For `PUT /playlists/{playlist_id}/items`, reorder operations use
`range_start`, `insert_before`, optional `range_length`, and the loaded
`snapshot_id`. Spotify returns a new `snapshot_id`; use it for any following
operation and report the final snapshot after a successful save.

The editor is track-focused. It must detect unsupported or unavailable
playlist items returned by Spotify and avoid silently dropping them during a
save.

---

## Optional Analysis

Audio-derived metadata is not a Spotify dependency of the MVP. Spotify audio
features and audio analysis are deprecated in the current Web API reference,
so the ordering workflow must work with no analysis provider configured.

When a permitted provider is configured, analysis values may power optional
badges, compatibility explanations, and sort options. Missing values must be
shown as unavailable rather than inferred.

---

## Rate Limits and Errors

- Read Spotify's returned error message and map it to meaningful user feedback.
- Refresh expired access tokens server-side; do not ask the user to reconnect for routine token expiration.
- Clear the local development connection when Spotify returns `401`, because
  revoked or invalid credentials should move the UI back to reconnect state.
- On HTTP `429`, honor `Retry-After` and use bounded exponential backoff; never retry in a tight loop.
- Surface authorization failures when required playlist modification scopes have not been granted.
- Preserve the loaded `snapshot_id` through save so concurrent playlist edits do not become silent overwrites.

For the playlist item operations in this MVP, the reference documents these
response statuses:

| Status | Handling |
|---|---|
| `200` | Return loaded items or the new snapshot ID. |
| `401` | Refresh an expired access token once, or require reconnection when refresh is not possible. |
| `403` | Explain that the user cannot read or modify that playlist with the current ownership/collaboration and scopes. |
| `429` | Wait according to `Retry-After` and apply bounded backoff before retrying. |

---

## Content and Attribution

- Use Spotify content only as needed for the current playlist editing workflow; do not retain playlist or analysis data as a history or cache in the MVP.
- Accompany displayed Spotify metadata and artwork with Spotify attribution and links back to the applicable Spotify content.
- Do not alter Spotify artwork.
- Do not use Spotify content to train machine learning or AI models.

---

## Development vs Production Mode

Spotify apps start in development mode. Access is limited to allowlisted users configured in the Spotify dashboard.

Before public release, check the current Spotify quota mode and policy requirements in the official dashboard and documentation.

---

## Official References

- [Spotify Web API reference](https://developer.spotify.com/reference/web-api/open-api-schema.yaml)
- [Authorization Code Flow](https://developer.spotify.com/documentation/web-api/tutorials/code-flow)
- [Redirect URI requirements](https://developer.spotify.com/documentation/web-api/concepts/redirect_uri)
- [Scopes](https://developer.spotify.com/documentation/web-api/concepts/scopes)
- [Rate limits](https://developer.spotify.com/documentation/web-api/concepts/rate-limits)
- [Spotify Developer Terms](https://developer.spotify.com/terms)

---

[← API — Endpoints](./api.md)
