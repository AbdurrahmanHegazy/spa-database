import { useEffect, useState } from "react";
import PageHeader from "../components/ui/PageHeader";
import StatCard from "../components/ui/StatCard";
import SectionCard from "../components/ui/SectionCard";
import StatusBadge from "../components/ui/StatusBadge";
import PageStateWrapper from "../components/ui/PageStateWrapper";
import { getAlertsData } from "../services/alertsService";

function AlertsPage() {
    const [alertsData, setAlertsData] = useState({
        summary: [],
        activeAlerts: [],
        eventHistory: [],
    });
    const [isLoading, setIsLoading] = useState(true);
    const [errorMessage, setErrorMessage] = useState("");

    useEffect(() => {
        async function loadAlerts() {
            try {
                setIsLoading(true);
                setErrorMessage("");

                const data = await getAlertsData();
                setAlertsData(data);
            } catch (error) {
                setErrorMessage(error.message || "Failed to load alerts data.");
            } finally {
                setIsLoading(false);
            }
        }

        loadAlerts();
    }, []);

    return (
        <PageStateWrapper
            isLoading={isLoading}
            errorMessage={errorMessage}
            isEmpty={alertsData.summary.length === 0}
            loadingTitle="Loading alerts..."
            loadingMessage="Fetching active alarms and recent events."
            errorTitle="Alerts unavailable"
            emptyTitle="No alerts data"
            emptyMessage="There are no alert or event records available right now."
        >
            <div className="alerts-page">
                <PageHeader
                    title="Alerts & Events"
                    subtitle="Track active alarms, warnings, and recent operational events."
                />

                <section className="alerts-summary-grid">
                    {alertsData.summary.map((item) => (
                        <StatCard
                            key={item.title}
                            title={item.title}
                            value={item.value}
                            subtitle={item.subtitle}
                            className="alerts-summary-card"
                        />
                    ))}
                </section>

                <section className="alerts-main-grid">
                    <SectionCard title="Active Alerts" badge="Needs Attention" badgeClassName="warning" className="alerts-panel">
                        <div className="alerts-list">
                            {alertsData.activeAlerts.map((alert) => (
                                <div key={`${alert.title}-${alert.source}`} className="alert-item-card">
                                    <div className="alert-item-main">
                                        <div>
                                            <strong>{alert.title}</strong>
                                            <span>{alert.source}</span>
                                        </div>

                                        <StatusBadge
                                            label={alert.severity === "critical" ? "Critical" : "Warning"}
                                            variant={alert.severity}
                                        />
                                    </div>

                                    <div className="alert-item-footer">
                                        <span>{alert.time}</span>
                                        <button className="ghost-button">Acknowledge</button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </SectionCard>

                    <SectionCard title="Recent Event History" badge="Today" className="events-panel">
                        <div className="event-history-list">
                            {alertsData.eventHistory.map((event) => (
                                <div key={`${event.title}-${event.timestamp}`} className="event-history-row">
                                    <div className="event-history-main">
                                        <strong>{event.title}</strong>
                                        <span>{event.source}</span>
                                    </div>

                                    <div className="event-history-meta">
                                        <span>{event.timestamp}</span>
                                        <StatusBadge label={event.type} variant={event.type} />
                                    </div>
                                </div>
                            ))}
                        </div>
                    </SectionCard>
                </section>
            </div>
        </PageStateWrapper>
    );
}

export default AlertsPage;