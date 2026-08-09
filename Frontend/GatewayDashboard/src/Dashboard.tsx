import { Card, CardContent } from "@/components/ui/card";
import { Chart } from "./dashboardComponents/Chart";
import DoughnutChart from "./dashboardComponents/doughnutChart";
import DistributionChart from "./dashboardComponents/DistributionChart";
import TopPathsTable from "./dashboardComponents/TopPaths";
import ApiKeyUsageTable from "./dashboardComponents/ApiKeyUsageTable";
import LiveRequestFeed from "./dashboardComponents/RequestFeed";
import { useDashboard } from "./Usedashboard";


// ── Helpers ───────────────────────────────────────────────────────────────────
const fmt    = (n?: number) => n != null ? n.toLocaleString() : "—";
const fmtMs  = (ms?: number) => ms != null ? `${ms.toFixed(1)}ms` : "—";
const fmtPct = (p?: number)  => p  != null ? `${p.toFixed(1)}%`  : "—";

// ── Stat card ─────────────────────────────────────────────────────────────────
function StatCard({
  label, value, sub,
}: {
  label: string;
  value: string;
  sub: string;
}) {
  return (
    <Card className="p-6 py-4 shadow-2xs bg-[#1c1917]">
      <CardContent className="p-0">
        <dt className="font-medium text-muted-foreground text-sm text-start">
          {label}
        </dt>
        <dd className="mt-2 flex items-baseline space-x-2.5">
          <span className="font-semibold text-3xl text-[#f96717] tabular-nums">
            {value}
          </span>
        </dd>
        <dt className="font-small text-muted-foreground text-sm text-start">
          {sub}
        </dt>
      </CardContent>
    </Card>
  );
}

// ── Error banner ──────────────────────────────────────────────────────────────
function ErrorBanner({ message, onRetry }: { message: string; onRetry: () => void }) {
  return (
    <div className="mx-10 mt-4 flex items-center justify-between rounded-lg border border-orange-900 bg-orange-950 px-4 py-3">
      <span className="text-sm text-red-400">Failed to load: {message}</span>
      <button
        onClick={onRetry}
        className="rounded bg-[#f97316] px-3 py-1 text-xs font-semibold text-white hover:bg-orange-500"
      >
        Retry
      </button>
    </div>
  );
}

// ── Dashboard ─────────────────────────────────────────────────────────────────
export function Dashboard() {
  const { data, loading, error, lastRefresh, refresh } = useDashboard(10000);

  // Map distribution to the shape DistributionChart expects
  const distData = data ? [
    { month: "< 10ms",   sales: data.distribution.under10  },
    { month: "10–50ms",  sales: data.distribution.under50  },
    { month: "50–100ms", sales: data.distribution.under100 },
    { month: "> 100ms",  sales: data.distribution.over100  },
  ] : [];

  // Map status codes to the shape DoughnutChart expects
  const statusData = data?.statuscodes.map(s => ({
    statusCode: s.statusCode,
    count: s.count,
  })) ?? [];

  // Map timeseries to the shape Chart (ApexCharts) expects
  const timeseriesValues = data?.timeseries.map(t => t.requests) ?? [];
  const timeseriesCategories = data?.timeseries.map(t =>
    new Date(t.time).toLocaleTimeString([], { 
      hour: "2-digit", 
      minute: "2-digit",
      timeZone: "Asia/Colombo" // ← or "UTC" if you prefer
    })
  ) ?? [];
  return (
    <>
    

      {/* ── Error banner ── */}
      {error && <ErrorBanner message={error} onRetry={refresh} />}

      {/* ── Loading state ── */}
      {loading && (
        <div className="flex items-center justify-center h-64 text-muted-foreground text-sm">
          Loading dashboard…
        </div>
      )}

      {/* ── Main grid ── */}
      {!loading && data && (
        <div className="flex w-full items-center justify-center p-10">
          <dl className="grid w-full grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">

            {/* ── Stat cards ── */}
            <StatCard
              label="TOTAL REQUESTS"
              value={fmt(data.summary.totalRequests)}
              sub="all time"
            />
            <StatCard
              label="AVG RESPONSE TIME"
              value={fmtMs(data.summary.avgResponseTimeMs)}
              sub="across all routes"
            />
            <StatCard
              label="ERROR RATE"
              value={fmtPct(data.summary.errorRate)}
              sub="429 + 401 responses"
            />
            <StatCard
              label="ACTIVE API KEYS"
              value={fmt(data.summary.activeApikeys)}
              sub="unique callers"
            />

            {/* ── Timeseries chart ── */}
            <Card className="h-full border-none bg-[#1c1917] p-6 py-4 shadow-2xs sm:col-span-1 lg:col-span-3">
              <CardContent className="p-0">
                <dt className="text-start font-medium text-muted-foreground text-sm">
                  REQUEST VOLUME — LAST 60 MIN
                </dt>
                {/* Pass real data to Chart — update Chart.tsx to accept these props */}
                <Chart
                  series={timeseriesValues}
                  categories={timeseriesCategories}
                />
              </CardContent>
            </Card>

            {/* ── Donut chart ── */}
            <Card className="h-full border-none bg-[#1c1917] p-6 py-4 shadow-2xs sm:col-span-1 lg:col-span-1">
              <CardContent className="p-0">
                <dt className="text-start font-medium text-muted-foreground text-sm">
                  STATUS CODES
                </dt>
                {/* Pass real data to DoughnutChart — update it to accept statusData prop */}
                <DoughnutChart statusData={statusData} />
              </CardContent>
            </Card>

            {/* ── Distribution chart ── */}
            <Card className="h-full border-none bg-[#1c1917] p-6 py-4 shadow-2xs sm:col-span-1 lg:col-span-2">
              <CardContent className="p-0">
                <dt className="text-start font-medium text-muted-foreground text-sm">
                  RESPONSE TIME DISTRIBUTION
                </dt>
                {/* Pass real data to DistributionChart — update it to accept data prop */}
                <DistributionChart data={distData} />
              </CardContent>
            </Card>

            {/* ── Top paths ── */}
            <Card className="h-full border-none bg-[#1c1917] p-6 py-4 shadow-2xs sm:col-span-1 lg:col-span-2">
              <CardContent className="p-0">
                <dt className="text-start font-medium text-muted-foreground text-sm">
                  TOP PATHS
                </dt>
                {/* Pass real data — update TopPathsTable to accept rows prop */}
                <TopPathsTable rows={data.paths} />
              </CardContent>
            </Card>

            {/* ── API key usage ── */}
            <Card className="h-full border-none bg-[#1c1917] p-6 py-4 shadow-2xs sm:col-span-1 lg:col-span-2">
              <CardContent className="p-0">
                <dt className="text-start font-medium text-muted-foreground text-sm">
                  API KEY USAGE
                </dt>
                {/* Pass real data — update ApiKeyUsageTable to accept rows prop */}
                <ApiKeyUsageTable rows={data.apikeys} />
              </CardContent>
            </Card>

            {/* ── Live feed ── */}
            <Card className="h-full border-none bg-[#1c1917] p-6 py-4 shadow-2xs sm:col-span-1 lg:col-span-2">
              <CardContent className="p-0">
                <dt className="text-start font-medium text-muted-foreground text-sm">
                  LIVE REQUEST FEED
                </dt>
                {/* Pass real data — update LiveRequestFeed to accept events prop */}
                <LiveRequestFeed events={data.recent} />
              </CardContent>
            </Card>

          </dl>
        </div>
      )}
    </>
  );
}