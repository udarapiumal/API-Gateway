import type { Path } from "@/apis/DashboardApi";
import React from "react";


const ERROR_THRESHOLD = 2;

export default function TopPathsTable({ rows }: { rows: Path[] }) {
  return (
    <div>
      <table className="mt-6 w-full border-collapse">
        <thead>
          <tr className="text-gray-500 text-xs">
            <th>Path</th>
            <th className="text-right font-normal pb-3">Requests</th>
            <th className="text-right font-normal pb-3">Avg Ms</th>
            <th className="text-right font-normal pb-3">Error %</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.path} className="border-t border-white/5">
              <td className="py-3 text-orange-400">{row.path}</td>
              <td className="py-3 text-right text-gray-100 tabular-nums">
                {row.requests.toLocaleString()}
              </td>
              <td className="py-3 text-right text-gray-400 tabular-nums">
                {row.avgMs.toFixed(1)}ms
              </td>
              <td
                className={`py-3 text-right tabular-nums ${
                  row.errorRate >= ERROR_THRESHOLD ? "text-red-400" : "text-gray-400"
                }`}
              >
                {row.errorRate.toFixed(1)}%
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}