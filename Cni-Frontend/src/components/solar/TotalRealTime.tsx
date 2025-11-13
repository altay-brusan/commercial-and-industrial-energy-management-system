import React from 'react';
import dynamic from 'next/dynamic';
import { COLORS } from '@/src/utils/color';
import { ApexOptions } from 'apexcharts';

const Chart = dynamic(() => import('react-apexcharts'), { ssr: false });

type ChartData = {
  time: string;
  prediction: number;
  production: number;
};

type SeriesConfig = {
  name: string;
  field: keyof ChartData;
  color: string;
};

type TotalRealTimeProps = {
  data: ChartData[];
  externalData?: ChartData[];
  series?: SeriesConfig[];
};

const TotalRealTime = ({ data, externalData, series }: TotalRealTimeProps) => {
  const usedData = externalData || data;
  const defaultSeries = [
    { name: 'Prediction', field: 'prediction', color: '#0d6efd' },
    { name: 'Production', field: 'production', color: '#198754' }
  ];
  const usedSeries = series || defaultSeries;

  const chartOptions: ApexOptions = {
    chart: {
      background: "#fff",
      type: 'area',
      height: 500,
      zoom: {
        type: 'x',
        enabled: true,
        autoScaleYaxis: true
      },
      toolbar: {
        show: true,
        tools: {
          download: false,
          selection: true,
          zoom: true,
          zoomin: true,
          zoomout: true,
          pan: true,
          reset: true
        }
      }
    },
    colors: usedSeries.map(s => s.color),
    dataLabels: {
      enabled: false
    },
    stroke: {
      curve: 'smooth',
      width: 2
    },
    fill: {
      type: 'gradient',
      gradient: {
        shadeIntensity: 1,
        inverseColors: false,
        opacityFrom: 0.5,
        opacityTo: 0.1,
        stops: [0, 90, 100]
      }
    },
    yaxis: {
      title: {
        text: 'Energy (kWh)'
      },
      labels: {
        formatter: function (val: number) {
          return val.toFixed(2);
        }
      }
    },
    xaxis: {
      categories: usedData.map(item => item.time),
      labels: {
        rotate: -45,
        rotateAlways: true,
        maxHeight: 100,
        trim: true,
        style: {
          fontSize: '11px'
        },
        formatter: function(value: any) {
            if (!value) return '';
            const parts = String(value).split(' ');
            return parts.length > 1 ? parts[1] : value;
          },
      },
      tickAmount: 10,
      axisTicks: {
        show: true
      },
      axisBorder: {
        show: true
      },
      tooltip: {
        enabled: true
      }
    },
    tooltip: {
      shared: true,
      intersect: false,
      y: {
        formatter: function (val: number) {
          return val.toFixed(2) + ' kWh';
        }
      }
    },
    legend: {
      position: 'top',
      horizontalAlign: 'right',
      floating: true,
      offsetY: -25,
      offsetX: -5
    }
  };

  const chartSeries = usedSeries.map(s => ({
    name: s.name,
    data: usedData.map(item => item[s.field as keyof typeof item])
  }));

  return (
    <div className="w-full" role="img" aria-label="Total Real Time Chart">
      <Chart
        options={chartOptions}
        series={chartSeries as any}
        type="area"
        height={500}
      />
    </div>
  );
};

export default TotalRealTime;
