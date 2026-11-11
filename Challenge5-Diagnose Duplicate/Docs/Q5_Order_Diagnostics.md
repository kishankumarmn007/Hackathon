Root Causes:

1. Dual-Write Anti pattern:
The API writes to the database and publishes to the message broker sequentially. If DB commits but publishing fails or times out, the client retries, writing a duplicate order. If the message publishes before DB transaction commits, a consumer queries for an order that does ot exist.

2. Deadlocks:
Concurrent retry requests lock rows across Orders and inventory tables in inconsistent orders.

Full Stack Resolution:

1. React: Generate a UUID Idempotency-Key on initial click; send the exact same key on all network retries.

2. ASP.NET Core API: Validate the Idempotency header before execution; return the cached response if seen.

3. Transactional Outbox Pattern: Save both the order entity and an OutboxEvent within the same atomic SQL transaction. A seperate asynchronous worker polls/streams the outbox table to dispacth messages to the broker.

4. Deadlock Mitigation: Standardise locking order across all endpoints(e.g., lock Orders before InventoryItems).

Observability and Rollout:
1. Track duplicate_requests_absorbed_total, outbox_publisher-log-seconds, and deadlock counters.
2. Deploy backend Idempotency and outbox worker first before updating client retry policies.