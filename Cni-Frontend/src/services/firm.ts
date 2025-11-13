
import { tryCatch } from "../utils/tryCatch";
import { httpClient } from "./http";


async function getFirms(pageNumber: number, pageSize: number) {
    const [error, result] = await tryCatch(httpClient.get('/Firms', { params: { pageNumber, pageSize } }));
    if (error) throw error;
    return result;
}

async function getFirmById(id: string) {
    const [error, result] = await tryCatch(httpClient.get(`/Firms/${id}`));
    if (error) throw error;
    return result;
}

async function createFirm(payload: any) {
    const [error, result] = await tryCatch(httpClient.post('/Firms', payload));
    if (error) throw error;
    return result;
}

async function updateFirm(id: string, payload: any) {
    const [error, result] = await tryCatch(httpClient.put(`/Firms/${id}`, payload));
    if (error) throw error;
    return result;
}

async function deleteFirm(id: number) {
    const [error, result] = await tryCatch(httpClient.delete(`/Firms/${id}`));
    if (error) throw error;
    return result;
}

export { getFirms, getFirmById, createFirm, updateFirm, deleteFirm };
