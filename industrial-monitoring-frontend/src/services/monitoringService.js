import { apiGet } from "./apiClient";

export async function getMonitoringData() {
    return apiGet("/Monitoring");
}