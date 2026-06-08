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
              <q-breadcrumbs-el label="Talent Hire" />
              <q-breadcrumbs-el label="Job Post" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center q-ml-lg">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <!-- <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge> -->
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
                          <label class="Cutomlabel q-mt-sm fs-13">Job Title</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.jobTitle"
                            fill-input
                            :dense="true"
                          />
                        </div>
                      </div>
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
                  label="Create Job"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onJobPostAdd(refreshJobPostList)"
                />
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
        @request="getAllJobPosts"
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
            :class="activeRowId == props.row.id ? 'highlight' : ''"
          >
            <q-td style="width: 20%;">
              {{ props.row.jobTitle }}
            </q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;">
              {{ props.row.criteria }}
            </q-td>
            <q-td class="text-center" style="width: 5%;">
              <q-checkbox 
                v-model="props.row.isActive" 
                @update:model-value="onSubmitJobPostStatus(props.row.id, props.row.isActive, refreshJobPostList)" 
              />
            </q-td>
            <q-td style="width: 25%;">
              {{ props.row.publishedJobDate }}
            </q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 25%;">
              {{ props.row.jobReference }}
            </q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <q-icon 
                name="o_visibility" 
                class="cursor-pointer q-mr-sm" size="xs" 
                @click="onJobPostView(props.row.id, refreshJobPostList)"
              >
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon 
                name="o_edit" 
                class="cursor-pointer q-mr-sm" 
                size="xs" 
                @click="onJobPostEdit(props.row.id, refreshJobPostList)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon 
                name="o_delete_outline" 
                class="cursor-pointer" 
                color="negative" 
                size="xs" 
                @click="onSubmitJobPostDelete(props.row.id, props.row.jobTitle, refreshJobPostList)"
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
import { getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";

import jobPostService from "modules/job-post/jobPost.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Leave Dialogs
import {
  initJobPostDialogs,
  onJobPostView,
  onJobPostAdd,
  onJobPostEdit
} from "src/modules/job-post/utils/dialogs.js";

// Shared Job Post Actions
import {
  initJobPostActions,
  onSubmitJobPostStatus,
  onSubmitJobPostDelete
} from "src/modules/job-post/utils/actions.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "Job Post";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "jobTitle", label: "Job Title", field: "jobTitle", align: "left", sortable: true },
  { name: "criteria", label: "Criteria", field: "criteria", align: "left", sortable: true },
  { name: "isActive", label: "Status", field: "isActive", align: "left", sortable: true },
  { name: "publishedJobDate", label: "Job Published Date", field: "publishedJobDate", align: "left", sortable: false },
  { name: "jobReference", label: "Job Reference", field: "jobReference", align: "left", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Job Posts
// ----------------------------------------------------------------------------------------------------------------

const getAllJobPosts = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  jobPostService.getAllJobPosts(payload).then((resp) => {
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
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------

initJobPostDialogs(activeRowId);
initJobPostActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshJobPostList = () => {
  getAllJobPosts({ pagination: pagination.value });
};

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
  jobTitle: getFilterValue("jobTitle", "")
});

const onAdvanceSearch = () => {
  refreshJobPostList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.jobTitle = "";
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

// ----------------------------
// Applied Filter Labels.
// ----------------------------

const appliedFilters = computed(() => ({
  ...(search.value.jobTitle ? { "Job Title": search.value.jobTitle } : {})
}));

function onClearFilters (key) {
  if (key === "Job Title") {
    search.value.jobTitle = "";
  }
  delete appliedFilters.value[key];
  refreshJobPostList();
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshJobPostList();
});

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  tableRef.value.requestServerInteraction();
  refreshJobPostList();
});
</script>
