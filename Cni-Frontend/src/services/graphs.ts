import { tryCatch } from "../utils/tryCatch";
import { httpClient } from "./http";


async function getEnergyProduction(id: string | number, startDate: Date | null | string, endDate: Date | null | string) {
    const [error, result] = await tryCatch(httpClient.get(`/Graphs/EnergyProduction/${id}`, { params: { startDate, endDate: endDate || null } }));
    if (error) throw error;
    return result;
}

async function getPowerProduction(id: string | number, startDate: Date | null | string, endDate: Date | null | string) {
    const [error, result] = await tryCatch(httpClient.get(`/Graphs/PowerProduction/${id}`, { params: { startDate, endDate: endDate || null } }));
    if (error) throw error;
    return result;
}

async function getInverterGraphData(id: string | number, startDate: Date | null | string, endDate: Date | null | string) {
    const [error, result] = await tryCatch(httpClient.get(`/Graphs/InverterGraphData/${id}`, { params: { startDate, endDate: endDate || null } }));
    if (error) throw error;
    return result;
}

async function getPowerGeneralGraphData(id: string | number, startDate: Date | null | string, endDate: Date | null | string) {
    const [error, result] = await tryCatch(httpClient.get(`/Graphs/PowerGeneralGraphData/${id}`, { params: { startDate, endDate: endDate || null } }));
    if (error) throw error;
    return result;
}

export const graphsService = {
    getEnergyProduction,
    getPowerProduction,
    getInverterGraphData,
    getPowerGeneralGraphData
};
