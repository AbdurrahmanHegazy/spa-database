function SectionCard({ title, badge, badgeClassName = "", children, className = "" }) {
    return (
        <div className={`dashboard-card dashboard-panel ${className}`}>
            <div className="panel-header">
                <h2>{title}</h2>
                {badge ? <span className={`panel-badge ${badgeClassName}`}>{badge}</span> : null}
            </div>

            {children}
        </div>
    );
}

export default SectionCard;