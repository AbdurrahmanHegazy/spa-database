function Topbar() {
    return (
        <header className="topbar">
            <div>
                <h1 className="topbar-title">Industrial Monitoring</h1>
                <p className="topbar-subtitle">Real-time operations dashboard</p>
            </div>

            <div className="topbar-status">
                <span className="status-dot" />
                <span>System Online</span>
            </div>
        </header>
    );
}

export default Topbar;