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
              <q-breadcrumbs-el label="SOP Templates" />
              <q-breadcrumbs-el label="List" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-md-4">
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
                          <label class="Cutomlabel q-mt-sm fs-13">Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.name" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Active/Inactive</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-option-group
                            v-model="search.isActive"
                            :options="[
                              { label: 'Active', value: true },
                              { label: 'Inactive', value: false }
                            ]"
                            type="radio"
                            inline
                            dense
                          />
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
              </div>
              <manageDropdownOptions
                v-model="showManageDropdownOptions"
                :manage-drop-down-types="manageDropDownTypes"
                :selected-field="selectedField"
              />
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Add Template" no-caps class="text-primary btnRounded" @click="onSOPTemplateAdd(refreshSOPTemplateList)" />
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_playlist_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
                  @click="showManageDropdownOptions = !showManageDropdownOptions"
                >
                  <q-tooltip>Manage Dropdowns</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
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
        @request="getAllSOPTemplates"
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
          <q-tr
            :props="props"
            :class="highlightedId == props.row.id ? 'highlight' : ''"
          >
            <q-td>
              {{ props.row.name }}
            </q-td>
            <q-td>
              {{ props.row.version }}
            </q-td>
            <q-td>
              {{ props.row.createdBy.person.fullName }}
            </q-td>
            <q-td class="text-center">
              {{ toDate(props.row.createdOnUtc) }}
            </q-td>
            <q-td>
              {{ props.row.updatedBy.person.fullName }}
            </q-td>
            <q-td class="text-center">
              {{ toDate(props.row.updatedOnUtc) }}
            </q-td>
            <q-td class="text-center">
              {{ props.row.isActive ? "Active" : "Inactive" }}
            </q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="onSOPTemplateView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon name="o_edit" class="cursor-pointer q-mr-sm" size="xs" @click="onSOPTemplateEdit(props.row.id, refreshSOPTemplateList)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onSubmitSOPTemplateDelete(props.row.id, props.row.name, refreshSOPTemplateList)">
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
          <q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed, onBeforeUnmount } from "vue";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";
import { getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";

import sopTemplateService from "../sopTemplate.service";

// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import manageDropdownOptions from "src/components/dataTable/_manageDropdownOptions.vue";

// SOP Change :- Shared Dropdowns
import manageDropdownModule from "src/modules/dropdown/utils/dropdowns.js";

// SOP Change :- Shared Project Dialogs
import {
  initSOPTemplateDialogs,
  onSOPTemplateView,
  onSOPTemplateAdd,
  onSOPTemplateEdit
} from "src/modules/sop-template/utils/dialogs.js";

// SOP Change :- Shared Project Actions
import {
  initSOPTemplateActions,
  onSubmitSOPTemplateDelete
} from "src/modules/sop-template/utils/actions.js";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);

const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

const showManageDropdownOptions = ref(false);
const { toDate } = useFilters();
const manageDropDownTypes = ref([]);

// local storage values
const localStorageKey = "SOP Template";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Table variables
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "name", label: "Template Name", field: "name", align: "left", sortable: true },
  { name: "version", label: "Version", field: "Version", align: "left", sortable: true },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left", sortable: true },
  { name: "createdOnUtc", label: "Created On", field: "createdOnUtc", align: "center", sortable: true },
  { name: "updatedBy.person.firstName", label: "Updated By", field: "updatedBy.person.firstName", align: "left", sortable: true },
  { name: "updatedOnUtc", label: "Updated On", field: "updatedOnUtc", align: "center", sortable: true },
  { name: "isActive", label: "Active/InActive", field: "isActive", align: "center", sortable: true }
]);

const highlightProjectId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightProjectId);
const highlightedId = computed(() => {
  return activeRowId.value;
});

// function setActiveRowIdInLocalStorage (id) {
//   const storedData = getLocalStorage(localStorageKey) || {};
//   setLocalStorage(localStorageKey, { ...storedData, activeRowId: id });
// }

// Search variables
const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

// Search variables
const search = ref({
  searchText: getFilterValue("searchText", ""),
  name: getFilterValue("name", ""),
  isActive: getFilterValue("isActive", true)
});

const handleDocumentClick = (event) => {
  const highlightElement = document.querySelector(".highlight");
  // Check if clicked inside the highlighted row or icons
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;
    const storedData = getLocalStorage(localStorageKey) || {};
    setLocalStorage(localStorageKey, { ...storedData, activeRowId: null });
  }
};

// Get/Map project list to table
const getAllSOPTemplates = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
  sopTemplateService.getAllSOPTemplateList(payload).then((resp) => {
    rows.value = resp.data.map(data => {
      return {
        ...data
      };
    });
    // console.log(rows.value);
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

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const refreshSOPTemplateList = () => {
  getAllSOPTemplates({ pagination: pagination.value });
};

// Search records as per parameters
const onSearch = () => {
  refreshSOPTemplateList();
};

// Clear search
const onClear = () => {
  search.value.projectIds = [];
  search.value.deploymentOwnerIds = [];
  clearLocalStorage(localStorageKey);
  onSearch();
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initSOPTemplateDialogs(activeRowId);
initSOPTemplateActions(activeRowId);

// ----------------------------
// Applied Filter Labels.
// ----------------------------
const appliedFilters = computed(() => ({
  ...(search.value.name ? { "Template Name": search.value.name } : {}),
  ...(search.value.isActive !== null
    ? {
      "Active/Inactive":
          search.value.isActive ? "Active" : "Inactive"
    }
    : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Template Name": return search.value.name?.length || 0;
  case "Active/Inactive":
    return search.value.isActive !== null ? 1 : 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Template Name") {
    search.value.name = "";
  } else if (key === "isActive") {
    search.value.isActive = true;
  }
  delete appliedFilters.value[key];
  refreshSOPTemplateList();
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { getDropdownTypesByModuleNameForDropdown } = manageDropdownModule();

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => search.value.searchText, () => {
  if (search.value.searchText) searchLoader.value = true;
  refreshSOPTemplateList();
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// On page rendering
onMounted(async () => {
  tableRef.value.requestServerInteraction();

  // Admin:- Manage all Release-Tracking Dropdowns and Types
  manageDropDownTypes.value = await getDropdownTypesByModuleNameForDropdown("SOP-Template");

  if (!activeRowId.value && highlightProjectId) {
    activeRowId.value = highlightProjectId;
  }
  document.addEventListener("click", handleDocumentClick);
});

</script>
