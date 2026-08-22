<template>
  <fieldset>
    <legend>Questions Answers</legend>
    <div class="q-mb-sm q-gutter-sm flex justify-end">
      <q-input
        v-model="filterQandA" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search"
        dense clearable
      >
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </div>
    <q-table
      ref="tableRef1"
      v-model:pagination="paginationQA"
      :class="QandARows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
      flat
      bordered
      :loading="loading"
      :rows="filteredQandA"
      :columns="columnsQandA"
      row-key="id"
      separator="cell"
      :filter="filterQandA"
      binary-state-sort
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      @request="getAllProjectQuestionAndAnswersForDashboard"
    >
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
        </q-tr>
      </template>
      <template #body="props">
        <q-tr
          :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''"
          :set="(preProjectName = null, preRequirementTitle = null)"
        >
          <q-td class="hoverable-cell" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;">
            <span
            v-if="preRequirementTitle !== props.row.requirement.title"
            :set="preRequirementTitle = props.row.requirement.title"
            @click="onRequirementView(props.row.requirementId)"
            >
            {{ props.row.requirement.title }}
            </span>
          </q-td>
          <q-td style="width: 25%; white-space: normal;">
            <div class="row no-wrap items-center justify-between">
              <span style="flex: 1; word-break: break-word; white-space: normal;">
                <span>{{ props.row.title }}</span>
              </span>
            </div>
          </q-td>
          <q-td style="width: 20%;">
            <span>
              <p v-html="props.row.description" />
            </span>            
          </q-td>
        </q-tr>
        <q-separator />
      </template>
    </q-table>
  </fieldset>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useQuasar } from "quasar";
import useFilters from "composables/useFilters";
import projectQuestionsAnswersService from "modules/project-questions-answers/projectQuestionsAnswers.service";

import {
  initRequirementDialogs,
  onRequirementView
} from "src/modules/requirement/utils/dialogs.js";

const props = defineProps({ projectId: { type: String, default: "" } });
const projectId = props.projectId;
const { toDate } = useFilters();
const $q = useQuasar();

const loading = ref(true);
const tableRef1 = ref();
const QandARows = ref([]);
const activeRowId = ref(null);
const filterQandA = ref("");
const paginationQA = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columnsQandA = ref([
  { name: "requirement.title", label: "Requirement", field: "requirement.title", align: "left", sortable: true },
  { name: "title", label: "Question", field: "title", align: "left", sortable: true },
  { name: "description", label: "Answer", field: "description", align: "left", sortable: true }
]);

const getAllProjectQuestionAndAnswersForDashboard = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    projectId
  };
  projectQuestionsAnswersService.getAllProjectQuestionAndAnswersForDashboard(payload).then((resp) => {
    QandARows.value = resp.projectQuestionsAnswerList;
    paginationQA.value.page = page;
    paginationQA.value.rowsPerPage = rowsPerPage;
    paginationQA.value.sortBy = sortBy;
    paginationQA.value.descending = descending;
    paginationQA.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initRequirementDialogs(activeRowId);

// for static filter
const filterRows = (data, searchTerm, columns) => {
  if (!searchTerm) return data; // If no filter, return all data
  const lowerCaseTerm = searchTerm.toLowerCase();
  return data.filter(row =>
    columns.some(column => {
      const value = column.field.split(".").reduce((obj, key) => obj?.[key], row); // Handle nested fields
      return String(value || "").toLowerCase().includes(lowerCaseTerm);
    })
  );
};

const qAndAColumns = columnsQandA.value;
const filteredQandA = computed(() => filterRows(QandARows.value, filterQandA.value, qAndAColumns));

// On page rendering
onMounted(() => {
  const props = { pagination: paginationQA.value };
  getAllProjectQuestionAndAnswersForDashboard(props);
});

</script>
