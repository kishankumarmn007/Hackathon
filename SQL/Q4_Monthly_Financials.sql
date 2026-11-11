WITH MonthlyOrders AS (
    SELECT
        o.customer_id,
        DATE_TRUNC('month', o.ordered_at) AS order_month,
        COUNT(DISTINCT o.order_id) AS order_count,
        COALESCE(SUM(oi.quantity * oi.unit_price), 0) AS gross_order_value
    FROM orders o
    JOIN order_items oi ON o.order_id = oi.order_id
    WHERE o.ordered_at >= '2026-01-01'
        AND o.ordered_at < '2027-01-01'
        AND o.status != 'CANCELLED'
    GROUP BY o.customer_id, DATE_TRUNC('month', o.ordered_at)
),

MonthlyPayments AS (
    SELECT
        o.customer_id,
        DATE_TRUNC('month', o.ordered_at) AS order_month,
        COALESCE(SUM(p.amount), 0) AS captured_payment_value
    FROM orders o
    JOIN payments p ON o.order_id = p.order_id
    WHERE o.ordered_at >= '2026-01-01'
        AND o.ordered_at < '2027-01-01'
        AND o.status != 'CANCELLED'
        AND p.status = 'CAPTURED'
    GROUP BY o.customer_id, DATE_TRUNC('month', o.ordered_at)
)

SELECT
    c.customer_id,
    c.name AS customer_name,
    TO_CHAR(mo.order_month, 'YYYY-MM') AS month,
    mo.order_count,
    mo.gross_order_value,
    COALESCE(mp.captured_payment_value, 0) AS captured_payment_value,
    (mo.gross_order_value - COALESCE(mp.captured_payment_value, 0)) AS outstanding_value
FROM MonthlyOrders mo
JOIN customer c ON mo.customer_id = c.customer_id
LEFT JOIN MonthlyPayments mp
    ON mo.customer_id = mp.customer_id
    AND mo.order_month = mp.order_month
ORDER BY c.customer_id, mo.order_month;