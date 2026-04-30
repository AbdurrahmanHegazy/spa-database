function ErrorState({ title, message }) {
    return (
        <div className="error-state">
            <h3>{title}</h3>
            <p>{message}</p>
        </div>
    );
}

export default ErrorState;