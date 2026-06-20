import { useCallback, useEffect, useState } from 'react'

const PAGE_SIZE = 12
const TRACK_PAGE_SIZE = 50

type SpotifyImage = {
  url: string
}

type SpotifyPlaylist = {
  id: string
  name: string
  description: string | null
  images: SpotifyImage[]
  owner: {
    display_name: string | null
  }
  items: {
    total: number
  }
  external_urls: {
    spotify: string
  } | null
}

type SpotifyPlaylistPage = {
  items: SpotifyPlaylist[]
  limit: number
  offset: number
  total: number
}

type SpotifyTrack = {
  id: string | null
  uri: string | null
  name: string
  type: string
  duration_ms: number | null
  explicit: boolean | null
  is_playable: boolean | null
  external_urls: {
    spotify: string
  } | null
  album: {
    name: string
    images: SpotifyImage[]
    release_date: string | null
  } | null
  artists: Array<{
    name: string
  }>
}

type SpotifyPlaylistItem = {
  added_at: string | null
  is_local: boolean
  item: SpotifyTrack | null
  unsupported_reason: string | null
}

type SpotifyPlaylistItemPage = {
  items: SpotifyPlaylistItem[]
  limit: number
  offset: number
  total: number
}

type SpotifyUserProfile = {
  id: string
  display_name: string | null
  images: SpotifyImage[]
}

type ApiError = {
  message?: string
}

type InitialAuthState = {
  authMessage: string | null
  error: string | null
  needsConnection: boolean
  shouldCleanUrl: boolean
}

class ApiRequestError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}

async function fetchPlaylists(requestedOffset: number, signal?: AbortSignal) {
  const response = await fetch(
    `/spotify/playlists?limit=${PAGE_SIZE}&offset=${requestedOffset}`,
    { signal },
  )
  const body = (await response.json().catch(() => null)) as SpotifyPlaylistPage | ApiError | null

  if (!response.ok) {
    const message =
      body && 'message' in body && body.message
        ? body.message
        : 'No se han podido obtener tus playlists.'

    throw new ApiRequestError(message, response.status)
  }

  return body as SpotifyPlaylistPage
}

async function fetchCurrentUser(signal?: AbortSignal) {
  const response = await fetch('/spotify/me', { signal })
  const body = (await response.json().catch(() => null)) as SpotifyUserProfile | ApiError | null

  if (!response.ok) {
    const message =
      body && 'message' in body && body.message
        ? body.message
        : 'No se ha podido obtener tu perfil de Spotify.'

    throw new ApiRequestError(message, response.status)
  }

  return body as SpotifyUserProfile
}

async function fetchPlaylistItems(
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
    const message =
      body && 'message' in body && body.message
        ? body.message
        : 'No se han podido obtener las canciones de la playlist.'

    throw new ApiRequestError(message, response.status)
  }

  return body as SpotifyPlaylistItemPage
}

function getInitialAuthState(): InitialAuthState {
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

function getReconnectMessage() {
  return 'La conexión con Spotify caducó o fue revocada. Vuelve a conectar tu cuenta.'
}

function isConnectionFailure(error: unknown) {
  return error instanceof ApiRequestError && (error.status === 401 || error.status === 403)
}

function isUnauthorized(error: unknown) {
  return error instanceof ApiRequestError && error.status === 401
}

function formatDuration(durationMs: number | null) {
  if (durationMs === null) {
    return '--:--'
  }

  const totalSeconds = Math.floor(durationMs / 1000)
  const minutes = Math.floor(totalSeconds / 60)
  const seconds = totalSeconds % 60

  return `${minutes}:${seconds.toString().padStart(2, '0')}`
}

function App() {
  const [initialAuthState] = useState(getInitialAuthState)
  const [page, setPage] = useState<SpotifyPlaylistPage | null>(null)
  const [profile, setProfile] = useState<SpotifyUserProfile | null>(null)
  const [selectedPlaylist, setSelectedPlaylist] = useState<SpotifyPlaylist | null>(null)
  const [trackPage, setTrackPage] = useState<SpotifyPlaylistItemPage | null>(null)
  const [offset, setOffset] = useState(0)
  const [trackOffset, setTrackOffset] = useState(0)
  const [error, setError] = useState<string | null>(initialAuthState.error)
  const [trackError, setTrackError] = useState<string | null>(null)
  const [authMessage, setAuthMessage] = useState<string | null>(initialAuthState.authMessage)
  const [needsConnection, setNeedsConnection] = useState(initialAuthState.needsConnection)
  const [isLoading, setIsLoading] = useState(true)
  const [isTracksLoading, setIsTracksLoading] = useState(false)

  const loadPlaylists = useCallback(async (requestedOffset: number) => {
    try {
      const body = await fetchPlaylists(requestedOffset)
      setNeedsConnection(false)
      setPage(body)
      setOffset(requestedOffset)
    } catch (loadError) {
      if (loadError instanceof ApiRequestError) {
        if (isConnectionFailure(loadError)) {
          setNeedsConnection(true)
          setPage(null)
          setProfile(null)
          setSelectedPlaylist(null)
          setTrackPage(null)
          setError(null)
          setAuthMessage(getReconnectMessage())
        } else {
          setNeedsConnection(false)
        }
      }

      setError(
        isConnectionFailure(loadError)
          ? null
          : loadError instanceof Error
            ? loadError.message
            : 'No se han podido obtener tus playlists.',
      )
    } finally {
      setIsLoading(false)
    }
  }, [])

  function requestPlaylists(requestedOffset: number) {
    setIsLoading(true)
    setError(null)
    void loadPlaylists(requestedOffset)
  }

  async function requestPlaylistItems(playlist: SpotifyPlaylist, requestedOffset: number) {
    setSelectedPlaylist(playlist)
    setTrackPage(null)
    setTrackOffset(requestedOffset)
    setIsTracksLoading(true)
    setTrackError(null)

    try {
      const body = await fetchPlaylistItems(playlist.id, requestedOffset)
      setNeedsConnection(false)
      setTrackPage(body)
    } catch (loadError) {
      if (isUnauthorized(loadError)) {
        setNeedsConnection(true)
        setPage(null)
        setProfile(null)
        setSelectedPlaylist(null)
        setTrackPage(null)
        setError(null)
        setAuthMessage(getReconnectMessage())
        return
      }

      setTrackPage(null)
      setTrackError(
        loadError instanceof Error
          ? loadError.message
          : 'No se han podido obtener las canciones de la playlist.',
      )
    } finally {
      setIsTracksLoading(false)
    }
  }

  async function disconnectSpotify() {
    setIsLoading(true)
    setError(null)

    try {
      const response = await fetch('/spotify/connection', { method: 'DELETE' })

      if (!response.ok) {
        throw new Error('No se ha podido desconectar Spotify.')
      }

      setPage(null)
      setProfile(null)
      setSelectedPlaylist(null)
      setTrackPage(null)
      setOffset(0)
      setTrackOffset(0)
      setNeedsConnection(true)
      setAuthMessage('Cuenta de Spotify desconectada.')
    } catch (disconnectError) {
      setError(
        disconnectError instanceof Error
          ? disconnectError.message
          : 'No se ha podido desconectar Spotify.',
      )
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    if (initialAuthState.shouldCleanUrl) {
      window.history.replaceState(null, '', window.location.pathname)
    }

    const controller = new AbortController()
    void Promise.allSettled([
      fetchCurrentUser(controller.signal),
      fetchPlaylists(0, controller.signal),
    ])
      .then(([profileResult, playlistsResult]) => {
        const failedResult = [profileResult, playlistsResult].find(
          (result) => result.status === 'rejected',
        )

        if (failedResult?.status === 'rejected' && isConnectionFailure(failedResult.reason)) {
          setNeedsConnection(true)
          setPage(null)
          setProfile(null)
          setSelectedPlaylist(null)
          setTrackPage(null)
          setError(null)
          setAuthMessage(getReconnectMessage())
          return
        }

        if (profileResult.status === 'fulfilled') {
          setProfile(profileResult.value)
        }

        if (playlistsResult.status !== 'fulfilled') {
          throw playlistsResult.reason
        }

        setNeedsConnection(false)
        setPage(playlistsResult.value)
        setOffset(0)

        if (profileResult.status === 'rejected') {
          setError(
            profileResult.reason instanceof Error
              ? profileResult.reason.message
              : 'No se ha podido obtener tu perfil de Spotify.',
          )
        }
      })
      .catch((loadError: unknown) => {
        if (loadError instanceof DOMException && loadError.name === 'AbortError') {
          return
        }

        if (isConnectionFailure(loadError)) {
          setNeedsConnection(true)
          setPage(null)
          setProfile(null)
          setSelectedPlaylist(null)
          setTrackPage(null)
          setError(null)
          setAuthMessage(getReconnectMessage())
        }

        setError(
          isConnectionFailure(loadError)
            ? null
            : loadError instanceof Error
              ? loadError.message
              : 'No se han podido obtener tus playlists.',
        )
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setIsLoading(false)
        }
      })

    return () => controller.abort()
  }, [initialAuthState.shouldCleanUrl])

  const startItem = page && page.total > 0 ? page.offset + 1 : 0
  const endItem = page ? Math.min(page.offset + page.items.length, page.total) : 0
  const canGoBack = offset > 0 && !isLoading
  const canGoForward = Boolean(page && offset + PAGE_SIZE < page.total && !isLoading)
  const trackStartItem = trackPage && trackPage.total > 0 ? trackPage.offset + 1 : 0
  const trackEndItem = trackPage ? Math.min(trackPage.offset + trackPage.items.length, trackPage.total) : 0
  const canGoBackTracks = Boolean(selectedPlaylist && trackOffset > 0 && !isTracksLoading)
  const canGoForwardTracks = Boolean(
    selectedPlaylist && trackPage && trackOffset + TRACK_PAGE_SIZE < trackPage.total && !isTracksLoading,
  )
  const profileName = profile ? profile.display_name ?? profile.id : ''

  return (
    <main className="shell">
      {profile && (
        <aside className="account-chip" aria-label="Perfil de Spotify conectado">
          {profile.images[0]?.url ? (
            <img src={profile.images[0].url} alt="" />
          ) : (
            <span aria-hidden="true">{profileName?.slice(0, 1).toUpperCase()}</span>
          )}
          <div>
            <p>Conectado como</p>
            <strong>{profileName}</strong>
          </div>
        </aside>
      )}

      <header className="hero">
        <div className="brand">
          <span className="brand-mark" aria-hidden="true">
            S
          </span>
          <span>Sona</span>
        </div>
        <p className="eyebrow">Spotify playlists</p>
        <h1>Tu música, lista para organizar.</h1>
        <p className="subtitle">
          Consulta tus playlists, selecciona una y carga sus canciones en el orden actual
          de Spotify.
        </p>
      </header>

      <section className="library" aria-label="Playlists de Spotify">
        <div className="library-header">
          <div>
            <p className="section-label">Biblioteca</p>
            <h2>Tus playlists</h2>
          </div>
          <button
            className="secondary-button"
            type="button"
            onClick={() => requestPlaylists(offset)}
            disabled={isLoading}
          >
            {isLoading ? 'Cargando...' : 'Actualizar'}
          </button>
        </div>

        {needsConnection && (
          <div className="notice connection">
            <div>
              <h3>Conecta tu cuenta de Spotify</h3>
              <p>
                {authMessage ??
                  'Inicia sesión con Spotify. Volverás automáticamente a Sona cuando la conexión esté lista.'}
              </p>
            </div>
            <a className="spotify-button" href="/spotify/connect">
              Conectar Spotify
            </a>
          </div>
        )}

        {authMessage && !needsConnection && (
          <div className="notice success" role="status">
            <p>{authMessage}</p>
            {page && (
              <button type="button" onClick={() => setAuthMessage(null)}>
                Cerrar
              </button>
            )}
          </div>
        )}

        {error && !needsConnection && (
          <div className="notice error" role="alert">
            <p>{error}</p>
            <button type="button" onClick={() => requestPlaylists(offset)}>
              Reintentar
            </button>
          </div>
        )}

        {isLoading && !page && !needsConnection && (
          <div className="loading" aria-live="polite">
            Cargando playlists...
          </div>
        )}

        {page && page.items.length === 0 && !error && (
          <div className="empty">
            <h3>No hay playlists disponibles</h3>
            <p>Spotify no ha devuelto ninguna playlist para esta cuenta.</p>
          </div>
        )}

        {page && page.items.length > 0 && (
          <>
            <div className="results-meta" aria-live="polite">
              Mostrando {startItem}-{endItem} de {page.total}
            </div>
            <div className="connection-actions">
              <button type="button" onClick={() => void disconnectSpotify()} disabled={isLoading}>
                Desconectar Spotify
              </button>
            </div>
            <div className="playlist-grid">
              {page.items.map((playlist) => (
                <article
                  className={`playlist-card${selectedPlaylist?.id === playlist.id ? ' selected' : ''}`}
                  key={playlist.id}
                >
                  {playlist.images[0]?.url ? (
                    <img src={playlist.images[0].url} alt="" loading="lazy" />
                  ) : (
                    <div className="cover-placeholder" aria-hidden="true">
                      ♪
                    </div>
                  )}
                  <div className="playlist-info">
                    <h3>{playlist.name}</h3>
                    <p className="owner">De {playlist.owner.display_name ?? 'Spotify'}</p>
                    <p className="tracks">{playlist.items.total} canciones</p>
                    <button
                      className="select-playlist"
                      type="button"
                      aria-pressed={selectedPlaylist?.id === playlist.id}
                      disabled={isTracksLoading && selectedPlaylist?.id === playlist.id}
                      onClick={() => void requestPlaylistItems(playlist, 0)}
                    >
                      {selectedPlaylist?.id === playlist.id ? 'Seleccionada' : 'Seleccionar'}
                    </button>
                    {playlist.external_urls?.spotify && (
                      <a
                        className="spotify-link"
                        href={playlist.external_urls.spotify}
                        target="_blank"
                        rel="noreferrer"
                      >
                        Abrir en Spotify
                      </a>
                    )}
                  </div>
                </article>
              ))}
            </div>
            <nav className="pagination" aria-label="Paginación de playlists">
              <button
                type="button"
                disabled={!canGoBack}
                onClick={() => requestPlaylists(Math.max(0, offset - PAGE_SIZE))}
              >
                Anterior
              </button>
              <button
                type="button"
                disabled={!canGoForward}
                onClick={() => requestPlaylists(offset + PAGE_SIZE)}
              >
                Siguiente
              </button>
            </nav>

            {selectedPlaylist && (
              <section className="track-panel" aria-label="Canciones de la playlist seleccionada">
                <div className="track-panel-header">
                  <div>
                    <p className="section-label">Playlist seleccionada</p>
                    <h2>{selectedPlaylist.name}</h2>
                    {trackPage && (
                      <p>
                        Mostrando {trackStartItem}-{trackEndItem} de {trackPage.total}
                      </p>
                    )}
                  </div>
                  <button
                    type="button"
                    onClick={() => void requestPlaylistItems(selectedPlaylist, trackOffset)}
                    disabled={isTracksLoading}
                  >
                    {isTracksLoading ? 'Cargando...' : 'Actualizar canciones'}
                  </button>
                </div>

                {trackError && (
                  <div className="notice error" role="alert">
                    <p>{trackError}</p>
                    <button
                      type="button"
                      onClick={() => void requestPlaylistItems(selectedPlaylist, trackOffset)}
                    >
                      Reintentar
                    </button>
                  </div>
                )}

                {isTracksLoading && !trackPage && !trackError && (
                  <div className="loading" aria-live="polite">
                    Cargando canciones...
                  </div>
                )}

                {trackPage && trackPage.items.length === 0 && !trackError && (
                  <div className="empty">
                    <h3>Playlist vacía</h3>
                    <p>Spotify no ha devuelto canciones para esta playlist.</p>
                  </div>
                )}

                {trackPage && trackPage.items.length > 0 && (
                  <>
                    <ol className="track-list">
                      {trackPage.items.map((playlistItem, index) => {
                        const track = playlistItem.item
                        const artists = track?.artists.length
                          ? track.artists.map((artist) => artist.name).join(', ')
                          : 'Artista desconocido'

                        return (
                          <li className="track-row" key={`${track?.uri ?? 'unsupported'}-${index}`}>
                            <span className="track-position">{trackPage.offset + index + 1}</span>
                            {track?.album?.images[0]?.url ? (
                              <img src={track.album.images[0].url} alt="" loading="lazy" />
                            ) : (
                              <div className="track-cover-placeholder" aria-hidden="true">
                                ♪
                              </div>
                            )}
                            <div className="track-copy">
                              <strong>{track?.name ?? 'Elemento no compatible'}</strong>
                              <span>
                                {track
                                  ? `${artists} · ${track.album?.name ?? 'Álbum desconocido'}`
                                  : playlistItem.unsupported_reason}
                              </span>
                            </div>
                            {playlistItem.is_local && <span className="track-badge">Local</span>}
                            {track?.explicit && <span className="track-badge">Explicit</span>}
                            <span className="track-duration">
                              {formatDuration(track?.duration_ms ?? null)}
                            </span>
                          </li>
                        )
                      })}
                    </ol>
                    <nav className="pagination" aria-label="Paginación de canciones">
                      <button
                        type="button"
                        disabled={!canGoBackTracks}
                        onClick={() =>
                          void requestPlaylistItems(
                            selectedPlaylist,
                            Math.max(0, trackOffset - TRACK_PAGE_SIZE),
                          )
                        }
                      >
                        Anterior
                      </button>
                      <button
                        type="button"
                        disabled={!canGoForwardTracks}
                        onClick={() =>
                          void requestPlaylistItems(selectedPlaylist, trackOffset + TRACK_PAGE_SIZE)
                        }
                      >
                        Siguiente
                      </button>
                    </nav>
                  </>
                )}
              </section>
            )}
          </>
        )}
      </section>

      <footer className="attribution">
        <a href="https://open.spotify.com" target="_blank" rel="noreferrer">
          <img src="/spotify-full-logo-white.svg" alt="Spotify" />
        </a>
      </footer>
    </main>
  )
}

export default App
