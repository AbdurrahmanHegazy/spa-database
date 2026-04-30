import SectionCard from "./SectionCard";

function FilterPanel({ title, badge, children, className = "" }) {
    return (
        <SectionCard title={title} badge={badge} className={className}>
            <div className="filter-panel-grid">{children}</div>
        </SectionCard>
    );
}

export default FilterPanel;