function StatusBadge({ label, variant = "default", className = "" }) {
    return (
        <span className={`status-badge status-badge-${variant} ${className}`}>
            {label}
        </span>
    );
}

export default StatusBadge;