import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import PageStateWrapper from "../components/ui/PageStateWrapper";
import PageHeader from "../components/ui/PageHeader";
import StatCard from "../components/ui/StatCard";
import SectionCard from "../components/ui/SectionCard";
import { getTagDetails } from "../services/tagDetailsService";

function TagDetailsPage() {
  const { tagId } = useParams();

  const [selectedTag, setSelectedTag] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    async function loadTagDetails() {
      try {
        setIsLoading(true);
        setErrorMessage("");

        const data = await getTagDetails(tagId);
        setSelectedTag(data);
      } catch (error) {
        setErrorMessage(error.message || "Failed to load tag details.");
      } finally {
        setIsLoading(false);
      }
    }

    loadTagDetails();
  }, [tagId]);

  const isEmpty = !selectedTag;

  const summaryCards = selectedTag
    ? [
        {
          title: "Current Value",
          value: selectedTag.summary?.currentValue ?? "--",
          subtitle: selectedTag.metadata?.find((x) => x.label === "Data Type")?.value ?? "Unknown",
        },
        {
          title: "Quality",
          value: selectedTag.summary?.quality ?? "Unknown",
          subtitle: "Signal status",
        },
        {
          title: "Last Update",
          value: selectedTag.summary?.lastUpdate ?? "--",
          subtitle: selectedTag.summary?.freshness ?? "--",
        },
        {
          title: "Device State",
          value: selectedTag.summary?.deviceState ?? "Unknown",
          subtitle: "Connectivity",
        },
      ]
    : [];

  const metadata = selectedTag?.metadata ?? [];
  const statistics = selectedTag?.statistics ?? [];

  return (
    <PageStateWrapper
      isLoading={isLoading}
      errorMessage={errorMessage}
      isEmpty={isEmpty}
      loadingTitle="Loading tag details..."
      loadingMessage="Fetching selected tag metadata and recent statistics."
      errorTitle="Tag details unavailable"
      emptyTitle="No tag details"
      emptyMessage="No details are available for this selected route."
    >
      <div className="tag-details-page">
        <PageHeader
          title="Tag / Device Details"
          subtitle="Detailed inspection view for the selected monitored signal."
          rightContent={
            <div className="tag-route-badge">
              <span>Route param:</span>
              <strong>{selectedTag?.routeParam ?? tagId}</strong>
            </div>
          }
        />

        <div
          style={{
            marginBottom: "16px",
            display: "flex",
            gap: "12px",
            flexWrap: "wrap",
          }}
        >
        <Link
    to="/monitoring"
    className="primary-button"
    style={{
        textDecoration: "none",
        display: "inline-flex", 
        alignItems: "center",
        justifyContent: "center",
    }}
>
    Back to Monitoring
</Link>
          {selectedTag?.metadata?.find((x) => x.label === "Redis Key")?.value ? (
            <span
              style={{
                alignSelf: "center",
                color: "#9fb0c3",
                fontSize: "14px",
              }}
            >
              Live source connected
            </span>
          ) : null}
        </div>

        <section className="tag-details-summary-grid">
          {summaryCards.map((item) => (
            <StatCard
              key={item.title}
              title={item.title}
              value={item.value}
              subtitle={item.subtitle}
              className="tag-summary-card"
            />
          ))}
        </section>

        <section className="tag-details-main-grid">
          <SectionCard title="Metadata" badge="Tag Info" className="tag-metadata-panel">
            <div className="metadata-list">
              {metadata.map((item) => (
                <div key={item.label} className="metadata-row">
                  <span>{item.label}</span>
                  <strong>{item.value}</strong>
                </div>
              ))}
            </div>
          </SectionCard>

          <SectionCard title="Recent Statistics" badge="Last 1 Hour" className="tag-stats-panel">
            <div className="detail-stats-list">
              {statistics.map((item) => (
                <div key={item.label} className="detail-stat-row">
                  <span>{item.label}</span>
                  <strong>{item.value}</strong>
                </div>
              ))}
            </div>
          </SectionCard>
        </section>

        <SectionCard title="Trend Preview" badge="Linked Analysis" className="tag-trend-panel">
          <div className="trend-chart-placeholder">
            Open the Trends page and use this tag to inspect the historical chart.
          </div>

          <div style={{ marginTop: "16px" }}>
            <Link
  to={`/trends?tagId=${encodeURIComponent(selectedTag?.metadata?.find((x) => x.label === "Tag Name")?.value ?? "")}`}
  className="primary-button"
  style={{ textDecoration: "none", display: "inline-block" }}
>
  Open Trends
</Link>
          </div>
        </SectionCard>
      </div>
    </PageStateWrapper>
  );
}

export default TagDetailsPage;