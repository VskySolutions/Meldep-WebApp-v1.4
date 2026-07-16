<template>
  <div>
    <div class="q-mb-sm row items-center justify-between">
      <div class="text-h6 text-weight-bold">
        <b>{{ title }}</b>
      </div>

      <q-input
        :model-value="search"
        @update:model-value="emit('update:search', $event)"
        outlined
        debounce="300"
        dense
        clearable
        placeholder="Search"
        class="search-box"
      >
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </div>

    <q-table
      v-model:pagination="pagination"
      bordered
      class="no-shadow"
      virtual-scroll
      :loading="loading"
      :rows="filteredRows"
      :columns="columns"
      row-key="id"
      separator="cell"
      binary-state-sort
      :rows-per-page-options="[20, 50, 100, 200, 500]"
    >
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th
            v-for="col in props.cols"
            :key="col.name"
            :props="props"
          >
            {{ col.label }}
          </q-th>
        </q-tr>
      </template>

      <template #body="props">
        <q-tr>
          <q-td style="width: 10%;" v-if="showType">
            {{ props.row.type }}
          </q-td>
          <q-td style="width: 10%;" class="text-right">
            #{{ props.row.number }}
          </q-td>
          <q-td
            class="ellipsis-cell"
            style="overflow-wrap: break-word; white-space: normal;"
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

          <q-td style="width: 10%;" >
            {{ props.row.date }}
          </q-td>

        </q-tr>
      </template>
    </q-table>
  </div>
</template>

<script setup>
import { computed, ref } from "vue";

const props = defineProps({
  title: String,
  rows: {
    type: Array,
    default: () => []
  },
  search: {
    type: String,
    default: ""
  },
  loading: Boolean,
  showType: {
    type: Boolean,
    default: true
  }
});

const emit = defineEmits(["update:search"]);

const pagination = ref({
  sortBy: "date",
  descending: true,
  page: 1,
  rowsPerPage: 20
});

const expandedRows = ref(new Set());

const toggleExpand = (id) => {
  if (expandedRows.value.has(id)) {
    expandedRows.value.delete(id);
  } else {
    expandedRows.value.add(id);
  }
};

const isExpanded = (id) => expandedRows.value.has(id);

const truncateText = (text, length = 80) =>
  text?.length > length ? `${text.substring(0, length)}...` : text;

const filteredRows = computed(() => {
  const keyword = props.search.trim().toLowerCase();

  if (!keyword) {
    return props.rows;
  }

  return props.rows.filter((row) =>
    Object.values(row).some((value) =>
      String(value ?? "").toLowerCase().includes(keyword)
    )
  );
});

const columns = computed(() => {
  const cols = [];

  if (props.showType) {
    cols.push({
      name: "type",
      label: "Type",
      field: "type",
      align: "left"
    });
  }

  cols.push(
    {
      name: "number",
      label: "Number",
      field: "number",
      align: "right"
    },
    {
      name: "name",
      label: "Name",
      field: "name",
      align: "left"
    },
    {
      name: "date",
      label: "Date",
      field: "date",
      align: "left"
    }
  );

  return cols;
});
</script>
