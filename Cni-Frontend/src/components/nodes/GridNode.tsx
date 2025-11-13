import { memo } from 'react';
import { Handle, Position } from '@xyflow/react';
import { CustomNodeData } from '../../types/node-types';

interface GridNodeProps {
    data: CustomNodeData;
}

export const GridNode = memo(({ data }: GridNodeProps) => {
    return (
        <div className="p-4 border-1 border-gray-500 border-dashed shadow-lg flex flex-column align-items-center text-white text-3xl" style={{ minHeight: '330px' }}>
            <Handle type="target" position={Position.Top} />

            <img height={80} src='/assets/icons/grid.svg' alt=''></img>
            <h3 className="text-2xl font-bold">Grid</h3>
            <div className="flex flex-column gap-2 mt-2">
                <div className="flex justify-content-between border-1 p-2 bg-black text-yellow-500">
                    <span>Consuption:</span>
                    <span>{data.powerConsumption} kW</span>
                </div>
                <div className="flex justify-content-between border-1 p-2 bg-black text-yellow-500">
                    <span>Reactive P:</span>
                    <span>{data.reactivePower} P/S</span>
                </div>
                <div className="flex justify-content-between border-1 p-2 bg-black text-yellow-500">
                    <span>Power Factor:</span>
                    <span>{data.powerFactor} P/S</span>
                </div>
            </div>

            <Handle type="source" position={Position.Bottom} />
        </div>
    );
});

GridNode.displayName = 'GridNode';
