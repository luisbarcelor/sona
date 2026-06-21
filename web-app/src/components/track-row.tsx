import { formatDuration } from '../lib/format-duration'
import type { SpotifyPlaylistItem } from '../types/spotify'

type TrackRowProps = {
  item: SpotifyPlaylistItem
  position: number
}

export function TrackRow({ item, position }: TrackRowProps) {
  const track = item.item
  const artists = track?.artists.length
    ? track.artists.map((artist) => artist.name).join(', ')
    : 'Artista desconocido'

  return (
    <li className="grid min-h-16 grid-cols-[30px_44px_minmax(0,1fr)_48px] items-center gap-3 rounded-lg border border-[#1c2721] bg-[#101713] px-3 py-2 sm:grid-cols-[40px_48px_minmax(0,1fr)_auto_auto_58px]">
      <span className="text-right text-[13px] text-[#8e9892] tabular-nums">{position}</span>
      {track?.album?.images[0]?.url ? (
        <img
          className="h-11 w-11 rounded-md bg-[#171f1b] object-cover sm:h-12 sm:w-12"
          src={track.album.images[0].url}
          alt=""
          loading="lazy"
        />
      ) : (
        <div
          className="grid h-11 w-11 place-items-center rounded-md bg-[#171f1b] text-[#58635c] sm:h-12 sm:w-12"
          aria-hidden="true"
        >
          ♪
        </div>
      )}
      <div className="grid min-w-0 gap-1">
        <strong className="truncate text-[15px] text-[#f3f6f4]">
          {track?.name ?? 'Elemento no compatible'}
        </strong>
        <span className="truncate text-[13px] text-[#9ba69f]">
          {track ? `${artists} · ${track.album?.name ?? 'Álbum desconocido'}` : item.unsupported_reason}
        </span>
      </div>
      {item.is_local && (
        <span className="hidden rounded-full border border-[#2e3b33] px-2 py-1 text-[11px] text-[#bfc8c2] sm:inline">
          Local
        </span>
      )}
      {track?.explicit && (
        <span className="hidden rounded-full border border-[#2e3b33] px-2 py-1 text-[11px] text-[#bfc8c2] sm:inline">
          Explicit
        </span>
      )}
      <span className="text-right text-[13px] text-[#8e9892] tabular-nums">
        {formatDuration(track?.duration_ms ?? null)}
      </span>
    </li>
  )
}
