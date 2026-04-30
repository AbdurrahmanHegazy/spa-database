function FilterInput({
    label,
    type = "text",
    value,
    onChange,
    placeholder = "",
    className = "",
}) {
    return (
        <div className={`filter-field ${className}`}>
            <label className="filter-field-label">{label}</label>
            <input
                className="filter-control"
                type={type}
                value={value}
                onChange={onChange}
                placeholder={placeholder}
            />
        </div>
    );
}

export default FilterInput;