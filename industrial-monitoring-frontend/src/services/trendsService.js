import { apiGet, buildQueryString } from "./apiClient";

export async function getTrendsData(filters = {}) {
    const queryString = buildQueryString(filters);
    return apiGet(`/Trends${queryString}`);
}