
import { tryCatch } from "../utils/tryCatch";
import { httpClient } from "./http";

const url = '/Facilities';

async function getFacilities(pageNumber: number, pageSize: number, firms?: number[]) {
    const [error, result] = await tryCatch(httpClient.get(url, { params: { pageNumber, pageSize, FirmIds: firms?.join(',') } }));
    if (error) throw error;
    return result;
}

async function getFacilityById(id: string) {
    const [error, result] = await tryCatch(httpClient.get(`${url}/${id}`));
    if (error) throw error;
    return result;
}

async function createFacility(payload: any) {
    const [error, result] = await tryCatch(httpClient.post(url, payload));
    if (error) throw error;
    return result;
}

async function updateFacility(id: string, payload: any) {
    const [error, result] = await tryCatch(httpClient.put(`${url}/${id}`, payload));
    if (error) throw error;
    return result;
}

async function deleteFacility(id: number) {
    const [error, result] = await tryCatch(httpClient.delete(`${url}/${id}`));
    if (error) throw error;
    return result;
}

export { getFacilities, getFacilityById, createFacility, updateFacility, deleteFacility };
