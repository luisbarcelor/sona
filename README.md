# Sona

> Organize Spotify playlists with clearer track flow, compatibility signals, and manual control.

Sona helps make playlist ordering less blind. It surfaces available track metadata and technical analysis so users can spot abrupt tempo changes, key clashes, or energy drops before saving a new order back to Spotify.

---

## Documentation

- [MVP — Features and scope](./docs/mvp.md)
- [Tech stack](./docs/stack.md)
- [Architecture](./docs/architecture.md)
- [API — Endpoints](./docs/api.md)
- [Spotify — Integration notes](./docs/spotify.md)

---

## Quick start

The implementation is being scaffolded. These requirements describe the intended local development environment for the MVP.

### Requirements

- .NET 10 SDK
- Node.js 24+
- Docker + docker-compose
- A [Spotify for Developers](https://developer.spotify.com) account

### Local backend setup

```bash
# Register this URI in the Spotify developer dashboard:
# https://127.0.0.1:7001/spotify/callback

dotnet user-secrets set --project backend/Sona.Api "Spotify:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set --project backend/Sona.Api "Spotify:ClientSecret" "YOUR_CLIENT_SECRET"

# Start the development backend
cd backend
dotnet run --project Sona.Api

# Start the frontend
cd ../web-app
npm install
npm run dev
```

The current Spotify integration routes are development-only and store a single
connected account's tokens in memory for testing. They must be replaced with
encrypted per-user persistence before deployment.

### Future persisted configuration

```env
SPOTIFY_CLIENT_ID=
SPOTIFY_CLIENT_SECRET=
SPOTIFY_REDIRECT_URI=https://127.0.0.1:7001/spotify/callback
DATABASE_URL=postgresql://localhost:5432/sona
```

---

## Project status

In development: MVP planning and scaffold.

## Third-party branding

`web-app/public/spotify-full-logo-white.svg` is an official Spotify brand asset
used only for required content attribution. It remains subject to Spotify's
branding guidelines and applicable rights, and is not licensed under this
project's MIT license.
