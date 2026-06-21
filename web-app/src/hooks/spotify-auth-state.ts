import { ApiRequestError } from '../api/spotify-api'

export type InitialAuthState = {
  authMessage: string | null
  error: string | null
  needsConnection: boolean
  shouldCleanUrl: boolean
}

export function getInitialAuthState(): InitialAuthState {
  const callbackParams = new URLSearchParams(window.location.search)
  const spotifyStatus = callbackParams.get('spotify')
  const spotifyError = callbackParams.get('spotify_error')

  return {
    authMessage: spotifyError
      ? `Spotify no ha podido completar la conexión: ${spotifyError}`
      : spotifyStatus === 'connected'
        ? 'Cuenta de Spotify conectada.'
        : null,
    error: null,
    needsConnection: Boolean(spotifyError),
    shouldCleanUrl: Boolean(spotifyStatus || spotifyError),
  }
}

export function getReconnectMessage() {
  return 'La conexión con Spotify caducó o fue revocada. Vuelve a conectar tu cuenta.'
}

export function isConnectionFailure(error: unknown) {
  return error instanceof ApiRequestError && (error.status === 401 || error.status === 403)
}

export function isUnauthorized(error: unknown) {
  return error instanceof ApiRequestError && error.status === 401
}
