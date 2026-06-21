import { buttonBase } from '../lib/styles'

type PaginationControlsProps = {
  canGoBack: boolean
  canGoForward: boolean
  label: string
  onNext: () => void
  onPrevious: () => void
}

export function PaginationControls({
  canGoBack,
  canGoForward,
  label,
  onNext,
  onPrevious,
}: PaginationControlsProps) {
  return (
    <nav className="mt-7 flex justify-center gap-2.5" aria-label={label}>
      <button className={buttonBase} type="button" disabled={!canGoBack} onClick={onPrevious}>
        Anterior
      </button>
      <button className={buttonBase} type="button" disabled={!canGoForward} onClick={onNext}>
        Siguiente
      </button>
    </nav>
  )
}
