import { useState, useEffect, useCallback } from "react";
import { fetchAll } from "./apis/DashboardApi";
import type { DashboardData } from "./apis/DashboardApi";


interface UseDashboardResult {
  data: DashboardData | null;
  loading: boolean;
  error: string | null;
  lastRefresh: Date;
  refresh: () => void;
}

export function useDashboard(intervalMs = 10000): UseDashboardResult {
  const [data, setData]               = useState<DashboardData | null>(null);
  const [loading, setLoading]         = useState(true);
  const [error, setError]             = useState<string | null>(null);
  const [lastRefresh, setLastRefresh] = useState<Date>(new Date());

  const load = useCallback(async () => {
    try {
      setError(null);
      const result = await fetchAll();
      setData(result);
      setLastRefresh(new Date());
    } catch (e: any) {
      setError(e.message ?? "Unknown error");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
    const t = setInterval(load, intervalMs);
    return () => clearInterval(t);
  }, [load, intervalMs]);

  return { data, loading, error, lastRefresh, refresh: load };
}