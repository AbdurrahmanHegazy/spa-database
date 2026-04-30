import EmptyState from "./EmptyState";
import ErrorState from "./ErrorState";

function PageStateWrapper({
  isLoading,
  errorMessage,
  isEmpty,
  loadingTitle = "Loading...",
  loadingMessage = "Please wait while data is being fetched.",
  errorTitle = "Something went wrong",
  emptyTitle = "No data available",
  emptyMessage = "There is no data to display right now.",
  children,
}) {
  if (isLoading) {
    return (
      <div className="loading-state">
        <div>
          <h3>{loadingTitle}</h3>
          <p>{loadingMessage}</p>
        </div>
      </div>
    );
  }

  if (errorMessage) {
    return <ErrorState title={errorTitle} message={errorMessage} />;
  }

  if (isEmpty) {
    return <EmptyState title={emptyTitle} message={emptyMessage} />;
  }

  return children;
}

export default PageStateWrapper;