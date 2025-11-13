'use client';
import { useEffect, useLayoutEffect, useRef } from 'react';
import * as am5 from '@amcharts/amcharts5';
import * as am5xy from '@amcharts/amcharts5/xy';
import * as am5radar from '@amcharts/amcharts5/radar';
import am5themes_Animated from '@amcharts/amcharts5/themes/Animated';

type EfficiencyGaugeProps = {
    id: string;
    data: any[];
};

const EfficiencyGauge = ({ id, data }: EfficiencyGaugeProps) => {
    const chartRef = useRef<am5.Root | null>(null);

    useLayoutEffect(() => {

        const root = am5.Root.new(id);
        chartRef.current = root;
        root.setThemes([am5themes_Animated.new(root)]);

        const chart = root.container.children.push(
            am5radar.RadarChart.new(root, {
                panX: false,
                panY: false,
                startAngle: 180,
                endAngle: 360
            })
        );

        chart.radarContainer.children.push(am5.Label.new(root, {
            text: "Efficiency",
            fontSize: 20,
            fontWeight: "bold",
            textAlign: "center",
            x: am5.percent(50),
            y: am5.percent(-10)
        }));

        const axisRenderer = am5radar.AxisRendererCircular.new(root, {
            innerRadius: -10,
            strokeOpacity: 1,
            strokeWidth: 15,
            strokeGradient: am5.LinearGradient.new(root, {
                rotation: 0,
                stops: [
                    { color: am5.color(0x19d228) }, // Yeşil
                    { color: am5.color(0xf4fb16) }, // Sarı
                    { color: am5.color(0xf6d32b) }, // Turuncu
                    { color: am5.color(0xfb7116) }  // Kırmızı
                ]
            })
        });

        const xAxis = chart.xAxes.push(
            am5xy.ValueAxis.new(root, {
                maxDeviation: 0,
                min: 0,
                max: 100,
                strictMinMax: true,
                renderer: axisRenderer
            })
        );

        const axisDataItem = xAxis.makeDataItem({});
        axisDataItem.set("value", 0);


        axisDataItem.set("bullet", am5xy.AxisBullet.new(root, {
            sprite: am5radar.ClockHand.new(root, {
                radius: am5.percent(99)
            })
        }));

        xAxis.createAxisRange(axisDataItem);
        axisDataItem?.get("grid")?.set("visible", false);

        const interval = setInterval(() => {
            axisDataItem.animate({
                key: "value",
                to: Math.round(Math.random() * 100),
                duration: 800,
                easing: am5.ease.out(am5.ease.cubic)
            });
        }, 2000);

        chart.appear(1000, 100);

        return () => {
            clearInterval(interval);
            root.dispose();
        };
    }, [id, data]);

    return <div id={id} style={{ width: '100%', height: '300px' }} />;
};

export default EfficiencyGauge;
