
import { tryCatch } from "../utils/tryCatch";
import { httpClient } from "./http";

const url = '/Plans';

async function getPlans(pageNumber: number, pageSize: number) {
    const [error, result] = await tryCatch(httpClient.get(url, { params: { pageNumber, pageSize } }));
    if (error) throw error;
    return result;
}

async function getPlanById(id: string) {
    const [error, result] = await tryCatch(httpClient.get(`${url}/${id}`));
    if (error) throw error;
    return result;
}

async function createPlan(payload: any) {
    const [error, result] = await tryCatch(httpClient.post(url, payload));
    if (error) throw error;
    return result;
}

async function updatePlan(id: string, payload: any) {
    const [error, result] = await tryCatch(httpClient.put(`${url}/${id}`, payload));
    if (error) throw error;
    return result;
}

async function deletePlan(id: number) {
    const [error, result] = await tryCatch(httpClient.delete(`${url}/${id}`));
    if (error) throw error;
    return result;
}

export { getPlans, getPlanById, createPlan, updatePlan, deletePlan };
