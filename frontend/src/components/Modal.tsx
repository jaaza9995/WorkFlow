import { useEffect, useRef } from 'react';
import type { ReactNode } from 'react';

// Bruker det innebygde <dialog>-elementet: Esc lukker, og tastaturfokus holdes inni dialogen.
// Vis den ved å rendre den betinget: {open && <Modal ...>}
export function Modal({
  title,
  onClose,
  children,
}: {
  title: string;
  onClose: () => void;
  children: ReactNode;
}) {
  const ref = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = ref.current;
    if (dialog && !dialog.open) dialog.showModal();
  }, []);

  return (
    <dialog
      ref={ref}
      className="modal"
      aria-labelledby="modal-title"
      onClose={onClose}
      onClick={(e) => {
        if (e.target === ref.current) onClose(); // klikk på bakgrunnen
      }}
    >
      <div className="modal-body">
        <div className="modal-head">
          <h2 id="modal-title">{title}</h2>
          <button type="button" className="btn btn-quiet" onClick={onClose}>
            Lukk
          </button>
        </div>
        {children}
      </div>
    </dialog>
  );
}
