import { apiGet } from "./apiClient";

export async function getDashboardData() {
    return apiGet("/Dashboard");
}