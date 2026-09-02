<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <!-- Header -->
    <q-card-section class="row items-center justify-end q-pb-sm">
      <!-- <q-avatar
        rounded
        color="blue-1"
        text-color="primary"
        icon="o_task_alt"
        size="36px"
      /> -->

      <div class="search-container position-relative">
        <searchFilterBar
          v-model="search.searchText"
          :loading="searchLoader"
          :applied-filters="appliedFilters"
          @toggle-filter="showFilter = !showFilter"
        />
        <!-- Dropdown Content -->
        <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
          <q-card class="q-pa-sm">
            <multiSelectDropdown
              v-model="search.projectIds"
              label="Project Name"
              :options="projectNameDropdown.list.value"
              :filter="projectNameDropdown.filter"
              class="hidden"
            />
            <multiSelectDropdown
              v-model="search.requirementIds"
              label="Requirement"
              :disable="!search.projectIds"
              :options="requirementsByProjectModuleIdForDropdown.list.value"
              :filter="requirementsByProjectModuleIdForDropdown.filter"
              class="hidden"
            />
            <div class="row items-center q-mb-sm">
              <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                <label class="Cutomlabel q-mt-sm fs-13">Question</label>
              </div>
              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                <q-input v-model="search.title" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
              </div>
            </div>
            <!-- Search and Clear Buttons -->
            <div class="row justify-end q-gutter-sm q-mb-sm">
              <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onSearch(); }" />
              <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClear" />
              <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
            </div>
          </q-card>
        </q-menu>
      </div>

      <q-btn
        v-if="projectId"
        icon="o_open_in_new"
        size="sm"
        outline
        class="text-primary q-ml-sm"
        style="padding: 3px 7px; min-height: 30px;"
        @click="$router.push({ path: '/project-questions-answers/list',
        state: {
          projectId: projectId,
          requirementId: props.requirementId
        }})"
      >
        <q-tooltip>Open Project Questions Answers</q-tooltip>
      </q-btn>
    </q-card-section>
    <q-separator />

    <!-- Table -->
    <q-table
      flat
      :rows="filteredQA"
      :columns="columns"
      :loading="loading"
      v-model:pagination="pagination"
      class="req-dashboard-table"
      row-key="id"
      separator="cell"
      no-data-label="No data available"
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      @request="getProjectQAByRequirementId"
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
        <!-- Original description row -->
        <q-tr
          v-if="props.row.description"
          :props="props"
        >
          <q-td
            style="width:40%;"
            class="cursor-pointer hoverable-cell"
            @click="onQuestionAnswersView(props.row.id)"
          >
            {{ props.row.title }}
          </q-td>

          <q-td style="width:60%;">
            <div v-html="props.row.description"></div>
          </q-td>
        </q-tr>
        <!-- Response answer rows -->
        <q-tr
          v-for="(answer) in props.row.projectQuestionsAnswersResponseLog"
          :key="answer.id"
          :props="props"
        >
          <!-- Don't repeat question -->
          <q-td
            style="width:40%;"
          >
          </q-td>

          <q-td style="width:60%;">
            <div v-html="answer.description"></div>
          </q-td>
        </q-tr>
        <!-- No answer -->
        <q-tr
          v-if="
            !props.row.description &&
            !props.row.projectQuestionsAnswersResponseLog?.length
          "
          :props="props"
        >
          <q-td
            style="width:40%;"
            class="cursor-pointer hoverable-cell"
            @click="onQuestionAnswersView(props.row.id)"
          >
            {{ props.row.title }}
          </q-td>

          <q-td style="width:60%;" class="text-grey">
            No answer available
          </q-td>
        </q-tr>
      </template>

    </q-table>
  </q-card>
</template>
<script setup>
import { notifyError } from "assets/utils";
import { computed, ref, watch } from "vue";
import { useAuthStore } from "stores/auth";

import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";

// SOP Change :- Shared Project Dialogs
import {
  initQuestionsAnswersDialogs,
  onQuestionAnswersView
} from "src/modules/project-questions-answers/utils/dialogs.js";


const emit = defineEmits(['summary']);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

// common variables
const authStore = useAuthStore();
const loading = ref(false);
const searchLoader = ref(false);
const showFilter = ref(false);
const siteId = computed(() => authStore.user?.siteId);

const rows = ref([]);
const columns = [
  { name: "title", label: "QUESTION", field: "title", align: "left", sortable: true },
  { name: "description", label: "ANSWER", field: "description", align: "left", sortable: true }
]

const projectId = ref('');
const getProjectQAByRequirementId = async ({ pagination: p }) => {
  loading.value = true;
  const { page, rowsPerPage, sortBy, descending } = p;

  const payload = {
    searchText: search.value.searchText,
    requirementId: props.requirementId,
    title: search.value.title,
    sortBy: sortBy,
    descending: descending,
    page: page,
    pageSize: rowsPerPage
  };

  try {
    loading.value = true;
    const resp = await requirementCenterService.getProjectQAByRequirementId(payload);
    rows.value = resp.projectQuestionsAnswerList || [];

    if (rows.value.length > 0) {
      projectId.value = rows.value[0].project?.id || '';
    } else {
      projectId.value = '';
    }
    Object.assign(pagination.value, {
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    });
    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: activeRowId.value,
      sorts
    });

    emit('summary', {
      total: rows.value.length
    });
  } catch (err) {
    console.error(err);
    notifyError({ message: "Failed to load Project questions answers" });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

const refreshProjectQAList = () => {
  getProjectQAByRequirementId({ pagination: pagination.value });
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  saveDataTableState
} = useSiteTableState({
  storageKey: "requirement-Center-Question-Answers-Tabular-List",
  siteId,
  defaultSearch: {
    searchText: "",
    title: "",
    requirementIds: []
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

const filteredQA = computed(() => {
  if (!search.value) return rows.value;

  return rows.value.filter(answer =>
    JSON.stringify(answer)
      .toLowerCase()
      .includes((search.value.quickSearch ?? '').toLowerCase())
  );
});

// Clear search
const onClear = () => {
  search.value.title = "";
  search.value.requirementIds = [];

  saveDataTableState({
    search: search.value
  });
  onSearch();
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initQuestionsAnswersDialogs(activeRowId);

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectNameDropdown
} = projectModule();

const { requirementsByProjectModuleIdForDropdown } = requirementModule();

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Applied Filter Labels.
// ----------------------------------------------------------------------------------------------------------------

const appliedFilters = computed(() => ({
  ...(search.value.title ? { "Question": search.value.title } : {})
}));

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

// Search records as per parameters
const onSearch = () => { refreshProjectQAList(); };

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
  async () => {
    await getProjectQAByRequirementId({
      pagination: pagination.value
    });
  },
  { immediate: true }
);

watch(
  () => search.value.searchText,
  () => {
    searchLoader.value = true;
    refreshProjectQAList();
  }
);

</script>
