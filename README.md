# Sona

> Reorder Spotify playlists with manual control, locked positions, and preview-before-save.

Sona is a safe playlist ordering workspace. It lets users inspect available
track metadata, reorder tracks manually or with simple rules, preserve locked
positions, review the change, and then save the new order back to Spotify.
Analysis-based compatibility signals are optional enrichment, not a dependency
of the MVP.

---

## Documentation

- [MVP — Features and scope](./docs/mvp.md)
- [Tech stack](./docs/stack.md)
- [Architecture](./docs/architecture.md)
- [API — Endpoints](./docs/api.md)
- [Spotify — Integration notes](./docs/spotify.md)
- [Technical debt](./TECHNICAL_DEBT.md)

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

# Start the frontend at http://127.0.0.1:5173
cd ../web-app
npm install
npm run dev
```

The current Spotify integration routes are development-only and store a single
connected account's tokens in memory for testing. The callback sets an
HTTP-only development session cookie and redirects back to the frontend. The
connection is lost when the backend process restarts, and the development
store must be replaced with encrypted per-user persistence before deployment.

### Checks

```bash
cd backend
dotnet test Sona.slnx

cd ../web-app
npm run lint
npm run build
```

Stop any running `Sona.Api` process before `dotnet test` if Windows reports
locked build output files.

### Future persisted configuration

```env
SPOTIFY_CLIENT_ID=
SPOTIFY_CLIENT_SECRET=
SPOTIFY_REDIRECT_URI=https://127.0.0.1:7001/spotify/callback
DATABASE_URL=postgresql://localhost:5432/sona
```

---

## Project status

In development: Spotify authentication and playlist listing are working for
local development and covered by backend tests. Playlist track loading and the
editor workflow are not implemented yet.

## Third-party branding

`web-app/public/spotify-full-logo-white.svg` is an official Spotify brand asset
used only for required content attribution. It remains subject to Spotify's
branding guidelines and applicable rights, and is not licensed under this
project's MIT license.
