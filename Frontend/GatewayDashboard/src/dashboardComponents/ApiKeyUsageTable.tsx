import type { ApiKey } from "@/apis/DashboardApi";
import React from "react";


function formatRelativeTime(iso: string): string {
  const seconds = Math.floor((Date.now() - new Date(iso).getTime()) / 1000);
  if (seconds < 60)   return `${seconds}s ago`;
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
  if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
  return `${Math.floor(seconds / 86400)}d ago`;
}

const shortKey = (k: string) => k ? `${k.slice(0, 8)}…` : "—";

export default function ApiKeyUsageTable({ rows }: { rows: ApiKey[] }) {
  return (
    <table className="w-full border-collapse font-mono text-sm mt-3">
      <thead>
        <tr className="text-muted-foreground text-xs">
          <th className="">Key</th>
          <th className="text-right font-normal pb-3">Requests</th>
          <th className="text-right font-normal pb-3">Avg Ms</th>
          <th className="text-right font-normal pb-3">Last Seen</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((row) => (
          <tr key={row.apikey} className="border-t border-white/5">
            <td className="py-3 text-gray-100">{shortKey(row.apikey)}</td>
            <td className="py-3 text-right text-gray-100 tabular-nums">
              {row.requests.toLocaleString()}
            </td>
            <td className="py-3 text-right text-gray-400 tabular-nums">
              {row.avgMs.toFixed(1)}ms
            </td>
            <td className="py-3 text-right text-muted-foreground tabular-nums">
              {formatRelativeTime(row.lastSeen)}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}