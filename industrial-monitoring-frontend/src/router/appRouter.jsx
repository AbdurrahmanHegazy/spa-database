import { createBrowserRouter } from "react-router-dom";
import AppLayout from "../components/layout/AppLayout";
import DashboardPage from "../pages/DashboardPage";
import MonitoringPage from "../pages/MonitoringPage";
import TrendsPage from "../pages/TrendsPage";
import AlertsPage from "../pages/AlertsPage";
import TagDetailsPage from "../pages/TagDetailsPage";
import NotFoundPage from "../pages/NotFoundPage";
import OpcUaExplorerPage from "../pages/OpcUaExplorerPage";

const appRouter = createBrowserRouter([
    {
        path: "/",
        element: <AppLayout />,
        children: [
            {
                index: true,
                element: <DashboardPage />,
            },
            {
                path: "monitoring",
                element: <MonitoringPage />,
            },
            {
                path: "trends",
                element: <TrendsPage />,
            },
            {
                path: "alerts",
                element: <AlertsPage />,
            },
            {
                path: "tags/:tagId",
                element: <TagDetailsPage />,
            },
            {
                path: "opcua-explorer",
                element: <OpcUaExplorerPage />,
            },
        ],
    },
    {
        path: "*",
        element: <NotFoundPage />,
    },
    
]);

export default appRouter;