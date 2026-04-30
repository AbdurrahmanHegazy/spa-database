import { Link } from "react-router-dom";
import { useEffect, useState } from "react";
import PageHeader from "../components/ui/PageHeader";
import StatusBadge from "../components/ui/StatusBadge";
import StatCard from "../components/ui/StatCard";
import PageStateWrapper from "../components/ui/PageStateWrapper";
import { getMonitoringData } from "../services/monitoringService";

function MonitoringPage() {
    const [monitoringData, setMonitoringData] = useState({
        summary: [],
        liveTags: [],
    });
    const [isLoading, setIsLoading] = useState(true);
    const [errorMessage, setErrorMessage] = useState("");

    useEffect(() => {
        async function loadMonitoring() {
            try {
                setIsLoading(true);
                setErrorMessage("");

                const data = await getMonitoringData();
                setMonitoringData(data);
            } catch (error) {
                setErrorMessage(error.message || "Failed to load monitoring data.");
            } finally {
                setIsLoading(false);
            }
        }

        loadMonitoring();
    }, []);

    return (
        <PageStateWrapper
            isLoading={isLoading}
            errorMessage={errorMessage}
            isEmpty={monitoringData.liveTags.length === 0}
            loadingTitle="Loading monitoring data..."
            loadingMessage="Fetching current values and live signal state."
            errorTitle="Monitoring unavailable"
            emptyTitle="No live monitoring data"
            emptyMessage="There are no active live tags available right now."
        >
            <div className="monitoring-page">
                <PageHeader
                    title="Real-Time Monitoring"
                    subtitle="Live industrial tag values and current operational state."
                    rightContent={
                        <div className="monitoring-live-indicator">
                            <span className="status-dot" />
                            <span>Live Stream Active</span>
                        </div>
                    }
                />

                <section className="monitoring-summary-grid">
                    {monitoringData.summary.map((item) => (
                        <StatCard
                            key={item.title}
                            title={item.title}
                            value={item.value}
                            subtitle={item.subtitle}
                            className="monitoring-summary-card"
                        />
                    ))}
                </section>

                <section className="dashboard-card monitoring-table-panel">
                    <div className="panel-header">
                        <h2>Live Tag Stream</h2>
                        <span className="panel-badge">Live Data</span>
                    </div>

                    <div className="monitoring-table">
                        <div className="monitoring-table-head">
                            <span>Group</span>
                            <span>Tag</span>
                            <span>Value</span>
                            <span>Quality</span>
                            <span>Freshness</span>
                            <span>Status</span>
                        </div>

                        <div className="monitoring-table-body">
                            {monitoringData.liveTags.map((item) => (
                                <Link
                                    key={item.id}
                                    to={`/tags/${item.id}`}
                                    className="monitoring-table-row clickable-row"
                                >
                                    <span>{item.group}</span>
                                    <span className="tag-name-cell">{item.tag}</span>
                                    <strong>{item.value}</strong>
                                    <span>{item.quality}</span>
                                    <span>{item.freshness}</span>
                                    <StatusBadge
                                        label={
                                            item.status === "online"
                                                ? "Online"
                                                : item.status === "warning"
                                                    ? "Warning"
                                                    : "Offline"
                                        }
                                        variant={item.status}
                                    />
                                </Link>
                            ))}
                        </div>
                    </div>
                </section>
            </div>
        </PageStateWrapper>
    );
}

export default MonitoringPage;