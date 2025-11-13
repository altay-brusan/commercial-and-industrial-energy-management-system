import { memo } from 'react';
import { Handle, Position } from '@xyflow/react';
import { CustomNodeData } from '../../types/node-types';
import { FaWifi } from 'react-icons/fa';

interface SmartMeterNodeProps {
    data: CustomNodeData;
}

export const SmartMeterNode = memo(({ data }: SmartMeterNodeProps) => {
    return (
        <div className="">
            <Handle type="target" position={Position.Top} />
            <div className="flex flex-column justify-content-center align-items-center h-5rem w-5rem border-1 border-gray-500 shadow-lg border-circle">
                <FaWifi size={30} />
                <span className='font-bold text-3xl'>SM</span>
            </div>
            <Handle type="source" position={Position.Bottom} />
        </div>
    );
});

SmartMeterNode.displayName = 'SmartMeterNode';
