<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <!-- Header -->
    <q-card-section class="row items-center justify-between q-pb-sm">
      <div class="row items-center">
        <q-avatar
          rounded
          color="blue-1"
          text-color="primary"
          icon="o_task_alt"
          size="36px"
        />

        <div class="q-ml-md">
          <div class="row items-center">
            <div class="text-subtitle1 text-weight-bold">
              Task
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
      </div>

      <div class="row items-center q-gutter-sm">
        <div class="text-caption text-grey-7">
          {{ doneCount }} done • {{ inProgressCount }} in progress
        </div>

        <q-btn
          v-if="projectId"
          icon="o_open_in_new"
          size="sm"
          outline
          class="text-primary"
          style="padding: 3px 7px; min-height: 30px;"
          @click="$router.push({ path: '/project-tasks',
          state: {
            projectId: projectId,
            projectModuleId: projectModuleId,
            requirementId: props.requirementId
          }})"
        >
          <q-tooltip>Open Task List</q-tooltip>
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
        <q-tr :props="props">
          <q-td style="width:15%;">
            #{{ props.row.projectTaskNumber }}
          </q-td>

          <q-td style="width:45%;">
            <span
              class="hoverable-cell"
              @click="onProjectTaskView(props.row.id)"
            >
              {{ props.row.name }}
            </span>
          </q-td>

          <q-td style="width:20%;">
            {{ props.row.owner }}
          </q-td>

          <q-td style="width:20%;">
            <q-chip
              dense
              class="fs-13"
              :style="{
                backgroundColor: props.row.statusBgColor,
                color: props.row.statusTextColor
              }"
            >
              {{ props.row.statusName }}
            </q-chip>
          </q-td>

          <q-td style="width:20%;">
            {{ props.row.dueDate }}
          </q-td>
        </q-tr>
      </template>
    </q-table>
  </q-card>
</template>
<script setup>
import { ref, watch, computed } from 'vue';
import { notifyError } from "assets/utils";

import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

// Shared Project Task Dialogs
import {
  initProjectTaskDialogs,
  onProjectTaskView
} from "src/modules/project-tasks/utils/dialogs.js";

const emit = defineEmits(['summary']);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

const loading = ref(false);
const activeRowId = ref(null);
const rows = ref([]);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = [
  { name: 'projectTaskNumber', label: 'TASK NO.', field: 'projectTaskNumber', align: 'left', sortable: true },
  { name: 'name', label: 'NAME', field: 'name', align: 'left', sortable: true },
  { name: 'owner', label: 'OWNER', field: 'owner', align: 'left', sortable: true },
  { name: 'statusName', label: 'STATUS', field: 'statusName', align: 'left', sortable: true },
  { name: 'dueDate', label: 'DUE DATE', field: 'dueDate', align: 'left', sortable: true }
]

const projectId = ref('');
const projectModuleId = ref('');
const getTasksByRequirementId = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;
  const payload = {
    requirementId: props.requirementId,
    sortBy: sortBy,
    // sorts: sorts.value,
    descending: descending,
    page: page,
    pageSize: rowsPerPage
  };

  try {
    loading.value = true;
    const resp = await requirementCenterService.getTasksByRequirementId(payload);
    rows.value = resp.map(item => ({
      ...item,
      owner: item.assignedTo?.person?.fullName ?? '-',
      statusName: item.status?.dropDownValue ?? '-',
      dueDate: item.endDate ?? '-',
      statusTextColor: item.status?.color ?? '-',
      statusBgColor: item.status?.bgColor ?? '-'
    }));
    if (resp.length) {
      projectId.value = resp[0].project.id;
      projectModuleId.value = resp[0].projectModuleId;
    }

    emit('summary', {
      total: rows.value.length,
      open: rows.value.filter(r => r.statusName === 'Open').length
    });
  } catch (err) {
    console.error(err);
    notifyError({ message: "Failed to load Test Cases" });
  } finally {
    loading.value = false;
  }
};

const doneCount = computed(() =>
  rows.value.filter(r => r.statusName === 'Close').length
)

const inProgressCount = computed(() =>
  rows.value.filter(r => r.statusName === 'Open').length
)

// ----------------------------------------------------------------------------------------------------------------
// DataTable :- Initialization Of Dialogs, Actions
// ----------------------------------------------------------------------------------------------------------------

initProjectTaskDialogs(activeRowId);
// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
  async () => {
    await getTasksByRequirementId({
      pagination: pagination.value
    });
  },
  { immediate: true }
);
</script>
