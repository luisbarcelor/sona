import { useMemo, useState } from 'react'
import { arrayMove } from '@dnd-kit/sortable'
import type { SpotifyPlaylistItem, SpotifyPlaylistItemPage } from '../types/spotify'

export type PlaylistEditorRow = {
  id: string
  item: SpotifyPlaylistItem
  originalPosition: number
}

export function usePlaylistEditor(page: SpotifyPlaylistItemPage | null) {
  const originalRows = useMemo(() => (page ? createEditorRows(page) : []), [page])
  const pageKey = useMemo(() => originalRows.map((row) => row.id).join('|'), [originalRows])
  const [editorState, setEditorState] = useState<{
    currentRows: PlaylistEditorRow[]
    pageKey: string
  }>({
    currentRows: [],
    pageKey: '',
  })
  const currentRows = editorState.pageKey === pageKey ? editorState.currentRows : originalRows

  const hasUnsavedChanges = useMemo(() => {
    if (originalRows.length !== currentRows.length) {
      return true
    }

    return originalRows.some((row, index) => row.id !== currentRows[index]?.id)
  }, [currentRows, originalRows])

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
          pageKey,
        }
      }

      return {
        currentRows: arrayMove(rows, activeIndex, overIndex),
        pageKey,
      }
    })
  }

  function resetChanges() {
    setEditorState({
      currentRows: originalRows,
      pageKey,
    })
  }

  return {
    currentRows,
    hasUnsavedChanges,
    moveRow,
    resetChanges,
  }
}

function createEditorRows(page: SpotifyPlaylistItemPage) {
  return page.items.map((item, index) => {
    const originalPosition = page.offset + index + 1
    const stableIdentity =
      item.item?.uri ??
      item.item?.id ??
      item.item?.name ??
      item.unsupported_reason ??
      (item.is_local ? 'local' : 'unsupported')

    return {
      id: `${originalPosition}:${stableIdentity}`,
      item,
      originalPosition,
    }
  })
}
