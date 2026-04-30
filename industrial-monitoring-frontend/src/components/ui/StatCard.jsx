function StatCard({ title, value, subtitle, className = "" }) {
    return (
        <article className={`dashboard-card stat-card ${className}`}>
            <span className="card-label">{title}</span>
            <strong className="card-value">{value}</strong>
            <p className="card-subtitle">{subtitle}</p>
        </article>
    );
}

export default StatCard;    