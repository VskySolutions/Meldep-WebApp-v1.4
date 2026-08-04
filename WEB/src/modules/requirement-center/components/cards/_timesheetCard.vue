<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <!-- Header -->
    <q-card-section class="row items-center justify-between q-pb-sm">
      <div class="row items-center">
        <q-avatar
          rounded
          color="green-1"
          text-color="positive"
          icon="o_schedule"
          size="36px"
        />

        <div class="q-ml-md row items-center">
          <div class="text-subtitle1 text-weight-bold">
            Timesheet
          </div>
          <q-badge
            color="grey-3"
            text-color="grey-8"
            rounded
            class="q-ml-sm"
          >
            {{ rows.length }}
          </q-badge>
        </div>
      </div>
      <div class="row items-center q-gutter-sm">
        <div class="text-caption text-grey-7">
          {{ totalHours }} hrs logged
        </div>

        <q-btn
          v-if="projectId"
          icon="o_open_in_new"
          size="sm"
          outline
          class="text-primary"
          style="padding: 3px 7px; min-height: 30px;"
          @click="$router.push({ path: '/timesheet',
            state: {
              projectId: projectId
            }
          })"
        >
          <q-tooltip>Open Timesheet List</q-tooltip>
        </q-btn>
      </div>
    </q-card-section>

    <q-separator />

    <!-- Table -->
    <q-table
      flat
      :rows="rows"
      :columns="columns"
      :loading="loading"
      v-model:pagination="pagination"
      row-key="id"
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      class="req-dashboard-table"
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
        <q-tr
          :props="props"
          :set="(prevDate = null, prevEmployee = null)"
        >
          <!-- Date -->
          <q-td>
            <span
              v-if="prevDate !== props.row.timesheetDate"
              :set="(prevDate = props.row.timesheetDate, prevEmployee = null)"
            >
              {{ props.row.timesheetDate }}
            </span>
          </q-td>

          <!-- Employee -->
          <q-td>
            <span
              v-if="prevEmployee !== props.row.employeeName"
              :set="prevEmployee = props.row.employeeName"
            >
              {{ props.row.employeeName }}
            </span>
          </q-td>

          <!-- Task -->
          <q-td>
            {{ props.row.taskName }}
          </q-td>

          <!-- Hours -->
          <q-td align="right">
            {{ props.row.hours }}
          </q-td>
        </q-tr>
      </template>
      <template #bottom-row>
        <q-tr class="bg-grey-2 text-weight-bold">
          <q-td colspan="3" class="text-right">Total Hours:</q-td>
          <q-td class="text-right">{{ totalHours }}</q-td>
        </q-tr>
      </template>
    </q-table>
  </q-card>
</template>

<script setup>
import { ref, watch, computed } from 'vue';
import { notifyError } from "assets/utils";

import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

const emit = defineEmits(['summary']);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

const loading = ref(false);
const rows = ref([]);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = [
  { name: 'timesheetDate', label: 'DATE', field: 'timesheetDate', align: 'left', sortable: true },
  { name: 'employeeName', label: 'EMPLOYEE', field: 'employeeName', align: 'left', sortable: true },
  { name: 'taskName', label: 'TASK', field: 'taskName', align: 'left', sortable: true },
  { name: 'hours', label: 'HOURS', field: 'hours', align: 'right', sortable: true }
]

const projectId = ref('');
const getTimesheetByRequirementId = async (requirementId) => {
  if (!requirementId) return;

  try {
    loading.value = true;

    const resp = await requirementCenterService.getTimesheetByRequirementId(requirementId);
    rows.value = resp.map(item => ({
      ...item,
      timesheetDate: item.timesheet?.timesheetDate ?? '-',
      employeeName: item.timesheet?.user?.person?.fullName ?? '-',
      taskName: item.task?.name ?? '-'
    }));

    if (resp.length) {
      projectId.value = resp[0].project.id;
    }

    emit('summary', {
      total: rows.value.length,
      totalHours: rows.value.reduce((sum, row) => sum + row.hours, 0)
    });
  } catch (err) {
    console.error(err);
    notifyError({ message: "Failed to load Test Cases" });
  } finally {
    loading.value = false;
  }
};

const totalHours = computed(() =>
  rows.value.reduce((sum, row) => sum + row.hours, 0)
);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
    async (id) => {
      await getTimesheetByRequirementId(id);
    },
  {
    immediate: true
  }
);
</script>

