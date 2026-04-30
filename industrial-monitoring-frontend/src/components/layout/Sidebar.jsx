import { NavLink } from "react-router-dom";

const navItems = [
    { label: "Dashboard", path: "/" },
    { label: "Monitoring", path: "/monitoring" },
    { label: "Trends", path: "/trends" },
    { label: "Alerts", path: "/alerts" },
];

function Sidebar() {
    return (
        <aside className="sidebar">
            <div className="sidebar-brand">
                <h2>IndMon</h2>
                <span>Industrial Monitoring</span>
            </div>

            <nav className="sidebar-nav">
                {navItems.map((item) => (
                    <NavLink
                        key={item.path}
                        to={item.path}
                        className={({ isActive }) =>
                            isActive ? "sidebar-link active" : "sidebar-link"
                        }
                    >
                        {item.label}
                    </NavLink>
                ))}
            </nav>
        </aside>
    );
}

export default Sidebar;