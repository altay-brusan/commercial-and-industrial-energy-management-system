/* eslint-disable react-hooks/exhaustive-deps */
import { useCallback, useEffect, useMemo, useRef } from 'react';
import {
    ReactFlow,
    Background,
    Controls,
    useNodesState,
    useEdgesState,
    Node,
    ReactFlowInstance,
    FitViewOptions,
    OnInit,
    Edge,
    Position,
    XYPosition
} from '@xyflow/react';
import '@xyflow/react/dist/style.css';

import { BessNode } from './nodes/BessNode';
import { GessNode } from './nodes/GessNode';
import { PcsNode } from './nodes/PcsNode';
import { GridNode } from './nodes/GridNode';
import { FacilityNode } from './nodes/FacilityNode';
import { GeneratorNode } from './nodes/GeneratorNode';
import { EvChargerNode } from './nodes/EvChargerNode';
import { HeatPumpNode } from './nodes/HeatPumpNode';
import { PpcNode } from './nodes/PpcNode';
import ButtonEdge from './edges/ButtonEdge';
import { LineNode } from './nodes/LineNode';
import { KeyNode } from './nodes/KeyNode';
import DevTools from './devtools/DevTools';
import { SmartMeterNode } from './nodes/SmartMeterNode';
import { TransformerNode } from './nodes/TransformerNode';
import { CapacitorNode } from './nodes/CapacitorNode';
import { Topology } from '../types/topology';

const edgeTypes = {
    buttonedge: ButtonEdge,
};

type FlowNode = Node;
type FlowEdge = Edge;


const getEdgeColorForFlow = (nodes: FlowNode[], currentEdge: FlowEdge): string => {
    const keyNodes = nodes.filter(node => node.type === 'keyNode');

    for (const keyNode of keyNodes) {
        const sourceNode = nodes.find(n => n.id === currentEdge.source);
        const targetNode = nodes.find(n => n.id === currentEdge.target);

        if (!sourceNode || !targetNode) continue;

        const isNearKeyNode = Math.abs(keyNode.position.y - sourceNode.position.y) < 100 ||
            Math.abs(keyNode.position.y - targetNode.position.y) < 100;

        const isInLineWithKeyNode = Math.abs(keyNode.position.x - sourceNode.position.x) < 50 ||
            Math.abs(keyNode.position.x - targetNode.position.x) < 50;

        if (isNearKeyNode && isInLineWithKeyNode) {
            const nodeData = typeof keyNode.data === 'string' ? JSON.parse(keyNode.data) : keyNode.data;
            return nodeData.status === 'closed' ? 'green' : 'bg-gray-500';
        }
    }

    return 'green';
};

const transformTopologyToFlow = (topology: Topology): { nodes: FlowNode[], edges: FlowEdge[] } => {
    const nodes: FlowNode[] = topology?.nodes?.map(node => {
        let position: XYPosition;

        try {
            position = typeof node.position === 'string' ?
                JSON.parse(node.position) : node.position;
        } catch {
            position = { x: 0, y: 0 };
        }

        let data: any;
        try {
            data = typeof node.data === 'string' ?
                JSON.parse(node.data) : node.data;
        } catch {
            data = {};
        }

        let style: any;
        try {
            style = typeof node.style === 'string' ?
                JSON.parse(node.style) : node.style;
        } catch {
            style = {};
        }

        return {
            id: node.id,
            type: node.type,
            position,
            data,
            style,
            targetPosition: Position.Top,
            sourcePosition: Position.Bottom,
            extent: 'parent',
            parentId: node.parentId
        } as FlowNode;
    });

    const edges: FlowEdge[] = topology?.edges?.map(edge => {
        let data: any;
        try {
            data = typeof edge.data === 'string' ?
                JSON.parse(edge.data) : edge.data;
        } catch {
            data = {};
        }

        let style: any;
        try {
            style = typeof edge.style === 'string' ?
                JSON.parse(edge.style) : edge.style;
        } catch {
            style = {};
        }

        const edgeColor = getEdgeColorForFlow(nodes, {
            id: edge.id,
            source: edge.source,
            target: edge.target,
            type: 'smoothstep',
            data,
            sourceHandle: edge.sourceHandle,
            targetHandle: edge.targetHandle,
            style: {
                strokeWidth: style?.strokeWidth || 5,
                ...style
            }
        } as FlowEdge);

        return {
            id: edge.id,
            source: edge.source,
            target: edge.target,
            type: 'smoothstep',
            data,
            sourceHandle: edge.sourceHandle,
            targetHandle: edge.targetHandle,
            style: {
                strokeWidth: style?.strokeWidth || 5,
                stroke: edgeColor,
                ...style
            }
        } as FlowEdge;
    });

    return { nodes, edges };
};

const defaultEdgeOptions = {
    type: 'smoothstep',
    animated: false,
    style: { strokeWidth: 5 }
};

const EnergySystemTopology = ({ topology }: { topology: Topology }) => {
    const { nodes: flowNodes, edges: flowEdges } = transformTopologyToFlow(topology);
    const [nodes, setNodes, onNodesChange] = useNodesState(flowNodes);
    const [edges, setEdges, onEdgesChange] = useEdgesState(flowEdges);
    const reactFlowInstance = useRef<ReactFlowInstance<any, any> | null>(null);
    const reactFlowWrapper = useRef<HTMLDivElement>(null);

    const handleKeyClick = useCallback((nodeId: string) => {
        const updatedNodes = nodes.map((node: any) => {
            if (node.data?.id === nodeId) {
                const nodeData = typeof node.data === 'string' ? JSON.parse(node.data) : node.data;
                return {
                    ...node,
                    data: {
                        ...nodeData,
                        status: nodeData.status === 'opened' ? 'closed' : 'opened'
                    }
                };
            }
            return node;
        });

        const updatedEdges = edges.map((edge: any) => {
            const keyNodes = updatedNodes.filter(node => node.type === 'keyNode');
            let edgeColor = 'green';

            for (const keyNode of keyNodes) {
                const sourceNode = updatedNodes.find(n => n.id === edge.source);
                const targetNode = updatedNodes.find(n => n.id === edge.target);

                if (!sourceNode || !targetNode) continue;

                const isNearKeyNode = Math.abs(keyNode.position.y - sourceNode.position.y) < 100 ||
                    Math.abs(keyNode.position.y - targetNode.position.y) < 100;

                const isInLineWithKeyNode = Math.abs(keyNode.position.x - sourceNode.position.x) < 50 ||
                    Math.abs(keyNode.position.x - targetNode.position.x) < 50;

                if (isNearKeyNode && isInLineWithKeyNode) {
                    const nodeData = typeof keyNode.data === 'string' ? JSON.parse(keyNode.data) : keyNode.data;
                    edgeColor = nodeData.status === 'closed' ? 'green' : 'bg-gray-500';
                    break;
                }
            }

            return {
                ...edge,
                style: {
                    ...(edge.style as any),
                    stroke: edgeColor
                }
            };
        });

        setNodes(updatedNodes as any);
        setEdges(updatedEdges as any);
    }, [nodes, edges]);

    const nodeTypes = useMemo(() => ({
        bessNode: BessNode,
        gessNode: GessNode,
        pcsNode: PcsNode,
        gridNode: GridNode,
        facilityNode: FacilityNode,
        generatorNode: GeneratorNode,
        evChargerNode: EvChargerNode,
        heatPumpNode: HeatPumpNode,
        ppcNode: PpcNode,
        lineNode: LineNode,
        keyNode: (props: any) => <KeyNode {...props} onClick={handleKeyClick} />,
        smartMeterNode: SmartMeterNode,
        capacitorNode: CapacitorNode,
        transformerNode: TransformerNode
    }), []);

    const fitViewOptions: FitViewOptions = {
        padding: 0.2,
        includeHiddenNodes: false,
        minZoom: 0.6,
        maxZoom: 1.5,
        duration: 800
    };

    const onInit: OnInit<any, any> = useCallback((instance) => {
        reactFlowInstance.current = instance;
        setTimeout(() => {
            instance.fitView(fitViewOptions);
        }, 100);
    }, []);

    useEffect(() => {
        const handleResize = () => {
            if (reactFlowInstance.current) {
                reactFlowInstance.current.fitView(fitViewOptions);
            }
        };

        window.addEventListener('resize', handleResize);
        return () => window.removeEventListener('resize', handleResize);
    }, [nodes, edges]);

    return (
        <div ref={reactFlowWrapper} style={{ fontFamily: 'monospace', height: '79.8vh' }} className="w-full">
            <ReactFlow
                nodes={nodes}
                edges={edges}
                onNodesChange={onNodesChange}
                onEdgesChange={onEdgesChange}
                nodeTypes={nodeTypes}
                edgeTypes={edgeTypes}
                onInit={onInit}
                fitView={true}
                fitViewOptions={fitViewOptions}
                defaultEdgeOptions={defaultEdgeOptions}
                nodesDraggable={false}
            >
                <Background />
                <Controls position='top-right' />
                {/* <DevTools /> */}
            </ReactFlow>
        </div>
    );
};

export default EnergySystemTopology;
