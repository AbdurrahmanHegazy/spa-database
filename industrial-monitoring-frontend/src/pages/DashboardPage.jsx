import { Link } from "react-router-dom";
import { useEffect, useState } from "react";
import StatCard from "../components/ui/StatCard";
import SectionCard from "../components/ui/SectionCard";
import PageStateWrapper from "../components/ui/PageStateWrapper";
import { getDashboardData } from "../services/dashboardService";

function DashboardPage() {
    const [dashboardData, setDashboardData] = useState({
        kpis: [],
        recentAlerts: [],
        topTags: [],
    });
    const [isLoading, setIsLoading] = useState(true);
    const [errorMessage, setErrorMessage] = useState("");

    useEffect(() => {
        async function loadDashboard() {
            try {
                setIsLoading(true);
                setErrorMessage("");

                const data = await getDashboardData();
                setDashboardData(data);
            } catch (error) {
                setErrorMessage(error.message || "Failed to load dashboard data.");
            } finally {
                setIsLoading(false);
            }
        }

        loadDashboard();
    }, []);

    return (
        <PageStateWrapper
    isLoading={isLoading}
    errorMessage={errorMessage}
    isEmpty={dashboardData.kpis.length === 0}
    loadingTitle="Loading dashboard..."
    loadingMessage="Fetching KPI and summary data."
    errorTitle="Dashboard unavailable"
    emptyTitle="No dashboard data"
    emptyMessage="There is no summary data available right now."
>
    <div className="dashboard-page">
        <section className="dashboard-kpis">
            {dashboardData.kpis.map((item) => (
                <StatCard
                    key={item.title}
                    title={item.title}
                    value={item.value}
                    subtitle={item.subtitle}
                />
            ))}
        </section>

        <section className="dashboard-status-grid">
            <SectionCard
                title="Recent Alerts"
                badge={`${dashboardData.recentAlerts.length} Active`}
                badgeClassName="warning"
            >
                <div className="alert-list">
                    {dashboardData.recentAlerts.length === 0 ? (
                        <div className="panel-placeholder">No active alerts.</div>
                    ) : (
                        dashboardData.recentAlerts.map((alert) => (
                            <div key={`${alert.title}-${alert.source}`} className="alert-row">
                                <strong>{alert.title}</strong>
                                <span>{alert.source}</span>
                            </div>
                        ))
                    )}
                </div>
            </SectionCard>

            <SectionCard title="Top Tags" badge="Preview" className="dashboard-top-tags-panel">
                <div className="tag-preview-list">
                    {dashboardData.topTags.length === 0 ? (
                        <div className="panel-placeholder">No enabled live tags available.</div>
                    ) : (
                        dashboardData.topTags.map((tag) => (
                            <Link
                                key={tag.id}
                                to={`/tags/${tag.id}`}
                                className="tag-preview-row clickable-card"
                            >
                                <span>{tag.name}</span>
                                <strong>{tag.value}</strong>
                            </Link>
                        ))
                    )}
                </div>
            </SectionCard>
        </section>

        <section className="dashboard-bottom-grid">
            <SectionCard title="Trend Snapshot" badge="Coming Next" className="dashboard-trend-panel">
                <div className="panel-placeholder">
                    Historical mini chart area will be placed here.
                </div>
            </SectionCard>
        </section>
    </div>
</PageStateWrapper>
    );
}

export default DashboardPage;