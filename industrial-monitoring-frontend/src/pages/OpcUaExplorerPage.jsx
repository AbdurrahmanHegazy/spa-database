import { useEffect, useState } from "react";
import PageHeader from "../components/ui/PageHeader";
import SectionCard from "../components/ui/SectionCard";
import PageStateWrapper from "../components/ui/PageStateWrapper";
import {
  getOpcUaSections,
  getOpcUaTagsBySectionId,
  syncDiscoveredSections,
  syncDiscoveredTagsForSection,
  updateTagEnabledState,
  updateTagsEnabledStateBySection,
} from "../services/opcUaDiscoveryService";

function OpcUaExplorerPage() {
  const [sections, setSections] = useState([]);
  const [selectedSection, setSelectedSection] = useState(null);
  const [tags, setTags] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isTagsLoading, setIsTagsLoading] = useState(false);
  const [isSyncingSections, setIsSyncingSections] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [tagSearch, setTagSearch] = useState("");

  useEffect(() => {
    loadSections();
  }, []);

  useEffect(() => {
    async function loadTags() {
      if (!selectedSection) {
        setTags([]);
        return;
      }
      setTagSearch("");

      try {
        setIsTagsLoading(true);
        setErrorMessage("");

        await syncDiscoveredTagsForSection(selectedSection.id);

        const data = await getOpcUaTagsBySectionId(selectedSection.id);
        setTags(data);

        await loadSections(selectedSection.id);
      } catch (error) {
        setErrorMessage(error.message || "Failed to load discovered OPC UA tags.");
      } finally {
        setIsTagsLoading(false);
      }
    }

    loadTags();
  }, [selectedSection?.id]);

  async function loadSections(keepSelectionId = null) {
    try {
      setIsLoading(true);
      setErrorMessage("");

      const data = await getOpcUaSections();
      setSections(data);

      if (data.length === 0) {
        setSelectedSection(null);
        return;
      }

      if (keepSelectionId) {
        const matchingSection = data.find((section) => section.id === keepSelectionId);
        setSelectedSection(matchingSection ?? data[0]);
        return;
      }

      setSelectedSection((currentSelected) => {
        if (!currentSelected) return data[0];

        const matchingSection = data.find((section) => section.id === currentSelected.id);
        return matchingSection ?? data[0];
      });
    } catch (error) {
      setErrorMessage(error.message || "Failed to load discovered OPC UA sections.");
    } finally {
      setIsLoading(false);
    }
  }

  async function handleSyncSections() {
    try {
      setIsSyncingSections(true);
      setErrorMessage("");

      await syncDiscoveredSections();
      await loadSections(selectedSection?.id ?? null);
    } catch (error) {
      setErrorMessage(error.message || "Failed to sync discovered sections.");
    } finally {
      setIsSyncingSections(false);
    }
  }

  async function handleAllTagsToggle(isEnabled) {
    if (!selectedSection) return;

    try {
      await updateTagsEnabledStateBySection(selectedSection.id, isEnabled);

      const refreshedTags = await getOpcUaTagsBySectionId(selectedSection.id);
      setTags(refreshedTags);

      await loadSections(selectedSection.id);
    } catch (error) {
      setErrorMessage(error.message || "Failed to update all tags state.");
    }
  }

  async function handleTagToggle(tagId, currentValue) {
    if (!selectedSection) return;

    try {
      const newValue = !currentValue;

      await updateTagEnabledState(tagId, newValue);

      const refreshedTags = await getOpcUaTagsBySectionId(selectedSection.id);
      setTags(refreshedTags);

      await loadSections(selectedSection.id);
    } catch (error) {
      setErrorMessage(error.message || "Failed to update tag state.");
    }
  }

  function getSectionStatus(section) {
    const totalTags = section.childCount ?? 0;
    const enabledTags = section.enabledTagsCount ?? 0;

    if (totalTags === 0 || enabledTags === 0) {
      return {
        label: "Fully disabled",
        color: "#f87171",
      };
    }

    if (enabledTags === totalTags) {
      return {
        label: "Fully enabled",
        color: "#4ade80",
      };
    }

    return {
      label: "Partially enabled",
      color: "#facc15",
    };
  }

  const filteredTags = tags.filter((tag) => {
  const search = tagSearch.trim().toLowerCase();

  if (!search) return true;

  return (
    (tag.displayName ?? "").toLowerCase().includes(search) ||
    (tag.tagName ?? "").toLowerCase().includes(search) ||
    (tag.nodeId ?? "").toLowerCase().includes(search)
  );
});

  return (
    <PageStateWrapper
      isLoading={isLoading}
      errorMessage={errorMessage}
      isEmpty={sections.length === 0}
      loadingTitle="Loading OPC UA Explorer..."
      loadingMessage="Fetching discovered sections from the backend."
      errorTitle="Explorer unavailable"
      emptyTitle="No discovered sections"
      emptyMessage="No OPC UA sections have been discovered yet."
    >
      <div className="opcua-explorer-page">
        <PageHeader
          title="OPC UA Explorer"
          subtitle="Browse discovered Unified project sections and tags."
        />

        <section
          style={{
            display: "grid",
            gridTemplateColumns: "360px 1fr",
            gap: "24px",
            alignItems: "start",
          }}
        >
          <SectionCard title="Discovered Sections" badge="Database-backed">
            <div style={{ marginBottom: "16px" }}>
              <button
                type="button"
                onClick={handleSyncSections}
                disabled={isSyncingSections}
                style={{
                  width: "100%",
                  padding: "12px 16px",
                  borderRadius: "12px",
                  border: "1px solid rgba(148,163,184,0.15)",
                  background: "rgba(59,130,246,0.18)",
                  color: "#e6edf3",
                  cursor: isSyncingSections ? "not-allowed" : "pointer",
                  fontWeight: 700,
                  opacity: isSyncingSections ? 0.7 : 1,
                }}
              >
                {isSyncingSections ? "Syncing Sections..." : "Sync Sections"}
              </button>
            </div>

            <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
              {sections.map((section) => {
                const isSelected = selectedSection?.id === section.id;
                const status = getSectionStatus(section);

                return (
                  <button
                    key={section.id}
                    type="button"
                    onClick={() => setSelectedSection(section)}
                    style={{
                      textAlign: "left",
                      padding: "14px 16px",
                      borderRadius: "14px",
                      border: isSelected
                        ? "1px solid rgba(59,130,246,0.7)"
                        : "1px solid rgba(148,163,184,0.12)",
                      background: isSelected
                        ? "rgba(30, 64, 175, 0.18)"
                        : "rgba(2, 8, 23, 0.7)",
                      color: "#e6edf3",
                      cursor: "pointer",
                    }}
                  >
                    <div style={{ fontWeight: 700 }}>{section.displayName}</div>

                    <div style={{ marginTop: "6px", fontSize: "13px", color: "#9fb0c3" }}>
                      Tags found: {section.childCount ?? 0}
                    </div>

                    <div
                      style={{
                        marginTop: "4px",
                        fontSize: "13px",
                        color: status.color,
                        fontWeight: 700,
                      }}
                    >
                      {status.label}
                    </div>

                    <div style={{ marginTop: "2px", fontSize: "12px", color: "#7f93ac" }}>
                      Enabled tags: {section.enabledTagsCount ?? 0} / {section.childCount ?? 0}
                    </div>

                    <div style={{ marginTop: "4px", fontSize: "12px", color: "#7f93ac" }}>
                      {section.nodeId}
                    </div>
                  </button>
                );
              })}
            </div>
          </SectionCard>

          <SectionCard
            title={selectedSection ? selectedSection.displayName : "Section Tags"}
            badge={
              isTagsLoading
                ? "Loading..."
                : tagSearch.trim()
                ? `${filteredTags.length} / ${tags.length} Tags`
                : `${tags.length} Tags`
            }          >
            {selectedSection && (
              <div
                style={{
                  marginBottom: "16px",
                  display: "flex",
                  gap: "12px",
                  flexWrap: "wrap",
                }}
              >
                <button
                  type="button"
                  onClick={() => handleAllTagsToggle(true)}
                  style={{
                    padding: "10px 14px",
                    borderRadius: "12px",
                    border: "1px solid rgba(148,163,184,0.15)",
                    background: "rgba(34,197,94,0.18)",
                    color: "#e6edf3",
                    cursor: "pointer",
                    fontWeight: 600,
                  }}
                >
                  Enable All Tags
                </button>

                <button
                  type="button"
                  onClick={() => handleAllTagsToggle(false)}
                  style={{
                    padding: "10px 14px",
                    borderRadius: "12px",
                    border: "1px solid rgba(148,163,184,0.15)",
                    background: "rgba(239,68,68,0.18)",
                    color: "#e6edf3",
                    cursor: "pointer",
                    fontWeight: 600,
                  }}
                >
                  Disable All Tags
                </button>
              </div>
            )}

            {selectedSection && (
                    <div style={{ marginBottom: "16px" }}>
                      <input
                        type="text"
                        value={tagSearch}
                        onChange={(e) => setTagSearch(e.target.value)}
                        placeholder="Search tags by display name, tag name, or node id..."
                        style={{
                          width: "100%",
                          padding: "12px 14px",
                          borderRadius: "12px",
                          border: "1px solid rgba(148,163,184,0.15)",
                          background: "rgba(2, 8, 23, 0.7)",
                          color: "#e6edf3",
                          outline: "none",
                        }}
                      />
                    </div>  
                  )}
                  
            {!selectedSection ? (
              <div style={{ color: "#9fb0c3" }}>Select a section to view its tags.</div>
            ) : (
              <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
                {filteredTags.length === 0 && (
                <div
                  style={{
                    padding: "16px",
                    borderRadius: "12px",
                    border: "1px solid rgba(148,163,184,0.10)",
                    background: "rgba(2, 8, 23, 0.7)",
                    color: "#9fb0c3",
                  }}
                >
                  No tags match your search.
                </div>
              )}
                {filteredTags.map((tag) => (
                  <div
                    key={tag.id}
                    style={{
                      padding: "14px 16px",
                      borderRadius: "14px",
                      border: "1px solid rgba(148,163,184,0.10)",
                      background: "rgba(2, 8, 23, 0.7)",
                    }}
                  >
                    <div
                      style={{
                        display: "flex",
                        justifyContent: "space-between",
                        gap: "12px",
                        alignItems: "center",
                      }}
                    >
                      <strong>{tag.displayName}</strong>
                      <span style={{ color: "#9fb0c3", fontSize: "13px" }}>
                        {tag.dataType ?? "Unknown"}
                      </span>
                    </div>

                    <div style={{ marginTop: "8px", fontSize: "13px", color: "#9fb0c3" }}>
                      Tag name: {tag.tagName}
                    </div>

                    <div style={{ marginTop: "4px", fontSize: "12px", color: "#7f93ac" }}>
                      {tag.nodeId}
                    </div>

                    <div
                      style={{
                        marginTop: "8px",
                        fontSize: "13px",
                        color: tag.isEnabled ? "#4ade80" : "#f87171",
                        fontWeight: 600,
                      }}
                    >
                      {tag.isEnabled ? "Enabled" : "Disabled"}
                    </div>

                    <div style={{ marginTop: "12px" }}>
                      <button
                        type="button"
                        onClick={() => handleTagToggle(tag.id, tag.isEnabled)}
                        style={{
                          padding: "8px 12px",
                          borderRadius: "10px",
                          border: "1px solid rgba(148,163,184,0.15)",
                          background: tag.isEnabled
                            ? "rgba(239,68,68,0.18)"
                            : "rgba(34,197,94,0.18)",
                          color: "#e6edf3",
                          cursor: "pointer",
                          fontWeight: 600,
                        }}
                      >
                        {tag.isEnabled ? "Disable Tag" : "Enable Tag"}
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </SectionCard>
        </section>
      </div>
    </PageStateWrapper>
  );
}

export default OpcUaExplorerPage;