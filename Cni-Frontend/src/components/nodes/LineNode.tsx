import { memo } from 'react';
import { Handle, Position } from '@xyflow/react';
import { CustomNodeData } from '../../types/node-types';
import { TbPlugConnected } from "react-icons/tb";

interface LineNodeProps {
    data: CustomNodeData;
}


export const LineNode = memo(({ data }: LineNodeProps) => {
    const value = data.value || 0;
    const segments = data.segments || [
        { color: '#3B82F6', percentage: 25 }, // mavi
        { color: '#10B981', percentage: 25 }, // yeşil
        { color: '#F59E0B', percentage: 25 }, // turuncu
        { color: '#8B5CF6', percentage: 25 }, // mor
    ];

    const handlePositions = [];
    let currentPosition = 0;

    handlePositions.push({
        position: 0,
        color: segments[0].color
    });

    segments.forEach((segment, index) => {
        currentPosition += segment.percentage;
        handlePositions.push({
            position: currentPosition,
            color: index < segments.length - 1 ? segments[index + 1].color : segments[index].color
        });
    });

    return (
        <div className="flex items-center justify-center relative text-3xl" style={{ width: '100%', minWidth: '400px', height: '5px' }}>
            <Handle
                type="target"
                position={Position.Top}
                id="target-1"
            />

            <div className="relative w-full px-4 bg-green-700 flex align-items-center">
                <div className='absolute left-0' style={{ top: "-30px" }}>
                    <TbPlugConnected size={16} />
                    <span>AC Line {value} kW</span>
                </div>

            </div>

            {handlePositions.map((handle, index) => (
                <Handle
                    key={`source-${index + 1}`}
                    type="source"
                    position={Position.Bottom}
                    id={`source-${index + 1}`}
                    style={{
                        left: `${handle.position}%`,
                    }}
                />
            ))}
        </div>
    );
});

LineNode.displayName = 'LineNode';
