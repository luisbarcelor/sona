import { sectionLabel } from '../lib/styles'

export function AppHeader() {
  return (
    <header className="max-w-175 pb-8 sm:pb-10">
      <div className="mb-10 flex items-center gap-2.5 text-xl font-bold text-[#f5faf6] sm:mb-14">
        <span
          className="grid h-9.5 w-9.5 place-items-center rounded-xl bg-[#7968f5] text-white"
          aria-hidden="true"
        >
          S
        </span>
        <span>Sona</span>
      </div>
      <p className={sectionLabel}>Spotify playlists</p>
      <h1 className="m-0 mb-4 max-w-3xl text-5xl leading-[1.03] font-bold text-[#f8faf9] sm:text-6xl">
        Tu música, lista para organizar.
      </h1>
      <p className="m-0 max-w-xl text-lg leading-7 text-[#a6afa9]">
        Consulta tus playlists, selecciona una y carga sus canciones en el orden actual de Spotify.
      </p>
    </header>
  )
}
