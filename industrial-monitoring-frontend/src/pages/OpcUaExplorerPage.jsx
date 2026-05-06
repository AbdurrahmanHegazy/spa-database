import { useEffect, useState } from "react";
import PageHeader from "../components/ui/PageHeader";
import SectionCard from "../components/ui/SectionCard";
import PageStateWrapper from "../components/ui/PageStateWrapper";
import {
  getOpcUaSections,
  getOpcUaTagsBySectionId,
  updateTagEnabledState,
  updateTagsEnabledStateBySection,
} from "../services/opcUaDiscoveryService";

function OpcUaExplorerPage() {
  const [sections, setSections] = useState([]);
  const [selectedSection, setSelectedSection] = useState(null);
  const [tags, setTags] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isTagsLoading, setIsTagsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    async function loadSections() {
      try {
        setIsLoading(true);
        setErrorMessage("");

        const data = await getOpcUaSections();
        setSections(data);

        if (data.length > 0) {
          setSelectedSection(data[0]);
        }
      } catch (error) {
        setErrorMessage(error.message || "Failed to load discovered OPC UA sections.");
      } finally {
        setIsLoading(false);
      }
    }

    loadSections();
  }, []);

  useEffect(() => {
    async function loadTags() {
      if (!selectedSection) {
        setTags([]);
        return;
      }

      try {
        setIsTagsLoading(true);
        setErrorMessage("");

        const data = await getOpcUaTagsBySectionId(selectedSection.id);
        setTags(data);
      } catch (error) {
        setErrorMessage(error.message || "Failed to load discovered OPC UA tags.");
      } finally {
        setIsTagsLoading(false);
      }
    }

    loadTags();
  }, [selectedSection]);

  

  async function handleSectionToggle() {
  if (!selectedSection) return;

  try {
    const newValue = !selectedSection.isEnabled;

    await updateSectionEnabledState(selectedSection.id, newValue);

    const updatedSections = sections.map((section) =>
      section.id === selectedSection.id
        ? { ...section, isEnabled: newValue }
        : section
    );

    setSections(updatedSections);
    setSelectedSection({ ...selectedSection, isEnabled: newValue });

    const refreshedTags = await getOpcUaTagsBySectionId(selectedSection.id);
    setTags(refreshedTags);
  } catch (error) {
    setErrorMessage(error.message || "Failed to update section state.");
  }
}

async function handleAllTagsToggle(isEnabled) {
  if (!selectedSection) return;

  try {
    await updateTagsEnabledStateBySection(selectedSection.id, isEnabled);

    const refreshedTags = await getOpcUaTagsBySectionId(selectedSection.id);
    setTags(refreshedTags);
  } catch (error) {
    setErrorMessage(error.message || "Failed to update all tags state.");
  }
}

async function handleTagToggle(tagId, currentValue) {
  try {
    const newValue = !currentValue;

    await updateTagEnabledState(tagId, newValue);

    const refreshedTags = await getOpcUaTagsBySectionId(selectedSection.id);
    setTags(refreshedTags);
  } catch (error) {
    setErrorMessage(error.message || "Failed to update tag state.");
  }
}

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
            <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
              {sections.map((section) => {
                const isSelected = selectedSection?.id === section.id;

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
                      Tags found: {section.childCount}
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
            badge={isTagsLoading ? "Loading..." : `${tags.length} Tags`}
          >
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

            {!selectedSection ? (
              <div style={{ color: "#9fb0c3" }}>Select a section to view its tags.</div>
            ) : (
              <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
                {tags.map((tag) => (
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

                    <div style={{ marginTop: "12px" }}>
                      <button
                        type="button"
                        onClick={() => handleTagToggle(tag.id, tag.isEnabled)}
                        style={{
                          padding: "8px 12px",
                          borderRadius: "10px",
                          border: "1px solid rgba(148,163,184,0.15)",
                          background: tag.isEnabled
                            ? "rgba(34,197,94,0.18)"
                            : "rgba(239,68,68,0.18)",
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