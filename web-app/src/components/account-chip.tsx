import type { SpotifyUserProfile } from '../types/spotify'

type AccountChipProps = {
  profile: SpotifyUserProfile
  profileName: string
}

export function AccountChip({ profile, profileName }: AccountChipProps) {
  return (
    <aside
      className="static mb-6 flex w-full max-w-[min(320px,calc(100vw-48px))] items-center gap-3 rounded-full border border-[#26332b] bg-[#0c110e]/90 py-2.5 pr-3.5 pl-2.5 shadow-[0_16px_40px_rgba(0,0,0,0.25)] sm:fixed sm:top-6 sm:right-6 sm:z-10 sm:mb-0"
      aria-label="Perfil de Spotify conectado"
    >
      {profile.images[0]?.url ? (
        <img
          className="h-9.5 w-9.5 shrink-0 rounded-full bg-[#1a241f] object-cover"
          src={profile.images[0].url}
          alt=""
        />
      ) : (
        <span
          className="grid h-9.5 w-9.5 shrink-0 place-items-center rounded-full bg-[#1a241f]"
          aria-hidden="true"
        >
          {profileName.slice(0, 1).toUpperCase()}
        </span>
      )}
      <div className="min-w-0">
        <p className="m-0 truncate text-[11px] text-[#8e9892]">Conectado como</p>
        <strong className="block truncate text-sm text-[#f3f6f4]">{profileName}</strong>
      </div>
    </aside>
  )
}
