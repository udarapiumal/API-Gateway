import type { RecentRequest } from "@/apis/DashboardApi";
import React, { useState, useEffect } from "react";


function statusColor(status: number): string {
  if (status >= 500) return "text-red-400";
  if (status >= 400) return "text-orange-400";
  return "text-green-400";
}

function formatRelativeTime(iso: string): string {
  const seconds = Math.floor((Date.now() - new Date(iso).getTime()) / 1000);
  if (seconds < 1)    return "0s ago";
  if (seconds < 60)   return `${seconds}s ago`;
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
  return `${Math.floor(seconds / 3600)}h ago`;
}

const shortKey = (k: string) => k ? `${k.slice(0, 8)}…` : "—";

export default function LiveRequestFeed({ events }: { events: RecentRequest[] }) {
  // Re-render every second so "Xs ago" labels stay current
  const [, forceTick] = useState(0);
  useEffect(() => {
    const t = setInterval(() => forceTick((n) => n + 1), 1000);
    return () => clearInterval(t);
  }, []);

  return (
    <div className="mt-3 max-h-72 overflow-y-auto pr-1 font-mono text-sm">
      {events.map((event) => (
        <div
          key={event.id}
          className="flex items-center gap-4 py-2.5 border-t border-white/5 first:border-t-0"
        >
          <span className={`w-10 shrink-0 font-semibold ${statusColor(event.statusCode)}`}>
            {event.statusCode}
          </span>
          <span className="w-24 shrink-0 text-gray-500 truncate">
            {shortKey(event.apiKey)}
          </span>
          <span className="flex-1 text-orange-400">{event.path}</span>
          <span className="w-14 shrink-0 text-right text-gray-100 tabular-nums">
            {event.responseTimeMs}ms
          </span>
          <span className="w-16 shrink-0 text-right text-muted-foreground tabular-nums">
            {formatRelativeTime(event.timestamp)}
          </span>
        </div>
      ))}
    </div>
  );
}