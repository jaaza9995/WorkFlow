import { useState } from 'react';
import type { ChangeEvent, FormEvent } from 'react';
import { api } from '../api';
import { errorMessage } from '../api/client';
import { useAuth } from '../auth/useAuth';
import { Modal } from '../components/Modal';
import { EmptyState, ErrorNote, Field, Loading, PageHeader } from '../components/ui';
import { useLoad } from '../useLoad';

function CustomerFormModal({ companyId, onSaved, onClose }: { companyId: number; onSaved: () => void; onClose: () => void }) {
  const [form, setForm] = useState({ name: '', email: '', phone: '' });
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const set = (key: keyof typeof form) => (e: ChangeEvent<HTMLInputElement>) =>
    setForm({ ...form, [key]: e.target.value });

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await api.customers.create({
        name: form.name.trim(),
        email: form.email.trim(),
        phone: form.phone.trim() || null,
        companyId,
      });
      onSaved();
    } catch (err) {
      setError(errorMessage(err));
      setBusy(false);
    }
  }

  return (
    <Modal title="Ny kunde" onClose={onClose}>
      <form onSubmit={submit} className="form">
        <Field label="Navn" hint="Kundens bedrift, for eksempel Coop">
          <input className="input" required maxLength={200} value={form.name} onChange={set('name')} />
        </Field>
        <Field label="E-post">
          <input className="input" type="email" required maxLength={255} value={form.email} onChange={set('email')} />
        </Field>
        <Field label="Telefon" hint="Valgfri">
          <input className="input" type="tel" maxLength={30} value={form.phone} onChange={set('phone')} />
        </Field>

        <ErrorNote message={error} />

        <div className="form-actions">
          <button type="button" className="btn" onClick={onClose}>
            Avbryt
          </button>
          <button type="submit" className="btn btn-primary" disabled={busy}>
            Opprett kunde
          </button>
        </div>
      </form>
    </Modal>
  );
}

export function CustomersPage() {
  const { user } = useAuth();
  const customers = useLoad(() => api.customers.list(), 'customers');
  const [creating, setCreating] = useState(false);

  if (!user) return null;
  const list = (customers.data ?? []).filter((c) => c.companyId === user.companyId);

  return (
    <>
      <PageHeader
        title="Kunder"
        subtitle="Bedriftene dere jobber for."
        actions={
          <button type="button" className="btn btn-primary" onClick={() => setCreating(true)}>
            Ny kunde
          </button>
        }
      />

      <ErrorNote message={customers.error} />
      {customers.loading && <Loading />}

      {customers.data && list.length === 0 && (
        <EmptyState title="Ingen kunder ennå">Opprett en kunde før du lager det første prosjektet.</EmptyState>
      )}

      {list.length > 0 && (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Navn</th>
                <th>E-post</th>
                <th>Telefon</th>
              </tr>
            </thead>
            <tbody>
              {list.map((c) => (
                <tr key={c.id}>
                  <td className="row-title">{c.name}</td>
                  <td>{c.email}</td>
                  <td>{c.phone ?? '–'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {creating && (
        <CustomerFormModal
          companyId={user.companyId}
          onClose={() => setCreating(false)}
          onSaved={() => {
            setCreating(false);
            customers.reload();
          }}
        />
      )}
    </>
  );
}
