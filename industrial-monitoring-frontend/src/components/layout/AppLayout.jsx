import { Outlet } from "react-router-dom";
import Sidebar from "./Sidebar";
import Topbar from "./Topbar";

function AppLayout() {
    return (
        <div className="layout-shell">
            <Sidebar />

            <div className="layout-main">
                <Topbar />

                <main className="layout-content">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}

export default AppLayout;