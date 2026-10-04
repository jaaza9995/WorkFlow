import { useState } from 'react';
import type { ChangeEvent, FormEvent } from 'react';
import { api } from '../api';
import { errorMessage } from '../api/client';
import type { TimeEntry } from '../api/types';
import { todayIso } from '../labels';
import { useLoad } from '../useLoad';
import { Modal } from './Modal';
import { ErrorNote, Field } from './ui';

interface Props {
  entry?: TimeEntry; // satt = rediger
  taskId?: number; // satt = oppgaven er allerede valgt
  taskLabel?: string; // navn på oppgaven, vises når den er valgt
  onSaved: () => void;
  onClose: () => void;
}

export function TimeEntryModal({ entry, taskId, taskLabel, onSaved, onClose }: Props) {
  const needsTaskChoice = !entry && taskId === undefined;
  const tasks = useLoad(
    async () => (needsTaskChoice ? await api.tasks.list({}) : []),
    `time-tasks-${needsTaskChoice}`,
  );

  const [form, setForm] = useState({
    taskId: taskId !== undefined ? String(taskId) : '',
    date: entry?.date.slice(0, 10) ?? todayIso(),
    hours: entry ? String(entry.hours) : '',
    description: entry?.description ?? '',
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

    const hours = Number(form.hours);
    if (!Number.isFinite(hours) || hours <= 0 || hours > 24) {
      return setError('Antall timer må være mellom 0 og 24.');
    }
    if (!entry && !form.taskId) return setError('Velg en oppgave.');

    const input = {
      date: form.date,
      hours,
      description: form.description.trim() || null,
    };

    setBusy(true);
    try {
      if (entry) {
        await api.timeEntries.update(entry.id, input);
      } else {
        await api.timeEntries.create({ ...input, taskId: Number(form.taskId) });
      }
      onSaved();
    } catch (err) {
      setError(errorMessage(err));
      setBusy(false);
    }
  }

  return (
    <Modal title={entry ? 'Rediger timer' : 'Registrer timer'} onClose={onClose}>
      <form onSubmit={submit} className="form">
        {taskLabel && <p className="muted">Oppgave: {taskLabel}</p>}

        {needsTaskChoice && (
          <Field label="Oppgave">
            <select className="input" required value={form.taskId} onChange={set('taskId')}>
              <option value="">Velg oppgave</option>
              {(tasks.data ?? []).map((t) => (
                <option key={t.id} value={t.id}>
                  {t.projectName}: {t.title}
                </option>
              ))}
            </select>
          </Field>
        )}

        <div className="form-row">
          <Field label="Dato">
            <input className="input" type="date" required value={form.date} onChange={set('date')} />
          </Field>
          <Field label="Timer">
            <input
              className="input"
              type="number"
              required
              min="0.01"
              max="24"
              step="0.25"
              inputMode="decimal"
              value={form.hours}
              onChange={set('hours')}
            />
          </Field>
        </div>

        <Field label="Hva jobbet du med?" hint="Valgfri">
          <textarea
            className="input"
            rows={2}
            maxLength={500}
            value={form.description}
            onChange={set('description')}
          />
        </Field>

        <ErrorNote message={error ?? tasks.error} />

        <div className="form-actions">
          <button type="button" className="btn" onClick={onClose}>
            Avbryt
          </button>
          <button type="submit" className="btn btn-primary" disabled={busy}>
            {entry ? 'Lagre endringer' : 'Registrer timer'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
