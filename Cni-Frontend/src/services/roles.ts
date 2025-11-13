import { tryCatch } from "../utils/tryCatch";
import { httpClient } from "./http";

const url = '/AppRoles/roles';

async function getRoles(pageNumber: number, pageSize: number) {
    const [error, result] = await tryCatch(httpClient.get(url, { params: { pageNumber, pageSize } }));
    if (error) throw error;
    return result;
}

async function deleteRole(roleId: string) {
    const [error, result] = await tryCatch(httpClient.delete(`${url}/${roleId}`));
    if (error) throw error;
    return result;
}

async function updateRole(roleId: string, role: { name: string; description: string }) {
    const [error, result] = await tryCatch(httpClient.put(`${url}/${roleId}`, role));
    if (error) throw error;
    return result;
}

async function addRole(role: { name: string; description: string }) {
    const [error, result] = await tryCatch(httpClient.post(url, role));
    if (error) throw error;
    return result;
}

export { getRoles, deleteRole, updateRole, addRole };
