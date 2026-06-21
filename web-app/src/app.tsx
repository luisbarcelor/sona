import { AccountChip } from './components/account-chip'
import { AppHeader } from './components/app-header'
import { PlaylistLibrary } from './components/playlist-library'
import { SpotifyAttribution } from './components/spotify-attribution'
import { useSpotifyLibrary } from './hooks/use-spotify-library'

function App() {
  const { actions, state } = useSpotifyLibrary()

  return (
    <main className="mx-auto w-[min(1180px,calc(100%-32px))] py-6 sm:w-[min(1180px,calc(100%-48px))] sm:py-10">
      {state.profile && <AccountChip profile={state.profile} profileName={state.profileName} />}
      <AppHeader />
      <PlaylistLibrary actions={actions} state={state} />
      <SpotifyAttribution />
    </main>
  )
}

export default App
