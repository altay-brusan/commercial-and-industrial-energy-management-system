/* eslint-disable react-hooks/exhaustive-deps */
import { useCallback, useEffect, useState } from 'react';

interface UseFetchOptions<T> {
  setLoading?: (isLoading: boolean) => void;
  deps?: readonly any[];
  reloadDeps?: readonly any[];
  defaultValue?: T | null;
}

export function useFetch<T>(
  fetcher: () => Promise<T>,
  options: UseFetchOptions<T> = {}
): [T | null, () => Promise<void>, React.Dispatch<React.SetStateAction<T | null>>] {
  const {
    defaultValue = null,
    setLoading = () => ({}),
    deps = [],
    reloadDeps = []
  } = options;

  const [data, setData] = useState<T | null>(defaultValue);

  const fetch = useCallback(async () => {
    try {
      setLoading(true);
      const result = await fetcher();
      setData(result);
    } catch (error) {
      console.error('Fetch error:', error);
    } finally {
      setLoading(false);
    }
  }, deps);

  useEffect(() => {
    fetch();
  }, reloadDeps);

  return [data, fetch, setData];
}
