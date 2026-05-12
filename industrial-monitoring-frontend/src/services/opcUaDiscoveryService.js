import axios from "axios";
import { apiGet } from "./apiClient";

const API_BASE_URL = "https://localhost:7105/api/opcua";

export async function getOpcUaSections() {
  const response = await axios.get(`${API_BASE_URL}/sections`);
  return response.data;
}

export async function getOpcUaTagsBySectionId(sectionId) {
  const response = await axios.get(`${API_BASE_URL}/sections/${sectionId}/tags`);
  return response.data;
}

export async function syncDiscoveredSections() {
  const response = await axios.post(`${API_BASE_URL}/sections/sync`);
  return response.data;
}

export async function syncDiscoveredTagsForSection(sectionId) {
  const response = await axios.post(`${API_BASE_URL}/sections/${sectionId}/tags/sync`);
  return response.data;
}

export async function updateSectionEnabledState(sectionId, isEnabled) {
  const response = await axios.put(
    `${API_BASE_URL}/sections/${sectionId}/enabled`,
    { isEnabled }
  );
  return response.data;
}

export async function updateTagEnabledState(tagId, isEnabled) {
  const response = await axios.put(
    `${API_BASE_URL}/tags/${tagId}/enabled`,
    { isEnabled }
  );
  return response.data;
}

export async function updateTagsEnabledStateBySection(sectionId, isEnabled) {
  const response = await axios.put(
    `${API_BASE_URL}/sections/${sectionId}/tags/enabled`,
    { isEnabled }
  );
  return response.data;
}

export async function getEnabledOpcUaTags() {
    return apiGet("/opcua/enabled-tags");
}