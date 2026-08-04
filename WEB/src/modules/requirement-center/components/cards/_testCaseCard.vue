<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <q-card-section class="row items-center justify-between q-pb-sm">
      <div class="row items-center">
        <q-avatar
          rounded
          color="teal-1"
          text-color="teal"
          icon="o_fact_check"
          size="36px"
        />
        <div class="q-ml-md row items-center">
          <div class="text-subtitle1 text-weight-bold">
            Test Cases
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
          {{ passedCount }}/{{ rows.length }} Passed
        </div>

        <q-btn
          v-if="projectId"
          icon="o_open_in_new"
          size="sm"
          outline
          class="text-primary"
          style="padding: 3px 7px; min-height: 30px;"
          @click="$router.push({ path: '/test-case',
            state: {
              projectId: projectId,
              projectModuleId: projectModuleId,
              requirementId: props.requirementId
            }
          })"
        >
          <q-tooltip>Open Test Case List</q-tooltip>
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
          <!-- Number -->
          <q-td style="width:15%;">
            #{{ props.row.testCaseNumber }}
          </q-td>

          <!-- Name -->
          <q-td style="width:45%;">
            <span
              class="hoverable-cell"
              @click="onTestCaseView(props.row.id, props.row.planId)"
            >
              {{ props.row.name }}
            </span>
          </q-td>

          <!-- Owner -->
          <q-td style="width:20%;">
            {{ props.row.owner }}
          </q-td>

          <!-- Status -->
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
        </q-tr>
      </template>
    </q-table>
  </q-card>
</template>

<script setup>
import { ref, watch, computed } from 'vue';
import { notifyError } from "assets/utils";

import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

// SOP Change :- Shared Project Dialogs
import {
  initTestCaseDialogs,
  onTestCaseView
} from "src/modules/test-case/utils/dialogs.js";

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
  { name: 'testCaseNumber', label: 'NUMBER', field: 'testCaseNumber', align: 'left', sortable: true },
  { name: 'name', label: 'NAME', field: 'name', align: 'left', sortable: true },
  { name: 'owner', label: 'TESTED BY', field: 'owner', align: 'left', sortable: true },
  { name: 'statusName', label: 'STATUS', field: 'statusName', align: 'left', sortable: true }
]

const projectId = ref('');
const projectModuleId = ref('');
const getTestCasesByRequirementId = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;
  const payload = {
    requirementId: props.requirementId,
    sortBy: sortBy,
    descending: descending,
    page: page,
    pageSize: rowsPerPage
  };

  try {
    loading.value = true;

    const resp = await requirementCenterService.getTestCasesByRequirementId(payload);
    rows.value = resp.map(item => ({
      ...item,
      owner: item.testedByEmployee?.person?.fullName ?? '-',
      statusName: item.status?.dropDownValue ?? '-',
      statusTextColor: item.status?.color ?? '-',
      statusBgColor: item.status?.bgColor ?? '-'
    }));

    if (resp.length) {
      projectId.value = resp[0].project.id;
      projectModuleId.value = resp[0].projectModuleId;
    }

    emit('summary', {
      total: rows.value.length,
      passed: rows.value.filter(r => r.statusName === 'Resolved').length
    });
  } catch (err) {
    console.error(err);
    notifyError({ message: "Failed to load Test Cases" });
  } finally {
    loading.value = false;
  }
};

const passedCount = computed(() =>
  rows.value.filter(r => r.status?.dropDownValue === 'Resolved').length
)

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initTestCaseDialogs(activeRowId);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
    async () => {
      await getTestCasesByRequirementId({
        pagination: pagination.value
      });
    },
  {
    immediate: true
  }
);
</script>
