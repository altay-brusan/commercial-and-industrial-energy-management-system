import { memo } from 'react';
import { Handle, Position } from '@xyflow/react';
import { CustomNodeData } from '../../types/node-types';

interface TransformerNodeProps {
    data: CustomNodeData;
}

export const TransformerNode = memo(({ data }: TransformerNodeProps) => {
    return (
        <div className="p-2 text-3xl">
            <Handle type="target" position={Position.Top} />
            <div className='flex gap-1 items-center relative'>
                <span className='text-2xl font-bold border-1 border-gray-500 flex justify-content-center align-items-center px-4 py-4'>MPPT</span>
                <div className='flex flex-column absolute' style={{ top: '50%', right: '-90px', transform: 'translateY(-50%)' }}>
                    <span className='font-bold'>{data.label}</span>
                    <span className='text-yellow-500 bg-black p-1'>{data.value}</span>
                </div>
            </div>
            <Handle type="source" position={Position.Bottom} />
        </div>

    );
});

TransformerNode.displayName = 'TransformerNode';
