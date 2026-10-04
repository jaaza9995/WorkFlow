import { useState } from 'react';
import type { ChangeEvent, FormEvent } from 'react';
import { api } from '../api';
import { errorMessage } from '../api/client';
import type { Customer, UserRole } from '../api/types';
import { useAuth } from '../auth/useAuth';
import { Modal } from '../components/Modal';
import { EmptyState, ErrorNote, Field, Loading, PageHeader } from '../components/ui';
import { fullName, roleLabel } from '../labels';
import { useLoad } from '../useLoad';

const roles: UserRole[] = ['Administrator', 'Manager', 'Employee', 'Customer'];

function UserFormModal({
  companyId,
  customers,
  onSaved,
  onClose,
}: {
  companyId: number;
  customers: Customer[];
  onSaved: () => void;
  onClose: () => void;
}) {
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    role: 'Employee',
    customerId: '',
  });
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const set =
    (key: keyof typeof form) => (e: ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
      setForm({ ...form, [key]: e.target.value });

  const isCustomer = form.role === 'Customer';

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    if (isCustomer && !form.customerId) return setError('Velg hvilken kunde brukeren tilhører.');

    setBusy(true);
    try {
      await api.users.create({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        password: form.password,
        role: form.role as UserRole,
        companyId,
        customerId: isCustomer ? Number(form.customerId) : null,
      });
      onSaved();
    } catch (err) {
      setError(errorMessage(err));
      setBusy(false);
    }
  }

  return (
    <Modal title="Ny bruker" onClose={onClose}>
      <form onSubmit={submit} className="form">
        <div className="form-row">
          <Field label="Fornavn">
            <input className="input" required maxLength={100} value={form.firstName} onChange={set('firstName')} />
          </Field>
          <Field label="Etternavn">
            <input className="input" required maxLength={100} value={form.lastName} onChange={set('lastName')} />
          </Field>
        </div>

        <Field label="E-post">
          <input className="input" type="email" required maxLength={255} value={form.email} onChange={set('email')} />
        </Field>

        <Field label="Passord" hint="Minst 8 tegn. Brukeren bruker det til å logge inn.">
          <input
            className="input"
            type="password"
            required
            minLength={8}
            autoComplete="new-password"
            value={form.password}
            onChange={set('password')}
          />
        </Field>

        <Field label="Rolle">
          <select className="input" value={form.role} onChange={set('role')}>
            {roles.map((r) => (
              <option key={r} value={r}>
                {roleLabel[r]}
              </option>
            ))}
          </select>
        </Field>

        {isCustomer && (
          <Field label="Tilhører kunde" hint="Kundebrukeren ser prosjektene til denne kunden.">
            <select className="input" value={form.customerId} onChange={set('customerId')}>
              <option value="">Velg kunde</option>
              {customers.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </Field>
        )}

        <ErrorNote message={error} />

        <div className="form-actions">
          <button type="button" className="btn" onClick={onClose}>
            Avbryt
          </button>
          <button type="submit" className="btn btn-primary" disabled={busy}>
            Opprett bruker
          </button>
        </div>
      </form>
    </Modal>
  );
}

export function UsersPage() {
  const { user } = useAuth();
  const users = useLoad(() => api.users.list(), 'users');
  const customers = useLoad(() => api.customers.list(), 'users-customers');
  const [creating, setCreating] = useState(false);

  if (!user) return null;
  const isAdmin = user.role === 'Administrator';
  const list = (users.data ?? []).filter((u) => u.companyId === user.companyId);
  const customerList = (customers.data ?? []).filter((c) => c.companyId === user.companyId);
  const customerName = (id: number | null) =>
    id === null ? '–' : (customerList.find((c) => c.id === id)?.name ?? '–');

  return (
    <>
      <PageHeader
        title="Brukere"
        subtitle="Alle som har tilgang til bedriftens WorkFlow."
        actions={
          isAdmin && (
            <button type="button" className="btn btn-primary" onClick={() => setCreating(true)}>
              Ny bruker
            </button>
          )
        }
      />

      <ErrorNote message={users.error} />
      {users.loading && <Loading />}

      {users.data && list.length === 0 && <EmptyState title="Ingen brukere å vise" />}

      {list.length > 0 && (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Navn</th>
                <th>E-post</th>
                <th>Rolle</th>
                <th>Kunde</th>
              </tr>
            </thead>
            <tbody>
              {list.map((u) => (
                <tr key={u.id}>
                  <td className="row-title">{fullName(u)}</td>
                  <td>{u.email}</td>
                  <td>{roleLabel[u.role]}</td>
                  <td>{customerName(u.customerId)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {creating && (
        <UserFormModal
          companyId={user.companyId}
          customers={customerList}
          onClose={() => setCreating(false)}
          onSaved={() => {
            setCreating(false);
            users.reload();
          }}
        />
      )}
    </>
  );
}
