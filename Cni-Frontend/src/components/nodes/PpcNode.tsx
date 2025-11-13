import { memo } from 'react';
import { Handle, Position } from '@xyflow/react';
import { CustomNodeData } from '../../types/node-types';

interface PpcNodeProps {
    data: CustomNodeData;
}

export const PpcNode = memo(({ data }: PpcNodeProps) => {
    return (
        <div className="p-4 border shadow-lg bg-orange-400 text-white w-10rem flex flex-column align-items-center text-3xl">
            <Handle type="target" position={Position.Top} />
            <div className="text-3xl font-bold">PPC</div>
            <Handle type="source" position={Position.Bottom} />
        </div>
    );
});

PpcNode.displayName = 'PpcNode';
