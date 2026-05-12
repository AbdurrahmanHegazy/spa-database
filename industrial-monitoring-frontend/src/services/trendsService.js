import { apiGet } from "./apiClient";
import { buildQueryString } from "./apiClient";
export async function getTrendsData(filters = {}) {
    const queryString = buildQueryString(filters);
    return apiGet(`/Trends${queryString}`);
}





export async function getEnabledOpcUaTags() {
    return apiGet("/opcua/enabled-tags");
}