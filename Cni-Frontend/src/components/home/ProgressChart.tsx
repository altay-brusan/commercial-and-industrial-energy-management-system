import { useEffect, useLayoutEffect, useRef } from 'react';
import * as am5 from '@amcharts/amcharts5';
import * as am5xy from '@amcharts/amcharts5/xy';
import am5themes_Animated from '@amcharts/amcharts5/themes/Animated';

type ProgressChartProps = {
  className?: string;
};

const ProgressChart = ({ className }: ProgressChartProps) => {
  const chartRef = useRef<am5.Root | null>(null);

  useLayoutEffect(() => {
    const root = am5.Root.new('chartdiv');
    chartRef.current = root;

    root.setThemes([am5themes_Animated.new(root)]);

    const chart = root.container.children.push(
      am5xy.XYChart.new(root, {
        panY: false,
        layout: root.horizontalLayout,
        paddingRight: 0,
        paddingLeft: 0,
      })
    );

    const data = [{
      category: '',
      şebeke: 68,
      bess: 12,
      jenerator: 20,
    }];

    const valueAxis = chart.yAxes.push(
      am5xy.ValueAxis.new(root, {
        renderer: am5xy.AxisRendererY.new(root, {
          strokeOpacity: 0.1
        })
      })
    );

    valueAxis.get("renderer").grid.template.setAll({
      visible: false
    });

    // X ekseni (Kategori ekseni) - Yatay eksen
    const categoryAxis = chart.xAxes.push(
      am5xy.CategoryAxis.new(root, {
        categoryField: "category",
        renderer: am5xy.AxisRendererX.new(root, {})
      })
    );

    categoryAxis.get("renderer").grid.template.setAll({
      visible: false
    });

    const yAxis = chart.yAxes.push(
      am5xy.CategoryAxis.new(root, {
        categoryField: 'category',
        renderer: am5xy.AxisRendererY.new(root, {
          minGridDistance: 10,
        }),
      })
    );
    yAxis.data.setAll(data);

    const xAxis = chart.xAxes.push(
      am5xy.ValueAxis.new(root, {
        renderer: am5xy.AxisRendererX.new(root, {}),
        min: 0,
        max: 100,
        visible: false,
      })
    );

    const makeSeries = (name: string, field: string, color: am5.Color) => {
      const series = chart.series.push(
        am5xy.ColumnSeries.new(root, {
          name: name,
          xAxis: xAxis,
          yAxis: yAxis,
          valueXField: field,
          categoryYField: 'category',
          sequencedInterpolation: true,
        })
      );

      series.columns.template.setAll({
        height: 30,
        fill: color,
        strokeOpacity: 0,
      });

      series.columns.template.adapters.add('tooltipText', () => {
        return `${name}: {valueX}%`;
      });

      series.bullets.push(function() {
        return am5.Bullet.new(root, {
          locationY: 1,
          sprite: am5.Label.new(root, {
            textAlign: "center",
            text: "{valueX}%",
            fill: am5.color("#000"),
            populateText: true
          })
        });
      });

      series.data.setAll(data);
      return series;
    };

    // Serileri tanımla
    makeSeries('Şebeke', 'şebeke', am5.color('#9747FF'));
    makeSeries('BESS', 'bess', am5.color('#47E3A1'));
    makeSeries('Jenerator', 'jenerator', am5.color('#F7931E'));

    chart.series.each((series) => {
      series.set('stacked', true);
    });


    return () => {
      root.dispose();
    };
  }, []);

  return (
    <div
      id="chartdiv"
      style={{ height: '85px' }}
      className={`w-full ${className}`}
      role="img"
      aria-label="Güç kaynağı dağılımı grafiği"
    />
  );
};

export default ProgressChart;
