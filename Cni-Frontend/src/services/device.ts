import { tryCatch } from '../utils/tryCatch';
import { httpClient } from './http';
import { DeviceListQuery, DeviceSummary } from '../types/device';

const url = '/devices';

async function list(query: DeviceListQuery) {
    const params: Record<string, any> = {
        pageNumber: query.pageNumber,
        pageSize: query.pageSize,
    };

    if (query.filter?.type) {
        params.type = query.filter.type;
    }
    if (query.filter?.status) {
        params.status = query.filter.status;
    }
    if (query.filter?.search) {
        params.search = query.filter.search;
    }

    const [error, result] = await tryCatch(httpClient.get(url, { params }));
    if (error) throw error;
    return result;
}

async function detail(id: string) {
    const [error, result] = await tryCatch(httpClient.get(`${url}/${id}`));
    if (error) throw error;
    return result;
}

async function create(payload: any) {
    const [error, result] = await tryCatch(httpClient.post(url, payload));
    if (error) throw error;
    return result;
}

async function update(id: string, payload: any) {
    const [error, result] = await tryCatch(httpClient.put(`${url}/${id}`, payload));
    if (error) throw error;
    return result;
}

async function deleteDevice(id: string) {
    const [error, result] = await tryCatch(httpClient.delete(`${url}/${id}`));
    if (error) throw error;
    return result;
}

async function getSummary(): Promise<DeviceSummary> {
    const [error, result] = await tryCatch(httpClient.get(`${url}/summary`));
    if (error) throw error;
    return result;
}

export const deviceService = {
    list,
    detail,
    create,
    update,
    deleteDevice,
    getSummary,
};
