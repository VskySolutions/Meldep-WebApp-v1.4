<template>
  <fieldset>
    <legend>Activity Details</legend>
    <div class="q-mb-sm q-gutter-sm flex justify-end">
      <q-input
        v-model="filterActivity"
        outlined
        class="bg-white q-mr-sm search-box"
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
      ref="tableRef"
      v-model:pagination="pagination"
      virtual-scroll
      class="border Custom-DataTable"
      :loading="loading"
      :rows="filteredActivity"
      :columns="columns"
      row-key="id"
      separator="cell"
      :filter="filterActivity"
      binary-state-sort
      no-data-label="No data available"
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      @request="getCandidateActivityLog"
    >
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
        </q-tr>
      </template>
      <template #body="props">
        <q-tr :props="props">
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">
            <div class="text-black" v-html="props.row.activityName" />
          </q-td>
          <q-td style="width: 5%;">
            {{ props.row.dueDate }}
          </q-td>
          <q-td style="width: 5%;">
            {{ props.row.priority.dropDownValue }}
          </q-td>
          <q-td style="width: 5%;">
            {{ props.row.employee.person.fullName }}
          </q-td>
        </q-tr>
      </template>
    </q-table>
  </fieldset>
</template>

<script setup>
import { ref, watch, computed } from "vue";
import candidateService from "modules/candidate/candidate.service";

// define props
const props = defineProps({ candidateId: { type: String, default: "" } });
const candidateId = props.candidateId;

// common variables
const loading = ref(true);
const filterActivity = ref("");

// define table variables
const rows = ref([]);
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "activityName", label: "Name", field: "activityName", align: "left", sortable: true },
  { name: "dueDate", label: "Due date", field: "dueDate", align: "left", sortable: true },
  { name: "priority.dropDownValue", label: "Priority", field: row => row.priority?.dropDownValue, align: "left", sortable: true },
  { name: "employee.person.fullName", label: "Owner", field: row => row.employee?.person?.fullName, align: "left", sortable: true }
]);

// getCandidateActivityLog
const getCandidateActivityLog = () => {
  loading.value = true;
  candidateService.getActivityLogById(candidateId).then((resp) => {
    rows.value = resp;
  }).finally(() => {
    loading.value = false;
  });
};

// for static filter
const filterRows = (data, searchTerm, columns) => {
  if (!searchTerm) return data;
  const lowerCaseTerm = searchTerm.toLowerCase();

  return data.filter(row =>
    columns.some(column => {
      const value = typeof column.field === "function" // This checks whether the field is a function or not. -field: row => row.priority?.dropDownValue
        ? column.field(row)
        : column.field.split(".").reduce((obj, key) => obj?.[key], row);
      return String(value || "").toLowerCase().includes(lowerCaseTerm);
    })
  );
};

const activityColumns = columns.value;
const filteredActivity = computed(() => filterRows(rows.value, filterActivity.value, activityColumns));

watch(() => candidateId, (newValue, oldValue) => {
  if (newValue) {
    getCandidateActivityLog();
  }
}, { immediate: true });

</script>
