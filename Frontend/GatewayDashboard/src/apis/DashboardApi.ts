const BASE = "https://localhost:7097/api/dashboard";

// ── Types ─────────────────────────────────────────────────────────────────────

export interface Summary {
  totalRequests: number;
  avgResponseTimeMs: number;
  errorRate: number;
  activeApikeys: number;
}

export interface TimeSeries {
  time: string;
  requests: number;
}

export interface StatusCode {
  statusCode: number;
  count: number;
}

export interface Path {
  path: string;
  requests: number;
  avgMs: number;
  errorRate: number;
}

export interface ApiKey {
  apikey: string;
  requests: number;
  avgMs: number;
  lastSeen: string;
}

export interface RecentRequest {
  id: number;
  apiKey: string;
  path: string;
  method: string;
  statusCode: number;
  responseTimeMs: number;
  timestamp: string;
}

export interface Distribution {
  under10: number;
  under50: number;
  under100: number;
  over100: number;
}

export interface DashboardData {
  summary: Summary;
  timeseries: TimeSeries[];
  statuscodes: StatusCode[];
  paths: Path[];
  apikeys: ApiKey[];
  recent: RecentRequest[];
  distribution: Distribution;
}

// ── Fetch ─────────────────────────────────────────────────────────────────────

const get = async <T>(path: string): Promise<T> => {
  const res = await fetch(`${BASE}/${path}`, {
    headers: {
      "X-Api-Key": "550e8400-e29b-41d4-a716-446655440000"
    }
  });
  if (!res.ok) throw new Error(`Failed to fetch ${path}: ${res.status}`);
  return res.json();
};

export const fetchAll = async (): Promise<DashboardData> => {
  const [summary, timeseries, statuscodes, paths, apikeys, recent, distribution] =
    await Promise.all([
      get<Summary>("summary"),
      get<TimeSeries[]>("timeseries"),
      get<StatusCode[]>("statuscodes"),
      get<Path[]>("paths"),
      get<ApiKey[]>("apikeys"),
      get<RecentRequest[]>("recent"),
      get<Distribution>("distribution"),
    ]);

  return { summary, timeseries, statuscodes, paths, apikeys, recent, distribution };
};