function FilterField({ label, value, className = "" }) {
    return (
        <div className={`filter-field ${className}`}>
            <label className="filter-field-label">{label}</label>
            <div className="filter-field-value">{value}</div>
        </div>
    );
}

export default FilterField;