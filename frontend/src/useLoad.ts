import { useCallback, useEffect, useRef, useState } from 'react';
import { errorMessage } from './api/client';

// Henter data når `key` endres. Mens nye data hentes, vises de gamle (ingen blinking).
// Kall reload() etter at du har endret noe, for å hente på nytt.
export function useLoad<T>(loader: () => Promise<T>, key: string) {
  const [data, setData] = useState<T | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [version, setVersion] = useState(0);

  const loaderRef = useRef(loader);
  useEffect(() => {
    loaderRef.current = loader;
  });

  useEffect(() => {
    let cancelled = false;

    loaderRef
      .current()
      .then((result) => {
        if (cancelled) return;
        setData(result);
        setError(null);
      })
      .catch((e: unknown) => {
        if (!cancelled) setError(errorMessage(e));
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [key, version]);

  const reload = useCallback(() => setVersion((v) => v + 1), []);

  return { data, error, loading, reload };
}
