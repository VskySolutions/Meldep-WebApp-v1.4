<template>
  <q-card flat bordered class="dashboard-card">
    <q-card-section class="row items-center justify-between q-pb-sm">
      <div class="row items-center">
        <q-avatar
          rounded
          color="red-1"
          text-color="negative"
          icon="o_bug_report"
          size="36px"
        />
        <div class="q-ml-md row items-center">
          <div class="text-subtitle1 text-weight-bold">
            Issues
          </div>
          <q-badge
            rounded
            color="grey-3"
            text-color="grey-8"
            class="q-ml-sm"
          >
            {{ rows.length }}
          </q-badge>
        </div>
      </div>
      <div class="row items-center q-gutter-sm">
        <div class="text-caption text-grey-7">
          {{ openIssues }} Open
        </div>
        <q-btn
          v-if="projectId"
          icon="o_open_in_new"
          size="sm"
          outline
          class="text-primary"
          style="padding: 3px 7px; min-height: 30px;"
          @click="$router.push({ path: '/issue',
            state: {
              projectId: projectId,
              projectModuleId: projectModuleId,
              requirementId: props.requirementId
            }
          })"
        >
          <q-tooltip>Open Issue List</q-tooltip>
        </q-btn>
      </div>
    </q-card-section>

    <q-separator />
    <q-table
      flat
      :rows="rows"
      :columns="columns"
      :loading="loading"
      v-model:pagination="pagination"
      row-key="issueNumber"
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
          <q-td style="width:10%;">
            #{{ props.row.issueNumber }}
          </q-td>

          <q-td style="width:40%;">
            <span
              class="hoverable-cell"
              @click="onIssueView(props.row.id)"
            >
              {{ props.row.name }}
            </span>
          </q-td>

          <q-td style="width:15%;">
            <q-chip
              dense
              class="fs-13"
              :style="{
                backgroundColor: props.row.priorityBgColor,
                color: props.row.priorityTextColor
              }"
            >
              {{ props.row.priorityName }}
            </q-chip>
          </q-td>

          <q-td style="width:15%;">
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
        </q-tr>
      </template>
    </q-table>
  </q-card>
</template>

<script setup>
import { ref, watch, computed } from 'vue'
import { notifyError } from 'assets/utils'
import requirementCenterService from 'src/modules/requirement-center/requirementCenter.service'

// Shared Issue Dialogs
import {
  initIssueDialogs,
  onIssueView
} from "src/modules/issue/utils/dialogs.js";

const emit = defineEmits(['summary'])

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
})

const loading = ref(false);
const rows = ref([]);
const activeRowId = ref(null);

const pagination = ref({
  sortBy: 'createdOnUtc',
  descending: true,
  rowsPerPage: 20,
  page: 1
})

const columns = [
  { name: 'issueNumber', label: 'NUMBER', field: 'issueNumber', align: 'left', sortable: true },
  { name: 'name', label: 'NAME', field: 'name', align: 'left', sortable: true },
  { name: 'priorityName', label: 'PRIORITY', field: 'priorityName', align: 'left', sortable: true },
  { name: 'statusName', label: 'STATUS', field: 'statusName', align: 'left', sortable: true }
]

const closedStatuses = [
  'Closed',
  'Done',
  'UAT Passed'
];

const projectId = ref('');
const projectModuleId = ref('');
const getIssuesByRequirementId = async (requirementId) => {
  if (!requirementId) return

  try {
    loading.value = true

    const resp = await requirementCenterService.getIssuesByRequirementId(requirementId)

    rows.value = resp.map(item => ({
      ...item,
      priorityName: item.priority?.dropDownValue ?? '-',
      priorityBgColor: item.priority?.bgColor ?? '#e0e0e0',
      priorityTextColor: item.priority?.color ?? '#000000',

      statusName: item.status?.dropDownValue ?? '-',
      statusBgColor: item.status?.bgColor ?? '#e0e0e0',
      statusTextColor: item.status?.color ?? '#000000'
    }))

    if (resp.length) {
      projectId.value = resp[0].project.id;
      projectModuleId.value = resp[0].projectModuleId;
    }

    emit('summary', {
      total: rows.value.length,
      open: rows.value.filter(r => !closedStatuses.includes(r.statusName)).length
    })

  } catch (err) {
    console.error(err)
    notifyError({ message: 'Failed to load Issues' })
  } finally {
    loading.value = false
  }
}

const openIssues = computed(() =>
  rows.value.filter(
    r => !closedStatuses.includes(r.statusName)
  ).length
);

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initIssueDialogs(activeRowId);

watch(
  () => props.requirementId,
  async id => {
    await getIssuesByRequirementId(id)
  },
  {
    immediate: true
  }
)
</script>
