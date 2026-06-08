<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-xs-2 col-sm-1 col-md-2 col-lg-3 col-xl-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Settings" clickable to="/Settings" />
              <q-breadcrumbs-el label="Reports" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-xs-3 col-sm-2 col-md-2 col-lg-2 col-xl-3">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-xs-7 col-sm-9 col-md-8 col-lg-7 col-xl-7">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container" style="position: relative; display: flex; align-items: center; width: 320px;">
                  <q-input
                    v-model="searchText"
                    :loading="searchLoader"
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
                  <q-btn
                    unelevated :color="Object.keys(appliedFilters).length > 0 ? 'primary' : 'grey-7'" text-color="white" size="" class="q-pa-xs q-mr-xs filter-btn" style="height: 40px; border-top-left-radius: 0; border-bottom-left-radius: 0;"
                    @click.stop="showFilter = !showFilter"
                  >
                    <q-badge v-if="Object.keys(appliedFilters).length > 0" color="green" floating>{{ Object.keys(appliedFilters).length }}</q-badge>
                    <q-icon name="o_filter_alt" size="sm" color="white" class="q-mr-xs" />
                    <q-item-label class="text-xs fs-12">
                      <span class="block">Set/Clear</span>
                      <span class="block">FILTER</span>
                    </q-item-label>
                    <q-tooltip anchor="bottom middle" self="top middle">Advanced Filter</q-tooltip>
                  </q-btn>
                  <!-- Dropdown Content -->
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Report Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.reportIds"
                            push
                            class="q-mx-sm w-100 h-auto"
                            clearable
                            use-input
                            use-chips
                            transition-show="jump-up"
                            transition-hide="jump-up"
                            hide-bottom-space
                            multiple
                            fill-input
                            input-debounce="0"
                            :dense="true"
                            :options="reportNameList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            @filter="getAllReportListForFilter"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center selection-drodown-item">
                                    <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                                    <span>{{ opt.text }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
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
              <div class="q-gutter-md">
                <q-btn icon="o_add" outline label="Assign Bulk" no-caps class="text-primary btnRounded q-ml-lg" @click="onAssignBulkUsers">
                  <q-tooltip>Assign Bulk</q-tooltip>
                </q-btn>
                <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-md" @click="$router.back()" />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
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
        @request="getReportUsers"
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
            <q-td style="width: 20%;">
              {{ props.row.reportName }}
            </q-td>
            <q-td
              style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 8%;"
            >
              <div v-if="props.row.reportUsers?.length" class="col-9 flex TaskActivity cursor-grab">
                <div v-for="(user, index) in props.row.reportUsers" :key="index" class="Person q-mr-xs">
                  <span>{{ getInitials(user.text || user.name) }}</span>
                  <q-tooltip>
                    <div>
                      <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                      <span>{{ user.text }}</span>
                    </div>
                    <div v-if="user.fullAccess">
                      <q-icon name="o_edit" color="white" size="xs" class="q-mr-xs" />
                      <span><q-icon name="o_done" color="white" size="xs" class="q-mr-xs" /></span>
                    </div>
                    <div v-if="user.viewOnly">
                      <q-icon name="o_visibility" color="white" size="xs" class="q-mr-xs" />
                      <span><q-icon name="o_done" color="white" size="xs" class="q-mr-xs" /></span>
                    </div>
                  </q-tooltip>
                </div>
              </div>
            </q-td>
            <q-td style="width: 3%;" class="text-center actions">
              <div class="relative-position inline-block">
                <q-icon
                  name="o_assignment_ind"
                  class="cursor-pointer"
                  size="xs"
                  @click="onAssignUser(props.row)"
                >
                  <q-tooltip>Assign Users</q-tooltip>
                </q-icon>

                <q-badge
                  v-if="Array.isArray(props.row.reportUsers) && props.row.reportUsers.length"
                  color="green"
                  floating
                >
                  {{ props.row.reportUsers.length }}
                </q-badge>
              </div>
            </q-td>
          </q-tr>
          <q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onMounted, watch, computed } from "vue";
import { useQuasar } from "quasar";
import { setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";

import assignBulkUsers from "modules/reports/components/assignBulkUsers.vue";
import assignUserToReport from "modules/reports/components/assignUsers.vue";

import reportService from "modules/reports/reports.service";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
// local storage values
const localStorageKey = "Reports";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const reportIds = filterLocalStorage ? filterLocalStorage.reportIds : [];
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Search variables
const search = ref({
  searchText,
  reportIds
});

// Table variables
const tableRef = ref();
const $q = useQuasar();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "reportName", label: "Report Name", field: "reportName", align: "left", sortable: true },
  { name: "reportName", label: "Assign To", field: "reportName", align: "left", sortable: true }
]);

const getReportUsers = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  // Check if there are any active filters
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
  reportService.getReportUsers(payload).then((resp) => {
    rows.value = resp.data.map(reportSettingsDetails => ({
      ...reportSettingsDetails,
      reportUsers: reportSettingsDetails.reportUserMapping
        ? reportSettingsDetails.reportUserMapping
          .map(mapping => {
            const id = mapping.user?.id;
            const name = mapping.user?.person?.fullName;
            const fullAccess = mapping.fullAccess;
            const viewOnly = mapping.viewOnly;
            return {
              value: String(id),
              text: name || "Unknown",
              fullAccess,
              viewOnly
            };
          })
        : []
    }));
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

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getReportUsers(propps);
};

// Clear search
const onClear = () => {
  search.value.reportIds = [];
  clearLocalStorage(localStorageKey);
  onSearch();
};
function getInitials (fullName) {
  return fullName
    .split(" ")
    .map(word => word[0])
    .join("")
    .toUpperCase();
}

// Get all report name list for dropdown
const reportNameList = ref([]);
const reportNameListFilter = ref([]);
function getAllReportListForDropdown () {
  reportService.getAllReportListForDropdown().then((resp) => {
    // Map the response data and then sort it alphabetically by 'text'
    const responseData = resp
      .map((item) => ({ text: item.reportName, value: item.reportId }))
      .sort((a, b) => a.text.localeCompare(b.text)); // Sort alphabetically by project name

    reportNameList.value = responseData;
    reportNameListFilter.value = responseData;
  });
}

// Search report name for dropdown
function getAllReportListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      reportNameList.value = reportNameListFilter.value;
    } else {
      reportNameList.value = reportNameListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Assign popup
const onAssignUser = (row) => {
  activeRowId.value = row.id;
  $q.dialog({
    component: assignUserToReport,
    componentProps: { id: row.id, reportName: row.reportName }
  }).onOk(() => {
    getReportUsers({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = row.id;
  });
};

// Assign Bulk users popup
const onAssignBulkUsers = () => {
  $q.dialog({
    component: assignBulkUsers,
    componentProps: {}
  }).onOk(() => {
    getReportUsers({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

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
  ...mapFilterToLabel(search.value.reportIds, reportNameList, "Report Name")
}));

function getFilterCount (key) {
  switch (key) {
  case "Report Name": return search.value.reportIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Report Name") {
    search.value.reportIds = [];
  }
  delete appliedFilters.value[key];
  getReportUsers({ pagination: pagination.value });
}

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  getAllReportListForDropdown();
});

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getReportUsers({ pagination: pagination.value });
});
</script>
<style scoped>
.TaskActivity{
  padding-right: 10px;
}
.TaskActivity .Person {
    border-radius: 50%;
    background-color: #5d5d5d;
    color: white;
    font-size: 12px;
    font-weight: 600;
    padding: 2px 3px;
    margin-right: 3px;
    transition: 0.5s all ease-in-out;
}
.cursor-grab{
  cursor: grab;
  background: none !important;
}
</style>
