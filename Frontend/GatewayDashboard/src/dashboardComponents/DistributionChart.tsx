import React from "react";
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid,
  Tooltip, ResponsiveContainer, Cell,
} from "recharts";

interface DistItem {
  month: string;
  sales: number;
}

function CustomTooltip({ active, payload, label }: any) {
  if (!active || !payload?.length) return null;
  return (
    <div className="bg-[#1c1917] text-white text-xs rounded px-2 py-1.5 shadow-lg">
      <div className="font-medium">{label}</div>
      <div>Requests: {payload[0].value.toLocaleString()}</div>
    </div>
  );
}

export default function DistributionChart({ data }: { data: DistItem[] }) {
  return (
    <div>
      <div className="pt-6 px-2 pb-0">
        <ResponsiveContainer width="100%" height={240}>
          <BarChart data={data} margin={{ top: 5, right: 20, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="5 5" stroke="#292524" vertical={false} />
            <XAxis
              dataKey="month"
              tickLine={false}
              axisLine={false}
              tick={{ fill: "#616161", fontSize: 12, fontFamily: "inherit", fontWeight: 400 }}
            />
            <YAxis
              tickLine={false}
              axisLine={false}
              tick={{ fill: "#616161", fontSize: 12, fontFamily: "inherit", fontWeight: 400 }}
            />
            <Tooltip content={<CustomTooltip />} cursor={{ fill: "rgba(255,255,255,0.04)" }} />
            <Bar dataKey="sales" barSize={24} radius={[2, 2, 0, 0]}>
              {data.map((_, i) => (
                <Cell key={i} fill="#f96717" fillOpacity={0.8} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}