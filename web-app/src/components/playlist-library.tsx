import { PLAYLIST_PAGE_SIZE, TRACK_PAGE_SIZE } from '../api/spotify-api'
import { buttonBase, sectionLabel } from '../lib/styles'
import type { SpotifyLibraryActions, SpotifyLibraryState } from '../hooks/use-spotify-library'
import { NoticePanel } from './notice-panel'
import { PaginationControls } from './pagination-controls'
import { PlaylistCard } from './playlist-card'
import { TrackPanel } from './track-panel'

type PlaylistLibraryProps = {
  actions: SpotifyLibraryActions
  state: SpotifyLibraryState
}

export function PlaylistLibrary({ actions, state }: PlaylistLibraryProps) {
  const {
    authMessage,
    error,
    isLoading,
    isTracksLoading,
    needsConnection,
    page,
    pagination,
    selectedPlaylist,
    trackError,
    trackPage,
    trackPagination,
  } = state

  return (
    <section
      className="min-h-72 rounded-3xl border border-[#1c2821] bg-[#0f1512]/90 p-5 sm:p-7"
      aria-label="Playlists de Spotify"
    >
      <div className="mb-7 flex items-end justify-between gap-4">
        <div>
          <p className={sectionLabel}>Biblioteca</p>
          <h2 className="m-0 text-2xl font-bold text-[#f3f6f4]">Tus playlists</h2>
        </div>
        <button
          className={`${buttonBase} min-w-28`}
          type="button"
          onClick={() => actions.requestPlaylists(pagination.offset)}
          disabled={isLoading}
        >
          {isLoading ? 'Cargando...' : 'Actualizar'}
        </button>
      </div>

      {needsConnection && (
        <NoticePanel
          title="Conecta tu cuenta de Spotify"
          action={
            <a
              className="block rounded-full border border-[#504895] bg-[#584cb4] px-5 py-2.5 font-bold text-white transition hover:border-[#6258b1] hover:bg-[#6558c8]"
              href="/spotify/connect"
            >
              Conectar Spotify
            </a>
          }
        >
          <p className="m-0">
            {authMessage ??
              'Inicia sesión con Spotify. Volverás automáticamente a Sona cuando la conexión esté lista.'}
          </p>
        </NoticePanel>
      )}

      {authMessage && !needsConnection && (
        <NoticePanel
          tone="success"
          action={
            page ? (
              <button className={buttonBase} type="button" onClick={actions.clearAuthMessage}>
                Cerrar
              </button>
            ) : null
          }
        >
          <p className="m-0">{authMessage}</p>
        </NoticePanel>
      )}

      {error && !needsConnection && (
        <NoticePanel
          tone="error"
          action={
            <button
              className={buttonBase}
              type="button"
              onClick={() => actions.requestPlaylists(pagination.offset)}
            >
              Reintentar
            </button>
          }
        >
          <p className="m-0">{error}</p>
        </NoticePanel>
      )}

      {isLoading && !page && !needsConnection && (
        <div className="grid min-h-44 place-content-center text-center text-[#a5afa8]" aria-live="polite">
          Cargando playlists...
        </div>
      )}

      {page && page.items.length === 0 && !error && (
        <div className="grid min-h-44 place-content-center text-center text-[#a5afa8]">
          <h3 className="m-0 mb-2 text-xl font-bold text-[#f3f6f4]">No hay playlists disponibles</h3>
          <p className="m-0">Spotify no ha devuelto ninguna playlist para esta cuenta.</p>
        </div>
      )}

      {page && page.items.length > 0 && (
        <>
          <div className="mb-4 text-sm text-[#8e9892]" aria-live="polite">
            Mostrando {pagination.startItem}-{pagination.endItem} de {page.total}
          </div>
          <div className="mb-4 flex justify-start sm:-mt-10 sm:justify-end">
            <button
              className={buttonBase}
              type="button"
              onClick={() => void actions.disconnectSpotify()}
              disabled={isLoading}
            >
              Desconectar Spotify
            </button>
          </div>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-[repeat(auto-fill,minmax(244px,1fr))]">
            {page.items.map((playlist) => (
              <PlaylistCard
                isLoadingTracks={isTracksLoading}
                isSelected={selectedPlaylist?.id === playlist.id}
                key={playlist.id}
                playlist={playlist}
                onSelect={(selected) => void actions.requestPlaylistItems(selected, 0)}
              />
            ))}
          </div>
          <PaginationControls
            canGoBack={pagination.canGoBack}
            canGoForward={pagination.canGoForward}
            label="Paginación de playlists"
            onNext={() => actions.requestPlaylists(pagination.offset + PLAYLIST_PAGE_SIZE)}
            onPrevious={() =>
              actions.requestPlaylists(Math.max(0, pagination.offset - PLAYLIST_PAGE_SIZE))
            }
          />

          {selectedPlaylist && (
            <TrackPanel
              error={trackError}
              isLoading={isTracksLoading}
              page={trackPage}
              pagination={trackPagination}
              playlist={selectedPlaylist}
              onNextPage={() =>
                void actions.requestPlaylistItems(
                  selectedPlaylist,
                  trackPagination.offset + TRACK_PAGE_SIZE,
                )
              }
              onPreviousPage={() =>
                void actions.requestPlaylistItems(
                  selectedPlaylist,
                  Math.max(0, trackPagination.offset - TRACK_PAGE_SIZE),
                )
              }
              onRefresh={() =>
                void actions.requestPlaylistItems(selectedPlaylist, trackPagination.offset)
              }
            />
          )}
        </>
      )}
    </section>
  )
}
