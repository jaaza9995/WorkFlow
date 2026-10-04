import { useState } from 'react';
import type { ChangeEvent, FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api';
import { errorMessage } from '../api/client';
import type { Project, ProjectStatus } from '../api/types';
import { useAuth } from '../auth/useAuth';
import { fullName, projectStatuses, projectStatusLabel, todayIso } from '../labels';
import { useLoad } from '../useLoad';
import { Modal } from './Modal';
import { ErrorNote, Field } from './ui';

interface Props {
  project?: Project;
  onSaved: (project: Project | null) => void;
  onClose: () => void;
}

export function ProjectFormModal({ project, onSaved, onClose }: Props) {
  const { user } = useAuth();
  const customers = useLoad(() => api.customers.list(), 'form-customers');
  const users = useLoad(() => api.users.list(), 'form-users');

  const [form, setForm] = useState({
    name: project?.name ?? '',
    description: project?.description ?? '',
    startDate: project?.startDate.slice(0, 10) ?? todayIso(),
    endDate: project?.endDate?.slice(0, 10) ?? '',
    status: project?.status ?? 'Planned',
    customerId: project ? String(project.customerId) : '',
    managerId: project
      ? String(project.responsibleManagerId)
      : user?.role === 'Manager'
        ? String(user.id)
        : '',
  });
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const set =
    (key: keyof typeof form) =>
    (e: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
      setForm({ ...form, [key]: e.target.value });

  const customerOptions = (customers.data ?? []).filter((c) => c.companyId === user?.companyId);
  const managerOptions = (users.data ?? []).filter(
    (u) => u.role === 'Manager' && u.companyId === user?.companyId,
  );

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError(null);

    if (!form.customerId) return setError('Velg en kunde.');
    if (!form.managerId) return setError('Velg en ansvarlig manager.');
    if (form.endDate && form.endDate < form.startDate) {
      return setError('Sluttdato kan ikke være før startdato.');
    }

    const input = {
      name: form.name.trim(),
      description: form.description.trim() || null,
      startDate: form.startDate,
      endDate: form.endDate || null,
      status: form.status as ProjectStatus,
      customerId: Number(form.customerId),
      responsibleManagerId: Number(form.managerId),
    };

    setBusy(true);
    try {
      if (project) {
        await api.projects.update(project.id, input);
        onSaved(null);
      } else {
        onSaved(await api.projects.create(input));
      }
    } catch (err) {
      setError(errorMessage(err));
      setBusy(false);
    }
  }

  return (
    <Modal title={project ? 'Rediger prosjekt' : 'Nytt prosjekt'} onClose={onClose}>
      <form onSubmit={submit} className="form">
        <Field label="Navn">
          <input className="input" required maxLength={200} value={form.name} onChange={set('name')} />
        </Field>

        <Field label="Beskrivelse">
          <textarea className="input" rows={3} value={form.description} onChange={set('description')} />
        </Field>

        <div className="form-row">
          <Field label="Startdato">
            <input className="input" type="date" required value={form.startDate} onChange={set('startDate')} />
          </Field>
          <Field label="Sluttdato" hint="Valgfri">
            <input className="input" type="date" value={form.endDate} onChange={set('endDate')} />
          </Field>
        </div>

        <Field label="Status">
          <select className="input" value={form.status} onChange={set('status')}>
            {projectStatuses.map((s) => (
              <option key={s} value={s}>
                {projectStatusLabel[s]}
              </option>
            ))}
          </select>
        </Field>

        <Field
          label="Kunde"
          hint={
            customers.data && customerOptions.length === 0
              ? undefined
              : 'Kunden får se prosjektet og fremdriften'
          }
        >
          <select className="input" value={form.customerId} onChange={set('customerId')}>
            <option value="">Velg kunde</option>
            {customerOptions.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </Field>
        {customers.data && customerOptions.length === 0 && (
          <p className="note">
            Du har ingen kunder ennå. <Link to="/kunder">Opprett en kunde</Link> først.
          </p>
        )}

        <Field label="Ansvarlig manager">
          <select className="input" value={form.managerId} onChange={set('managerId')}>
            <option value="">Velg manager</option>
            {managerOptions.map((u) => (
              <option key={u.id} value={u.id}>
                {fullName(u)}
              </option>
            ))}
          </select>
        </Field>
        {users.data && managerOptions.length === 0 && (
          <p className="note">
            Ingen brukere har rollen Manager ennå. <Link to="/brukere">Opprett en manager</Link> først.
          </p>
        )}

        <ErrorNote message={error ?? customers.error ?? users.error} />

        <div className="form-actions">
          <button type="button" className="btn" onClick={onClose}>
            Avbryt
          </button>
          <button type="submit" className="btn btn-primary" disabled={busy}>
            {project ? 'Lagre endringer' : 'Opprett prosjekt'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
