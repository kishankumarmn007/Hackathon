from collections import defaultdict

def summarize_orders(events):
    seen_event_ids = set()
    deduped_events = []
    for ev in events:
        if ev['event_id'] not in seen_event_ids:
            seen_event_ids.add(ev['event_id'])
            deduped_events.append(ev)

    grouped_by_order = defaultdict(list)
    for ev in deduped_events:
        grouped_by_order[ev['order_id']].append(ev)

    summaries = []

    for order_id in sorted(grouped_by_order.keys()):
        order_events = sorted(
            grouped_by_order[order_id],
            key=lambda x: (x.get('timestamp', 0), x.get('event_id', ''))
        )

        status = 'Created'
        items = defaultdict(int)

        for ev in order_events:
            etype = ev.get('type')

            if status in ('PAID', 'CANCELLED'):
                continue
            if etype == 'PAID':
                status = 'PAID'
            elif etype == 'CANCELLED':
                status = 'CANCELLED'
            elif etype == 'ITEM_ADDED':
                sku = ev.get('sku')
                items[sku] += ev.get('qty', 0)
            elif etype == 'ITEM_REMOVED':
                sku = ev.get('sku')
                items[sku] = max(0, items[sku] - ev.get('qty', 0))
                if items[sku] == 0:
                    del items[sku]


        summaries.append({
            'order_id' : order_id,
            'status': status,
            'items': dict(items)
        })

    return summaries