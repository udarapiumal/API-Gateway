import { useEffect, useRef } from "react";
import ApexCharts, { type ApexOptions } from "apexcharts";

interface ChartProps {
  series: number[];
  categories: string[];
}

export function Chart({ series, categories }: ChartProps) {
  const chartRef = useRef<HTMLDivElement | null>(null);
  const chartInstance = useRef<ApexCharts | null>(null);

  useEffect(() => {
    if (!chartRef.current) return;

    // Destroy previous instance before creating a new one
    if (chartInstance.current) {
      chartInstance.current.destroy();
    }

    const chartConfig: ApexOptions = {
      series: [
        {
          name: "Requests",
          data: series,
        },
      ],
      chart: {
        type: "line",
        height: 240,
        background: "transparent",
        toolbar: { show: false },
      },
      title: { text: "" },
      dataLabels: { enabled: false },
      colors: ["#f96717"],
      stroke: {
        lineCap: "round",
        curve: "smooth",
        width: 2,
      },
      markers: { size: 0 },
      xaxis: {
        axisTicks:  { show: false },
        axisBorder: { show: false },
        categories,
        labels: {
          style: {
            colors: "#616161",
            fontSize: "12px",
            fontFamily: "inherit",
            fontWeight: 400,
          },
        },
      },
      yaxis: {
        labels: {
          style: {
            colors: "#616161",
            fontSize: "12px",
            fontFamily: "inherit",
            fontWeight: 400,
          },
        },
      },
      grid: {
        show: true,
        borderColor: "#292524",
        strokeDashArray: 5,
        xaxis: { lines: { show: true } },
        padding: { top: 5, right: 20 },
      },
      fill:    { opacity: 0.8 },
      tooltip: { theme: "dark" },
      theme:   { mode: "dark" },
    };

    chartInstance.current = new ApexCharts(chartRef.current, chartConfig);
    chartInstance.current.render();

    return () => {
      chartInstance.current?.destroy();
      chartInstance.current = null;
    };
  }, [series, categories]); // re-render chart when data changes

  return (
    <div className="relative flex flex-col rounded-xl bg-clip-border text-gray-700 shadow-md">
      <div className="px-2 pt-6 pb-0">
        <div ref={chartRef} />
      </div>
    </div>
  );
}