<template>
  <q-table
    v-model:pagination="innerPagination"
    :selected="innerSelected"
    :rows="displayedRows"
    :columns="effectiveColumns"
    :visible-columns="effectiveVisibleColumns"
    :row-key="rowKey"
    :loading="loading"
    :selection="selectable ? 'multiple' : 'none'"
    :rows-per-page-options="rowsPerPageOptions"
    :sort-method="clientSort ? sortMethod : undefined"
    :table-row-class-fn="rowClassFn"
    :table-row-style-fn="rowStyleFn"
    binary-state-sort
    flat
    bordered
    :dense="dense"
    :class="['app-data-table', { 'with-selection': selectable, 'with-actions': hasActions, 'app-data-table--dense': dense }]"
    @request="onRequest"
    @update:selected="onSelectedChange"
  >
    <!-- Top bar: title, page actions, column menu, refresh. -->
    <template #top>
      <div class="app-data-table__bar">
        <div v-if="title" class="text-subtitle1 text-weight-medium text-primary app-data-table__title">{{ title }}</div>
        <q-space />
        <slot name="actions" />

        <q-btn flat round dense color="grey-8" icon="o_view_column" aria-label="Columns">
          <q-tooltip>Columns</q-tooltip>
          <q-menu>
            <q-list dense style="min-width: 240px;">
              <q-item-label header class="text-grey-7">Show columns — drag to reorder</q-item-label>
              <q-item
                v-for="(col, index) in orderedToggleableColumns"
                :key="col.name"
                draggable="true"
                :class="['app-col-item', { 'app-col-item--over': overIndex === index, 'app-col-item--drag': dragIndex === index }]"
                @dragstart="onColDragStart(index, $event)"
                @dragover.prevent="onColDragOver(index)"
                @drop.prevent="onColDrop(index)"
                @dragend="onColDragEnd"
              >
                <q-item-section side class="app-col-item__handle">
                  <q-icon name="o_drag_indicator" class="text-grey-6" />
                </q-item-section>
                <q-item-section side>
                  <q-checkbox v-model="visibleColumnNames" :val="col.name" dense @click.stop />
                </q-item-section>
                <q-item-section>{{ col.label }}</q-item-section>
              </q-item>
              <q-separator />
              <q-item clickable @click="resetColumns">
                <q-item-section class="text-primary">Reset columns</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </q-btn>

        <q-btn flat round dense color="grey-8" icon="o_refresh" :loading="loading" aria-label="Refresh" @click="$emit('refresh')">
          <q-tooltip>Refresh</q-tooltip>
        </q-btn>
      </div>
      <!-- A list's own counted shortcuts, on a row of their own under the bar. -->
      <div v-if="$slots['quick-filters']" class="full-width q-mt-sm">
        <slot name="quick-filters" />
      </div>
      <div v-if="selectable && selectionTotal" class="app-data-table__bulk full-width q-mt-sm">
        <q-chip dense color="primary" text-color="white">{{ selectionTotal }} selected</q-chip>
        <slot name="bulk-actions" :selected="innerSelected" />
      </div>
    </template>

    <template #no-data>
      <slot name="no-data">
        <div class="full-width column flex-center q-pa-lg text-grey-6">
          <q-icon name="o_inbox" size="32px" class="q-mb-sm" />
          No Data Available
        </div>
      </slot>
    </template>

    <!-- Resizable header cells (sorting still works via q-th :props). -->
    <template #header-cell="hp">
      <q-th :props="hp" :style="columnStyle(hp.col.name)" class="app-th">
        {{ hp.col.label }}
        <div
          v-if="hp.col.name !== 'actions'"
          class="app-th__resize"
          @mousedown.stop.prevent="startResize($event, hp.col.name)"
          @click.stop
        />
      </q-th>
    </template>

    <!-- Every other slot (body-cell-xxx, body, …) is the page's and goes straight to QTable. -->
    <template v-for="(_, name) in forwardedSlots" #[name]="slotProps" :key="name">
      <slot :name="name" v-bind="slotProps" />
    </template>
  </q-table>
</template>

<script setup>
// The platform's data table: server paging and sorting, persisted column visibility / order / widths /
// page size, a row-selection bar, pinned and coloured rows, and a custom empty state.
import { computed, ref, watch, useSlots, toRef } from "vue";
import { usePreferences } from "composables/usePreferences";
import useColumnResize from "composables/dataTable/useColumnResize.js";
import useColumnOrder from "composables/dataTable/useColumnOrder.js";

const props = defineProps({
  rows: { type: Array, default: () => [] },
  // Column contract: { name, label, field: string | (row) => value, align: "left", sortable, default,
  // width? (initial px), sort? ((row) => key for client sorting) }. Name the actions column "actions".
  columns: { type: Array, default: () => [] },
  rowKey: { type: String, default: "id" },
  loading: { type: Boolean, default: false },
  title: { type: String, default: "" },
  // Enables persistence of the reader's layout; unique across the app.
  pageKey: { type: String, default: "" },
  totalRecords: { type: Number, default: 0 },
  selectable: { type: Boolean, default: false },
  // Controlled selection (row objects); leave null to let the table own it.
  selected: { type: Array, default: null },
  // Overrides the "n selected" count when the selection spans pages the table cannot see.
  selectionCount: { type: Number, default: null },
  // The page owns page, size and sort; the table reports changes through `request`.
  pagination: { type: Object, default: null },
  defaultSortBy: { type: String, default: "updatedOnUtc" },
  defaultDescending: { type: Boolean, default: true },
  rowsPerPageOptions: { type: Array, default: () => [10, 20, 50, 100] },
  // Sort here instead of asking the server.
  clientSort: { type: Boolean, default: false },
  // Row keys floated to the top of the page and tinted (pinned records).
  pinnedRowKeys: { type: Array, default: () => [] },
  // Row tints as { [rowKey]: "#hex" } — a 4px stripe on the row's first cell.
  rowColours: { type: Object, default: () => ({}) },
  // Extra class(es) for a row, from the page ((row) => string).
  rowClass: { type: Function, default: null },
  // Compact rows (a density toggle on the page).
  dense: { type: Boolean, default: false }
});

const emit = defineEmits(["request", "refresh", "update:pagination", "update:selected"]);

const prefs = props.pageKey ? usePreferences(props.pageKey) : null;

// ---- Pagination ----
const innerPagination = ref({
  page: 1,
  rowsPerPage: prefs?.get("pageSize", 20) ?? 20,
  sortBy: props.defaultSortBy,
  descending: props.defaultDescending,
  rowsNumber: props.totalRecords,
  ...(props.pagination || {})
});

watch(() => props.totalRecords, (total) => {
  innerPagination.value = { ...innerPagination.value, rowsNumber: total };
});

// The page's sort follows the page: a list that resets to page 1 on a new sort, or restores a remembered
// one, has to be able to say so and see the header arrow move.
watch(() => props.pagination, (next) => {
  if (next) innerPagination.value = { ...innerPagination.value, ...next };
}, { deep: true });

const onRequest = ({ pagination: next }) => {
  innerPagination.value = { ...innerPagination.value, ...next };
  prefs?.merge({ pageSize: next.rowsPerPage });
  // Page, size and sort all go back to the server — a new sort is a new question about the whole set.
  emit("update:pagination", innerPagination.value);
  if (!props.clientSort) emit("request", innerPagination.value);
};

// ---- Selection ----
const innerSelected = ref(props.selected ? [...props.selected] : []);
watch(() => props.selected, (val) => {
  if (val) innerSelected.value = val;
});
const onSelectedChange = (val) => {
  innerSelected.value = val;
  emit("update:selected", val);
};
const selectionTotal = computed(() => props.selectionCount ?? innerSelected.value.length);

// ---- Client sorting ----
const sortValue = (col, name) => {
  const accessor = col?.sort ?? col?.field ?? name;
  return typeof accessor === "function" ? accessor : (row) => row[accessor];
};

const isIsoInstant = (v) => typeof v === "string" && /^\d{4}-\d{2}-\d{2}[T ]/.test(v);

const compare = (x, y) => {
  if (x == null && y == null) return 0;
  if (x == null) return 1;
  if (y == null) return -1;
  if (typeof x === "boolean" || typeof y === "boolean") return (x === y) ? 0 : (x ? 1 : -1);
  if (typeof x === "number" && typeof y === "number") return x - y;
  if (isIsoInstant(x) && isIsoInstant(y)) return Date.parse(x) - Date.parse(y);
  return String(x).localeCompare(String(y), undefined, { numeric: true });
};

const sortMethod = (rows, sortBy, descending) => {
  if (!sortBy) return rows;
  const get = sortValue(props.columns.find((c) => c.name === sortBy), sortBy);
  const sorted = [...rows].sort((a, b) => compare(get(a), get(b)));
  return descending ? sorted.reverse() : sorted;
};

// ---- Pinned / coloured / classed rows ----
const hasActions = computed(() => props.columns.some((c) => c.name === "actions"));
const pinnedSet = computed(() => new Set(props.pinnedRowKeys));

const displayedRows = computed(() => {
  if (!props.pinnedRowKeys.length) return props.rows;
  const isPinned = (row) => pinnedSet.value.has(row[props.rowKey]);
  const pinned = props.rows.filter(isPinned);
  return pinned.length ? [...pinned, ...props.rows.filter((row) => !isPinned(row))] : props.rows;
});

const rowClassFn = (row) => {
  const classes = [];
  if (pinnedSet.value.has(row?.[props.rowKey])) classes.push("app-data-table__row--pinned");
  const extra = props.rowClass?.(row);
  if (extra) classes.push(extra);
  return classes.join(" ");
};

const rowStyleFn = (row) => {
  const colour = props.rowColours?.[row?.[props.rowKey]];
  return colour ? `--app-row-colour: ${colour}` : "";
};

// ---- Column visibility (persisted) ----
const toggleableColumns = computed(() => props.columns.filter((c) => c.name !== "actions"));
const allColumnNames = computed(() => props.columns.map((c) => c.name));

// Only columns flagged `default: true` show initially (all, when none are flagged).
const defaultColumnNames = () => {
  const data = props.columns.filter((c) => c.name !== "actions");
  const flagged = data.filter((c) => c.default === true).map((c) => c.name);
  return flagged.length ? flagged : data.map((c) => c.name);
};

const visibleColumnNames = ref(prefs?.get("visibleColumns", null) ?? defaultColumnNames());

// "actions" is always shown; the menu only toggles data columns.
const effectiveVisibleColumns = computed(() => {
  const set = new Set(visibleColumnNames.value);
  if (allColumnNames.value.includes("actions")) set.add("actions");
  return [...set];
});

watch(visibleColumnNames, (val) => prefs?.set("visibleColumns", val), { deep: true });

// ---- Column order (drag to reorder in the menu, persisted) ----
const columnsRef = toRef(props, "columns");
const { reorder, resetOrder, orderColumns } = useColumnOrder({
  columns: columnsRef,
  initialOrder: prefs?.get("columnOrder", null),
  saveOrderState: (o) => prefs?.set("columnOrder", o)
});

const orderedToggleableColumns = computed(() => orderColumns(toggleableColumns.value));

const dragIndex = ref(null);
const overIndex = ref(null);
const onColDragStart = (i, e) => {
  dragIndex.value = i;
  if (e.dataTransfer) e.dataTransfer.effectAllowed = "move";
};
const onColDragOver = (i) => { overIndex.value = i; };
const onColDrop = (i) => {
  if (dragIndex.value !== null) reorder(dragIndex.value, i);
  dragIndex.value = null;
  overIndex.value = null;
};
const onColDragEnd = () => { dragIndex.value = null; overIndex.value = null; };

// ---- Column widths (drag the header edge, persisted) ----
// A column may name its own starting width; the rest start at 120px.
const defaultWidths = () => {
  const widths = {};
  for (const col of props.columns) {
    if (col.name !== "actions") widths[col.name] = col.width || 120;
  }
  return widths;
};
const savedWidths = prefs?.get("columnWidths", null);
const resizeWidths = ref(savedWidths && Object.keys(savedWidths).length ? savedWidths : defaultWidths());
const saveWidths = (widths) => prefs?.set("columnWidths", widths);
const { startResize } = useColumnResize({ columns: columnsRef, resizeWidths, saveResizableWidthState: saveWidths });

// One reset for the whole layout: which columns, in what order, how wide.
const resetColumns = () => {
  visibleColumnNames.value = defaultColumnNames();
  resetOrder();
  resizeWidths.value = defaultWidths();
  saveWidths(resizeWidths.value);
};

const columnStyle = (name) => {
  const w = name !== "actions" ? resizeWidths.value?.[name] : null;
  return w ? { width: `${w}px`, minWidth: `${w}px` } : {};
};

// Widths go on header AND body cells so the columns line up, then the user's order is applied (the
// "actions" column always sinks last and can never be hidden).
const effectiveColumns = computed(() => {
  const withWidths = props.columns.map((c) => {
    const w = c.name !== "actions" ? resizeWidths.value?.[c.name] : null;
    const style = w ? `width:${w}px;min-width:${w}px;` : undefined;
    return {
      ...c,
      ...(style ? { style, headerStyle: style } : {}),
      ...(c.name === "actions" ? { required: true } : {})
    };
  });
  return orderColumns(withWidths);
});

// Slots other than the ones defined here are forwarded straight to QTable.
const slots = useSlots();
const reserved = ["top", "no-data", "actions", "bulk-actions", "header-cell", "quick-filters"];
const forwardedSlots = computed(() =>
  Object.fromEntries(Object.entries(slots).filter(([name]) => !reserved.includes(name))));
</script>

<style scoped>
.app-data-table {
  border-radius: 12px;
  box-shadow: none;
}
.app-data-table :deep(.q-table__top) {
  padding: 8px 10px;
  flex-direction: column;
  align-items: stretch;
}
.app-data-table__bar {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 6px;
  width: 100%;
}
.app-data-table__bulk {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 4px 8px;
}
.app-data-table :deep(thead tr th) {
  position: sticky;
  top: 0;
  z-index: 1;
  background: #f7f9fb;
  color: var(--ink-700, #3a4d54);
  font-weight: 600;
  border-bottom: 1px solid var(--line, #dde6e8);
}
.app-data-table :deep(tbody td) {
  font-size: 13px;
  color: var(--ink-900, #16262c);
  vertical-align: middle;
}
.app-data-table :deep(tbody tr:hover td) {
  background: #f8fbfd;
}
/* Comfortable rows by default; the compact reading tightens the vertical rhythm only. */
.app-data-table :deep(tbody td) {
  padding-top: 7px;
  padding-bottom: 7px;
}
.app-data-table--dense :deep(tbody td) {
  padding-top: 2px;
  padding-bottom: 2px;
  font-size: 12.5px;
}
.app-th {
  position: relative;
}

/* ---- Pinned and coloured rows ----
   A pinned row is a shade warmer than its neighbours; a coloured one carries a stripe down its left
   edge. Neither may shout — the row's own badges are what the reader is scanning for. */
.app-data-table :deep(tbody tr.app-data-table__row--pinned td) {
  background: #fdfaf0;
}
.app-data-table :deep(tbody tr td:first-child) {
  position: relative;
}
.app-data-table :deep(tbody tr td:first-child)::before {
  content: "";
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  background: var(--app-row-colour, transparent);
}
.app-th__resize {
  position: absolute;
  top: 0;
  right: 0;
  width: 6px;
  height: 100%;
  cursor: col-resize;
  user-select: none;
}
.app-th__resize:hover {
  background: var(--q-primary);
  opacity: 0.4;
}

/* ---- The actions column is a row of controls, not prose ----
   It never wraps and sizes itself to what it holds; the table's middle section scrolls sideways if the
   whole table outgrows the window. Everything placed in an actions cell must be inline-level. */
.app-data-table.with-actions :deep(thead tr th:last-child),
.app-data-table.with-actions :deep(tbody tr td:last-child) {
  white-space: nowrap;
  width: 1%;
}
.app-data-table.with-actions :deep(tbody tr td:last-child > *) {
  vertical-align: middle;
}

/* Left-align the selection (checkbox) column header + body cells. */
.app-data-table.with-selection :deep(thead th:first-child),
.app-data-table.with-selection :deep(tbody td:first-child) {
  text-align: left;
  width: 1%;
  white-space: nowrap;
}

/* Draggable column-reorder rows in the columns menu. */
.app-col-item {
  cursor: grab;
}
.app-col-item__handle {
  min-width: 0;
  padding-right: 4px;
}
.app-col-item--drag {
  opacity: 0.5;
}
.app-col-item--over {
  border-top: 2px solid var(--q-primary);
}
</style>
