import { COLORS } from '@/src/utils/color';
import { Handle, Position } from '@xyflow/react';
import React from 'react';

interface GeneratorNodeProps {
    data: {
        generation: number;
        fuel: number;
    };
}

export const GeneratorNode: React.FC<GeneratorNodeProps> = ({ data }) => {
    return (
        <div className="p-4 border-1 border-gray-500 border-dashed shadow-lg flex flex-column align-items-center text-white text-3xl" style={{ minHeight: '330px' }}>
            <Handle type="target" position={Position.Top} />

            <img height={80} src='/assets/icons/generator.svg' alt=''></img>
            <h3 className="text-2xl font-bold">Generator</h3>
            <div className="flex flex-column gap-2 mt-2">
                <div className="flex justify-content-between border-1 p-2 bg-black text-yellow-500">
                    <span>Generation: </span>
                    <span>{data.generation} kW</span>
                </div>
                <div className="flex justify-content-between border-1 p-2 bg-black text-yellow-500">
                    <span>Fuel</span>
                    <span>{data.fuel}%</span>
                </div>
            </div>

            <Handle type="source" position={Position.Bottom} />
        </div>
    );
};

