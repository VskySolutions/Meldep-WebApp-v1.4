<template>
  <div>
    <div class="q-mb-sm row items-center justify-between">
      <div class="text-h6 text-weight-bold">
        <b>{{ props.title }}</b>
      </div>
      <q-input
        :model-value="search"
        @update:model-value="$emit('update:search', $event)"
        outlined
        class="bg-white search-box"
        debounce="300"
        placeholder="Search"
        dense
        clearable
      >
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </div>
    <q-table
      v-model:pagination="paginationModel"
      bordered
      class="no-shadow"
      virtual-scroll
      :loading="loading"
      :rows="rows"
      :columns="columns"
      row-key="id"
      separator="cell"
      binary-state-sort
      :rows-per-page-options="[20,50,100,200,500]"
    >
      <template #header="props">
        <q-tr
          :props="props"
          class="bg-primary text-white"
        >
          <q-th
            v-for="col in props.cols"
            :key="col.name"
            :props="props"
          >
            {{ col.label }}
          </q-th>
          <q-th
            v-if="showRemoveIcon"
            auto-width
          >
            Action
          </q-th>
        </q-tr>
      </template>

      <template #body="props">
        <q-tr :class="{ 'bg-red-2': props.row.isDeleted }">
          <q-td class="text-center" style="width:10%">
            <q-checkbox
              v-model="props.row.checkboxStatus"
              :disable="isCheckboxDisabled(props.row)"
            />
          </q-td>
          <q-td
            v-if="showType"
            style="width:12%"
          >
            {{ props.row.type }}
          </q-td>
          <q-td class="text-right" style="width:8%">
            #{{ props.row.number }}
          </q-td>
          <q-td
            class="ellipsis-cell"
            style="width:60%;white-space:normal;overflow-wrap:break-word"
          >
            <div>
              {{
                isExpanded(props.row.id)
                  ? props.row.name
                  : truncateText(props.row.name)
              }}
              <span
                v-if="props.row.name?.length > 80"
                class="text-primary cursor-pointer q-ml-xs"
                @click="toggleExpand(props.row.id)"
              >
                {{ isExpanded(props.row.id) ? "less" : "...more" }}
              </span>
            </div>
          </q-td>
          <q-td style="width:10%">
            {{ props.row.date }}
          </q-td>
         <q-td
            v-if="showRemoveIcon"
            class="text-center"
            style="width:6%"
          >
            <q-btn
              v-if="props.row.isMapped"
              flat
              round
              dense
              color="negative"
              icon="o_delete"
              :disable="props.row.isDeleted"
              @click="confirmDelete(props.row)"
            />
          </q-td>
        </q-tr>
      </template>
    </q-table>
    <div class="q-mt-sm">
      <slot name="footer"/>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";

// SOP Change :- Shared Project release tracking Actions
import {
  initReleaseTrackingActions,
  onSubmitRetestingItemDelete
} from "src/modules/project-release-tracking/utils/actions.js";

const props = defineProps({
  title: String,
  search: String,
  rows: Array,
  columns: Array,
  loading: Boolean,
  pagination: Object,
  checkboxDisabled: Boolean,
  showType: Boolean,
  showRemoveIcon: {
    type: Boolean,
    default: false
  }
});

const activeRowId = ref(null);

const emit = defineEmits([
  "update:search",
  "remove",
  "mark-deleted"
]);

const confirmDelete = (row) => {
  onSubmitRetestingItemDelete(
    row.id,
    row.name,
    () => emit("mark-deleted", row)
  );
};

const isCheckboxDisabled = (row) => {
  if (props.showRemoveIcon) {
    return props.checkboxDisabled || row.isMapped || row.isDeleted;
  }

  return props.checkboxDisabled;
};

const expandedRows = ref(new Set());

const paginationModel = computed({
  get: () => props.pagination,
  set: () => {}
});

const toggleExpand = (id) => {
  expandedRows.value.has(id)
    ? expandedRows.value.delete(id)
    : expandedRows.value.add(id);
};

const isExpanded = id => expandedRows.value.has(id);

const truncateText = (text, len = 80) =>
  text?.length > len ? text.slice(0, len) + "..." : text;

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initReleaseTrackingActions(activeRowId);
</script>
