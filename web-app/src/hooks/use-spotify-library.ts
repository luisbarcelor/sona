import { useCallback, useEffect, useState } from 'react'
import {
  PLAYLIST_PAGE_SIZE,
  TRACK_PAGE_SIZE,
  deleteSpotifyConnection,
  fetchCurrentUser,
  fetchPlaylistEditor,
  fetchPlaylists,
} from '../api/spotify-api'
import {
  getInitialAuthState,
  getReconnectMessage,
  isConnectionFailure,
  isUnauthorized,
} from './spotify-auth-state'
import type {
  SpotifyPlaylist,
  SpotifyPlaylistEditor,
  SpotifyPlaylistPage,
  SpotifyUserProfile,
} from '../types/spotify'
import { usePlaylistEditor, type PlaylistEditorState } from './use-playlist-editor'

export type SpotifyLibraryState = {
  authMessage: string | null
  error: string | null
  isLoading: boolean
  isTracksLoading: boolean
  needsConnection: boolean
  page: SpotifyPlaylistPage | null
  pagination: {
    canGoBack: boolean
    canGoForward: boolean
    endItem: number
    offset: number
    startItem: number
  }
  profile: SpotifyUserProfile | null
  profileName: string
  selectedPlaylist: SpotifyPlaylist | null
  trackEditor: PlaylistEditorState
  trackError: string | null
  trackPage: SpotifyPlaylistEditor | null
  trackPagination: {
    canGoBack: boolean
    canGoForward: boolean
    endItem: number
    offset: number
    startItem: number
  }
}

export type SpotifyLibraryActions = {
  clearAuthMessage: () => void
  disconnectSpotify: () => Promise<void>
  requestPlaylistItems: (playlist: SpotifyPlaylist) => Promise<void>
  requestPlaylists: (requestedOffset: number) => void
  setTrackOffset: (requestedOffset: number) => void
}

export function useSpotifyLibrary(): {
  actions: SpotifyLibraryActions
  state: SpotifyLibraryState
} {
  const [initialAuthState] = useState(getInitialAuthState)
  const [page, setPage] = useState<SpotifyPlaylistPage | null>(null)
  const [profile, setProfile] = useState<SpotifyUserProfile | null>(null)
  const [selectedPlaylist, setSelectedPlaylist] = useState<SpotifyPlaylist | null>(null)
  const [trackPage, setTrackPage] = useState<SpotifyPlaylistEditor | null>(null)
  const [trackLoadKey, setTrackLoadKey] = useState(0)
  const [offset, setOffset] = useState(0)
  const [trackOffset, setTrackOffset] = useState(0)
  const [error, setError] = useState<string | null>(initialAuthState.error)
  const [trackError, setTrackError] = useState<string | null>(null)
  const [authMessage, setAuthMessage] = useState<string | null>(initialAuthState.authMessage)
  const [needsConnection, setNeedsConnection] = useState(initialAuthState.needsConnection)
  const [isLoading, setIsLoading] = useState(true)
  const [isTracksLoading, setIsTracksLoading] = useState(false)
  const trackEditor = usePlaylistEditor(
    selectedPlaylist?.id ?? null,
    trackPage?.snapshot_id ?? null,
    trackPage?.items ?? null,
    trackLoadKey,
  )

  const clearConnectedState = useCallback(() => {
    setPage(null)
    setProfile(null)
    setSelectedPlaylist(null)
    setTrackPage(null)
    setTrackLoadKey(0)
    setTrackOffset(0)
  }, [])

  const moveToReconnectState = useCallback(() => {
    setNeedsConnection(true)
    clearConnectedState()
    setError(null)
    setTrackError(null)
    setAuthMessage(getReconnectMessage())
  }, [clearConnectedState])

  const loadPlaylists = useCallback(
    async (requestedOffset: number) => {
      try {
        const body = await fetchPlaylists(requestedOffset)
        setNeedsConnection(false)
        setPage(body)
        setOffset(requestedOffset)
      } catch (loadError) {
        if (isConnectionFailure(loadError)) {
          moveToReconnectState()
        } else {
          setNeedsConnection(false)
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
    },
    [moveToReconnectState],
  )

  function requestPlaylists(requestedOffset: number) {
    if (!guardUnsavedTrackChanges()) {
      return
    }

    setIsLoading(true)
    setError(null)
    void loadPlaylists(requestedOffset)
  }

  async function requestPlaylistItems(playlist: SpotifyPlaylist) {
    if (!guardUnsavedTrackChanges()) {
      return
    }

    setSelectedPlaylist(playlist)
    setTrackPage(null)
    setTrackOffset(0)
    setIsTracksLoading(true)
    setTrackError(null)

    try {
      const body = await fetchPlaylistEditor(playlist.id)
      setNeedsConnection(false)
      setTrackPage(body)
      setTrackLoadKey((currentKey) => currentKey + 1)
    } catch (loadError) {
      if (isUnauthorized(loadError)) {
        moveToReconnectState()
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

  function setTrackPageOffset(requestedOffset: number) {
    const lastOffset = Math.max(0, trackEditor.currentRows.length - TRACK_PAGE_SIZE)
    setTrackOffset(Math.min(Math.max(0, requestedOffset), lastOffset))
  }

  function guardUnsavedTrackChanges() {
    if (!trackEditor.hasUnsavedChanges) {
      return true
    }

    setTrackError('Revierte los cambios antes de salir de esta edición.')
    return false
  }

  async function disconnectSpotify() {
    if (!guardUnsavedTrackChanges()) {
      return
    }

    setIsLoading(true)
    setError(null)

    try {
      await deleteSpotifyConnection()
      clearConnectedState()
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
          moveToReconnectState()
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
          moveToReconnectState()
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
  }, [initialAuthState.shouldCleanUrl, moveToReconnectState])

  const startItem = page && page.total > 0 ? page.offset + 1 : 0
  const endItem = page ? Math.min(page.offset + page.items.length, page.total) : 0
  const trackStartItem = trackPage && trackPage.total > 0 ? trackOffset + 1 : 0
  const trackEndItem = trackPage ? Math.min(trackOffset + TRACK_PAGE_SIZE, trackPage.total) : 0
  const profileName = profile ? profile.display_name ?? profile.id : ''

  return {
    actions: {
      clearAuthMessage: () => setAuthMessage(null),
      disconnectSpotify,
      requestPlaylistItems,
      requestPlaylists,
      setTrackOffset: setTrackPageOffset,
    },
    state: {
      authMessage,
      error,
      isLoading,
      isTracksLoading,
      needsConnection,
      page,
      pagination: {
        canGoBack: offset > 0 && !isLoading,
        canGoForward: Boolean(page && offset + PLAYLIST_PAGE_SIZE < page.total && !isLoading),
        endItem,
        offset,
        startItem,
      },
      profile,
      profileName,
      selectedPlaylist,
      trackEditor,
      trackError,
      trackPage,
      trackPagination: {
        canGoBack: Boolean(selectedPlaylist && trackOffset > 0 && !isTracksLoading),
        canGoForward: Boolean(
          selectedPlaylist &&
            trackPage &&
            trackOffset + TRACK_PAGE_SIZE < trackEditor.currentRows.length &&
            !isTracksLoading,
        ),
        endItem: trackEndItem,
        offset: trackOffset,
        startItem: trackStartItem,
      },
    },
  }
}
