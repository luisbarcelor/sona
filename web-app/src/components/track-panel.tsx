import {
  DndContext,
  KeyboardSensor,
  PointerSensor,
  closestCenter,
  useSensor,
  useSensors,
  type DragEndEvent,
} from '@dnd-kit/core'
import {
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable'
import { buttonBase, sectionLabel } from '../lib/styles'
import type {
  SpotifyPlaylist,
  SpotifyPlaylistItemPage,
} from '../types/spotify'
import { usePlaylistEditor } from '../hooks/use-playlist-editor'
import { NoticePanel } from './notice-panel'
import { PaginationControls } from './pagination-controls'
import { TrackRow } from './track-row'

type TrackPanelProps = {
  error: string | null
  isLoading: boolean
  onNextPage: () => void
  onPreviousPage: () => void
  onRefresh: () => void
  page: SpotifyPlaylistItemPage | null
  pagination: {
    canGoBack: boolean
    canGoForward: boolean
    endItem: number
    startItem: number
  }
  playlist: SpotifyPlaylist
}

export function TrackPanel({
  error,
  isLoading,
  onNextPage,
  onPreviousPage,
  onRefresh,
  page,
  pagination,
  playlist,
}: TrackPanelProps) {
  const editor = usePlaylistEditor(page)
  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    }),
  )

  function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event

    if (!over) {
      return
    }

    editor.moveRow(String(active.id), String(over.id))
  }

  return (
    <section className="mt-8 border-t border-[#243029] pt-7" aria-label="Canciones de la playlist seleccionada">
      <div className="mb-5 flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-end">
        <div className="min-w-0">
          <p className={sectionLabel}>Playlist seleccionada</p>
          <h2 className="m-0 mb-1.5 max-w-2xl truncate text-2xl font-bold text-[#f3f6f4]">
            {playlist.name}
          </h2>
          {page && (
            <p className="m-0 text-sm text-[#8e9892]">
              Mostrando {pagination.startItem}-{pagination.endItem} de {page.total}
            </p>
          )}
        </div>
        <button
          className={buttonBase}
          type="button"
          onClick={onRefresh}
          disabled={isLoading || editor.hasUnsavedChanges}
        >
          {isLoading ? 'Cargando...' : 'Actualizar canciones'}
        </button>
      </div>

      {editor.hasUnsavedChanges && (
        <NoticePanel
          tone="success"
          title="Cambios sin guardar"
          action={
            <button className={buttonBase} type="button" onClick={editor.resetChanges}>
              Revertir cambios
            </button>
          }
        >
          <p className="m-0">
            Has cambiado el orden de esta página de canciones. La playlist no se guardará en Spotify
            todavía. Revierte los cambios antes de actualizar o cambiar de página.
          </p>
        </NoticePanel>
      )}

      {error && (
        <NoticePanel
          tone="error"
          action={
            <button className={buttonBase} type="button" onClick={onRefresh}>
              Reintentar
            </button>
          }
        >
          <p className="m-0">{error}</p>
        </NoticePanel>
      )}

      {isLoading && !page && !error && (
        <div className="grid min-h-44 place-content-center text-center text-[#a5afa8]" aria-live="polite">
          Cargando canciones...
        </div>
      )}

      {page && page.items.length === 0 && !error && (
        <div className="grid min-h-44 place-content-center text-center text-[#a5afa8]">
          <h3 className="m-0 mb-2 text-xl font-bold text-[#f3f6f4]">Playlist vacía</h3>
          <p className="m-0">Spotify no ha devuelto canciones para esta playlist.</p>
        </div>
      )}

      {page && page.items.length > 0 && (
        <>
          <DndContext
            collisionDetection={closestCenter}
            sensors={sensors}
            onDragEnd={handleDragEnd}
          >
            <SortableContext
              items={editor.currentRows.map((row) => row.id)}
              strategy={verticalListSortingStrategy}
            >
              <ol className="grid list-none gap-2 p-0">
                {editor.currentRows.map((row, index) => (
                  <TrackRow
                    id={row.id}
                    item={row.item}
                    key={row.id}
                    originalPosition={row.originalPosition}
                    position={page.offset + index + 1}
                  />
                ))}
              </ol>
            </SortableContext>
          </DndContext>
          <PaginationControls
            canGoBack={pagination.canGoBack && !editor.hasUnsavedChanges}
            canGoForward={pagination.canGoForward && !editor.hasUnsavedChanges}
            label="Paginación de canciones"
            onNext={onNextPage}
            onPrevious={onPreviousPage}
          />
        </>
      )}
    </section>
  )
}
