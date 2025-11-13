import { Handle, Position } from '@xyflow/react';
import React from 'react';

interface EvChargerNodeProps {
    data: {
        powerConsumption: number;
    };
}

export const EvChargerNode: React.FC<EvChargerNodeProps> = ({ data }) => {
    return (
        <div className="p-4 border-1 border-gray-500 border-dashed shadow-lg flex flex-column align-items-center text-3xl" style={{ minHeight: '330px' }}>
            <Handle type="target" position={Position.Top} />

            <img height={80} src='/assets/icons/ev-charger.svg' alt=''></img>
            <h3 className="text-2xl font-bold">EV Charger</h3>
            <div className="flex flex-column gap-2 mt-2">
                <div className="flex justify-content-between border-1 p-2 bg-black text-yellow-500">
                    <span>Power Consumption: </span>
                    <span>{data.powerConsumption} kW</span>
                </div>
            </div>

            <Handle type="source" position={Position.Bottom} />
        </div>
    );
};

