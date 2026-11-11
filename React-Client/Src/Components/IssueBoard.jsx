import React, { useEffect, useRef, useState } from 'react';

export function IssueBoard({ loadIssues }) {
    const [query, setQuery] = useState('');
    const [status, setStatus] = useState('All');
    const [issues, setIssues] = useState([]);
    const [selectedId, setSelectedId] = useState(null);
    const [loading, setLoading] = useState(false);
    const [ error, setError] = useState(null);
    const [retryTrigger, setRetryTrigger] = useState(0);

    const abortControllerRef = useRef(null);

    useEffect(() => {
        const controller = new AbortController();
        abortControllerRef.current = controller;
        const timeoutId = setTimeout(async () => {
            setLoading(true);
            setError(null);

            try {
                const data = await loadIssues({
                    query,
                    status,
                    signal: controller.signal
                });

                if (!controller.signal.aborted) {
                    setIssues(data);
                    setSelectedId(prev => (data.some(item => item.id === prev) ? prev : null));
                    setLoading(false);
                }
            } catch (err) {
                if (err.name !== 'AbortError' && !controller.signal.aborted) {
                    setError(err.message || 'failed to load issues.');
                    setLoading(false);
                }
            }
        }, 300);

        return () => {
            clearTimeout(timeoutId);
            controller.abort();
            if (abortControllerRef.current === controller) {
                abortControllerRef.current = null;
            }
        };
    }, [loadIssues, query, retryTrigger, status]);

    return (
        <section aria-label="Issue Management Board">
            <div>
                <label htmlFor="search-input">Search Issues:</label>
                <input
                    id="search-input"
                    type="text"
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    placeholder="Filter Issues..."
                />

                <label htmlFor="status-select">Status Filter:</label>
                <select id="status-select" value={status} onChange={(e) => setStatus(e.target.value)}>
                    <option value="All">All</option>
                    <option value="OPEN">Open</option>
                    <option value="CLOSED">Closed</option>
                </select>
            </div>

            {loading && <div role="status">Loading issues...</div>}
            {error && (
                <div role="alert">
                    <p>{error}</p>
                    <button type="button" onClick={() => setRetryTrigger(c => c + 1)}>Retry</button>
                </div>
            )}

            {!loading && !error && issues.length === 0 && <p>No issues found.</p>}

            <ul>
                {issues.map(item => (
                    <li
                        key={item.id}
                        onClick={() => setSelectedId(item.id)}
                        style={{ fontWeight: selectedId === item.id ? 'bold' : 'normal', cursor: 'pointer' }}
                    >
                        {item.title} ({item.status})
                    </li>
                ))}
            </ul>
        </section>
    );
}