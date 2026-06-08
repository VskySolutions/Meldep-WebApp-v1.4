<template>
  <q-page padding>
    <q-card class="reports">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Report Portal" />
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
          <div class="col-12 col-md-5">
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
                    unelevated
                    :color="Object.keys(appliedFilters).length > 0 ? 'primary' : 'grey-7'"
                    text-color="white"
                    class="q-pa-xs q-mr-xs filter-btn"
                    style="height: 40px; border-top-left-radius: 0; border-bottom-left-radius: 0;"
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
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Group Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.reportGroupId"
                            push
                            class="q-mx-sm w-100 h-auto"
                            clearable
                            use-input
                            transition-show="jump-up"
                            transition-hide="jump-up"
                            hide-bottom-space
                            input-debounce="0"
                            :dense="true"
                            :options="reportGroupList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                            @filter="getAllReportGroupListForFilter"
                          >
                            <template #option="{ itemProps, opt }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center selection-drodown-item">
                                    <span>{{ opt.text }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div>
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
              <div class="q-ml-sm">
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_add"
                  label="Add Report"
                  outline
                  no-caps
                  class="text-primary btnRounded q-mr-sm"
                  @click="onAddReport"
                />
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_rule"
                  outline
                  class="text-primary btnRounded hidden"
                  @click="$router.push('/reports/groupRoleAssignmentList')"
                >
                  <q-tooltip>Group Role Assignment</q-tooltip>
                </q-btn>
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_lock"
                  outline
                  class="text-primary btnRounded q-ml-sm hidden"
                  @click="$router.push('/reports/assign-users-to-report')"
                >
                  <q-tooltip>Security</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="q-table-container q-pa-none" style="overflow: hidden !important;">
        <q-table
          ref="tableRef"
          v-model:pagination="pagination"
          :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
          :loading="loading" :rows="rows"
          :columns="columns"
          row-key="id"
          separator="cell"
          no-data-label="No data available"
          binary-state-sort
          :rows-per-page-options="[20, 50, 100, 200, 500]"
          @request="getReports"
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
            <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''" :set="(preReportGroupName = null)">
              <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;">
                <span v-if="preReportGroupName !== props.row.reportGroup.dropDownValue" :set="preReportGroupName = props.row.reportGroup.dropDownValue">{{ props.row.reportGroup.dropDownValue }}</span>
              </q-td>
              <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;">
                {{ props.row.reportName }}
              </q-td>
              <!-- <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;">
                <a
                  :href="props.row.url"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="text-primary"
                >
                  {{ props.row.url }}
                </a>
              </q-td> -->
              <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;" class="RichTextEditor">
                <p v-html="props.row.reportDescription" />
              </q-td>
              <q-td style="width: 5%;" class="text-center actions">
                <a
                  :href="props.row.url"
                  target="_blank"
                  class="text-underline text-dark"
                >
                  <q-icon
                    name="o_bar_chart_4_bars"
                    class="cursor-pointer q-mr-sm"
                    style="display: inline-block;"
                  >
                    <q-tooltip>View</q-tooltip>
                  </q-icon>
                </a>
                <!-- <q-icon
                  v-if="props.row.isEditable || props.row.hasReportGroupAccess && role === 'admin'"
                  name="o_edit"
                  class="cursor-pointer q-mr-sm"
                  size="xs"
                  @click="onEditReport(props.row.id)"
                >
                  <q-tooltip>Edit</q-tooltip>
                </q-icon> -->
                <q-icon
                  v-if="role === 'admin'"
                  name="o_edit"
                  class="cursor-pointer q-mr-sm"
                  size="xs"
                  @click="onEditReport(props.row.id)"
                >
                  <q-tooltip>Edit</q-tooltip>
                </q-icon>
                <!-- <a
                  :href="`/report-list/${props.row.reportId}`"
                  target="_blank"
                  class="text-underline text-dark"
                >
                  <q-icon
                    name="o_bar_chart_4_bars"
                    class="cursor-pointer q-mr-sm"
                    style="display: inline-block;"
                  >
                    <q-tooltip>View</q-tooltip>
                  </q-icon>
                </a> -->
                <q-icon
                  v-if="role === 'admin'"
                  name="o_delete_outline"
                  class="cursor-pointer"
                  color="negative"
                  size="xs"
                  @click="onDeleteReport(props.row)"
                >
                  <q-tooltip>Delete</q-tooltip>
                </q-icon>
              </q-td>
            </q-tr>
            <q-separator />
          </template>
        </q-table>
      </div>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import { useQuasar } from "quasar";
import { setLocalStorage, clearLocalStorage, getLocalStorage, zwConfirmDelete, notifySuccess } from "assets/utils";
import { useAuthStore } from "stores/auth";
import editReport from "modules/reports/components/addEdit.vue";
import commonService from "services/common.service";
import reportService from "modules/reports/reports.service";

// Common variables
const $q = useQuasar();
const authStore = useAuthStore();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

// local storage values
const localStorageKey = "Reports";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const reportIds = filterLocalStorage ? filterLocalStorage.reportIds : [];
const reportGroupId = filterLocalStorage ? filterLocalStorage.reportGroupId : "";
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Search variables
const search = ref({
  searchText,
  reportGroupId,
  reportIds
});

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "reportGroup.dropDownValue", label: "Group  Name", field: "reportGroup.dropDownValue", align: "left", sortable: true },
  { name: "reportName", label: "Report Name", field: "reportName", align: "left", sortable: true },
  // { name: "url", label: "Link", field: "url", align: "left", sortable: true },
  { name: "reportDescription", label: "Report Description", field: "reportDescription", align: "left", sortable: true }
]);

// Get/Map report list to table
const getReports = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    ...search.value
  };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  reportService.getReports(payload).then((resp) => {
    rows.value = resp.data.map(reportSettingsDetails => {
      const hasFullAccess = reportSettingsDetails.reportUserMapping?.[0]?.fullAccess ?? false;
      const hasReportGroupAccess = reportSettingsDetails.reportUserMapping?.length === 0;
      return {
        ...reportSettingsDetails,
        isEditable: role === "admin" || hasFullAccess,
        hasReportGroupAccess
      };
    });

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
  getReports(propps);
};

// Clear search
const onClear = () => {
  search.value.reportIds = [];
  search.value.reportGroupId = "";
  clearLocalStorage(localStorageKey);
  onSearch();
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get All Dropdowns
// --------------------------------------------------------------------------------------------------------------------------------------------------

// Get all report group list for dropdown
const reportGroupList = ref([]);
const reportGroupListFilter = ref([]);
function getAllReportGroupListForDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    reportGroupList.value = responseData;
    reportGroupListFilter.value = responseData;
  });
}

// Search  report group for dropdown
function getAllReportGroupListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      reportGroupList.value = reportGroupListFilter.value;
    } else {
      reportGroupList.value = reportGroupListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all report name list for dropdown
const reportNameList = ref([]);
const reportNameListFilter = ref([]);
function getAllReportListForDropdown () {
  reportService.getAllReportListForDropdown().then((resp) => {
    // Map the response data and then sort it alphabetically by 'text'
    const responseData = resp
      .map((item) => ({ text: item.reportName, value: item.id }))
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

// Create Report popup
const onAddReport = () => {
  $q.dialog({
    component: editReport,
    componentProps: {}
  }).onOk(() => {
    getReports({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Edit Report popup
const onEditReport = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: editReport,
    componentProps: { id }
  }).onOk(() => {
    getReports({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// Delete record
const onDeleteReport = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.reportName}` }, () => {
    reportService.deleteReport(item.id).then(resp => {
      notifySuccess({ message: "Report is deleted successfully." });
      getReports({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
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

const mapSingleFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapSingleFilterToLabel(search.value.reportGroupId, reportGroupList, "Group Name"),
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
  } else if (key === "Group Name") {
    search.value.reportGroupId = "";
  }
  delete appliedFilters.value[key];
  getReports({ pagination: pagination.value });
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getReports({ pagination: pagination.value });
});

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  getAllReportGroupListForDropDown("Report Group");
  getAllReportListForDropdown();
});

</script>
