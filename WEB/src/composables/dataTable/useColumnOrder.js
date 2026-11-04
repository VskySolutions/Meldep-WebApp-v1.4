import { ref } from "vue";

// Column ordering for AppDataTable: the user's preferred order of the data columns (the "actions" column
// is always kept last), with drag/move reordering. Saved names no longer in the column set are dropped and
// new columns are appended, so a list that gains a column does not lose the reader's layout.
export default function useColumnOrder ({ columns, initialOrder, saveOrderState }) {
  const dataNames = () => columns.value.filter((c) => c.name !== "actions").map((c) => c.name);

  const reconcile = (saved) => {
    const current = dataNames();
    if (!Array.isArray(saved) || !saved.length) return current;
    const kept = saved.filter((n) => current.includes(n));
    const added = current.filter((n) => !kept.includes(n));
    return [...kept, ...added];
  };

  const order = ref(reconcile(initialOrder));

  const persist = () => saveOrderState?.([...order.value]);

  // Move the item at fromIndex to toIndex within the order.
  const reorder = (fromIndex, toIndex) => {
    if (fromIndex == null || toIndex == null || fromIndex === toIndex) return;
    if (fromIndex < 0 || toIndex < 0 || fromIndex >= order.value.length || toIndex >= order.value.length) return;
    const next = [...order.value];
    const [moved] = next.splice(fromIndex, 1);
    if (moved == null) return;
    next.splice(toIndex, 0, moved);
    order.value = next;
    persist();
  };

  const resetOrder = () => {
    order.value = dataNames();
    persist();
  };

  // Sort a column array by the saved order; columns not in the order (e.g. "actions") sink last.
  const orderColumns = (cols) => {
    const rank = new Map(order.value.map((n, i) => [n, i]));
    return [...cols].sort((a, b) => {
      const ra = rank.has(a.name) ? rank.get(a.name) : Number.MAX_SAFE_INTEGER;
      const rb = rank.has(b.name) ? rank.get(b.name) : Number.MAX_SAFE_INTEGER;
      return ra - rb;
    });
  };

  return { order, reorder, resetOrder, orderColumns };
}
