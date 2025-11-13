import { memo } from 'react';
import { Handle, Position } from '@xyflow/react';
import { CustomNodeData } from '../../types/node-types';
import { FaWifi } from 'react-icons/fa';

interface CapacitorNodeProps {
    data: CustomNodeData;
}

export const CapacitorNode = memo(({ data }: CapacitorNodeProps) => {
    return (
        <div className="relative text-2xl">
            <Handle type="target" position={Position.Top} />
            <div className='flex gap-1 items-center'>
                {data.value && <FaWifi className='text-red-500 absolute top-0 capacitor-wifi' />}
                <img width={40} src={'/assets/icons/capacitor.svg'} alt=''></img>
                <div className='flex flex-column capacitor-data-center absolute' style={{ right: "-250px" }}>
                    <span className='font-bold'>{data.label}</span>
                    <span className='text-red-500 font-bold'>{data.value}</span>
                </div>
            </div>
            <Handle type="source" position={Position.Bottom} />
        </div>
    );
});

CapacitorNode.displayName = 'CapacitorNode';
