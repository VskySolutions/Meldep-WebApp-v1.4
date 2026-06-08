<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Org Management" />
              <q-breadcrumbs-el label="Training Portal" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center q-ml-lg">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Training Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.name"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="text"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.employeeDesignationIds"
                        label="Assigned To"
                        :options="employeeDesignationForDropdown.list.value"
                        :filter="employeeDesignationForDropdown.filter"
                      />
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onAdvanceSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onAdvanceClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
              <div class="q-ml-xs">
                <q-btn
                  icon="o_add"
                  outline
                  label="Add Training"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onTrainingPortalAdd(refreshTrainingPortalList)"
                />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>

      <q-table
        ref="tableRef"
        v-model:pagination="pagination"
        :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
        :loading="loading"
        :rows="rows"
        :columns="columns"
        row-key="id"
        separator="cell"
        no-data-label="No data available"
        binary-state-sort
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        @request="getTrainingPortals"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td style="width: 5%;" class="hidden">#{{ props.row.trainingPortalNumber }}</q-td>
            <q-td style="width: 15%;">{{ props.row.name }}</q-td>
            <q-td style="width: 20%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;"><a
              :href="props.row.url"
              target="_blank"
              rel="noopener noreferrer"
              class="text-primary"
            >
              {{ props.row.url }}
            </a></q-td>
            <q-td style="width: 30%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;"><div class="RichTextEditor" v-html="props.row.description" /></q-td>
            <q-td style="width: 20%;">
              <span v-if="props.row.trainingPortalMappings.length > 0">
                <span v-for="(employeeMapping, index) in props.row.trainingPortalMappings" :key="employeeMapping.id">
                  <span text-color="black">{{ employeeMapping.employeeDesignationType.dropDownValue }}</span>
                  <span v-if="index !== props.row.trainingPortalMappings.length - 1"><br></span>
                </span>
              </span>
            </q-td>
            <q-td style="width: 10%;" class="text-center actions">
              <q-icon
                name="o_visibility"
                class="cursor-pointer q-mr-sm"
                @click="onTrainingPortalView(props.row.id, refreshTrainingPortalList)"
              >
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                @click="onTrainingPortalEdit(props.row.id, refreshTrainingPortalList)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon
                v-if="props.row.file?.virtualPath"
                name="o_insert_drive_file"
                class="cursor-pointer q-mr-sm"
                @click="openFile(props.row.file.virtualPath)"
              >
                <q-tooltip>View File</q-tooltip>
              </q-icon>
              <q-icon
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                @click="onDelete(props.row)"
              >
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr><q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import { zwConfirmDelete, notifySuccess, setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";

import trainingService from "modules/training-portal/trainingPortal.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Leave Dialogs
import {
  initTrainingPortalDialogs,
  onTrainingPortalView,
  onTrainingPortalAdd,
  onTrainingPortalEdit
} from "src/modules/training-portal/utils/dialogs.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "TrainingPortal";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshTrainingPortalList = () => {
  getTrainingPortals({ pagination: pagination.value });
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  // { name: "TrainingPortalNumber", label: "Training Number", field: "TrainingPortalNumber", align: "left", sortable: true },
  { name: "name", label: "Training Name", field: "name", align: "left", sortable: true, style: "width: 10%" },
  { name: "url", label: "URL/Link", field: "url", align: "left", sortable: true, style: "width: 10%" },
  { name: "description", label: "Description", field: "description", align: "left", sortable: true, style: "width: 10%" },
  { name: "trainingPortalMappings.employeeDesignationType.dropDownValue", label: "Assigned To", field: "trainingPortalMappings.employeeDesignationType.dropDownValue", align: "left", sortable: false, style: "width: 10%" }
]);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Training portal
// ----------------------------------------------------------------------------------------------------------------

const getTrainingPortals = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  trainingService.getTrainingPortals(payload).then((resp) => {
    rows.value = resp.data;
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initTrainingPortalDialogs(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

// Search variables
const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

const search = ref({
  searchText: getFilterValue("searchText", ""),
  name: getFilterValue("name", ""),
  employeeDesignationIds: getFilterValue("employeeDesignationIds", [])
});

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshTrainingPortalList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.employeeDesignationIds = [];
  search.value.name = "";
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------

const { employeeDesignationForDropdown } = employeeModule();

const openFile = (path) => {
  if (!path) return;
  window.open(`${path}`, "_blank", "noopener,noreferrer");
};

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.name}` }, () => {
    trainingService.deleteTraining(item.id).then(resp => {
      notifySuccess({ message: "Training is deleted successfully." });
      refreshTrainingPortalList();
    });
  }, () => {
    activeRowId.value = null;
  });
};

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshTrainingPortalList();
});

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
  ...mapFilterToLabel(search.value.employeeDesignationIds, employeeDesignationForDropdown.list, "Assigned To"),
  ...(search.value.name ? { "Training Name": search.value.name } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Assigned To": return search.value.employeeDesignationIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Assigned To") {
    search.value.employeeDesignationIds = [];
  } else if (key === "Training Name") {
    search.value.name = "";
  }
  delete appliedFilters.value[key];
  refreshTrainingPortalList();
}

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  employeeDesignationForDropdown.load("Employee Designation");
  tableRef.value.requestServerInteraction();
  refreshTrainingPortalList();
});
</script>
