import { tryCatch } from "@/src/utils/tryCatch";
import { httpClient } from "./http";

interface User {
    id: number;
    name: string;
    email: string;
}

interface RegisterPayload {
    name: string;
    email: string;
    password: string;
}

interface LoginPayload {
    email: string;
    password: string;
}

async function register(payload: RegisterPayload) {
    const [error, result] = await tryCatch(httpClient.post('/Users/register', payload));
    if (error) throw error;
    return result.data;
}

async function login(payload: LoginPayload) {
    const [error, result] = await tryCatch(httpClient.post('/Users/login', payload));
    if (error) throw error;
    return result;
}

async function refresh() {
    const [error, result] = await tryCatch(httpClient.post('/Users/refresh'));
    if (error) throw error;
    return result.data.data;
}

async function confirmEmail() {
    const [error, result] = await tryCatch(httpClient.get('/Users/confirmEmail'));
    if (error) throw error;
    return result.data.data;
}

async function resendConfirmationEmail(email: string) {
    const [error, result] = await tryCatch(httpClient.post('/Users/resendConfirmationEmail', { email }));
    if (error) throw error;
    return result.data;
}

async function forgotPassword(email: string) {
    const [error, result] = await tryCatch(httpClient.post('/Users/forgotPassword', { email }));
    if (error) throw error;
    return result.data.data;
}

async function resetPassword(payload: {
    email: string;
    resetCode: string;
    newPassword: string;
}) {
    const [error, result] = await tryCatch(httpClient.post('/Users/resetPassword', payload));
    if (error) throw error;
    return result.data.data;
}

async function userInfo() {
    const [error, result] = await tryCatch(httpClient.get('/Users/userInfo'));
    if (error) throw error;
    return result;
}

async function updateUserInfo(payload: {
    userId: string;
    name: string;
    email: string;
    surname: string;
}) {
    const [error, result] = await tryCatch(httpClient.put('/Users/update', payload));
    if (error) throw error;
    return result;
}

async function updateUserProfile(payload: {
    email: string;
    name: string;
    surname: string;
    description?: string;
}) {
    const [error, result] = await tryCatch(httpClient.put('/Users/updateprofile', payload));
    if (error) throw error;
    return result;
}


async function manage2FA(payload: { method: string }) {
    const [error, result] = await tryCatch(httpClient.post('/Users/manage/2fa', payload));
    if (error) throw error;
    return result.data.data;
}

async function manageInfo() {
    const [error, result] = await tryCatch(httpClient.get('/Users/manage/info'));
    if (error) throw error;
    return result.data.data;
}

export const authService = {
    register,
    login,
    refresh,
    confirmEmail,
    resendConfirmationEmail,
    forgotPassword,
    resetPassword,
    userInfo,
    manage2FA,
    manageInfo,
    updateUserInfo,
    updateUserProfile
};
