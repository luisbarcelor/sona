export type SpotifyImage = {
  url: string
}

export type SpotifyPlaylist = {
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
  snapshot_id: string
}

export type SpotifyPlaylistPage = {
  items: SpotifyPlaylist[]
  limit: number
  offset: number
  total: number
}

export type SpotifyTrack = {
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

export type SpotifyPlaylistItem = {
  added_at: string | null
  is_local: boolean
  item: SpotifyTrack | null
  unsupported_reason: string | null
}

export type SpotifyPlaylistItemPage = {
  items: SpotifyPlaylistItem[]
  limit: number
  offset: number
  total: number
}

export type SpotifyPlaylistEditor = {
  playlist_id: string
  snapshot_id: string
  items: SpotifyPlaylistItem[]
  total: number
}

export type SpotifyUserProfile = {
  id: string
  display_name: string | null
  images: SpotifyImage[]
}
