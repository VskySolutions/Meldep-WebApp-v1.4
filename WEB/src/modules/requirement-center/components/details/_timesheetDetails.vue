<template>
  <q-card flat bordered>
    <q-card-section class="row bg-primary text-white q-pa-sm items-center justify-between">
      <div class="row items-center">
        <q-icon
          :name="groupIcon"
          size="sm"
          class="q-mr-md"
        />

        <div>
          <div class="text-caption text-blue-2">
            {{ groupTitle }}
          </div>

          <div class="text-h6 text-weight-bold">
            {{ props.group?.name }}
          </div>
        </div>
      </div>

      <q-chip color="white" text-color="primary">
        {{ totalHours }} hrs
      </q-chip>
    </q-card-section>
    <q-separator />

    <q-table
      v-model:pagination="pagination"
      :loading="loading"
      :rows="rows"
      :columns="columns"
      row-key="id"
      separator="cell"
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      binary-state-sort
      class="Custom-DataTable"
      no-data-label="No task activity available"
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
        <q-tr :props="props">
          <!-- Project -->
          <q-td style="width:15%;">
            {{ props.row.project?.name || "-" }}
          </q-td>

          <!-- Task -->
          <q-td style="width:20%;">
            <span class="text-primary text-weight-medium">
              {{ props.row.task?.name || "-" }}
            </span>
          </q-td>

          <!-- Activity -->
          <q-td style="width:15%;">
            {{ props.row.projectActivity?.name || "-" }}
          </q-td>

          <q-td style="width:35%;">
            <div
              class="description-content"
              v-html="props.row.description || '-'"
            />
          </q-td>

          <!-- Employee -->
          <q-td style="width:10%;">
            {{ props.row.timesheet?.user?.person?.fullName || "-" }}
          </q-td>

          <!-- Hours -->
          <q-td
            style="width:5%;"
            class="text-right"
          >
            {{ props.row.hours }}
          </q-td>
        </q-tr>

        <!-- Total Row -->
        <q-tr v-if="props.pageIndex === rows.length - 1">
          <q-td colspan="5" class="text-right">
            <b>Total Hours:</b>
          </q-td>

          <q-td class="text-right">
            <b>{{ totalHours }}</b>
          </q-td>
        </q-tr>
      </template>
    </q-table>
  </q-card>
</template>

<script setup>
import { ref, watch, computed } from "vue";
import { notifyError } from "assets/utils";
import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  },
  group: {
    type: Object,
    required: true
  }
});

const loading = ref(false);
const rows = ref([]);

const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = [
  { name: "project", label: "Project", field: row => row.project?.name ?? "-", align: "left" },
  { name: "task", label: "Task", field: row => row.task?.name ?? "-", align: "left" },
  { name: "activity", label: "Activity", field: row => row.projectActivity?.name ?? "-", align: "left" },
  { name: "description", label: "Activity Details", field: "description", align: "left" },
  { name: "employee", label: "Employee", field: row => row.timesheet?.user?.person?.fullName ?? "-", align: "left" },
  { name: "hours", label: "Hours", field: "hours", align: "right" }
];

const totalHours = computed(() =>
  Array.isArray(rows.value)
    ? rows.value.reduce((sum, row) => sum + (row.hours || 0), 0)
    : 0
);

const getTimesheetDetails = async () => {
  if (!props.requirementId || !props.group) {
    rows.value = [];
    return;
  }

  try {
    loading.value = true;

    const response = await requirementCenterService.getTimesheetDetails(
  props.requirementId,
  props.group.groupBy,
  props.group.id
);

rows.value = Array.isArray(response) ? response : [];
  } catch (err) {
    notifyError({ message: "Failed to load Timesheet Details" });
  } finally {
    loading.value = false;
  }
};

const groupIcon = computed(() => {
  switch (props.group?.groupBy) {
    case "employee":
      return "o_person";
    case "task":
      return "o_task";
    default:
      return "o_calendar_month";
  }
});

const groupTitle = computed(() => {
  switch (props.group?.groupBy) {
    case "employee":
      return "EMPLOYEE";
    case "task":
      return "TASK";
    default:
      return "DATE";
  }
});

watch(
  [() => props.requirementId, () => props.group],
  getTimesheetDetails,
  {
    immediate: true,
    deep: true
  }
);
</script>
<style>
.description-content {
  white-space: normal;
  overflow-wrap: break-word;
  word-break: break-word;
}

.description-content * {
  max-width: 100%;
}
</style>
