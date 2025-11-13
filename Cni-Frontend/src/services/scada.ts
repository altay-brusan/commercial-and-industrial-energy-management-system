import { tryCatch } from "../utils/tryCatch";
import { httpClient } from "./http";


async function getScadaData(id: string | number, time: Date | null | string) {
    const [error, result] = await tryCatch(httpClient.get(`/Scada/${id}`, { params: { time } }));
    if (error) throw error;
    return result;
}

async function changeTime(time: Date | null | string) {
    const [error, result] = await tryCatch(httpClient.post(`/Scada/ChangeTime`, { time }));
    if (error) throw error;
    return result;
}

export const scadaService = {
    getScadaData,
    changeTime
};
