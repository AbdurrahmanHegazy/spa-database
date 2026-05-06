const API_BASE_URL = "http://localhost:5090/api/opcua";

export async function getOpcUaSections() {
  const response = await fetch(`${API_BASE_URL}/sections`);

  if (!response.ok) {
    throw new Error(`Failed to load OPC UA sections. Status: ${response.status}`);
  }

  return await response.json();
}

export async function getOpcUaTagsBySectionId(sectionId) {
  const response = await fetch(`${API_BASE_URL}/sections/${sectionId}/tags`);

  if (!response.ok) {
    throw new Error(`Failed to load OPC UA tags. Status: ${response.status}`);
  }

  return await response.json();
}

export async function updateSectionEnabledState(sectionId, isEnabled) {
  const response = await fetch(`${API_BASE_URL}/sections/${sectionId}/enabled`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ isEnabled }),
  });

  if (!response.ok) {
    throw new Error(`Failed to update section state. Status: ${response.status}`);
  }
}
export async function updateTagsEnabledStateBySection(sectionId, isEnabled) {
  const response = await fetch(`http://localhost:5090/api/opcua/sections/${sectionId}/tags/enabled`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ isEnabled }),
  });

  if (!response.ok) {
    throw new Error(`Failed to update section tags state. Status: ${response.status}`);
  }
}

export async function updateTagEnabledState(tagId, isEnabled) {
  const response = await fetch(`${API_BASE_URL}/tags/${tagId}/enabled`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ isEnabled }),
  });

  if (!response.ok) {
    throw new Error(`Failed to update tag state. Status: ${response.status}`);
  }
}