import { useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { formatDuration } from '../lib/format-duration'
import type { SpotifyPlaylistItem } from '../types/spotify'

type TrackRowProps = {
  id: string
  item: SpotifyPlaylistItem
  originalPosition: number
  position: number
}

export function TrackRow({ id, item, originalPosition, position }: TrackRowProps) {
  const {
    attributes,
    isDragging,
    listeners,
    setNodeRef,
    transform,
    transition,
  } = useSortable({ id })
  const track = item.item
  const artists = track?.artists.length
    ? track.artists.map((artist) => artist.name).join(', ')
    : 'Artista desconocido'
  const moved = position !== originalPosition

  return (
    <li
      ref={setNodeRef}
      className={`grid min-h-16 grid-cols-[30px_44px_minmax(0,1fr)_48px_32px] items-center gap-3 rounded-lg border bg-[#101713] py-2 pr-2 pl-3 transition-[border-color,box-shadow,opacity] sm:grid-cols-[40px_48px_minmax(0,1fr)_minmax(0,max-content)_58px_32px] ${
        isDragging
          ? 'z-10 border-[#7e73f2] opacity-80 shadow-[0_18px_40px_rgba(0,0,0,0.32)]'
          : moved
            ? 'border-[#4b456f]'
            : 'border-[#1c2721]'
      }`}
      style={{
        transform: CSS.Translate.toString(transform),
        transition: isDragging ? undefined : transition,
      }}
    >
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
      <div className="hidden items-center gap-2 sm:flex">
        {item.is_local && (
          <span className="rounded-full border border-[#2e3b33] px-2 py-1 text-[11px] text-[#bfc8c2]">
            Local
          </span>
        )}
        {track?.explicit && (
          <span className="rounded-full border border-[#2e3b33] px-2 py-1 text-[11px] text-[#bfc8c2]">
            Explicit
          </span>
        )}
        {moved && (
          <span className="rounded-full border border-[#4b456f] px-2 py-1 text-[11px] text-[#c7c0ff]">
            Movida
          </span>
        )}
      </div>
      <span className="text-right text-[13px] text-[#8e9892] tabular-nums">
        {formatDuration(track?.duration_ms ?? null)}
      </span>
      <button
        className="grid h-8 w-8 touch-none cursor-grab place-items-center rounded-md border border-[#2e3b33] text-[#9ba69f] active:cursor-grabbing"
        type="button"
        aria-label={`Mover ${track?.name ?? 'elemento no compatible'}`}
        {...attributes}
        {...listeners}
      >
        ⋮⋮
      </button>
    </li>
  )
}
