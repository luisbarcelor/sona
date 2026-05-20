# Sona

> Organize your Spotify playlists by BPM, key, and energy. Sort and fine-tune your music flow.

Sona solves a specific problem: organizing a Spotify playlist is a blind process. You can't tell if there's a jarring BPM jump between tracks, clashing keys, or an energy drop at the wrong moment. Sona turns those technical data points into visible, actionable decisions.

---

## Documentation

- [MVP — Features and scope](./docs/mvp.md)
- [Tech stack](./docs/stack.md)
- [Architecture](./docs/architecture.md)
- [API — Endpoints](./docs/api.md)
- [Spotify — Integration notes](./docs/spotify.md)

---

## Quick start

### Requirements

- .NET 8 SDK
- Node.js 20+
- Docker + docker-compose
- A [Spotify for Developers](https://developer.spotify.com) account

### Setup

```bash
# Clone the repository
git clone https://github.com/yourusername/sona
cd sona

# Configure environment variables
cp .env.example .env
# Edit .env with your Spotify Client ID and Client Secret

# Start infrastructure (PostgreSQL)
docker-compose up -d

# Start the backend
cd Sona.Api
dotnet run

# Start the frontend
cd sona-web
npm install
npm run dev
```

### Required environment variables

```env
SPOTIFY_CLIENT_ID=your_client_id
SPOTIFY_CLIENT_SECRET=your_client_secret
SPOTIFY_REDIRECT_URI=http://localhost:5000/auth/callback
DATABASE_URL=postgresql://localhost:5432/sona
```

---

## Project status

🚧 In development — MVP
