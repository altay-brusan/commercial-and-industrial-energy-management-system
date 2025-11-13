import { memo } from 'react';
import { Handle, Position } from '@xyflow/react';
import { CustomNodeData } from '../../types/node-types';
import { useTranslations } from 'next-intl';

interface PcsNodeProps {
    data: CustomNodeData;
}

export const PcsNode = memo(({ data }: PcsNodeProps) => {
    const t = useTranslations();
    return (
        <div className="p-7 border bg-blue-100 shadow-lg text-2xl relative text-center">
            <div className='absolute text-3xl font-bold' style={{ top: "-60%", transform: 'translateX(-50%)', minWidth: "250px" }}>DC Line 500 kW </div>
            <Handle type="target" position={Position.Top} />
            <div className="text-3xl font-bold">PCS</div>
            <Handle type="source" position={Position.Bottom} />
            <div className="absolute w-full" style={{ top: "50%", transform: 'translate(80%, -50%)' }}>
                <div className="w-full" style={{ minWidth: "260px" }}>
                    <div className="bg-black text-green-500 mt-1 p-1 px-2">{t("dashboard.efficiency")}: {data.efficiency} %</div>
                    <div className="bg-black text-red-500 mt-1 p-1 px-2">{t("dashboard.acPower")}: {data.acPower} W</div>
                    <div className="bg-black text-gray-400 mt-1 p-1 px-2">{t("dashboard.dcPower")}: {data.dcPower} W</div>
                </div>
            </div>
        </div>
    );
});

PcsNode.displayName = 'PcsNode';
