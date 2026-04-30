const API_BASE_URL = "http://localhost:5090/api";

export async function apiGet(path) {
    const response = await fetch(`${API_BASE_URL}${path}`);

    if (!response.ok) {
        throw new Error(`GET ${path} failed with status ${response.status}`);
    }

    return response.json();
}

export function buildQueryString(params) {
    const searchParams = new URLSearchParams();

    Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== "") {
            searchParams.append(key, value);
        }
    });

    const queryString = searchParams.toString();
    return queryString ? `?${queryString}` : "";
}