# Sona MVP Scope

[← Back to README](../README.md)

---

## Overview

**Sona** is a Spotify playlist ordering workspace.

Its purpose is to help users reorder playlists deliberately by showing useful track metadata, allowing controlled manual and rule-based ordering, preserving locked tracks, previewing changes, and saving the final order back to Spotify.

Sona is not a Spotify clone, recommendation engine, AI playlist generator, or professional DJ tool. The MVP focuses on safe playlist reordering and a clean technical foundation for working with Spotify as an external provider.

---

## Problem

Organizing a Spotify playlist can be a blind process.

Spotify users often want a playlist to flow better, but reordering tracks manually is tedious and lacks context. Users cannot easily compare adjacent tracks, lock important tracks in place, experiment with ordering rules, or preview the final change before saving.

Sona makes playlist ordering more controlled by giving users a dedicated workspace for inspecting, reordering, locking, auto-sorting, previewing, and saving playlist changes.

---

## Target User

Sona is built for Spotify users who care about playlist coherence.

This includes people who want their workout, dinner, focus, party, or personal playlists to feel less random and avoid jarring changes in mood, tempo, artist grouping, or energy.

The MVP is not aimed at professional DJs. It is aimed at regular Spotify users who want more control than Spotify's native playlist editor provides.

---

## Product Positioning

Sona should be positioned as:

> A safe playlist ordering workspace for Spotify, with optional audio-analysis enrichment when available.

It should not be positioned as:

> A Spotify BPM/key/energy sorter.

The reason is that Spotify audio-analysis and audio-features endpoints are
deprecated in the current Web API reference and access may also vary by app
mode or policy. Therefore, the MVP must provide value even when advanced
audio-analysis data is unavailable.

---

## Main User Flow

```text
Login with Spotify
→ View playlists
→ Select a playlist
→ Load playlist tracks
→ Inspect available metadata
→ Reorder manually or auto-sort
→ Lock important tracks
→ Preview ordering changes
→ Save reordered playlist back to Spotify
```

---

## MVP Goals

The MVP should prove that Sona can:

1. Authenticate a user through Spotify.
2. Load the user's Spotify playlists.
3. Load tracks from a selected playlist.
4. Display useful track metadata.
5. Allow manual reordering through drag and drop.
6. Allow tracks to be locked in place.
7. Auto-sort tracks using available metadata while respecting locks.
8. Optionally show compatibility signals when enough analysis data exists.
9. Preview the ordering change before saving.
10. Save the new order back to Spotify safely.
11. Keep Spotify tokens server-side.
12. Keep the core application independent from Spotify-specific analysis data.

---

## Included Features

### 1. Authentication

The MVP includes Spotify OAuth authentication.

#### Included

- Login with Spotify.
- OAuth2 Authorization Code Flow from the secure backend.
- Spotify callback handling.
- Session-based authentication in the application.
- Spotify access and refresh tokens stored server-side.
- Logout.

#### Not Included

- Email/password registration.
- Manual account creation.
- Password reset.
- Email verification.
- User profile management.
- Admin user management.

#### Notes

Spotify is the identity provider. The app may maintain a minimal local user record to associate Spotify identity, sessions, tokens, and future user-owned data.

---

### 2. Playlist Listing

The user can view their Spotify playlists.

#### Included

- Fetch playlists for the authenticated Spotify user.
- Display playlist name.
- Display playlist image when available.
- Display basic playlist metadata such as track count and owner when available.
- Select one playlist to edit.

#### Not Included

- Editing multiple playlists at the same time.
- Creating new playlists.
- Deleting playlists.
- Sharing playlists.
- Collaborative playlist management.

---

### 3. Playlist Track Loading

After selecting a playlist, the app loads its tracks into the editor.

#### Included

- Fetch playlist tracks from Spotify.
- Preserve original playlist order.
- Display tracks in an editable ordering workspace.
- Handle pagination if the playlist contains more tracks than one Spotify API response returns.
- Preserve enough data to compare the original order with the edited order.

#### Track Metadata

The MVP should display available Spotify metadata such as:

- Track title.
- Artist.
- Album.
- Duration.
- Explicit flag.
- Release date, if available.
- Album artwork, if available.
- Track restriction or playability state, if supplied by Spotify.

#### Optional Analysis Data

When an analysis provider is available, the app may display enriched data such as:

- BPM.
- Key.
- Energy.
- Valence.
- Danceability.
- Other compatibility-related signals.

The MVP must still work when this data is unavailable.

---

### 4. Playlist Editor

The playlist editor is the core screen of the MVP.

#### Included

- Display playlist tracks in current order.
- Drag and drop reordering.
- Track locking.
- Visual indication for locked tracks.
- Manual repositioning of unlocked tracks.
- Reset unsaved changes.
- Detect whether the order has changed.
- Prevent accidental save when there are no changes.

#### Locking Rules

A locked track should keep its fixed position during auto-sort operations.

Example:

```text
Original:
1. Track A
2. Track B [locked]
3. Track C
4. Track D

After auto-sort:
1. Track D
2. Track B [locked]
3. Track A
4. Track C
```

The exact algorithm can evolve, but the MVP must preserve locked positions when applying automatic ordering rules.

---

### 5. Auto-Sort

The app can automatically reorder tracks using available fields.

#### Included Sort Options

The MVP should support a small set of reliable sort modes:

- Sort by artist.
- Sort by album.
- Sort by release date, if available.
- Sort by duration.
- Sort by explicit/non-explicit flag.
- Sort by analysis field only if available.

#### Optional Sort Options

If analysis data exists, the app may support:

- Sort by BPM.
- Sort by key.
- Sort by energy.
- Sort by valence.

#### Requirements

- Auto-sort must respect locked tracks.
- Auto-sort must be reversible before saving.
- Auto-sort must not save automatically.
- The user must preview or confirm the final order before saving.

---

### 6. Compatibility Indicator

The app may show a compatibility indicator between adjacent tracks.

#### Included

- Show compatibility only when enough data exists.
- Avoid pretending precision when data is incomplete.
- Clearly distinguish available signals from unavailable signals.
- Prefer simple explainable indicators over opaque scoring.

#### Example

```text
Track A → Track B
Compatibility: Medium
Reason: similar tempo, different energy level
```

#### Important Constraint

Compatibility signals are optional enrichment. The MVP must not depend on Spotify audio-features or audio-analysis endpoints being available.

---

### 7. Change Preview / Diff

Before saving, the user should be able to review what changed.

#### Included

- Show that the playlist order has changed.
- Show moved track count.
- Show locked track count.
- Show whether locked tracks were preserved.
- Show unavailable/skipped tracks if applicable.
- Show original position and new position for moved tracks where practical.
- Ask for confirmation before saving.

#### Example

```text
Changes to apply:
- 24 tracks reordered
- 3 locked tracks preserved
- 0 unavailable tracks skipped
- Playlist snapshot before save: abc123
```

This is a core MVP feature because it makes the app feel safe rather than destructive.

---

### 8. Save Reordered Playlist

The app can save the edited order back to Spotify.

#### Included

- Apply reorder operations through Spotify's `PUT /playlists/{playlist_id}/items` endpoint.
- Supply the loaded `snapshot_id` when applying a reorder and retain each returned snapshot ID through a multi-operation save.
- Handle Spotify API errors and surface meaningful user feedback.
- Handle expired access tokens through refresh flow.
- Respect `Retry-After` and apply bounded exponential backoff for HTTP `429` responses.
- Show success state after save.
- Show error state when save fails.
- Keep before/after playlist snapshot IDs in transient save state and expose them where useful.

#### Snapshot Awareness

The app should track the playlist snapshot when the playlist is loaded and when it is saved.

The MVP does not need a full version history UI, but it should be aware that the playlist may have changed externally.

#### Required Scopes

- `playlist-read-private` for private playlist access.
- `playlist-modify-private` to save edits to private playlists.
- `playlist-modify-public` to save edits to public playlists.

Do not request collaborative-playlist access unless collaborative playlists are
included in an implemented editing flow.

#### Not Included

- Full restore UI.
- Multi-version history.
- Cross-device conflict resolution.
- Collaborative real-time editing.

---

## Technical Scope

### Backend Architecture

The backend should use a layered architecture.

#### Suggested Layers

```text
Domain
Application
Infrastructure
API
```

#### Domain Layer

The domain should focus on Sona's own concepts, not on cloning Spotify's data model.

Good domain concepts:

- OrderingSession.
- TrackPosition.
- LockedTrack.
- ReorderPlan.
- OrderingRule.
- CompatibilitySignal.
- SavePlan.

Avoid making the domain layer a direct copy of Spotify models such as:

- SpotifyPlaylist.
- SpotifyTrack.
- SpotifyArtist.
- SpotifyAlbum.

Spotify-specific DTOs should belong near the Infrastructure/API boundary.

---

### Provider Abstraction

The core app should not depend directly on Spotify audio-analysis data.

Use an abstraction for analysis data.

Example:

```csharp
public interface ITrackAnalysisProvider
{
    Task<IReadOnlyDictionary<string, TrackAnalysis>> GetAnalysisAsync(
        IReadOnlyCollection<TrackIdentity> tracks,
        CancellationToken cancellationToken);
}
```

Possible implementations:

```text
NullAnalysisProvider
PermittedExternalAnalysisProvider
```

For MVP, `NullAnalysisProvider` is valid. It ensures the app still works when
no permitted audio-analysis source is available. Do not build the MVP on
deprecated Spotify analysis endpoints or retain Spotify analysis data as a
cache.

---

### Spotify Integration

Spotify integration should be isolated in Infrastructure.

#### Included

- Spotify auth client.
- Spotify API client.
- Typed HttpClient usage.
- Token refresh handling.
- Playlist fetching.
- Playlist track fetching.
- Playlist reorder/save operation using `/playlists/{playlist_id}/items`.
- Bounded retry handling for rate limiting that honors `Retry-After`.
- Error mapping from Spotify responses to application errors.

#### Important

Controllers should not call Spotify directly.

Preferred flow:

```text
Controller
→ Application use case
→ Spotify provider/client abstraction
→ Infrastructure Spotify client
```

---

### Session and Token Handling

#### Included

- Session-based authentication.
- Spotify tokens stored server-side.
- Access token expiration handling.
- Refresh token usage.
- Logout clears local session.

#### Not Included

- Frontend-stored Spotify access tokens.
- Long-lived tokens in localStorage.
- Email/password auth.

---

### Persistence

The MVP may use persistence depending on implementation needs.

#### Minimal Persistence

- Local users linked to Spotify identity.
- Server-side token storage.

#### Suggested Tables

```text
Users
SpotifyTokens
```

The working order and loaded snapshot ID are transient editor/session state.
Do not retain Spotify content or playlist histories beyond what is needed to
complete the immediate editing workflow.

---

### Frontend

The frontend should focus on usability and clear state transitions.

#### Included

- Login screen.
- Playlist list.
- Playlist editor screen.
- Track table/list.
- Drag and drop interaction.
- Lock/unlock interaction.
- Sort controls.
- Optional compatibility indicators.
- Save preview/confirmation.
- Success and error states.

#### Important States

The UI should handle:

- Loading playlists.
- Loading tracks.
- Empty playlist.
- Missing analysis data.
- Unsaved changes.
- Save in progress.
- Save success.
- Save failure.
- Spotify auth expired.

---

## Out of Scope for MVP

The following are intentionally excluded from the MVP:

- Email/password registration.
- Full local account system.
- Multiple simultaneous playlist editing.
- Playlist sharing.
- Collaboration between users.
- AI playlist generation.
- Support for Apple Music, Tidal, YouTube Music, or other platforms.
- Full version history UI.
- Full undo/restore UI.
- Recommendation engine.
- Professional DJ transition tools.
- Beatmatching engine.
- Audio playback or Spotify Web Playback SDK integration.
- Mobile app.
- Native desktop app.
- Microservices.
- Kubernetes.
- Payment/billing.
- Admin dashboard.

---

## Explicit Non-Goals

Sona MVP is not trying to:

1. Replace Spotify's full client.
2. Recommend new music.
3. Generate playlists automatically from prompts.
4. Become a DJ mixing tool.
5. Build a social playlist platform.
6. Support every music provider.
7. Guarantee BPM/key/energy data for every track.
8. Depend on restricted Spotify audio-analysis endpoints.

---

## MVP Success Criteria

The MVP is successful when a user can:

1. Log in with Spotify.
2. Select one of their playlists.
3. See the playlist tracks in order.
4. Reorder tracks manually.
5. Lock selected tracks.
6. Apply an auto-sort while locks are preserved.
7. Preview the final change.
8. Save the new order back to Spotify.
9. Understand when analysis data is unavailable.
10. Complete the flow without exposing Spotify tokens to the frontend.

---

## Portfolio Value

This project demonstrates:

- OAuth2 integration.
- Secure server-side token handling.
- External API integration.
- Clean separation between application logic and provider-specific infrastructure.
- Playlist ordering domain logic.
- Drag and drop frontend behavior.
- Safe mutation workflow.
- Snapshot-aware saves.
- Error handling around third-party APIs.
- Practical Clean Architecture without over-engineering.

The project should be presented as a safe external-API mutation workflow, not just a Spotify API wrapper.

---

## Risks

### Spotify API Restrictions

Some Spotify Web API data, especially audio features, audio analysis, and recommendations, may be deprecated or restricted depending on app mode and Spotify policy.

Mitigation:

- Treat analysis data as optional.
- Build the MVP around ordering workflow.
- Use provider abstraction.
- Implement `NullAnalysisProvider`.
- Never make BPM/key/energy mandatory for core functionality.

---

### Over-Engineering

There is a risk of overbuilding the architecture for a small app.

Mitigation:

- Keep the domain focused on ordering sessions, locks, rules, and save plans.
- Avoid fake DDD around Spotify-owned entities.
- Avoid microservices.
- Avoid Kubernetes.
- Avoid unnecessary event sourcing.
- Build the smallest robust version first.

---

### Product Ambiguity

There is a risk that Sona looks like a generic playlist editor.

Mitigation:

- Emphasize controlled ordering.
- Emphasize locked tracks.
- Emphasize preview-before-save.
- Emphasize optional compatibility signals.
- Emphasize safe saving back to Spotify.

---

## Recommended MVP Build Order

### Phase 1: Authentication and Playlist Loading

- Spotify login.
- Spotify callback.
- Session creation.
- Logout.
- Fetch current user.
- Fetch playlists.
- Select playlist.
- Fetch playlist tracks.

### Phase 2: Editor Foundation

- Display tracks.
- Preserve original order.
- Drag and drop reorder.
- Detect unsaved changes.
- Reset changes.

### Phase 3: Locks and Sorting

- Lock/unlock tracks.
- Sort by reliable metadata.
- Preserve locked positions during sort.
- Show missing data states.

### Phase 4: Save Flow

- Preview ordering diff.
- Save reordered playlist to Spotify.
- Handle expired token.
- Handle Spotify API errors.
- Show success/error result.
- Track before/after snapshot IDs where available.

### Phase 5: Optional Enrichment

- Add analysis provider abstraction.
- Add `NullAnalysisProvider`.
- Add compatibility indicator when enough data exists.
- Add analysis-based sorting only when available.

---

## Final MVP Definition

Sona MVP is a Spotify-connected playlist ordering workspace that lets users load a playlist, inspect available metadata, reorder tracks manually or by simple rules, lock important tracks, preview the resulting order, and save the reordered playlist back to Spotify.

The MVP must work without advanced audio-analysis data. Analysis-based compatibility and sorting are optional enhancements, not core dependencies.
