/* eslint-disable react-hooks/exhaustive-deps */
import { memo, useCallback, useState } from 'react';
import { Handle, Position } from '@xyflow/react';
import { CustomNodeData } from '../../types/node-types';

interface KeyNodeProps {
    data: CustomNodeData;
    onClick: (id: string) => void;
}

export const KeyNode =({ data, onClick }: KeyNodeProps) => {
    const [status, setStatus] = useState(data.status);

    const handleClick = useCallback((id: string) => {
        onClick(id);
        setStatus(status === 'opened' ? 'closed' : 'opened');
    }, [status]);

    return (
        <div className="">
            <Handle style={{ marginLeft: status === 'opened' ? "-4px" : "0px" }} type="target" position={Position.Top} />
            {status === 'opened' ?
                <img width={status ? 20 : 12} src={status ? '/assets/icons/opened-key.svg' : '/assets/icons/closed-key.svg'} alt='' onClick={() => handleClick(data.id || '')}></img>
                :
                <img width={status ? 12 : 20} src={status ? '/assets/icons/closed-key.svg' : '/assets/icons/opened-key.svg'} alt='' onClick={() => handleClick(data.id || '')}></img>
            }
            <Handle style={status === 'opened' ? { marginLeft: "-4px", marginBottom: "2px" } : { marginBottom: "2px" }} type="source" position={Position.Bottom} />
        </div>

    );
};

KeyNode.displayName = 'KeyNode';
