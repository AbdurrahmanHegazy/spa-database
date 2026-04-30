function FilterSelect({
    label,
    value,
    onChange,
    options = [],
    className = "",
}) {
    return (
        <div className={`filter-field ${className}`}>
            <label className="filter-field-label">{label}</label>
            <select className="filter-control" value={value} onChange={onChange}>
                {options.map((option) => (
                    <option key={option.value} value={option.value}>
                        {option.label}
                    </option>
                ))}
            </select>
        </div>
    );
}

export default FilterSelect;