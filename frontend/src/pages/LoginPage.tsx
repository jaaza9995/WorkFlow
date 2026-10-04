import { useState } from 'react';
import type { ChangeEvent, FormEvent } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { errorMessage } from '../api/client';
import { ErrorNote, Field } from '../components/ui';
import { useAuth } from '../auth/useAuth';
import { taskStatusLabel, taskStatuses } from '../labels';

export function LoginPage() {
  const { user, login, registerCompany } = useAuth();
  const location = useLocation();
  const from = (location.state as { from?: string } | null)?.from ?? '/';

  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [form, setForm] = useState({
    email: '',
    password: '',
    companyName: '',
    firstName: '',
    lastName: '',
  });
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  if (user) return <Navigate to={from} replace />;

  const set = (key: keyof typeof form) => (e: ChangeEvent<HTMLInputElement>) =>
    setForm({ ...form, [key]: e.target.value });

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      if (mode === 'login') {
        await login(form.email.trim(), form.password);
      } else {
        await registerCompany({
          companyName: form.companyName.trim(),
          firstName: form.firstName.trim(),
          lastName: form.lastName.trim(),
          email: form.email.trim(),
          password: form.password,
        });
      }
    } catch (err) {
      setError(errorMessage(err));
      setBusy(false);
    }
  }

  const registering = mode === 'register';

  return (
    <div className="login">
      <section className="login-intro">
        <p className="brand brand-light">WorkFlow</p>
        <h1>Prosjekter, oppgaver og timer på ett sted</h1>
        <p className="login-lead">
          Se hva som skal gjøres, hvem som har ansvaret, og hvor langt hvert prosjekt har kommet.
        </p>

        <ol className="flow" aria-label="Slik går en oppgave gjennom systemet">
          {taskStatuses.map((status) => (
            <li key={status}>{taskStatusLabel[status]}</li>
          ))}
        </ol>
      </section>

      <section className="login-form">
        <form onSubmit={submit} className="form">
          <h2>{registering ? 'Opprett bedrift' : 'Logg inn'}</h2>

          {registering && (
            <>
              <Field label="Bedriftens navn">
                <input className="input" required maxLength={200} value={form.companyName} onChange={set('companyName')} />
              </Field>
              <div className="form-row">
                <Field label="Fornavn">
                  <input className="input" required maxLength={100} value={form.firstName} onChange={set('firstName')} />
                </Field>
                <Field label="Etternavn">
                  <input className="input" required maxLength={100} value={form.lastName} onChange={set('lastName')} />
                </Field>
              </div>
            </>
          )}

          <Field label="E-post">
            <input className="input" type="email" required autoComplete="email" value={form.email} onChange={set('email')} />
          </Field>

          <Field label="Passord" hint={registering ? 'Minst 8 tegn' : undefined}>
            <input
              className="input"
              type="password"
              required
              minLength={registering ? 8 : undefined}
              autoComplete={registering ? 'new-password' : 'current-password'}
              value={form.password}
              onChange={set('password')}
            />
          </Field>

          {registering && (
            <p className="muted small">
              Du blir administrator for den nye bedriften. Andre brukere legger du til etterpå.
            </p>
          )}

          <ErrorNote message={error} />

          <button type="submit" className="btn btn-primary btn-block" disabled={busy}>
            {registering ? 'Opprett bedrift' : 'Logg inn'}
          </button>

          <p className="muted small login-switch">
            {registering ? 'Har du allerede en konto?' : 'Er bedriften din ny i WorkFlow?'}{' '}
            <button
              type="button"
              className="link-button"
              onClick={() => {
                setMode(registering ? 'login' : 'register');
                setError(null);
              }}
            >
              {registering ? 'Logg inn' : 'Opprett bedrift'}
            </button>
          </p>
        </form>
      </section>
    </div>
  );
}
