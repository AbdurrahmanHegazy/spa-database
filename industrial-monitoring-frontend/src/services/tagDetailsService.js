import { apiGet } from "./apiClient";

export async function getTagDetails(tagId) {
    return apiGet(`/Tags/${encodeURIComponent(tagId)}`);
}