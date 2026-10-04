import { useState } from 'react';
import type { ChangeEvent, FormEvent } from 'react';
import { api } from '../api';
import { errorMessage } from '../api/client';
import type { TaskPriority, User } from '../api/types';
import { fullName, priorities, priorityLabel } from '../labels';
import { Modal } from './Modal';
import { ErrorNote, Field } from './ui';

interface Props {
  projectId: number;
  members: User[];
  onSaved: () => void;
  onClose: () => void;
}

export function TaskFormModal({ projectId, members, onSaved, onClose }: Props) {
  const [form, setForm] = useState({
    title: '',
    description: '',
    priority: 'Medium',
    dueDate: '',
    assignedTo: '',
  });
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const set =
    (key: keyof typeof form) =>
    (e: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
      setForm({ ...form, [key]: e.target.value });

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await api.tasks.create({
        title: form.title.trim(),
        description: form.description.trim() || null,
        priority: form.priority as TaskPriority,
        dueDate: form.dueDate || null,
        projectId,
        assignedToUserId: form.assignedTo ? Number(form.assignedTo) : null,
      });
      onSaved();
    } catch (err) {
      setError(errorMessage(err));
      setBusy(false);
    }
  }

  return (
    <Modal title="Ny oppgave" onClose={onClose}>
      <form onSubmit={submit} className="form">
        <Field label="Tittel">
          <input className="input" required maxLength={200} value={form.title} onChange={set('title')} />
        </Field>

        <Field label="Beskrivelse">
          <textarea className="input" rows={3} value={form.description} onChange={set('description')} />
        </Field>

        <div className="form-row">
          <Field label="Prioritet">
            <select className="input" value={form.priority} onChange={set('priority')}>
              {priorities.map((p) => (
                <option key={p} value={p}>
                  {priorityLabel[p]}
                </option>
              ))}
            </select>
          </Field>
          <Field label="Frist" hint="Valgfri">
            <input className="input" type="date" value={form.dueDate} onChange={set('dueDate')} />
          </Field>
        </div>

        <Field
          label="Ansvarlig"
          hint={
            members.length === 0
              ? 'Ingen er med i prosjektet ennå. Legg til ansatte under Team først.'
              : 'Må være med i prosjektet. Kan velges senere.'
          }
        >
          <select className="input" value={form.assignedTo} onChange={set('assignedTo')}>
            <option value="">Ikke tildelt</option>
            {members.map((m) => (
              <option key={m.id} value={m.id}>
                {fullName(m)}
              </option>
            ))}
          </select>
        </Field>

        <ErrorNote message={error} />

        <div className="form-actions">
          <button type="button" className="btn" onClick={onClose}>
            Avbryt
          </button>
          <button type="submit" className="btn btn-primary" disabled={busy}>
            Opprett oppgave
          </button>
        </div>
      </form>
    </Modal>
  );
}
