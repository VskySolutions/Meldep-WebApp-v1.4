<template>
  <q-card flat bordered>
    <q-separator />

    <q-card-section class="row q-col-gutter-sm">
      <div class="col" style="width: 60%;">
        <q-input
          v-model="search"
          dense
          outlined
          placeholder="Search timesheet..."
          clearable
        >
          <template #prepend>
            <q-icon name="o_search" />
          </template>
        </q-input>
      </div>

      <div class="col-auto" style="width: 40%;">
        <formSingleSelectDropdown
          v-model="groupBy"
          placeholder="Group By"
          :isClearable="false"
          :options="groupOptions"
        />
      </div>
    </q-card-section>

    <q-separator />

    <q-table
      v-model:pagination="pagination"
      flat
      :loading="loading"
      :rows="filteredRows"
      :columns="columns"
      row-key="timesheetDate"
      separator="cell"
      binary-state-sort
      no-data-label="No data available"
      :rows-per-page-options="[20, 50, 100, 200]"
      style="height: calc(100vh - 220px)"
    >
      <template #loading>
        <q-inner-loading showing color="primary">
          <q-spinner size="40px" />
        </q-inner-loading>
      </template>

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
        <q-tr
          :props="props"
          class="cursor-pointer"
          :class="{ 'bg-blue-1': selectedTimesheet === props.row.id }"
          @click="selectedTimesheet = props.row.id; selectGroup(props.row);"
        >
          <q-td class="q-pa-none">
            <div
              class="column q-gutter-xs"
              style="white-space: normal; word-break: break-word;"
            >
              <div class="row items-center justify-between">
                <div class="row items-center">
                  <q-icon
                    :name="groupIcon"
                    size="18px"
                    class="q-mr-sm"
                  />

                  <div class="text-caption text-weight-bold text-black">
                    {{ props.row.name }}
                  </div>
                </div>

                <q-badge
                  rounded
                  color="primary"
                >
                  {{ props.row.count }}
                  <q-tooltip>Entries</q-tooltip>
                </q-badge>
              </div>

              <div class="text-caption text-grey-7">
                <strong>Hours:</strong> {{ props.row.hours }}
              </div>
            </div>
          </q-td>
        </q-tr>

        <q-separator />
      </template>
      <template #bottom-row>
        <q-tr class="bg-grey-2 text-weight-bold">
          <q-td class="text-left">Total Hrs: {{ totalHours }}</q-td>
        </q-tr>
        <q-separator />
      </template>

    </q-table>
  </q-card>
</template>

<script setup>
import { computed, ref, watch } from "vue";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

const emit = defineEmits(["select"]);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

const loading = ref(false);
const search = ref("");
const selectedTimesheet = ref(null);
const rows = ref([]);

const pagination = ref({
  page: 1,
  rowsPerPage: 20,
  sortBy: "updatedOnUtc",
  descending: true
});

const columns = [
  {
    name: "timesheet",
    label: "Timesheets",
    field: "date",
    align: "left"
  }
];
const groupBy = ref("date");

const groupOptions = [
  { text: "Group by Date", value: "date" },
  { text: "Group by Employee", value: "employee" },
  { text: "Group by Task", value: "task" }
];

const groupIcon = computed(() => {
  switch (groupBy.value) {
    case "employee":
      return "o_person";
    case "task":
      return "o_task";
    default:
      return "o_calendar_month";
  }
});

const selectGroup = row => {
  const selected = {
    ...row,
    groupBy: groupBy.value
  };

  console.log("Emit:", selected);

  emit("select", selected);
};

const getGroupedTimesheetsByRequirementId = async () => {
  if (!props.requirementId) return;

  try {
    loading.value = true;

    rows.value = await requirementCenterService.getGroupedTimesheetsByRequirementId(
      props.requirementId,
      groupBy.value
    );

    if (rows.value.length) {
      selectedTimesheet.value = rows.value[0].id;

      // automatically select first row
      selectGroup(rows.value[0]);
    }
  } finally {
    loading.value = false;
  }
};

const totalHours = computed(() =>
  rows.value.reduce((sum, row) => sum + row.hours, 0)
)

const filteredRows = computed(() => {
  if (!search.value) return rows.value;

  return rows.value.filter(x =>
    x.name.toLowerCase().includes(search.value.toLowerCase())
  );
});

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------
watch(
  () => [props.requirementId, groupBy.value],
  getGroupedTimesheetsByRequirementId,
  { immediate: true }
);

</script>
