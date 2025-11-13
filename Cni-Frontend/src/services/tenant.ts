
import { tryCatch } from "../utils/tryCatch";
import { httpClient } from "./http";

const url = '/Tenants';

async function getTenants(pageNumber: number, pageSize: number) {
    const [error, result] = await tryCatch(httpClient.get(url, { params: { pageNumber, pageSize } }));
    if (error) throw error;
    return result;
}

async function getTenantById(id: string) {
    const [error, result] = await tryCatch(httpClient.get(`${url}/${id}`));
    if (error) throw error;
    return result;
}

async function createTenant(payload: any) {
    const [error, result] = await tryCatch(httpClient.post(url, payload));
    if (error) throw error;
    return result;
}

async function updateTenant(id: string, payload: any) {
    const [error, result] = await tryCatch(httpClient.put(`${url}/${id}`, payload));
    if (error) throw error;
    return result;
}

async function deleteTenant(id: number) {
    const [error, result] = await tryCatch(httpClient.delete(`${url}/${id}`));
    if (error) throw error;
    return result;
}

export { getTenants, getTenantById, createTenant, updateTenant, deleteTenant };
