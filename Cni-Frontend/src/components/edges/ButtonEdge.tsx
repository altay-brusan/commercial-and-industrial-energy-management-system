import React from 'react';
import {
    BaseEdge,
    EdgeLabelRenderer,
    getSmoothStepPath,
    useReactFlow,
    type EdgeProps,
} from '@xyflow/react';
import type { ReactElement } from 'react';
import { classNames } from 'primereact/utils';

interface InfoData {
    name: string;
    value: string;
    icon: string;
    secondIcon?: string | React.ComponentType<any>;
    className?: string;
}

interface Data {
    buttonIcon?: any;
    buttonColor?: string;
    buttonSize?: number;
    onButtonClick?: (id: string) => void;
    buttonStyle?: React.CSSProperties;
    buttonClassName?: string;
    label?: string;
    customIcon?: string;
    infoData?: InfoData;
    [key: string]: any;
}

interface ButtonEdgeProps extends EdgeProps {
    data?: Data;
}

export default function ButtonEdge({
    id,
    sourceX,
    sourceY,
    targetX,
    targetY,
    sourcePosition,
    targetPosition,
    style = {},
    markerEnd,
    data,
}: ButtonEdgeProps) {
    const { setEdges } = useReactFlow();
    const [edgePath, labelX, labelY] = getSmoothStepPath({
        sourceX,
        sourceY,
        sourcePosition,
        targetX,
        targetY,
        targetPosition,
    });

    const dx = targetX - sourceX;
    const dy = targetY - sourceY;
    const angle = Math.atan2(dy, dx);

    const distance = 0;

    const buttonX = sourceX + Math.cos(angle) * distance;
    const buttonY = (sourceY + Math.sin(angle) * distance) + 20;

    const buttonEdgeData = data as Data;
    const infoData = data?.infoData as InfoData;

    const { secondIcon: SecondIcon } = infoData || {};

    const {
        buttonIcon: ButtonIcon,

        buttonColor = 'red',
        buttonSize = 30,
        onButtonClick,
        buttonStyle = {},
        buttonClassName = 'w-5rem h-5rem border-circle',
        label = ""
    } = buttonEdgeData || {};

    const handleEdgeClick = () => {
        if (onButtonClick) {
            onButtonClick(id);
        } else {
            setEdges((edges) => edges.filter((edge) => edge.id !== id));
        }
    };

    return (
        <>
            <BaseEdge path={edgePath} markerEnd={markerEnd} style={style} />
            <EdgeLabelRenderer>
                <div
                    className="absolute pointer-events-auto transform-origin-center nodrag nopan"
                    style={{
                        transform: `translate(${data?.customTranslate ? data.customTranslate : -50}%, 0%) translate(${buttonX}px, ${buttonY}px)`,
                    }}
                >
                    {data?.infoData ?
                        <div className='flex gap-2 mb-4 bg-white'>
                            {SecondIcon && (typeof SecondIcon === 'string'
                                ? <img className="text-red-500" src={SecondIcon} alt="Second Icon" />
                                : <SecondIcon className="text-red-500" />)}
                            <img width={40} src={data.infoData.icon ?? "/assets/icons/invertor.svg"} alt="" />
                            {data.infoData.name && data.infoData.value && <div className='flex flex-column'>
                                <span style={{ padding: '2px' }} className={classNames('text-sm  font-bold')}>{data.infoData.name}</span>
                                <span style={{ padding: '2px' }} className={classNames('text-yellow-500 text-sm bg-black', data.infoData?.className)}>{data.infoData.value}</span>
                            </div>}

                        </div>
                        : null
                    }
                    {(data?.customIcon || data?.buttonIcon) && (
                        <button
                            className={buttonClassName}
                            onClick={handleEdgeClick}
                            style={buttonStyle}
                            aria-label="Edge button"
                            tabIndex={0}
                            onKeyDown={(e) => e.key === 'Enter' && handleEdgeClick()}
                        >
                            {data?.customIcon ? <img width={buttonSize} src={data.customIcon as string} alt="" /> : <ButtonIcon color={buttonColor} size={buttonSize} />}
                            {label && label}
                        </button>
                    )}
                </div>
            </EdgeLabelRenderer>
        </>
    );
}
