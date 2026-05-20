# Tech stack

[← Back to README](../README.md)

---

## Summary

| Layer | Technology | Reason |
|---|---|---|
| Backend | .NET 8, ASP.NET Core | Primary stack, Clean Architecture |
| Database | PostgreSQL | Session and token persistence |
| ORM | Entity Framework Core | Typed data access |
| Frontend | React + TypeScript + Vite | Authenticated SPA, no SSR/SEO needed |
| Server state | TanStack Query | Caching and backend request management |
| Local state | Zustand | Editor session state (track order, locks) |
| Drag and drop | dnd-kit | Most solid DnD library for React |
| Styles | Tailwind CSS | Utility-first, consistent, fast |
| Auth | Spotify OAuth2 | Authorization Code Flow |
| Infra | Docker + docker-compose | Local PostgreSQL and deployment |

---

## Technical decisions

### Why Vite and not Next.js
The app is fully behind a login wall. With no public indexable content there's no need for SSR or SEO. Next.js would add complexity without providing anything useful. Vite is lighter and faster to set up for an authenticated SPA.

### Why Zustand and not Redux or Context API
- **Context API**: causes unnecessary re-renders for state that changes frequently, like drag and drop and lock toggles.
- **Redux**: too much boilerplate for a solo project. Redux Toolkit improves it but it's still more than needed.
- **Zustand**: minimal boilerplate, selective re-renders, entire store setup in one file.

### Separation of concerns in the frontend
- **TanStack Query**: server data (playlists, tracks, audio features from the backend)
- **Zustand**: local editor session state (current track order, locked tracks)

They are complementary, not alternatives.

### Why not the Spotify Playback SDK
The app organizes playlists, it doesn't play music. The Playback SDK requires Premium on the user's account and adds unnecessary complexity. 30-second previews are served using the `preview_url` field from the Web API and a native HTML `<audio>` element.

---

[Architecture →](./architecture.md)
