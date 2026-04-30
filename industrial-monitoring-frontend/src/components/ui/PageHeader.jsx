function PageHeader({ title, subtitle, rightContent = null, className = "" }) {
    return (
        <section className={`page-header ${className}`}>
            <div>
                <h1>{title}</h1>
                <p>{subtitle}</p>
            </div>

            {rightContent ? <div className="page-header-right">{rightContent}</div> : null}
        </section>
    );
}

export default PageHeader;