<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw !important;max-width: 60vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader justify-between">
        <div class="text-h2 text-white">Test Case Status Release History</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-table
            v-model:pagination="pagination"
            :loading="loading"
            :rows="rows"
            :columns="testCaseHistoryColumns"
            row-key="mappingId"
            separator="cell"
            :rows-per-page-options="[20, 50, 100, 200, 500]"
            binary-state-sort
            class="Custom-DataTable"
            no-data-label="No task activity available"
            :filter="filter"
          >
            <template #header="props">
              <q-tr :props="props" class="bg-primary text-white">
                <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
              </q-tr>
            </template>
            <template #body="props">
              <q-tr :props="props" :class="activeRowId == props.row.mappingId ? 'highlight' : ''">
                <q-td>{{ props.row.releaseVersion }}</q-td>
                <q-td
                  class="common-q-td hoverable-cell"
                >
                  <div class="row no-wrap items-center justify-between">
                    {{ props.row.currentStatus ?? "-" }}
                    <div class="row items-center q-gutter-sm q-ml-sm" style="flex-shrink: 0;">
                      <q-icon
                        name="o_history"
                        size="xs"
                        class="cursor-pointer"
                        @click="onTestCaseStatusChangeLog(props.row.releaseVersion, props.row.testCaseId, props.row.mappingId, props.row.testCaseName, 'Test Case Status')"
                      >
                        <q-tooltip>Change Log</q-tooltip>
                      </q-icon>
                    </div>
                  </div>
                </q-td>
                <q-td>{{ props.row.testedBy ?? "-" }}</q-td>
                <q-td>{{ props.row.testedDate ?? "-" }}</q-td>
                <q-td>{{ props.row.comment ?? "-" }}</q-td>
                <q-td>
                  <div class="row no-wrap items-center justify-between">
                    <span style="flex: 1; word-break: break-word; white-space: normal;">
                      <span
                      v-if="props.row.issueNumber"
                        class="hoverable-cell"
                        @click="onIssueView(props.row.issueId)"
                      >
                        #{{ props.row.issueNumber }}
                        <q-tooltip>View Issue</q-tooltip>
                      </span>
                      <span v-else>-</span>
                    </span>
                  </div>
                </q-td>
                <q-td>
                  <q-badge
                    v-if="props.row.isRemoved"
                    color="negative"
                    label="Removed"
                  />
                  <span v-else>-</span>
                </q-td>
              </q-tr>
            </template>
          </q-table>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, watch } from "vue";
import _ from "lodash";

import testcaseService from "modules/test-case/testCase.service";

// Shared Test Case Dialogs
import {
  initTestCaseDialogs,
  onTestCaseStatusChangeLog
} from "src/modules/test-case/utils/dialogs.js";


// Shared Issue Dialogs
import {
  initIssueDialogs,
  onIssueView
} from "src/modules/issue/utils/dialogs.js";

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const testCaseId = props.id;

// Common variables
const loading = ref(true);
const rows = ref([]);
const activeRowId = ref(null);

const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const testCaseHistoryColumns = [
  { name: "releaseVersion", label: "Release Version", field: "releaseVersion", align: "left" },
  { name: "currentStatus", label: "Status", field: "currentStatus", align: "left" },
  { name: "testedBy", label: "Tested By", field: "testedBy", align: "left" },
  { name: "testedDate", label: "Tested Date", field: "testedDate", align: "left" },
  { name: "comment", label: "Comment", field: "comment", align: "left" },
  { name: "issueNumber", label: "Issue No.", field: "issueNumber", align: "left" },
  { name: "removed", label: "Removed", field: "isRemoved", align: "left" }
];

const getReleaseWiseTestCaseHistory = (testCaseId) => {
  loading.value = true;
  testcaseService.getReleaseWiseTestCaseHistory(testCaseId)
  .then(resp => {
      rows.value = resp;
  });
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------

initTestCaseDialogs(activeRowId);
initIssueDialogs(activeRowId);

watch(() => testCaseId, (newValue, oldValue) => {
  if (newValue) {
    getReleaseWiseTestCaseHistory(testCaseId);
  }
}, { immediate: true });
</script>
