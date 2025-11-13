import { memo } from 'react';
import { Handle, Position } from '@xyflow/react';
import { CustomNodeData } from '../../types/node-types';
import { Badge } from 'primereact/badge';
import { COLORS } from '@/src/utils/color';

interface BessNodeProps {
    data: CustomNodeData;
}

export const BessNode = memo(({ data }: BessNodeProps) => {
    const soc = data.soc || 0;

    const getBatteryColor = () => {
        if (soc > 80) return 'bg-green-500';
        if (soc > 60) return 'bg-green-400';
        if (soc > 30) return 'bg-yellow-500';
        if (soc > 10) return 'bg-orange-500';
        return 'bg-red-500';
    };

    const getStateColor = () => {
        switch (data.state) {
            case 'charging': return 'bg-green-500';
            case 'discharging': return 'bg-orange-400';
            case 'standby': return 'bg-blue-500';
            case 'error': return 'bg-red-500';
            default: return 'bg-blue-300';
        }
    };

    const getTemperatureClass = () => {
        const temp = data.temperature || 0;
        if (temp > 60) return 'text-red-500';
        if (temp > 45) return 'text-orange-500';
        if (temp > 30) return 'text-yellow-500';
        return 'text-green-500';
    };

    return (
        <div
            className="border-2 shadow-lg overflow-hidden transition-all duration-300 hover:shadow-2xl border-dashed text-3xl"
        >

            <div className="p-4">
                <div className="flex align-items-center mb-4">
                    <div className="relative border-2 border-gray-500 overflow-hidden flex flex-column" style={{ width: '6rem', height: '10rem' }}>

                        <div
                            className={`absolute bottom-0 w-full ${getBatteryColor()} transition-all duration-300`}
                            style={{ height: `${soc}%` }}
                        ></div>

                        <div className="absolute top-0 right-0 bottom-0 left-0 flex align-items-center justify-content-center text-lg font-bold text-white z-10">
                            {soc}%
                        </div>
                    </div>


                    <div className="ml-4 flex-grow-1">
                        <div className='font-bold'>{data.label}</div>
                        <div className="bg-black text-green-500 mt-1 p-1 px-2">Soc: {soc}%</div>
                        <div className="bg-black text-red-500 mt-1 p-1 px-2">Temp: {data.temperature}°C</div>
                        <div className="bg-black text-gray-400 mt-1 p-1 px-2">State: {data.state}</div>
                    </div>
                </div>
            </div>

            <Handle type="source" position={Position.Bottom} />
        </div>
    );
});

BessNode.displayName = 'BessNode';
