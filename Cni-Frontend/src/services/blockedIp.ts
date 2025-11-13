import { tryCatch } from "@/src/utils/tryCatch";
import { httpClient } from "./http";

export interface BlockedIpMail {
    id: string;
    address: string;
    type: 'IP' | 'EMAIL';
    reason: string;
    status: boolean;
    createdAt: string;
    updatedAt: string;
}

export interface BlockedIpMailListQuery {
    pageNumber: number;
    pageSize: number;
    type?: 'IP' | 'EMAIL' | '';
}

async function list(payload: BlockedIpMailListQuery) {
    const [error, result] = await tryCatch(httpClient.get('/blocked-addresses', { params: payload }));

    if (error) throw error;

    return result;
}

async function detail(id: string) {
    const [error, result] = await tryCatch(httpClient.get(`/blocked-addresses/${id}`));

    if (error) throw error;

    return result;
}

async function deleteBlockedAddress(id: string) {
    const [error, result] = await tryCatch(httpClient.delete(`/blocked-addresses/${id}`));

    if (error) throw error;

    return result;
}

async function updateBlockedAddress(id: string, payload: Partial<BlockedIpMail>) {
    const [error, result] = await tryCatch(httpClient.put(`/blocked-addresses/${id}`, payload));

    if (error) throw error;

    return result;
}

async function createBlockedAddress(payload: Omit<BlockedIpMail, 'id' | 'createdAt' | 'updatedAt'>) {
    const [error, result] = await tryCatch(httpClient.post(`/blocked-addresses`, payload));

    if (error) throw error;

    return result;
}

export const blockedIpService = {
    list,
    detail,
    deleteBlockedAddress,
    updateBlockedAddress,
    createBlockedAddress
};
