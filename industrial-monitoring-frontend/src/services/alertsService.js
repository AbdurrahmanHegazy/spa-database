import { apiGet } from "./apiClient";

export async function getAlertsData() {
    return apiGet("/alerts");
}