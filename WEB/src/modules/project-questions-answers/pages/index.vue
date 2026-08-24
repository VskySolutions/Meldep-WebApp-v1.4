<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Project Management" />
              <q-breadcrumbs-el label="Project Questions Answers" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-4">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center q-mr-xs">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    class="search-bar"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Question</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.title" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.projectIds"
                        label="Project Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.requirementIds"
                        label="Requirement"
                        :options="requirementsByProjectModuleIdForDropdown.list.value"
                        :filter="requirementsByProjectModuleIdForDropdown.filter"
                      />
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
              <!-- <manageDropdownOptions
                v-model="showManageDropdownOptions"
                :manage-drop-down-types="manageDropDownTypes"
                :selected-field="selectedField"
              /> -->
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Add Question Answers" no-caps class="text-primary btnRounded q-mr-xs" @click="onQuestionAnswersAdd(search.projectIds?.[0], search.requirementIds?.[0], refreshQuestionsAnswersList)" />
                <!-- <q-btn
                  v-if="role === 'admin'"
                  icon="o_playlist_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="showManageDropdownOptions = !showManageDropdownOptions"
                >
                  <q-tooltip>Manage Dropdowns</q-tooltip>
                </q-btn> -->
                 <!-- Reset Column Width -->
                <q-btn
                  icon="o_refresh"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="resetColumnsWidth()"
                >
                  <q-tooltip>Reset Columns Width</q-tooltip>
                </q-btn>
                <!-- Column Hide/Show -->
                <columnVisibilityMenu
                  :all-column-names="allColumnNames"
                  :selected-column-names="selectedColumnNames"
                  @update:selected-column-names="selectedColumnNames = $event"
                  @select-all-columns="selectAllColumns"
                  @default-columns="defaultColumns"
                />
                <!-- Button to Open Sorting Dialog -->
                <q-btn
                  color="primary"
                  icon="o_sort"
                  class="btnRounded q-ml-sm"
                  @click="showSortDialog = true"
                >
                  <q-badge v-if="selectedSortCount > 0" color="green" floating class="q-ml-xs">
                    {{ selectedSortCount }}
                  </q-badge>
                  <q-tooltip>Sort</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="table-project-questions-answers">
        <div class="table-scroll-container">
          <q-table
            ref="tableRef"
            v-model:pagination="pagination"
            :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
            :loading="loading"
            :rows="rows"
            :columns="computedColumns"
            row-key="id"
            separator="cell"
            no-data-label="No data available"
            binary-state-sort
            :rows-per-page-options="[20, 50, 100, 200, 500]"
            @request="getAllQuestionAnswers"
          >
            <template #loading>
              <q-inner-loading showing color="primary">
                <q-spinner-ios size="40px" class="q-mt-xl" />
              </q-inner-loading>
            </template>
            <template #header="props">
              <q-tr :props="props" class="bg-primary text-white">
                <!-- <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th> -->
                <q-th
                  v-for="col in props.cols"
                  :key="col.name"
                  :props="props"
                  :style="{
                    width: (resizeWidths?.[col.name] || 120) + 'px',
                    minWidth: '80px',
                    position: 'relative'
                  }"
                  @click="!isResizing && col.sortable"
                >
                  {{ col.label }}
                  <div class="resize-handle" @mousedown="(e) => startResize(e, col.name)" />
                </q-th>
                <q-th auto-width class="text-center">Actions</q-th>
              </q-tr>
            </template>
            <template #body="props">
              <q-tr
                :props="props"
                :class="[
                  highlightedId == props.row.id ? 'highlight' : ''
                ]"
                :set="(preProjectName = null, resetTracking())"
              >
                <q-td v-if="selectedColumnNames.includes('project.name')" style="white-space: normal;" class="hoverable-cell">
                  <div class="row no-wrap items-center justify-between">
                    <span style="flex: 1; word-break: break-word; white-space: normal;">
                      <span
                        v-if="preProjectName !== props.row.project.name"
                        :set="preProjectName = props.row.project.name"
                        @click="onProjectView(props.row.project.id)"
                      >{{ props.row.project.name }}
                      </span>
                    </span>
                    <div
                      v-if="shouldShowIcons(props.row.project.name, index)"
                      class="row items-center q-gutter-sm q-ml-sm"
                      style="flex-shrink: 0;"
                    >
                      <q-icon
                        name="o_radio_button_checked" size="xs"
                        class="cursor-pointer"
                        @click="setActiveRowIdInLocalStorage(props.row.id);
                                $router.push({ path: '/project-center', state: { projectId: props.row.project.id } })"
                      >
                        <q-tooltip>Project Center</q-tooltip>
                      </q-icon>
                    </div>
                  </div>
                </q-td>
                <!-- <q-td v-if="selectedColumnNames.includes('requirement.title')"
                  class="common-q-td hoverable-cell"
                  style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;"
                  @click="onRequirementView(props.row.requirement?.id)"
                >
                  <span v-if="props.row.requirement?.title">
                    {{ props.row.requirement?.title }}
                  </span>
                </q-td> -->
                <q-td
                  v-if="selectedColumnNames.includes('requirement.title')"
                  class="common-q-td hoverable-cell"
                >
                  <div class="row no-wrap items-center justify-between">
                    <span>
                      <span
                        class="cursor-pointer"
                        @click="onRequirementView(props.row.requirement?.id)"
                      >
                        {{ props.row.requirement?.title }}
                      </span>
                    </span>
                    <div
                      class="row items-center q-gutter-sm q-ml-sm"
                      style="flex-shrink: 0;"
                    >
                      <q-icon
                        name="o_radio_button_checked"
                        size="xs" class="cursor-pointer"
                        @click="setActiveRowIdInLocalStorage(props.row.id);$router.push({ path: '/requirement-center', state: { requirementId: props.row.requirement?.id } })"
                      >
                        <q-tooltip>Requirement Center</q-tooltip>
                      </q-icon>
                    </div>
                  </div>
                </q-td>
                <q-td v-if="selectedColumnNames.includes('title')" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;" class="cursor-pointer" @click="onQuestionAnswersView(props.row.id)">
                  {{ props.row.title }}
                </q-td>
                <q-td v-if="selectedColumnNames.includes('lastAnswer')" class="common-q-td hoverable-cell" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                  <span
                    @click="onAnswerTimelineView(
                    props.row.id,
                    `${props.row.project.name} : ${props.row.title}`
                    )"
                  >
                  {{ truncateText(props.row.lastAnswer) }}
                  <q-tooltip>
                    View Answers
                  </q-tooltip>
                </span>
                </q-td>
                <q-td
                  v-if="selectedColumnNames.includes('createdBy.person.firstName')"
                  class="common-q-td"
                >
                  {{ props.row.createdBy.person.fullName }}
                </q-td>
                <q-td
                  v-if="selectedColumnNames.includes('createdOnUtc')"
                  class="common-q-td"
                >
                  {{ props.row.createdOnUtc }}
                </q-td>
                <q-td
                  v-if="selectedColumnNames.includes('updatedBy.person.firstName')"
                  class="common-q-td"
                >
                  {{ props.row.updatedBy.person.fullName }}
                </q-td>
                <q-td
                  v-if="selectedColumnNames.includes('updatedOnUtc')"
                  class="common-q-td"
                >
                  {{ props.row.updatedOnUtc }}
                </q-td>
                <q-td class="text-center actions">
                  <q-icon name="o_visibility" class="cursor-pointer q-mr-xs" size="xs" @click="onQuestionAnswersView(props.row.id)">
                    <q-tooltip>View</q-tooltip>
                  </q-icon>
                  <q-icon name="o_edit" class="cursor-pointer q-mr-xs" size="xs" @click="onQuestionAnswersEdit(props.row.id, refreshQuestionsAnswersList)">
                    <q-tooltip>Edit</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_history"
                    class="cursor-pointer q-mr-xs"
                    size="xs"
                    @click="onQuestionAnswersEdit(
                      props.row.id,
                      refreshQuestionsAnswersList,
                      true
                    )"
                  >
                    <q-tooltip>Response Log</q-tooltip>
                  </q-icon>
                  <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onSubmitQuestionsAnswersDelete(props.row.id, props.row.title, refreshQuestionsAnswersList)">
                    <q-tooltip>Delete</q-tooltip>
                  </q-icon>
                </q-td>
              </q-tr>
              <q-separator />
            </template>
          </q-table>
        </div>
      </div>
    </q-card>
  </q-page>
  <!-- Multi-Column Level Sorting -->
  <multiColumnSortingDialog
    v-model="showSortDialog"
    :columns="columns"
    :multi-sort="multiSort"
    @add="addSortLevel"
    @remove="removeSortLevel"
    @apply="applyMultiSort"
  />
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed, onBeforeUnmount } from "vue";
import { useAuthStore } from "stores/auth";

import projectQuestionsAnswersService from "modules/project-questions-answers/projectQuestionsAnswers.service";

// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import multiColumnSortingDialog from "src/components/dataTable/_multiColumnSortingDialog.vue";
import columnVisibilityMenu from "src/components/dataTable/_columnVisibilityMenu.vue";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
// import manageDropdownOptions from "src/components/dataTable/_manageDropdownOptions.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";
// import manageDropdownModule from "src/modules/dropdown/utils/dropdowns.js";

// SOP Change :- Shared Scripts DataTable Features
import { useColumnManager } from "composables/dataTable/useColumnManager.js";
import useColumnResize from "composables/dataTable/useColumnResize.js";
import useMultiSort from "composables/dataTable/useMultiSort.js";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Project Dialogs
import {
  initQuestionsAnswersDialogs,
  onQuestionAnswersView,
  onAnswerTimelineView,
  onQuestionAnswersAdd,
  onQuestionAnswersEdit
} from "src/modules/project-questions-answers/utils/dialogs.js";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView
} from "src/modules/project/utils/dialogs.js";

import {
  initRequirementDialogs,
  onRequirementView
} from "src/modules/requirement/utils/dialogs.js";

// SOP Change :- Shared Project Actions
import {
  initQuestionsAnswersActions,
  onSubmitQuestionsAnswersDelete
} from "src/modules/project-questions-answers/utils/actions.js";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const authStore = useAuthStore();
// const user = authStore.user;
// const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
// const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";
// const showManageDropdownOptions = ref(false);
// const manageDropDownTypes = ref([]);
const showSortDialog = ref(false);

const siteId = computed(() => authStore.user?.siteId);
const highlightedId = computed(() => activeRowId.value);
const selectedProjectId = ref(history.state?.projectId);
const selectedProjectModuleId = ref(history.state?.projectModuleId);

// Table variables
const tableRef = ref();
const rows = ref([]);
const shownProjects = new Set();
const columns = ref([
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true, default: true },
  { name: "requirement.title", label: "Requirement", field: "requirement.title", align: "left", sortable: true, default: true },
  { name: "title", label: "Question", field: "title", align: "left", sortable: true, default: true },
  { name: "lastAnswer", label: "Answer", field: "lastAnswer", align: "left", sortable: true, default: true },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true, default: false },
  { name: "updatedBy.person.firstName", label: "Updated By", field: "updatedBy.person.firstName", align: "left", sortable: true, default: true },
  { name: "updatedOnUtc", label: "Updated Date", field: "updatedOnUtc", align: "left", sortable: true, default: true }
]);

const defaultSearch = {
  searchText: "",
  title: "",
  projectIds: [],
  requirementIds: []
};

const defaultPagination = {
  sortBy: "createdOnUtc",
  descending: true,
  rowsPerPage: 20,
  page: 1
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  resizeWidths,
  selectedColumnNames,
  getTableState,
  saveDataTableState,
  saveResizableWidthState,
  saveColumnsState
} = useSiteTableState({
  storageKey: "project-Questions-Answers-Index",
  siteId,

  defaultSearch,
  defaultPagination,
  defaultSorts: {},
  defaultResizableWidth: {},

  defaultColumns: columns.value
    .filter(col => col.default === true)
    .map(col => col.name)
});

const handleDocumentClick = (event) => {
  const highlightElement = document.querySelector(".highlight");
  // Check if clicked inside the highlighted row or icons
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;
    saveDataTableState({
      activeRowId: null
    });
  }
};

const tableState = getTableState();

if (selectedProjectId.value) {
  tableState.search.projectIds = [selectedProjectId.value];
  tableState.search.projectModuleIds = [selectedProjectModuleId.value];

  // Optional: persist the new state
  saveDataTableState({
    search: tableState.search
  });
}

// Get/Map project list to table
const getAllQuestionAnswers = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;
  loading.value = true;

  const sorts = {};
  const multi = multiSort.value;
  for (let i = 0; i < multi.length; i++) {
    const s = multi[i];
    if (s.column && s.direction) {
      sorts[s.column] = s.direction;
    }
  }

  const payload = { page, pageSize: rowsPerPage, sortBy, descending, sorts, ...search.value };
  saveDataTableState({
    search: search.value,
    pagination: p,
    activeRowId: activeRowId.value,
    sorts
  });
  projectQuestionsAnswersService.getAllQuestionAnswers(payload).then((resp) => {
    rows.value = resp.projectQuestionsAnswerList;
    Object.assign(pagination.value, {
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    });
    saveDataTableState({
      search: { ...search.value },
      pagination: { ...pagination.value },
      activeRowId: activeRowId.value
    });
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

function setActiveRowIdInLocalStorage(id) {
  activeRowId.value = id;

  saveDataTableState({
    activeRowId: id
  });
}
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
// truncate text after 60 characters
const truncateText = (htmlText, limit = 60) => {
  const plainText = htmlText?.replace(/<[^>]*>/g, '')?.replace(/&nbsp;/g, ' ') || ''

  return plainText.length > limit
    ? plainText.substring(0, limit) + '...'
    : plainText
}

const refreshQuestionsAnswersList = () => {
  getAllQuestionAnswers({ pagination: pagination.value });
};

// Search records as per parameters
const onSearch = () => {
  refreshQuestionsAnswersList();
};

// Clear search
const onClear = () => {
  search.value.title = "";
  search.value.projectIds = [];
  search.value.requirementIds = [];
  saveDataTableState({
    search: {
      ...defaultSearch
    }
  });
  onSearch();
};

function resetTracking () {
  shownProjects.clear(); // Clear the set before rendering rows
}

function shouldShowIcons (projectName) {
  if (shownProjects.has(projectName)) {
    return false;
  } else {
    shownProjects.add(projectName);
    return true;
  }
}

const lsSorts = sorts.value || null;
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Column resize functionality (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  startResize,
  resetColumnsWidth,
  isResizing
} = useColumnResize({
  columns,
  resizeWidths,
  saveResizableWidthState
});
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Hide/Show Columns (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  selectAllColumns,
  defaultColumns,
  allColumnNames,
  computedColumns
} = useColumnManager({
  columns,
  selectedColumnNames,
  saveColumnsState,
  isResizing
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Sort Filter (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  multiSort,
  addSortLevel,
  removeSortLevel,
  applyMultiSort,
  selectedSortCount
} = useMultiSort({
  lsSorts,
  saveDataTableState,
  onApplySort: () => {
    refreshQuestionsAnswersList();
  }
});

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initQuestionsAnswersDialogs(activeRowId);
initProjectDialogs(activeRowId);
initRequirementDialogs(activeRowId);
initQuestionsAnswersActions(activeRowId);

// ----------------------------
// Applied Filter Labels.
// ----------------------------
const mapFilterToLabel = (ids, list, label) => {
  if (!Array.isArray(ids) || !ids.length) return {};

  const text = ids
    .map(id => {
      const match = list.value.find(item => item.value === id);
      return match ? match.text : id;
    })
    .join(", ");

  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...(search.value.title ? { "Question": search.value.title } : {}),
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name"),
  ...mapFilterToLabel(search.value.requirementIds, requirementsByProjectModuleIdForDropdown.list, "Requirement")
}));

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Requirement": return search.value.requirementIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Question") {
    search.value.title = "";
  } else if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Requirement") {
    search.value.requirementIds = [];
  }
  refreshQuestionsAnswersList();
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectNameDropdown
} = projectModule();

// const { getDropdownTypesByModuleNameForDropdown } = manageDropdownModule();
const { requirementsByProjectModuleIdForDropdown } = requirementModule();

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  if (search.value.searchText) searchLoader.value = true;
  refreshQuestionsAnswersList();
});

watch(() => search.value.projectIds, async (newValue, oldValue) => {
  if (search.value?.projectIds?.length === 0) search.value.requirementIds = [];
  if (search.value?.projectIds?.length === 0 || newValue === oldValue) return;

  requirementsByProjectModuleIdForDropdown.load('', newValue);
}, { immediate: true });

watch(activeRowId, (val) => {
  const formattedSorts = {};

  for (const s of multiSort.value) {
    if (s.column && s.direction) {
      formattedSorts[s.column] = s.direction;
    }
  }

  saveDataTableState({
    search: search.value,
    pagination: pagination.value,
    activeRowId: val,
    sorts: formattedSorts
  });
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// On page rendering
onMounted(async () => {
  tableRef.value.requestServerInteraction();
  projectNameDropdown.load();
  if (search.value.projectIds.length > 0) requirementsByProjectModuleIdForDropdown.load('', search.value.projectIds);

  // Admin:- Manage all Release-Tracking Dropdowns and Types
  // manageDropDownTypes.value = await getDropdownTypesByModuleNameForDropdown("Release-Tracking");

  document.addEventListener("click", handleDocumentClick);
});

</script>
<style scoped>
.table-project-questions-answers .Custom-DataTable {
  min-width: max-content;
}
</style>
