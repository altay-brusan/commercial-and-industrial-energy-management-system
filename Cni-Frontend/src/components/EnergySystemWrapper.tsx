/* eslint-disable react-hooks/exhaustive-deps */
"use client"
import React, { useState, useEffect, useCallback, useRef } from 'react';
import EnergySystemTopology from './EnergySystemTopology';
import { ProgressSpinner } from 'primereact/progressspinner';
import { ReactFlowProvider } from '@xyflow/react';
import { useTenant } from '../context/TenantContext';
import { getTopologiesByFacilityId } from '../services/topologies';
import { useFetch } from '../hooks/use-fetch';
import { Topology, TopologyNode } from '../types/topology';
import { DashboardRealTimeData } from '../models/DashboardRealTimeData';
import webSocketService from '../services/WebSocketService';
import { tryCatch } from '../utils/tryCatch';
import { scadaService } from '../services/scada';
import { useLoading } from '../context/LoadingContext';

interface NodeData {
    id: string;
    label?: string;
    soc?: number;
    temperature?: number;
    state?: string;
    performance?: number;
    generation?: number;
    powerConsumption?: number;
    powerFactor?: number;
    value?: string;
    status?: string;
}

interface Node {
    id: string;
    type: string;
    extent: string | null;
    parentId: string;
    data: NodeData;
    position: {
        x: number;
        y: number;
    };
    style: any;
    targetHandleCount: number | null;
    sourceHandleCount: number | null;
}

const EnergySystemWrapper: React.FC = () => {
    const [loading, setLoading] = useState(false);
    const { startDate, tenant } = useTenant();
    const [topology] = useFetch<Topology>(() => getTopologiesByFacilityId(tenant || ''), { deps: [], reloadDeps: [], setLoading });
    const [data, setData] = useState<DashboardRealTimeData>();
    const [isConnected, setIsConnected] = useState(false);
    const connectionRef = useRef<(() => void) | null>(null);
    const { showLoading, hideLoading } = useLoading();
    const [convertedTopology, setConvertedTopology] = useState<Topology | null>(null);

    const updateTopologyWithRealTimeData = useCallback(() => {
        if (!topology || !data) return;
        setLoading(true);
        const updatedNodes = topology.nodes.map((node: TopologyNode) => {
            const bessDevice = data.bessDevices.find(device => device.nodeId.toString() === node.id);
            if (bessDevice && node.type === 'bessNode') {
                return {
                    ...node,
                    data: {
                        ...node.data,
                        soc: Number(bessDevice.stateOfCharge.toFixed(2)),
                        temperature: Number(bessDevice.batteryTemperature.toFixed(2)),
                        state: bessDevice.status.operatingMode === 1 ? 'active' : 'standby'
                    }
                };
            }

            const solarDevice = data.solarDevices.find(device => device.nodeId.toString() === node.id);
            if (solarDevice && node.type === 'gessNode') {
                return {
                    ...node,
                    data: {
                        ...node.data,
                        performance: Number(solarDevice.performanceRatio.toFixed(2)),
                        temperature: Number(solarDevice.pvPanelTemperature.toFixed(2)),
                        generation: Number(solarDevice.dcOutput.power.toFixed(2))
                    }
                };
            }

            const pcsDevice = data.pcsDevices.find(device => device.nodeId.toString() === node.id);
            if (pcsDevice && node.type === 'pcsNode') {
                return {
                    ...node,
                    data: {
                        ...node.data,
                        efficiency: Number(pcsDevice.efficiency.toFixed(2)),
                        acPower: Number(pcsDevice.acPower.totalActivePower.toFixed(2)),
                        dcPower: Number(pcsDevice.dcPower.power.toFixed(2))
                    }
                };
            }

            const gridDevice = data.gridDevices.find(device => device.nodeId.toString() === node.id);
            if (gridDevice && node.type === 'gridNode') {
                return {
                    ...node,
                    data: {
                        ...node.data,
                        powerConsumption: Number(gridDevice.acPower.totalActivePower.toFixed(2)),
                        powerFactor: Number(gridDevice.acPower.totalPowerFactor.toFixed(2)),
                        reactivePower: Number(gridDevice.acPower.totalReactivePower.toFixed(2)),
                        temperature: 60 // Grid transformatör sıcaklığı sabit
                    }
                };
            }

            const generatorDevice = data.generatorDevices.find(device => device.nodeId.toString() === node.id);
            if (generatorDevice && node.type === 'generatorNode') {
                return {
                    ...node,
                    data: {
                        ...node.data,
                        generation: Number(generatorDevice.totalPower.toFixed(2)),
                        fuel: 82 // Yakıt seviyesi sabit
                    }
                };
            }

            if (data.facilityData.nodeId.toString() === node.id && node.type === 'facilityNode') {
                return {
                    ...node,
                    data: {
                        ...node.data,
                        powerConsumption: Number(data.facilityData.acPower.totalActivePower.toFixed(2)),
                        powerFactor: Number(data.facilityData.acPower.totalPowerFactor.toFixed(2))
                    }
                };
            }

            return node;
        });

        setConvertedTopology({
            ...topology,
            nodes: updatedNodes
        });
        setLoading(false);
    }, [data, topology, setConvertedTopology]);

    useEffect(() => {
        updateTopologyWithRealTimeData();
    }, [topology, data]);

    const getScadaData = async () => {
        showLoading();
        const [error, result] = await tryCatch(scadaService.getScadaData(tenant, startDate));
        if (error) {
            hideLoading();
            throw error;
        }
        hideLoading();
        return result;
    }

    useEffect(() => {
        const SIGNALR_HUB_URL = 'https://derms-api.n2nenergy.com/liveDataHub';
        let isSubscribed = true;

        const connectToSignalR = async () => {
            try {
                if (!webSocketService.isConnected()) {
                    await webSocketService.connect(SIGNALR_HUB_URL);
                }

                if (isSubscribed) {
                    setIsConnected(webSocketService.isConnected());

                    const unsubscribe = await webSocketService.subscribe<DashboardRealTimeData>('FacilityUpdate', (data) => {
                        if (isSubscribed) {
                            setData(data);
                        }
                    });

                    connectionRef.current = unsubscribe;

                    await webSocketService.invoke('SubscribeToFacility', 6);
                }
            } catch (error) {
                console.error('SignalR bağlantı hatası:', error);
                if (isSubscribed) {
                    setIsConnected(false);
                }
            }
        };

        const fetchData = async () => {
            if (!startDate) {
                await connectToSignalR();
            } else {
                try {
                    const scadaData = await getScadaData();
                    setData(scadaData);
                } catch (error) {
                    console.error('Scada veri çekme hatası:', error);
                }
            }
        };

        fetchData();

        const connectionInterval = setInterval(() => {
            if (isSubscribed && !startDate) {
                const connected = webSocketService.isConnected();
                setIsConnected(connected);

                if (!connected) {
                    connectToSignalR();
                }
            }
        }, 5000);

        return () => {
            isSubscribed = false;
            if (connectionRef.current) {
                connectionRef.current();
                connectionRef.current = null;
            }
            clearInterval(connectionInterval);
        };
    }, [startDate]);

    if (loading) {
        return (
            <div className="flex flex-column align-items-center justify-content-center h-screen">
                <ProgressSpinner style={{ width: '50px', height: '50px' }} strokeWidth="4" animationDuration="1s" />
                <div className="mt-3 text-lg text-primary font-medium">Enerji Sistemi Yükleniyor...</div>
                <div className="text-sm text-gray-500 mt-2">Topoloji bilgileri hazırlanıyor, lütfen bekleyin</div>
            </div>
        );
    }

    return (
        <ReactFlowProvider>
            {convertedTopology && convertedTopology.nodes.length > 0 && <EnergySystemTopology topology={convertedTopology} />}
        </ReactFlowProvider>
    );
};

export default EnergySystemWrapper;
