import { buttonCompact } from '../lib/styles'
import type { SpotifyPlaylist } from '../types/spotify'

type PlaylistCardProps = {
  isLoadingTracks: boolean
  isSelected: boolean
  onSelect: (playlist: SpotifyPlaylist) => void
  playlist: SpotifyPlaylist
}

export function PlaylistCard({
  isLoadingTracks,
  isSelected,
  onSelect,
  playlist,
}: PlaylistCardProps) {
  return (
    <article
      className={`flex overflow-hidden rounded-lg border bg-[#111814] transition ${
        isSelected
          ? 'border-[#7e73f2] shadow-[inset_0_0_0_1px_rgba(126,115,242,0.4)]'
          : 'border-[#1c2721] hover:border-[#344238]'
      }`}
    >
      <div className="flex w-full flex-col">
        {playlist.images[0]?.url ? (
          <img
            className="aspect-square w-full bg-[#171f1b] object-contain"
            src={playlist.images[0].url}
            alt=""
            loading="lazy"
          />
        ) : (
          <div
            className="grid aspect-square w-full place-items-center bg-[#171f1b] text-5xl text-[#58635c]"
            aria-hidden="true"
          >
            ♪
          </div>
        )}
        <div className="flex min-h-56 flex-1 flex-col p-[18px]">
          <h3 className="m-0 mb-1.5 truncate text-lg leading-6 font-bold text-[#f3f6f4]">
            {playlist.name}
          </h3>
          <p className="m-0 text-sm text-[#9ba69f]">De {playlist.owner.display_name ?? 'Spotify'}</p>
          <p className="mt-1.5 mb-5 text-sm text-[#9ba69f]">{playlist.items.total} canciones</p>
          <button
            className={`${buttonCompact} mt-auto mb-3 self-start ${
              isSelected ? 'border-[#6f66d7] bg-[#40398a]' : ''
            }`}
            type="button"
            aria-pressed={isSelected}
            disabled={isLoadingTracks && isSelected}
            onClick={() => onSelect(playlist)}
          >
            {isSelected ? 'Seleccionada' : 'Seleccionar'}
          </button>
          {playlist.external_urls?.spotify && (
            <a
              className="self-start text-sm font-semibold text-[#beb7ff] hover:underline"
              href={playlist.external_urls.spotify}
              target="_blank"
              rel="noreferrer"
            >
              Abrir en Spotify
            </a>
          )}
        </div>
      </div>
    </article>
  )
}
