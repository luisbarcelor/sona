import { useCallback, useEffect, useState } from 'react'

const PAGE_SIZE = 12

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

type ApiError = {
  message?: string
}

type InitialAuthState = {
  authMessage: string | null
  error: string | null
  needsConnection: boolean
  shouldCleanUrl: boolean
}

class PlaylistRequestError extends Error {
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

    throw new PlaylistRequestError(message, response.status)
  }

  return body as SpotifyPlaylistPage
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

function App() {
  const [initialAuthState] = useState(getInitialAuthState)
  const [page, setPage] = useState<SpotifyPlaylistPage | null>(null)
  const [offset, setOffset] = useState(0)
  const [error, setError] = useState<string | null>(initialAuthState.error)
  const [authMessage, setAuthMessage] = useState<string | null>(initialAuthState.authMessage)
  const [needsConnection, setNeedsConnection] = useState(initialAuthState.needsConnection)
  const [isLoading, setIsLoading] = useState(true)

  const loadPlaylists = useCallback(async (requestedOffset: number) => {
    try {
      const body = await fetchPlaylists(requestedOffset)
      setNeedsConnection(false)
      setPage(body)
      setOffset(requestedOffset)
    } catch (loadError) {
      if (loadError instanceof PlaylistRequestError) {
        if (loadError.status === 401) {
          setNeedsConnection(true)
          setPage(null)
          setError(null)
          setAuthMessage(getReconnectMessage())
        } else {
          setNeedsConnection(false)
        }
      }

      setError(
        loadError instanceof PlaylistRequestError && loadError.status === 401
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

  async function disconnectSpotify() {
    setIsLoading(true)
    setError(null)

    try {
      const response = await fetch('/spotify/connection', { method: 'DELETE' })

      if (!response.ok) {
        throw new Error('No se ha podido desconectar Spotify.')
      }

      setPage(null)
      setOffset(0)
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
    void fetchPlaylists(0, controller.signal)
      .then((initialPage) => {
        setNeedsConnection(false)
        setPage(initialPage)
        setOffset(0)
      })
      .catch((loadError: unknown) => {
        if (loadError instanceof DOMException && loadError.name === 'AbortError') {
          return
        }

        if (loadError instanceof PlaylistRequestError && loadError.status === 401) {
          setNeedsConnection(true)
          setPage(null)
          setError(null)
          setAuthMessage(getReconnectMessage())
        }

        setError(
          loadError instanceof PlaylistRequestError && loadError.status === 401
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

  return (
    <main className="shell">
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
          Consulta las playlists de tu cuenta conectada y abre cualquiera directamente en
          Spotify.
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
                <article className="playlist-card" key={playlist.id}>
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
