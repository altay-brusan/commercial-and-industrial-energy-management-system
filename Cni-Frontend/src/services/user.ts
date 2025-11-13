import { tryCatch } from "@/src/utils/tryCatch";
import { httpClient } from "./http";

interface User {
    name: string;
    email: string;
    userId: string;
    surname: string;
    title?: string;
    description?: string;
    userFirmFacilities?: {
        firmId: number;
        facilityId: number;
    }[]
}

export interface UserListQuery {
    pageNumber: number;
    pageSize: number;
}


async function list(payload: UserListQuery) {
    const [error, result] = await tryCatch(httpClient.get('/users', { params: payload }));

    if (error) throw error;

    return result;
}

async function detail(id: string) {
    const [error, result] = await tryCatch(httpClient.get(`/users/${id}`));

    if (error) throw error;

    return result;
}

async function deleteUser(id: string) {
    const [error, result] = await tryCatch(httpClient.delete(`/users/${id}`));

    if (error) throw error;

    return result;
}

async function updateUser(payload: User) {
    const [error, result] = await tryCatch(httpClient.put(`/users/update`, payload));

    if (error) throw error;

    return result;
}

async function createUser(payload: User) {
    const [error, result] = await tryCatch(httpClient.post(`/users/register`, payload));

    if (error) throw error;

    return result;
}

async function userRoles(id: string, payload: {
    userId: string;
    roleName: string;
}) {
    const [error, result] = await tryCatch(httpClient.post(`/users/${id}/roles`, payload));

    if (error) throw error;

    return result;
}

export const userService = { list, detail, deleteUser, updateUser, createUser, userRoles }
