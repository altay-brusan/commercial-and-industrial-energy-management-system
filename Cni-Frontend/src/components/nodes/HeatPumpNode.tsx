import { Handle, Position } from '@xyflow/react';
import React from 'react';

interface HeatPumpNodeProps {
    data: {
        powerConsumption: number;
    };
}

export const HeatPumpNode: React.FC<HeatPumpNodeProps> = ({ data }) => {
    return (
        <div className="p-4 border-1 border-gray-500 border-dashed shadow-lg flex flex-column align-items-center text-3xl" style={{ minHeight: '330px' }}>
            <Handle type="target" position={Position.Top} />

            <img height={80} src='/assets/icons/heat-pumb.svg' alt=''></img>
            <h3 className="text-3xl font-bold">Heat Pump</h3>
            <div className="flex flex-column gap-2 mt-2">
                <div className="flex justify-content-between p-2 bg-black text-yellow-500">
                    <span>Power Consumption: </span>
                    <span>{data.powerConsumption} kW</span>
                </div>
            </div>

            <Handle type="source" position={Position.Bottom} />
        </div>
    );
};
