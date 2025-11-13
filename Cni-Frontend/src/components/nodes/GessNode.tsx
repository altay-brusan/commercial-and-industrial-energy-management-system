import { memo } from 'react';
import { Handle, Position } from '@xyflow/react';
import { CustomNodeData } from '../../types/node-types';
import { WiDaySunny, WiDayCloudy, WiCloudy } from 'react-icons/wi';
import { IoIosNotifications } from 'react-icons/io';
import { useRouter } from 'next/navigation';
import { Tooltip } from 'primereact/tooltip';

interface GessNodeProps {
    data: CustomNodeData;
}

export const GessNode = memo(({ data }: GessNodeProps) => {
    const performance = data.performance || 0;
    const generation = data.generation || 0;
    const temperature = data.temperature || 0;
    const router = useRouter();

    const getPerformanceIcon = () => {
        if (performance > 80) return <WiDaySunny size={36} className="text-yellow-500" />;
        if (performance > 50) return <WiDayCloudy size={36} className="text-yellow-400" />;
        if (performance > 20) return <WiCloudy size={36} className="text-yellow-300" />;
        return <WiCloudy size={36} className="text-gray-400" />;
    };

    const getPerformanceColor = () => {
        if (performance > 80) return '#f59e0b';
        if (performance > 50) return '#d97706';
        if (performance > 20) return '#92400e';
        return '#78716c';
    };

    const getTemperatureSeverity = () => {
        if (temperature > 65) return 'danger';
        if (temperature > 45) return 'warning';
        return 'success';
    };

    return (
        <div
            className="border-2 shadow-lg overflow-hidden transition-all duration-300 hover:shadow-2xl border-dashed relative text-3xl"
        >
            <div className="p-4">
                <div className="flex align-content-center justify-content-center gap-4 mb-4">
                    <img width={90} src='/assets/icons/solar.svg' alt=''></img>
                    <div className="flex-grow-1">
                        <div className='font-bold'>{data.label}</div>
                        <div className="bg-black text-green-500 mt-1 p-1">Performance: {performance}%</div>
                        <div className="bg-black text-red-500 mt-1 p-1">Temperature: {data.temperature}°C</div>
                        <div className="bg-black text-gray-400 mt-1 p-1">Generation: {data.generation} kWh</div>
                    </div>
                </div>
            </div>
            <Handle type="source" position={Position.Bottom} />
            {data.temperature && data.temperature > 65 && (
                <div className="absolute top-0 right-0" onClick={() => router.push(`/notification`)}>
                    <Tooltip target=".custom-target-icon" />

                    <IoIosNotifications
                        className="custom-target-icon"
                        data-pr-tooltip="Temperature is too high"
                        data-pr-position="right"
                        data-pr-at="right+5 top"
                        data-pr-my="left center-2"
                        size={30} color='red' />
                </div>
            )}
        </div>
    );
});

GessNode.displayName = 'GessNode';
