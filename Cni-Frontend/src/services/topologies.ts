
import { tryCatch } from "../utils/tryCatch";
import { httpClient } from "./http";

const url = '/Topologies';

async function getTopologiesByFacilityId(facilityId: string) {
    const [error, result] = await tryCatch(httpClient.get(`${url}/${facilityId}`));
    if (error) throw error;
    return result;
}

export { getTopologiesByFacilityId };
