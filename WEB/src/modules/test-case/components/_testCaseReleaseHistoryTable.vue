<template>
  <div>
    <!-- <div class="q-gutter-y-md"> -->
    <div class="q-mb-sm row items-center justify-between">
      <div class="text-h6 text-weight-bold">
        <b>{{ title }}</b>
      </div>
      <q-input
        :model-value="props.search"
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
      v-model:pagination="pagination"
      :loading="props.loading"
      :rows="filteredRows"
      :columns="tableColumns"
      row-key="mappingId"
      separator="cell"
      binary-state-sort
      class="Custom-DataTable"
      no-data-label="No history available"
      :rows-per-page-options="[20,50,100,200,500]"
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
          :class="activeRowId == props.row.mappingId ? 'highlight' : ''"
        >
          <!-- Release -->
          <q-td v-if="showReleaseVersion">
            {{ props.row.releaseVersion }}
          </q-td>
          <template v-else>
            <q-td>
              #{{ props.row.testCaseNumber }}
            </q-td>
            <q-td>
              {{ props.row.testCaseName }}
            </q-td>
          </template>
          <q-td
            v-if="props.row.isEditable && statusEditable"
            class="common-q-td"
            :class="{ 'hoverable-cell' : props.row.isEditable }"
            @click="activeEdit = { rowId: props.row.testCaseId, field: 'status' }"
          >
            <quickEditSingleSelect
              field="status"
              :row-id="props.row.testCaseId"
              :value="props.row.statusId"
              :display-value="props.row.currentStatus"
              :editable="true"
              :show-history="true"
              :options="testCaseStatusDropdownSingleSelect.list.value"
              :active-edit="activeEdit"
              :loading="updatingRow.status === props.row.testCaseId"
              @cancel="activeEdit = { rowId: null, field: null }"
              @submit="({ rowId, value }) =>
                onSubmitTestCaseStatus(
                  rowId,
                  value,
                  props.row.mappingId,
                  refreshHistory
                )"
              @history="() => onTestCaseStatusChangeLog(props.row.releaseVersion, props.row.testCaseId, props.row.mappingId, props.row.testCaseName, 'Test Case Status')"
            />
          </q-td>
          <q-td
            v-else
            class="common-q-td"
          >
            <div>
              <div class="row items-center justify-between no-wrap">
                <span class="ellipsis">
                  {{ props.row.currentStatus ?? "-" }}
                </span>
                <q-icon
                  name="o_history"
                  size="xs"
                  class="cursor-pointer"
                  @click="
                    onTestCaseStatusChangeLog(
                      props.row.releaseVersion,
                      props.row.testCaseId,
                      props.row.mappingId,
                      props.row.testCaseName,
                      'Test Case Status'
                    )
                  "
                >
                  <q-tooltip>Change Log</q-tooltip>
                </q-icon>
              </div>
            </div>
          </q-td>
          <q-td>
            {{ props.row.testedBy ?? "-" }}
          </q-td>
          <q-td>
            {{ props.row.testedDate ?? "-" }}
          </q-td>
          <q-td>
            {{ props.row.comment ?? "-" }}
          </q-td>
          <!-- Issue -->
          <q-td>
            <div class="row no-wrap items-center justify-between">
              <span
                style="
                  flex:1;
                  word-break:break-word;
                  white-space:normal;
                "
              >
                <span
                  v-if="props.row.issueNumber"
                  class="hoverable-cell"
                  @click="onIssueView(props.row.issueId)"
                >
                  #{{ props.row.issueNumber }}
                  <q-tooltip>
                    View Issue
                  </q-tooltip>
                </span>
                <span v-else>
                  -
                </span>
              </span>
            </div>
          </q-td>
          <q-td>
            <q-badge
              v-if="props.row.isRemoved"
              color="negative"
              label="Removed"
            />
            <span v-else>
              -
            </span>
          </q-td>
        </q-tr>
      </template>
    </q-table>
  </div>
  <!-- </div> -->
</template>

<script setup>
import { ref, computed, onMounted } from "vue";

import {
  initTestCaseDialogs,
  onTestCaseStatusChangeLog
} from "modules/test-case/utils/dialogs";

import {
  initIssueDialogs,
  onIssueView
} from "modules/issue/utils/dialogs";


// Shared Test Case Actions
import {
  onSubmitTestCaseStatus,
  updatingRow
} from "src/modules/test-case/utils/actions.js";

// SOP Change :- Shared Dropdowns
import testCaseModule from "src/modules/test-case/utils/dropdowns.js";

// SOP Change :- Shared DataTable Views
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";

const props = defineProps({
  rows: {
    type: Array,
    default: () => []
  },
  search: {
    type: String,
    default: ""
  },
  loading: {
    type: Boolean,
    default: false
  },
  showReleaseVersion: {
    type: Boolean,
    default: true
  },
  statusEditable: {
    type: Boolean,
    default: true
  }
});

const activeRowId = ref(null);
const activeEdit = ref({ rowId: null, field: null });
const emit = defineEmits(["refresh", "update:search"]);

const refreshHistory = () => {
  emit("refresh");
};

const filteredRows = computed(() => {
  const keyword = props.search?.trim().toLowerCase();

  if (!keyword) {
    return props.rows;
  }

  return props.rows.filter(row => {
    const values = props.showReleaseVersion
      ? [
          row.releaseVersion,
          row.currentStatus,
          row.testedBy,
          row.testedDate,
          row.comment,
          row.issueNumber
        ]
      : [
          row.testCaseNumber,
          row.testCaseName,
          row.currentStatus,
          row.testedBy,
          row.testedDate,
          row.comment,
          row.issueNumber
        ];

    return values.some(value =>
      String(value ?? "").toLowerCase().includes(keyword)
    );
  });
});

initTestCaseDialogs(activeRowId);
initIssueDialogs(activeRowId);

const pagination = ref({
  sortBy: "testedDate",
  descending: true,
  page: 1,
  rowsPerPage: 20
});

const tableColumns = computed(() => {
  const firstColumn = props.showReleaseVersion
    ? {
        name: "releaseVersion",
        label: "Release Version",
        field: "releaseVersion",
        align: "left"
      }
    : [
        {
          name: "testCaseNumber",
          label: "Test Case No.",
          field: "testCaseNumber",
          align: "left"
        },
        {
          name: "testCaseName",
          label: "Test Case",
          field: "testCaseName",
          align: "left"
        }
      ];

  return [
    ...(Array.isArray(firstColumn) ? firstColumn : [firstColumn]),
    {
      name: "currentStatus",
      label: "Status",
      field: "currentStatus",
      align: "left"
    },
    {
      name: "testedBy",
      label: "Tested By",
      field: "testedBy",
      align: "left"
    },
    {
      name: "testedDate",
      label: "Tested Date",
      field: "testedDate",
      align: "left",
      sortable: true,
      sort: (a, b) => new Date(a) - new Date(b)
    },
    {
      name: "comment",
      label: "Comment",
      field: "comment",
      align: "left"
    },
    {
      name: "issueNumber",
      label: "Issue No.",
      field: "issueNumber",
      align: "left"
    },
    {
      name: "removed",
      label: "Removed",
      field: "isRemoved",
      align: "left"
    }
  ];
});

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  testCaseStatusDropdownSingleSelect
} = testCaseModule();

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  testCaseStatusDropdownSingleSelect.load("Test Case Status");
});
</script>
