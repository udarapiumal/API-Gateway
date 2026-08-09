import type { StatusCode } from "@/apis/DashboardApi";
import React from "react";
import { PieChart, Pie, Cell } from "recharts";


const COLORS: Record<number, string> = {
  200: "#4ade80",
  401: "#f87171",
  429: "#fbbf24",
};

const DEFAULT_COLOR = "#a8a29e";

const statusLabel = (code: number): string => {
  if (code === 200) return "200 OK";
  if (code === 401) return "401 Unauth";
  if (code === 429) return "429 Limited";
  return String(code);
};

export default function DoughnutChart({ statusData }: { statusData: StatusCode[] }) {
  const data = statusData.map((s) => ({
    name:  statusLabel(s.statusCode),
    value: s.count,
    color: COLORS[s.statusCode] ?? DEFAULT_COLOR,
  }));

  return (
    <div>
      <div className="py-6 flex justify-center">
        <div className="relative w-[240px] h-[240px]">
          <PieChart width={240} height={240}>
            <Pie
              data={data}
              dataKey="value"
              nameKey="name"
              innerRadius={80}
              outerRadius={110}
              paddingAngle={2}
              stroke="none"
            >
              {data.map((entry, i) => (
                <Cell key={i} fill={entry.color} />
              ))}
            </Pie>
          </PieChart>
        </div>
      </div>

      {/* Legend */}
      <div className="flex flex-wrap justify-center gap-4 -mt-2 mb-2">
        {data.map((entry) => (
          <div key={entry.name} className="flex items-center gap-1.5 text-sm text-gray-400">
            <span
              className="w-2.5 h-2.5 rounded-full"
              style={{ backgroundColor: entry.color }}
            />
            <span>{entry.name}</span>
            <span className="text-gray-500">({entry.value.toLocaleString()})</span>
          </div>
        ))}
      </div>
    </div>
  );
}