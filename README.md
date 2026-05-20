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

### Planned local setup

```bash
# Configure environment variables
cp .env.example .env
# Fill in local Spotify app credentials

# Start infrastructure (PostgreSQL)
docker-compose up -d

# Start the backend
cd backend/Sona.Api
dotnet run

# Start the frontend
cd web-app
npm install
npm run dev
```

### Required environment variables

```env
SPOTIFY_CLIENT_ID=
SPOTIFY_CLIENT_SECRET=
SPOTIFY_REDIRECT_URI=http://localhost:5000/auth/callback
DATABASE_URL=postgresql://localhost:5432/sona
```

---

## Project status

In development: MVP planning and scaffold.
