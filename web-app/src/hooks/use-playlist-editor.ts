import { useMemo, useState } from 'react'
import { arrayMove } from '@dnd-kit/sortable'
import type { SpotifyPlaylistItem } from '../types/spotify'

export type PlaylistEditorRow = {
  id: string
  item: SpotifyPlaylistItem
  isUnsupported: boolean
  originalPosition: number
  occurrenceKey: string
  trackUri: string | null
  unsupportedReason: string | null
}

export type PlaylistEditorState = {
  canPrepareSave: boolean
  currentRows: PlaylistEditorRow[]
  hasUnsavedChanges: boolean
  movedRows: PlaylistEditorRow[]
  moveRow: (activeId: string, overId: string) => void
  orderedTrackUris: string[]
  resetChanges: () => void
  snapshotId: string | null
  unsupportedRows: PlaylistEditorRow[]
}

export function usePlaylistEditor(
  playlistId: string | null,
  snapshotId: string | null,
  items: SpotifyPlaylistItem[] | null,
  loadKey: number,
): PlaylistEditorState {
  const originalRows = useMemo(() => (items ? createEditorRows(items) : []), [items])
  const sessionKey = useMemo(
    () => (playlistId ? `${playlistId}:${loadKey}:${originalRows.map((row) => row.id).join('|')}` : ''),
    [loadKey, originalRows, playlistId],
  )
  const [editorState, setEditorState] = useState<{
    currentRows: PlaylistEditorRow[]
    sessionKey: string
  }>({
    currentRows: [],
    sessionKey: '',
  })
  const currentRows = editorState.sessionKey === sessionKey ? editorState.currentRows : originalRows

  const hasUnsavedChanges = useMemo(() => {
    if (originalRows.length !== currentRows.length) {
      return true
    }

    return originalRows.some((row, index) => row.id !== currentRows[index]?.id)
  }, [currentRows, originalRows])
  const movedRows = useMemo(
    () => currentRows.filter((row, index) => row.originalPosition !== index + 1),
    [currentRows],
  )
  const unsupportedRows = useMemo(
    () => currentRows.filter((row) => row.isUnsupported),
    [currentRows],
  )
  const orderedTrackUris = useMemo(
    () => currentRows.flatMap((row) => row.trackUri ? [row.trackUri] : []),
    [currentRows],
  )
  const canPrepareSave = hasUnsavedChanges && snapshotId !== null && unsupportedRows.length === 0

  function moveRow(activeId: string, overId: string) {
    if (activeId === overId) {
      return
    }

    const rows = currentRows

    setEditorState(() => {
      const activeIndex = rows.findIndex((row) => row.id === activeId)
      const overIndex = rows.findIndex((row) => row.id === overId)

      if (activeIndex === -1 || overIndex === -1) {
        return {
          currentRows: rows,
          sessionKey,
        }
      }

      return {
        currentRows: arrayMove(rows, activeIndex, overIndex),
        sessionKey,
      }
    })
  }

  function resetChanges() {
    setEditorState({
      currentRows: originalRows,
      sessionKey,
    })
  }

  return {
    canPrepareSave,
    currentRows,
    hasUnsavedChanges,
    movedRows,
    moveRow,
    orderedTrackUris,
    resetChanges,
    snapshotId,
    unsupportedRows,
  }
}

function createEditorRows(items: SpotifyPlaylistItem[]) {
  return items.map((item, index) => {
    const originalPosition = index + 1
    const stableIdentity =
      item.item?.uri ??
      item.item?.id ??
      item.item?.name ??
      item.unsupported_reason ??
      (item.is_local ? 'local' : 'unsupported')
    const occurrenceKey = `${originalPosition}:${stableIdentity}`
    const trackUri = item.item?.uri ?? null
    const unsupportedReason = item.unsupported_reason ?? (trackUri ? null : 'Track URI is unavailable.')

    return {
      id: `playlist-row:${occurrenceKey}`,
      isUnsupported: trackUri === null,
      item,
      occurrenceKey,
      originalPosition,
      trackUri,
      unsupportedReason,
    }
  })
}
