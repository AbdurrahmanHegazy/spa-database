  import { useEffect, useState } from "react";
  import {
    ResponsiveContainer,
    LineChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
  } from "recharts";
  import PageHeader from "../components/ui/PageHeader";
  import FilterPanel from "../components/ui/FilterPanel";
  import FilterInput from "../components/ui/FilterInput";
  import FilterSelect from "../components/ui/FilterSelect";
  import StatCard from "../components/ui/StatCard";
  import SectionCard from "../components/ui/SectionCard";
  import PageStateWrapper from "../components/ui/PageStateWrapper";
  import { getTrendsData } from "../services/trendsService";
  import { getEnabledOpcUaTags } from "../services/opcUaDiscoveryService";

  function TrendsPage() {
    const [filterMode, setFilterMode] = useState("preset");

    const [trendsData, setTrendsData] = useState({
      filters: {
        selectedTag: "",
        timeRange: "",
        from: "",
        to: "",
      },
      stats: [],
      sampleRows: [],
      chartPoints: [],
    });

    const [filters, setFilters] = useState({
      selectedTag: "",
      timeRange: "last-1h",
      from: "",
      to: "",
    });

    const [enabledTags, setEnabledTags] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isTagsLoading, setIsTagsLoading] = useState(false);
    const [errorMessage, setErrorMessage] = useState("");

    function formatDateTimeLocal(date) {
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, "0");
      const day = String(date.getDate()).padStart(2, "0");
      const hours = String(date.getHours()).padStart(2, "0");
      const minutes = String(date.getMinutes()).padStart(2, "0");

      return `${year}-${month}-${day}T${hours}:${minutes}`;
    }

    function handleFilterChange(field, value) {
      setFilters((prev) => ({
        ...prev,
        [field]: value,
      }));
    }

    async function loadTrends(requestFilters = {}) {
      try {
        setIsLoading(true);
        setErrorMessage("");

        const data = await getTrendsData(requestFilters);
        setTrendsData(data);
      } catch (error) {
        setErrorMessage(error.message || "Failed to load trends data.");
      } finally {
        setIsLoading(false);
      }
    }

    async function handleApplyFilters() {
      if (!filters.selectedTag) {
        setErrorMessage("Please select an enabled tag first.");
        return;
      }

      const requestFilters =
        filterMode === "preset"
          ? {
              tagId: filters.selectedTag,
              timeRange: filters.timeRange,
            }
          : {
              tagId: filters.selectedTag,
              from: filters.from,
              to: filters.to,
              timeRange: "custom",
            };

      await loadTrends(requestFilters);
    }

    useEffect(() => {
      const now = new Date();
      const oneHourAgo = new Date(now.getTime() - 60 * 60 * 1000);

      setFilters((prev) => ({
        ...prev,
        from: formatDateTimeLocal(oneHourAgo),
        to: formatDateTimeLocal(now),
      }));
    }, []);

    useEffect(() => {
      async function loadEnabledTags() {
        try {
          setIsTagsLoading(true);

          const data = await getEnabledOpcUaTags();
          setEnabledTags(data);

          if (data.length > 0) {
            const defaultTagNodeId = data[0].nodeId;

            setFilters((prev) => ({
              ...prev,
              selectedTag: defaultTagNodeId,
            }));

            await loadTrends({
              tagId: defaultTagNodeId,
              timeRange: "last-1h",
            });
          } else {
            setIsLoading(false);
          }
        } catch (error) {
          setErrorMessage(error.message || "Failed to load enabled tags.");
          setIsLoading(false);
        } finally {
          setIsTagsLoading(false);
        }
      }

      loadEnabledTags();
    }, []);

    return (
      <PageStateWrapper
        isLoading={isLoading}
        errorMessage={errorMessage}
        isEmpty={enabledTags.length === 0 || trendsData.stats.length === 0}
        loadingTitle="Loading trends..."
        loadingMessage="Fetching historical values and statistics."
        errorTitle="Trends unavailable"
        emptyTitle="No trends data"
        emptyMessage="There are no enabled tags or historical trend data available right now."
      >
        <div className="trends-page">
          <PageHeader
            title="Historical Trends"
            subtitle="Analyze historical values over a selected time period."
          />

          <FilterPanel title="Trend Filters" badge="Interactive Controls">
            <FilterSelect
              label="Filter Mode"
              value={filterMode}
              onChange={(e) => setFilterMode(e.target.value)}
              options={[
                { value: "preset", label: "Preset Range" },
                { value: "custom", label: "Custom Range" },
              ]}
            />

            <FilterSelect
              label="Selected Tag"
              value={filters.selectedTag}
              onChange={(e) => handleFilterChange("selectedTag", e.target.value)}
              disabled={isTagsLoading || enabledTags.length === 0}
              options={[
                { value: "", label: isTagsLoading ? "Loading enabled tags..." : "Select enabled tag" },
                ...enabledTags.map((tag) => ({
                  value: tag.nodeId,
                  label: `${tag.sectionName} / ${tag.displayName}`,
                })),
              ]}
            />

            <FilterSelect
              label="Time Range"
              value={filters.timeRange}
              onChange={(e) => handleFilterChange("timeRange", e.target.value)}
              disabled={filterMode !== "preset"}
              options={[
                { value: "last-15m", label: "Last 15 Minutes" },
                { value: "last-1h", label: "Last 1 Hour" },
                { value: "last-6h", label: "Last 6 Hours" },
                { value: "last-24h", label: "Last 24 Hours" },
              ]}
            />

            <FilterInput
              label="From"
              type="datetime-local"
              value={filters.from}
              onChange={(e) => handleFilterChange("from", e.target.value)}
              disabled={filterMode !== "custom"}
            />

            <FilterInput
              label="To"
              type="datetime-local"
              value={filters.to}
              onChange={(e) => handleFilterChange("to", e.target.value)}
              disabled={filterMode !== "custom"}
            />

            <div className="filter-actions">
              <button className="primary-button" onClick={handleApplyFilters}>
                Apply Filters
              </button>
            </div>
          </FilterPanel>

          <section className="trends-stats-grid">
            {trendsData.stats.map((item) => (
              <StatCard
                key={item.title}
                title={item.title}
                value={item.value}
                subtitle={item.subtitle}
                className="trends-stat-card"
              />
            ))}
          </section>

          <section className="trends-main-grid">
            <SectionCard
              title="Trend Chart"
              badge="Live Preview"
              className="trends-chart-panel"
            >
              <div className="trend-chart-container">
                <ResponsiveContainer width="100%" height={320}>
                  <LineChart data={trendsData.chartPoints}>
                    <CartesianGrid stroke="rgba(148, 163, 184, 0.12)" />
                    <XAxis dataKey="time" stroke="#9fb0c3" />
                    <YAxis stroke="#9fb0c3" />
                    <Tooltip
                      contentStyle={{
                        background: "#081121",
                        border: "1px solid rgba(148, 163, 184, 0.15)",
                        borderRadius: "12px",
                        color: "#e6edf3",
                      }}
                    />
                    <Line
                      type="monotone"
                      dataKey="value"
                      stroke="#3b82f6"
                      strokeWidth={3}
                      dot={{ r: 3 }}
                      activeDot={{ r: 5 }}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            </SectionCard>

            <SectionCard
              title="Sample Values"
              badge="Preview"
              className="trends-samples-panel"
            >
              <div className="sample-values-list">
                {trendsData.sampleRows.map((row, index) => (
                  <div key={`${row.time}-${index}`} className="sample-value-row">
                    <span>{row.time}</span>
                    <strong>{row.value}</strong>
                  </div>
                ))}
              </div>
            </SectionCard>
          </section>
        </div>
      </PageStateWrapper>
    );
  }

  export default TrendsPage;