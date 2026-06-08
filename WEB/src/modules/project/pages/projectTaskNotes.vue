<!-- eslint-disable vue/no-v-html -->
<template>
  <q-page padding>
    <q-inner-loading :showing="loading" color="primary" class="z-max" />
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-xxl-4 col-xl-4 col-lg-4 col-md-3 col-sm-6 col-xs-12">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Project Management" />
              <q-breadcrumbs-el icon="o_assignment" label="Project & Task Notes" />
            </q-breadcrumbs>
          </div>
          <div v-if="selectedProjectId" class="col-xxl-8 col-xl-8 col-lg-8 col-md-9 col-sm-6 col-xs-12">
            <div class="row items-center justify-end no-wrap">
              <div class="q-gutter-sm">
                <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary q-ml-md" @click="$router.back()">
                  <q-tooltip anchor="bottom middle" self="top middle">Back To List</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
        <div class="row q-col-gutter-x-md q-mt-md">
          <div class="col-12 col-md-4 col-lg-3">
            <div class="row items-center justify-end no-wrap q-mb-sm">
              <div class="search-container position-relative">
                <searchFilterBar
                  v-model="search.searchText"
                  :loading="searchProjectLoader"
                  :applied-filters="appliedFilters"
                  @toggle-filter="showFilter = !showFilter"
                />
                <!-- Dropdown Content -->
                <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                  <q-card class="q-pa-sm">
                    <multiSelectDropdown
                      v-model="search.customerIds"
                      label="Customer Name"
                      :options="customerNameDropdown.list.value"
                      :filter="customerNameDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.companyContactIds"
                      label="Company Contact"
                      :options="companyContactNameDropdown.list.value"
                      :filter="companyContactNameDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.projectIds"
                      label="Name"
                      :options="projectNameDropdown.list.value"
                      :filter="projectNameDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.projectCoordinatorIds"
                      label="Coordinator"
                      :options="activeEmployeesDropdown.list.value"
                      :filter="activeEmployeesDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.projectLeadsIds"
                      label="Leads"
                      :options="activeEmployeesDropdown.list.value"
                      :filter="activeEmployeesDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.projectStatusIds"
                      label="Status"
                      :options="projectStatusDropdown.list.value"
                      :filter="projectStatusDropdown.filter"
                      :isShowAll="true"
                    />
                    <singleSelectDropdown
                      v-model="search.statusId"
                      label="Active/Inactive"
                      :options="projectActiveInActiveDropdown.list.value"
                    />
                    <multiSelectDropdown
                      v-model="search.projectPriorityIds"
                      label="Priority"
                      :options="projectPrioritiesDropdown.list.value"
                      :filter="projectPrioritiesDropdown.filter"
                      :isShowAll="true"
                    />
                    <multiSelectDropdown
                      v-model="search.projectTypeIds"
                      label="Type"
                      :options="projectTypesDropdown.list.value"
                      :filter="projectTypesDropdown.filter"
                      :isShowAll="true"
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
            <q-table
              ref="tableRef"
              v-model:pagination="pagination"
              :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
              :loading="loading"
              :rows="rows"
              :columns="projectColumns"
              row-key="id"
              separator="cell"
              no-data-label="No data available"
              binary-state-sort
              :rows-per-page-options="[15,30,50,100]"
              @request="getProjects"
            >
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">
                    {{ col.label }}
                  </q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="['cursor-pointer', activeProjectId == props.row.id ? 'bg-green-2' : '']" @click="handleProjectClick(props.row)">
                  <q-td class="highlight-text" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                    <div class="row items-center justify-between">
                      <!-- Left-aligned project name -->
                      <div class="text-left" style="flex: 1; white-space: normal; word-break: break-word;">
                        {{ props.row.name }}
                      </div>
                      <!-- Right-aligned icons with badges -->
                      <div class="row items-center q-mt-xs">
                        <div
                          v-if="props.row.projectNotesCount > 0"
                          class="flex items-center justify-center bg-grey-2 text-black font-bold chat-bubble"
                          style="min-width: 24px; padding: 2px 6px; font-size: 10px; line-height: 1; position: relative;"
                        >
                          {{ props.row.projectNotesCount }}N
                          <q-tooltip>Notes</q-tooltip>
                        </div>
                        <div
                          v-if="props.row.totalTaskCount > 0"
                          class="flex items-center justify-center bg-grey-2 text-black text-bold q-ml-sm"
                          style="min-width: 24px; padding: 2px 6px; border: 1px solid #000; border-radius: 4px; font-size: 10px; line-height: 1;"
                        >
                          {{ props.row.totalTaskCount }}T
                          <q-tooltip>Tasks</q-tooltip>
                        </div>
                      </div>
                    </div>
                  </q-td>
                </q-tr>
                <q-separator />
              </template>
            </q-table>
          </div>
          <div class="col-12 col-md-3 col-lg-3">
            <div class="row items-center justify-end no-wrap q-mb-sm">
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="searchTaskModule.searchText"
                    :loading="searchTaskLoader"
                    :applied-filters="appliedTaskFilters"
                    @toggle-filter="showTaskFilter = !showTaskFilter"
                  />
                  <!-- Dropdown Content -->
                  <q-menu v-model="showTaskFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showTaskFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Number</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="searchTaskModule.projectTaskNumber" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div>
                            <q-input v-model="searchTaskModule.name" fill-input :dense="true" class="q-mx-sm" />
                          </div>
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="searchTaskModule.activityOwners"
                        label="Activity Owner"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="searchTaskModule.statusIds"
                        label="Status"
                        :options="projectTaskStatusForDropdown.list.value"
                        :filter="projectTaskStatusForDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="searchTaskModule.priorityIds"
                        label="Priority"
                        :options="projectTaskPrioritiesForDropdown.list.value"
                        :filter="projectTaskPrioritiesForDropdown.filter"
                        :isShowAll="true"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Show Closed</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-checkbox
                            v-model="searchTaskModule.isShowCloseStatus"
                            color="indigo-12"
                            @click="ShowClosedPT()"
                          />
                        </div>
                      </div>
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showTaskFilter = false; onTaskSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onTaskClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showTaskFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
            </div>
            <q-table
              ref="tableRef"
              v-model:pagination="paginationTasks"
              :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
              :loading="loading"
              :rows="taskRows"
              :columns="taskColumns"
              row-key="id"
              separator="cell"
              no-data-label="No data available"
              binary-state-sort
              :rows-per-page-options="[15,30,50,100]"
              @request="getProjectTasks"
            >
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">
                    {{ col.label }}
                  </q-th>
                  <q-th auto-width class="text-center hidden">Actions</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="['cursor-pointer', activeTaskId == props.row.id ? 'bg-green-2' : '']" @click="LoadProjectNotes(props.row, 'Project Task')">
                  <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                    <div class="row items-start justify-between q-mr-sm">
                      <!-- Left-aligned project name -->
                      <div class="text-left" style="flex: 1; white-space: normal; word-break: break-word;">
                        {{ props.row.name }}
                      </div>
                      <!-- Right-aligned icons with badges -->
                      <div class="row items-center q-mt-xs" style="flex-wrap: nowrap;">
                        <div v-if="props.row.projectTaskNotesCount > 0" class="relative-position">
                          <q-icon
                            name="o_chat"
                            class="cursor-pointer"
                            size="xs"
                          >
                            <q-tooltip>Notes</q-tooltip>
                          </q-icon>
                          <q-badge
                            color="grey-7"
                            text-color="white"
                            :label="props.row.projectTaskNotesCount"
                            style="position: absolute; right: -6px; top: -8px;"
                          />
                        </div>
                      </div>
                    </div>
                  </q-td>
                </q-tr>
                <q-separator />
              </template>
            </q-table>
          </div>
          <div class="col-12 col-md-5 col-lg-6">
            <div class="row items-center justify-end no-wrap q-mb-sm">
              <q-input
                v-model="searchNotesText"
                :loading="searchNotesLoader"
                outlined
                dense
                clearable
                debounce="300"
                placeholder="Search"
                class="bg-white search-box"
                style="flex: 1; border-top-right-radius: 0; border-bottom-right-radius: 0; border-top-right-radius: 0; max-width: 250px;"
              >
                <template #prepend>
                  <q-icon name="o_search" />
                </template>
              </q-input>
            </div>
            <div class="notes-box-shadow">
              <!-- Header -->
              <div class="bg-primary text-white q-pa-md" style="position: sticky; top: 0; z-index: 10; border-radius: 4px 4px 0 0;">
                <div class="row items-center justify-between">
                  <div v-if="activeProjectName && activeTaskName" class="text-h6">
                    {{ activeProjectName + ' >> ' + activeTaskName }}
                  </div>
                  <div v-else-if="activeProjectName" class="text-h6">
                    {{ activeProjectName }}
                  </div>
                </div>
              </div>
              <!-- Timeline Section (scrollable) -->
              <div class="col scroll q-px-sm" style="overflow-y: auto; flex-grow: 1; height: 64vh; display: flex; flex-direction: column-reverse;">
                <q-timeline color="secondary">
                  <q-timeline-entry
                    v-for="(notes, index) in filteredNotes"
                    :key="index"
                    :subtitle="`${notes.createdOnUtc} - ${notes.user?.person?.fullName}`"
                    :icon="done_all"
                    :color="'primary'"
                  >
                    <div class="fs-14 note-row">
                      <template v-if="editingNoteId === notes.id && storedUser === notes.user.userName">
                        <div class="relative">
                          <div class="col-11">
                            <q-editor
                              v-model="editingNoteValue"
                              class="full-width"
                              :dense="$q.screen.lt.md"
                              :toolbar="toolbar"
                              :fonts="fonts"
                              @blur="(e) => handleEditorBlur(e, notes)"
                              @keyup="onKeyUpMessage('ENV')"
                            />
                            <q-list v-if="showSuggestionsInEdit" class="suggestions mention-dropdown bg-white shadow-3 rounded-borders bordered q-pa-sm scroll">
                              <q-item v-for="(user, index) in filteredUsers" :key="index" clickable @click="addMention(user, 'ENV')">
                                <q-item-section class="text-black">{{ user.text }}</q-item-section>
                              </q-item>
                            </q-list>
                          </div>
                          <!-- Actions -->
                          <div class="flex gap-2 justify-end mt-2">
                            <q-btn
                              icon="o_check"
                              color="primary"
                              round
                              dense
                              :loading="editNoteProcessing"
                              :disable="editNoteProcessing || processing"
                              flat
                              @click="sendNote(notes)"
                            >
                              <q-tooltip>Save</q-tooltip>
                            </q-btn>
                            <q-btn
                              icon="o_close"
                              color="negative"
                              round
                              dense
                              flat
                              @mousedown.prevent
                              @click="cancelEditing(notes)"
                            >
                              <q-tooltip>Cancel</q-tooltip>
                            </q-btn>
                          </div>
                        </div>
                      </template>
                      <template v-else>
                        <div class="note-wrapper cursor-pointer RichTextEditor" @click="startEditing(notes)">
                          <span class="text-black note-text" v-html="notes.note" />
                          <q-tooltip v-if="storedUser === notes.user.userName">
                            Click to edit
                          </q-tooltip>
                        </div>
                      </template>
                      <q-btn flat dense round color="primary" icon="o_more_vert" :class="storedUser === notes.user.userName ? '' : 'hidden'">
                        <q-tooltip>More Options</q-tooltip>
                        <q-menu auto-close>
                          <q-list style="min-width: 40px">
                            <q-item v-close-popup clickable>
                              <q-item-section>
                                <q-item v-ripple clickable @click="onDelete(notes)">
                                  <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                                  <q-item-section class="text-negative">Delete</q-item-section>
                                </q-item>
                              </q-item-section>
                            </q-item>
                          </q-list>
                        </q-menu>
                      </q-btn>
                    </div>
                  </q-timeline-entry>
                </q-timeline>
                <div v-if="rowsNotes.length === 0">
                  <h5 class="text-center text-red">No Notes Available</h5>
                </div>
              </div>
              <!-- Footer -->
              <div class="bg-white" style="position: sticky; bottom: 0; z-index: 10; border-top: 0px solid #ccc;">
                <div v-if="!!activeProjectName || !!activeTaskName" class="row items-center no-wrap">
                  <div class="col-11">
                    <q-editor
                      v-model="newNote"
                      class="q-ml-lg q-mb-sm"
                      placeholder="Type your note..."
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                      style="width: 92%;"
                      @keyup="onKeyUpMessage('NN')"
                    />
                    <q-list v-if="showSuggestions" class="suggestions mention-dropdown bg-white shadow-3 rounded-borders bordered q-pa-sm scroll">
                      <q-item v-for="(user, index) in filteredUsers" :key="index" clickable @click="addMention(user,'NN')">
                        <q-item-section class="text-black">{{ user.text }}</q-item-section>
                      </q-item>
                    </q-list>
                  </div>
                  <div class="col-1">
                    <q-btn
                      icon="o_send"
                      color="primary"
                      round
                      flat
                      :loading="processing"
                      :disable="!hasContent || processing || editNoteProcessing"
                      @click="sendNote()"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup>
import { ref, onMounted, watch, computed } from "vue";
import { notifySuccess, zwConfirmDelete } from "assets/utils";
import { useQuasar } from "quasar";
import { useAuthStore } from "stores/auth";

import commonService from "services/common.service";
import projectService from "modules/project/projects.service";
import taskService from "modules/project-tasks/projectTasks.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// SOP Change :- Shared Scripts DataTable Features
import useSiteTableState from "composables/datatable/useSiteTableState.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";

// Shared Dropdowns
import customerModule from "src/modules/customer/utils/dropdowns.js";
import companyContactsModule from "src/modules/company-contacts/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Variable Declarations
// --------------------------------------------------------------------------------------------------------------------------------------------------
const loading = ref(true);
const processing = ref(false);

const authStore = useAuthStore();
const user = authStore.user;
const siteId = computed(() => user?.siteId || null);

const editNoteProcessing = ref(false);
const disableLoadMore = ref(false);
const storedUser = user?.username;
const selectedProjectId = history.state?.projectId ?? null;
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);

// projects
const showFilter = ref(false);
const searchProjectLoader = ref(false);

// task
const showTaskFilter = ref(false);
const searchTaskLoader = ref(false);

// editing notes
const editingNoteId = ref(null);
const editingNoteValue = ref("");
const originalNoteValue = ref("");
const searchNotesLoader = ref(false);

// notes
const newNote = ref("");
const activeNoteContext = ref({
  moduleId: null,
  type: "",
  module: "",
  sub_Module: "",
  subModuleId: ""
});

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project List
// --------------------------------------------------------------------------------------------------------------------------------------------------

const rows = ref([]);
const projectColumns = [
  { name: "name", label: "Projects", align: "left", field: "name", sortable: true }
];

const defaultProjectState = {
  search: {
    searchText: "",
    projectIds: selectedProjectId ? [selectedProjectId] : [],
    projectCoordinatorIds: [],
    projectLeadsIds: [],
    projectStatusIds: [],
    statusId: null,
    projectPriorityIds: [],
    projectTypeIds: [],
    customerIds: [],
    companyContactIds: []
  },

  pagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  },

  activeProjectId: "",
  projectName: ""
};

const {
  search,
  pagination,
  getTableState: getProjectState,
  saveState: saveProjectState
} = useSiteTableState({
  storageKey: "project-Task-Notes",
  siteId,
  tableKey: "dataTable-Projects",

  defaultSearch: defaultProjectState.search,
  defaultPagination: defaultProjectState.pagination
});

const projectState = getProjectState();

const activeProjectId = ref(
  projectState?.activeProjectId ||
  defaultProjectState.activeProjectId
);

const activeProjectName = ref(
  projectState?.projectName ||
  defaultProjectState.projectName
);

// Get/Map project list to table
const getProjects = async (props) => {
  loading.value = true;
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  const resp = await projectService.getAllProjectsForNotes(payload);
  rows.value = resp.data || [];

  pagination.value = {
    ...pagination.value,
    page,
    rowsPerPage,
    sortBy,
    descending,
    rowsNumber: resp.total
  };

  if (search.value.projectIds?.length > 0) {
    activeProjectId.value = search.value.projectIds[0];
  }

  saveProjectState({
    search: search.value,

    pagination: pagination.value,

    activeProjectId: activeProjectId.value,
    projectName: activeProjectName.value
  });
  loading.value = false;
  searchProjectLoader.value = false;
};

const refreshProjectList = () => {
  getProjects({ pagination: pagination.value });
};

const refreshProjectTaskList = () => {
  getProjectTasks({ pagination: paginationTasks.value });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project & Task Notes List
// --------------------------------------------------------------------------------------------------------------------------------------------------
const rowsNotes = ref([]);
const paginationNotes = ref({ sortBy: "createdOnUtc", descending: false, rowsPerPage: 20, page: 1 });
const defaultNotesState = {
  searchNotesText: ""
};

const {
  getTableState: getNotesState,
  saveState: saveNotesState
} = useSiteTableState({
  storageKey: "project-Task-Notes",
  siteId,
  tableKey: "notes",

  defaultSearch: defaultNotesState,
  defaultPagination: {}
});

const notesState = getNotesState();
const searchNotesText = ref(notesState?.searchNotesText || "");

const LoadProjectNotes = async (notes, type) => {
  loading.value = true;
  if (type === "Projects") {
    activeProjectId.value = notes?.id ? notes?.id : notes?.moduleId;
    activeProjectName.value = notes?.name ? notes?.name : notes?.module;
    activeNoteContext.value = {
      moduleId: notes?.id ? notes?.id : notes?.moduleId,
      module: notes?.name ? notes?.name : notes?.module,
      sub_Module: notes?.name ? notes?.name : notes?.sub_Module,
      subModuleId: notes?.id ? notes?.id : notes?.subModuleId,
      type
    };
    // Update Local Storage.
    saveNotesState({
      activeProjectId: activeProjectId.value,
      projectName: activeProjectName.value
    });
  } else if (type === "Project Task") {
    activeTaskId.value = notes?.id ? notes?.id : notes?.subModuleId;
    activeTaskName.value = notes?.name ? notes?.name : notes?.sub_Module;
    const projectId = notes?.project?.id || notes?.projectId || notes?.moduleId;
    const projectName = notes?.project?.name || notes?.projectName || notes?.module;
    activeNoteContext.value = {
      moduleId: projectId,
      module: projectName,
      sub_Module: notes?.name ? notes?.name : notes?.sub_Module,
      subModuleId: notes?.id ? notes?.id : notes?.subModuleId,
      type
    };
    saveNotesState({
      ...getNotesState(),
      activeTaskId: activeTaskId.value
    });
  }
  disableLoadMore.value = false;
  // notes
  if (notes?.id) {
    const resp = await commonService.getAllNoteByTypeAndRecord(notes?.id, type, false);
    rowsNotes.value = resp;
    paginationNotes.value = {
      ...paginationNotes.value,
      rowsNumber: resp.total
    };
  }
  loading.value = false;
  searchNotesLoader.value = false;
};

// handleProjectClick
const handleProjectClick = async (project) => {
  activeProjectId.value = project.id;
  activeProjectName.value = project.name;
  activeTaskName.value = "";
  await LoadTasks(project.id); // First load tasks
  await LoadProjectNotes(project, "Projects"); // Then load notes
  getProjectEmployees(activeProjectId.value);
};

const isCancelling = ref(false);
// editing notes
const startEditing = (notes) => {
  editingNoteId.value = notes.id;
  editingNoteValue.value = notes.note;
  originalNoteValue.value = notes.note;
  isCancelling.value = false;
};

// handleEditorBlur
const handleEditorBlur = (event, note) => {
// ignore if cancel in progress
  if (isCancelling.value) {
    return;
  }
  // If blur is because of toolbar click, ignore
  if (event.relatedTarget && event.relatedTarget.closest(".q-editor__toolbar")) {
    return;
  }
  // If no changes → exit without saving
  if (editingNoteValue.value.trim() === (originalNoteValue.value || "").trim()) {
    editingNoteId.value = null; // close edit mode
  }
  // sendNote(note);
};

const cancelEditing = (note) => {
  isCancelling.value = true; // block blur save
  editingNoteId.value = null;
  editingNoteValue.value = "";
  if (note) {
    note.note = originalNoteValue.value; // restore original text
  }
  // reset flag after tick
  setTimeout(() => (isCancelling.value = false), 0);
};

const isEditorEmpty = (html = "") => {
  return html
    .replace(/<br\s*\/?>/gi, "")
    .replace(/&nbsp;/gi, "")
    .replace(/<[^>]*>/g, "")
    .trim()
    .length === 0;
};

const hasContent = computed(() => {
  return !isEditorEmpty(newNote.value);
});

const showSuggestions = ref(false);
const showSuggestionsInEdit = ref(false);
const mentionStart = ref(-1);
const filteredUsers = ref([]);

const onKeyUpMessage = (flag) => {
  const selection = window.getSelection();
  const range = selection.rangeCount > 0 ? selection.getRangeAt(0) : null;
  if (!range) return;

  const cursorPos = range.startOffset; // Get cursor position inside `q-editor`
  let textContent;
  if (flag === "ENV") {
    textContent = editingNoteValue.value.replace(/<[^>]*>/g, ""); // Remove HTML tags
  } else {
    textContent = newNote.value.replace(/<[^>]*>/g, ""); // Remove HTML tags
  }
  const lastAtPos = textContent.lastIndexOf("@", cursorPos - 1);

  // Show mention list if "@" is followed by any character
  if (lastAtPos !== -1 && cursorPos >= lastAtPos + 1) {
    mentionStart.value = lastAtPos;
    const searchTerm = textContent.slice(lastAtPos + 1, cursorPos).toLowerCase();

    // Filter user list based on search term
    filteredUsers.value = projectEmployeeList.value.filter((user) =>
      searchTerm ? user.text.toLowerCase().includes(searchTerm) : true
    );

    if (flag === "ENV") {
      showSuggestionsInEdit.value = filteredUsers.value.length > 0;
    } else {
      showSuggestions.value = filteredUsers.value.length > 0;
    }
  } else {
    if (flag === "ENV") { showSuggestionsInEdit.value = false; } else { showSuggestions.value = false; }
  }
};
const addMention = (user, flag) => {
  if (mentionStart.value === -1) return; // No active mention

  const editor = document.querySelector(".q-editor__content");
  if (!editor) return;

  const selection = window.getSelection();
  const range = selection.rangeCount > 0 ? selection.getRangeAt(0) : null;
  if (!range) return;

  // Get the current content
  const content = flag === "ENV" ? editingNoteValue.value : newNote.value;

  // Preserve text before and after the mention being typed
  const beforeMention = content.slice(0, mentionStart.value); // Text before "@"
  const afterMention = content.slice(mentionStart.value); // Text after "@"

  // Find the end of the mention (first space, punctuation, or end of string)
  const mentionEndMatch = afterMention.match(/(\s|<\/?[a-z][\s\S]*>|&nbsp;|$)/);
  const mentionEndIndex = mentionEndMatch ? mentionEndMatch.index : afterMention.length;

  // Extract remaining text after the mention
  const remainingText = afterMention.slice(mentionEndIndex);

  // Format mention
  const nameParts = user.text.split(" ");
  const formattedName = `${nameParts[0]} ${nameParts[nameParts.length - 1]}`.trim();
  const mentionTag = `<span class="tagged-user">@${formattedName}</span>&nbsp;`;

  // Insert mention at the correct position
  if (flag === "ENV") {
    editingNoteValue.value = beforeMention + mentionTag + remainingText;
    showSuggestionsInEdit.value = false;
  } else {
    newNote.value = beforeMention + mentionTag + remainingText;
    showSuggestions.value = false;
  }

  mentionStart.value = -1;

  // Restore Cursor Position after mention insertion
  setTimeout(() => {
    if (editor) {
      const mentionSpan = editor.querySelector(".tagged-user:last-child");
      if (mentionSpan && mentionSpan.nextSibling) {
        range.setStartAfter(mentionSpan.nextSibling);
        range.setEndAfter(mentionSpan.nextSibling);
      } else {
        range.setStartAfter(mentionSpan);
        range.setEndAfter(mentionSpan);
      }
      selection.removeAllRanges();
      selection.addRange(range);
    }
  }, 0);
};

const extractMentionedUsers = (text) => {
  const mentionedNames = [...text.matchAll(/@([\w\s]+)/g)].map(match => match[1].trim().toLowerCase());
  return projectEmployeeList.value.filter(emp => {
    const empFullName = emp.text.trim().toLowerCase();
    const empParts = empFullName.split(/\s+/);
    const empFirstName = empParts[0];
    const empLastName = empParts[empParts.length - 1];

    return mentionedNames.some(name => {
      const mention = name.toLowerCase();
      return (
        mention === empFullName || // full match
        mention === `${empFirstName} ${empLastName}` // first + last match
      );
    });
  }).map(emp => emp.value);
};

// save note
const sendNote = async (note = null) => {
  // Prevent double submit
  if (processing.value || editNoteProcessing.value) return;
  try {
    // Determine if we're editing or adding
    const isEditing = !!note;

    // Get the value being saved
    const noteValue = (isEditing ? editingNoteValue.value : newNote.value) || "";

    if (isEditorEmpty(noteValue)) {
      if (isEditing) editingNoteId.value = null;
      return;
    }

    // Validate
    if (!noteValue || !noteValue.trim()) {
      if (isEditing) editingNoteId.value = null; // close edit mode
      return;
    }

    // Enable correct loader
    if (isEditing) {
      editNoteProcessing.value = true;
    } else {
      processing.value = true;
    }
    // Extract mentioned users from the note
    const mentionedUsers = extractMentionedUsers(isEditing ? editingNoteValue.value : newNote.value);
    const payload = {
      id: isEditing ? note.id : null,
      subModuleId: isEditing ? note.subModuleId : activeNoteContext.value.subModuleId,
      moduleId: isEditing ? note.moduleId : activeNoteContext.value.moduleId,
      note: isEditing ? editingNoteValue.value : newNote.value,
      type: isEditing ? note.type : activeNoteContext.value.type,
      module: isEditing ? note.module : activeNoteContext.value.module,
      sub_Module: isEditing ? note.sub_Module : activeNoteContext.value.sub_Module,
      taggedPersonId: mentionedUsers.join(",")
    };
    await commonService.saveNote(payload)
      .then((resp) => {
        notifySuccess({ message: "Note is saved successfully." });
        if (isEditing) {
          editingNoteId.value = null;
        } else {
          newNote.value = ""; // clear input after add
        }
        // reload based on module/sub_Module/type
        if (activeNoteContext.value.type === "Projects") {
          // Reload project notes
          LoadProjectNotes({
            id: activeProjectId.value,
            name: activeProjectName.value,
            type: "Projects"
          }, "Projects");
          refreshProjectList();
        } else if (activeNoteContext.value.type === "Project Task") {
          // Reload task notes
          LoadProjectNotes({
            id: activeTaskId.value,
            name: activeTaskName.value,
            type: "Project Task",
            projectId: activeProjectId.value,
            projectName: activeProjectName.value
          }, "Project Task");
          refreshProjectTaskList();
        }
      });
  } catch (error) {
    console.error("Error in submitting the note:", error);
  } finally {
    // processing.value = true;
    // editNoteProcessing.value = true;
    setTimeout(() => {
      processing.value = false;
      editNoteProcessing.value = false;
    }, 1500);
  }
};

// onDelete
const onDelete = (item) => {
  // activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.user.person.firstName + " " + item.user.person.lastName}` }, () => {
    commonService.deleteNote(item.id).then(resp => {
      notifySuccess({ message: "Note is deleted successfully." });
      // reload based on module/sub_Module/type
      if (activeNoteContext.value.type === "Projects") {
        // Reload project notes
        LoadProjectNotes({
          id: activeProjectId.value,
          name: activeProjectName.value,
          type: "Projects"
        }, "Projects");
        refreshProjectList();
      } else if (activeNoteContext.value.type === "Project Task") {
        // Reload task notes
        LoadProjectNotes({
          id: activeTaskId.value,
          name: activeTaskName.value,
          type: "Project Task",
          projectId: activeProjectId.value,
          projectName: activeProjectName.value
        }, "Project Task");
        refreshProjectTaskList();
      }
    });
  }, () => {
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Task List
// --------------------------------------------------------------------------------------------------------------------------------------------------

const taskRows = ref([]);
const taskColumns = [
  { name: "name", label: "Tasks", align: "left", field: "name", sortable: true }
];

const defaultTaskState = {
  search: {
    searchText: "",
    projectIds: [],
    projectTaskNumber: 0,
    statusIds: [],
    priorityIds: [],
    name: "",
    customerIds: [],
    companyContactIds: [],
    projectLeadsIds: [],
    activityOwners: [],
    isShowCloseStatus: false
  },

  pagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  },

  activeTaskId: null,
  activeTaskName: ""
};

const {
  search: searchTaskModule,
  pagination: paginationTasks,
  getTableState: getTaskState,
  saveState: saveTaskState
} = useSiteTableState({
  storageKey: "project-Task-Notes",
  siteId,
  tableKey: "dataTable-ProjectTasks",

  defaultSearch: defaultTaskState.search,
  defaultPagination: defaultTaskState.pagination
});

const taskState = getTaskState();

const activeTaskId = ref(
  taskState?.activeTaskId ||
  defaultTaskState.activeTaskId
);

const activeTaskName = ref(
  taskState?.activeTaskName ||
  defaultTaskState.activeTaskName
);

async function LoadTasks (projectId) {
  try {
    searchTaskModule.value.projectIds = [projectId];
    await refreshProjectTaskList();
  } catch (error) {
    console.error("Error loading tasks:", error);
  } finally {
    loading.value = false;
    searchTaskLoader.value = false;
  }
}

const getProjectTasks = async (props) => {
  loading.value = true;

  try {
    const { page, rowsPerPage, sortBy, descending } = props.pagination;

    const taskNumber = searchTaskModule.value.projectTaskNumber
      ? searchTaskModule.value.projectTaskNumber
        .replace(/[^0-9]/g, "")
        .replace(/^0+(?!$)/, "")
      : "";

    searchTaskModule.value.projectTaskNumber = taskNumber || 0;

    const payload = {
      page,
      pageSize: rowsPerPage,
      sortBy,
      descending,
      ...searchTaskModule.value
    };

    const resp = await taskService.getAllProjectTasksForNotes(payload);

    taskRows.value = resp.data || [];

    paginationTasks.value = {
      ...paginationTasks.value,
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    };

    saveTaskState({
      search: searchTaskModule.value,

      pagination: paginationTasks.value,

      activeTaskId: activeTaskId.value,
      activeTaskName: activeTaskName.value
    });

    const activeProject = rows.value.find(
      p => p.id === activeProjectId.value
    );

    if (activeProject) {
      activeProject.totalTaskCount = resp.total;
    }
  } finally {
    loading.value = false;
    searchTaskLoader.value = false;
  }
};

// close tasks
const ShowClosedPT = () => {
  refreshProjectTaskList();
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear for project
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onSearch = () => {
  refreshProjectList();
};

const onClear = () => {
  search.value.searchText = "";
  search.value.projectIds = [];
  search.value.projectCoordinatorIds = [];
  search.value.projectLeadsIds = [];
  search.value.projectStatusIds = [];
  search.value.statusId = null;
  search.value.projectPriorityIds = [];
  search.value.projectTypeIds = [];
  search.value.customerIds = [];
  search.value.companyContactIds = [];
  saveProjectState(defaultProjectState);
  onSearch();
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear for tasks
// --------------------------------------------------------------------------------------------------------------------------------------------------

const onTaskSearch = () => {
  refreshProjectTaskList();
};

const onTaskClear = () => {
  searchTaskModule.value.searchText = "";
  searchTaskModule.value.projectIds = activeProjectId.value ? [activeProjectId.value] : [];
  searchTaskModule.value.projectTaskNumber = "";
  searchTaskModule.value.statusIds = [];
  searchTaskModule.value.priorityIds = [];
  searchTaskModule.value.name = "";
  searchTaskModule.value.customerIds = [];
  searchTaskModule.value.companyContactIds = [];
  searchTaskModule.value.projectLeadsIds = [];
  searchTaskModule.value.activityOwners = [];
  searchTaskModule.value = {
    ...defaultTaskState.search,
    projectIds: activeProjectId.value ? [activeProjectId.value] : []
  };

  saveTaskState({
    ...getTaskState(),
    search: searchTaskModule.value,
    pagination: defaultTaskState.pagination,
    activeTaskId: activeTaskId.value,
    activeTaskName: activeTaskName.value
  });

  onTaskSearch();
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Advance Filter:- Applied Filter Labels.
// --------------------------------------------------------------------------------------------------------------------------------------------------

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

const mapSingleFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

// for project
const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.customerIds, customerNameDropdown.list, "Customer"),
  ...mapFilterToLabel(search.value.companyContactIds, companyContactNameDropdown.list, "Company Contact"),
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Name"),
  ...mapFilterToLabel(search.value.projectCoordinatorIds, activeEmployeesDropdown.list, "Coordinator"),
  ...mapFilterToLabel(search.value.projectLeadsIds, activeEmployeesDropdown.list, "Leads"),
  ...mapFilterToLabel(search.value.projectStatusIds, projectStatusDropdown.list, "Status"),
  ...mapSingleFilterToLabel(search.value.statusId, projectActiveInActiveDropdown.list, "Active/Inactive"),
  ...mapFilterToLabel(search.value.projectPriorityIds, projectPrioritiesDropdown.list, "Priority"),
  ...mapFilterToLabel(search.value.projectTypeIds, projectTypesDropdown.list, "Type")
}));

// for tasks
const appliedTaskFilters = computed(() => ({
  ...(searchTaskModule.value.projectTaskNumber > 0 ? { Number: searchTaskModule.value.projectTaskNumber } : {}),
  ...(searchTaskModule.value.name ? { Name: searchTaskModule.value.name } : {}),
  ...mapFilterToLabel(searchTaskModule.value.activityOwners, activeEmployeesDropdown.list, "Activity Owner"),
  ...mapFilterToLabel(searchTaskModule.value.statusIds, projectTaskStatusForDropdown.list, "Status"),
  ...mapFilterToLabel(searchTaskModule.value.priorityIds, projectTaskPrioritiesForDropdown.list, "Priority")
}));

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const { customerNameDropdown } = customerModule();
const { companyContactNameDropdown } = companyContactsModule();
const { activeEmployeesDropdown } = employeeModule();
const {
  projectNameDropdown,
  projectActiveInActiveDropdown,
  projectPrioritiesDropdown,
  projectTypesDropdown,
  projectStatusDropdown
} = projectModule();

const { projectTaskPrioritiesForDropdown, projectTaskStatusForDropdown } = projectTaskModule();
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Advance Filter:- Dropdown Functions
// --------------------------------------------------------------------------------------------------------------------------------------------------

const projectEmployeeList = ref([]);

function getProjectEmployees (projectId) {
  projectService.getProjectEmployees(projectId).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.text, value: item.value }));
    projectEmployeeList.value = responseData;
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Search: When user types in search
// --------------------------------------------------------------------------------------------------------------------------------------------------
const filteredNotes = computed(() => {
  if (!searchNotesText.value) return rowsNotes.value;
  const search = searchNotesText.value.toLowerCase();

  return rowsNotes.value.filter(note =>
    note.note?.toLowerCase().includes(search) ||
    note.user?.person?.fullName?.toLowerCase().includes(search) ||
    note.createdOnUtc.includes(search)
  );
});

watch(() => searchTaskModule.value.searchText, () => {
  if (searchTaskModule.value.searchText) searchTaskLoader.value = true;
  refreshProjectTaskList();
});

watch(() => search.value.searchText, () => {
  searchProjectLoader.value = true;
  refreshProjectList();
});

watch(() => searchNotesText.value, (val) => {
  saveNotesState({
    searchNotesText: val
  });

  if (val) searchNotesLoader.value = true;

  if (activeNoteContext.value?.moduleId) {
    LoadProjectNotes(activeNoteContext.value, activeNoteContext.value.type);
  }
});

watch(() => searchTaskModule.value.isShowCloseStatus, (val) => {
  saveTaskState({
    ...getTaskState(),
    search: {
      ...searchTaskModule.value,
      isShowCloseStatus: val
    }
  });
});
// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Page Load
// --------------------------------------------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  // Advance Filter Functions
  await projectNameDropdown.load();
  activeEmployeesDropdown.load(user.siteId);

  // project status
  await projectStatusDropdown.load("Project Status");
  // set default in progress status
  const inProgressStatusValue = projectStatusDropdown.getValueByLabel("in progress");
  // Set Default values for advance filter
  if (
    !Array.isArray(search.value.projectStatusIds) ||
    search.value.projectStatusIds.length === 0
  ) {
    search.value.projectStatusIds = [inProgressStatusValue];
  }

  await projectActiveInActiveDropdown.load("Project Active Status");
  const activeValue = await projectActiveInActiveDropdown.getValueByLabel("Active");
  // Set Default values for advance filter
  if (search.value.statusId === null || search.value.statusId === undefined) search.value.statusId = activeValue;

  projectPrioritiesDropdown.load("Project Priorities");
  projectTypesDropdown.load("Project Type");
  projectTaskStatusForDropdown.load("Task Status");
  projectTaskPrioritiesForDropdown.load("Task Priorities");
  customerNameDropdown.load();
  companyContactNameDropdown.load();

  await refreshProjectList();
  if (activeProjectId.value) {
    await LoadTasks(activeProjectId.value);
  }
});

</script>
<style scoped>
.note-row {
  display: flex;
  align-items: center;
  gap: 6px;
}

.note-row .label {
  font-weight: bold;
  white-space: nowrap;
}
.note-input {
  flex: 1;
  min-width: 100px;
}
.note-text {
  display: inline-block; /* shrink-wraps to text width */
}
.note-row .q-btn {
  visibility: hidden; /* hide by default */
}

.note-row:hover .q-btn {
  visibility: visible; /* show when row hovered */
}
.notes-box-shadow {
  box-shadow: 0 1px 5px rgba(0, 0, 0, 0.2), 0 2px 2px rgba(0, 0, 0, 0.14), 0 3px 1px -2px rgba(0, 0, 0, 0.12) !important;
  background-color: #fff;
  border-radius: 4px 4px 4px 4px !important;
}
.chat-bubble {
  border-radius: 12px 12px 12px 0; /* rounded except bottom-left corner */
  border: 1px solid #000;
}

.chat-bubble::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: -6px; /* adjust horizontal position of tail */
  width: 0;
  height: 0;
  border: 6px solid transparent;
  border-top-color: #e4f6ff; /* same as bg-grey-2 */
  border-bottom: 0;
  border-left: 0;
  margin-bottom: -6px;
}
.tagged-user {
  color: var(--q-primary); /* Apply primary color */
  font-weight: bold;
  background-color: rgba(33, 150, 243, 0.1); /* Light blue background */
  padding: 2px 4px;
  border-radius: 4px;
  display: inline-block;
}
.mention-dropdown {
  width: 300px;
  max-width: 100%;
  max-height: 200px;
  overflow-y: auto;
}
</style>
