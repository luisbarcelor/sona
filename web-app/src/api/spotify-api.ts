import type {
  SpotifyPlaylistItemPage,
  SpotifyPlaylistPage,
  SpotifyUserProfile,
} from '../types/spotify'

export const PLAYLIST_PAGE_SIZE = 12
export const TRACK_PAGE_SIZE = 50

type ApiError = {
  message?: string
}

export class ApiRequestError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}

export async function fetchPlaylists(requestedOffset: number, signal?: AbortSignal) {
  const response = await fetch(
    `/spotify/playlists?limit=${PLAYLIST_PAGE_SIZE}&offset=${requestedOffset}`,
    { signal },
  )
  const body = (await response.json().catch(() => null)) as SpotifyPlaylistPage | ApiError | null

  if (!response.ok) {
    throw new ApiRequestError(
      readApiMessage(body, 'No se han podido obtener tus playlists.'),
      response.status,
    )
  }

  return body as SpotifyPlaylistPage
}

export async function fetchCurrentUser(signal?: AbortSignal) {
  const response = await fetch('/spotify/me', { signal })
  const body = (await response.json().catch(() => null)) as SpotifyUserProfile | ApiError | null

  if (!response.ok) {
    throw new ApiRequestError(
      readApiMessage(body, 'No se ha podido obtener tu perfil de Spotify.'),
      response.status,
    )
  }

  return body as SpotifyUserProfile
}

export async function fetchPlaylistItems(
  playlistId: string,
  requestedOffset: number,
  signal?: AbortSignal,
) {
  const response = await fetch(
    `/spotify/playlists/${encodeURIComponent(playlistId)}/items?limit=${TRACK_PAGE_SIZE}&offset=${requestedOffset}`,
    { signal },
  )
  const body = (await response.json().catch(() => null)) as SpotifyPlaylistItemPage | ApiError | null

  if (!response.ok) {
    throw new ApiRequestError(
      readApiMessage(body, 'No se han podido obtener las canciones de la playlist.'),
      response.status,
    )
  }

  return body as SpotifyPlaylistItemPage
}

export async function deleteSpotifyConnection() {
  const response = await fetch('/spotify/connection', { method: 'DELETE' })

  if (!response.ok) {
    throw new Error('No se ha podido desconectar Spotify.')
  }
}

function readApiMessage(body: SpotifyPlaylistPage | SpotifyPlaylistItemPage | SpotifyUserProfile | ApiError | null, fallback: string) {
  return body && 'message' in body && body.message ? body.message : fallback
}
