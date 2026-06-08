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
              <q-breadcrumbs-el label="Reports" clickable to="/reports" />
              <q-breadcrumbs-el label="Report Group Role Assignment" />
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
          <q-separator />
          <div class="col-12 col-md-5">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container" style="position: relative; display: flex; align-items: center; width: 320px;">
                  <q-input v-model="searchText" :loading="searchLoader" outlined dense clearable debounce="300" placeholder="Search" class="bg-white search-box" style="flex: 1; border-top-right-radius: 0; border-bottom-right-radius: 0; border-top-right-radius: 0; max-width: 250px;">
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
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Role Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.siteRoleIds"
                            push
                            class="q-mx-sm w-100 h-auto"
                            clearable
                            use-input
                            use-chips
                            hide-bottom-space
                            :dense="true"
                            multiple
                            input-debounce="0"
                            :options="siteRolesList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            @filter="getAllSitesRoleListForFilter"
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
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Report Group Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.reportGroupIds"
                            push
                            class="q-mx-sm w-100 h-auto"
                            clearable
                            use-input
                            use-chips
                            transition-show="jump-up"
                            transition-hide="jump-up"
                            hide-bottom-space
                            :dense="true"
                            multiple
                            fill-input
                            input-debounce="0"
                            :options="reportGroupList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                            @filter="getAllReportGroupListForFilter"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center selection-drodown-item">
                                    <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                                    <div style="flex: 1; word-break: break-word;">{{ opt.text }}</div>
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
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Add Role To Group " no-caps class="text-primary btnRounded" @click="onAdd" />
                <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-sm" @click="$router.back()" />
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
        @request="getAllReportGroupRoles"
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
            <q-td>
              <span v-if="props.row.showRoleName">
                {{ props.row.sitesRoles.applicationRole.name }}
              </span></q-td>
            <q-td>{{ props.row.reportGroup.dropDownValue }}</q-td>
            <q-td auto-width class="text-center actions">
              <q-icon
                :name="props.row.active ? 'o_check_circle' : 'o_block'"
                :color="props.row.active ? 'positive' : 'negative'"
                class="q-mr-xs hoverable-cell"
                @click="toggleActiveStatus(props.row)"
              >
                <q-tooltip>{{ props.row.active ? 'Set Inactive?' : 'Set Active?' }}</q-tooltip>
              </q-icon>
              <q-icon
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                @click="onDelete(props.row)">
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
import { useQuasar } from "quasar";
import { ref, onMounted, watch, computed } from "vue";
import { zwConfirmDelete, notifySuccess, setLocalStorage, getLocalStorage, clearLocalStorage } from "assets/utils";

import commonService from "services/common.service";
import sitesService from "modules/sites/site.service";
import reportService from "modules/reports/reports.service";

import assignGroupRole from "modules/reports/components/assignGroupRole.vue";

// ----------------------------
// Common variables
// ----------------------------
const $q = useQuasar();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);

// ----------------------------
// local storage values
// ----------------------------
const localStorageKey = "Report_Role_Group";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const siteRoleIds = filterLocalStorage ? filterLocalStorage.siteRoleIds : [];
const reportGroupIds = filterLocalStorage ? filterLocalStorage.reportGroupIds : [];
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// ----------------------------
// Search variables
// ----------------------------
const search = ref({
  searchText,
  siteRoleIds,
  reportGroupIds
});

// ----------------------------
// Table variables
// ----------------------------
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "siteRoleId", label: "Role Name", field: "siteRoleId", align: "left", sortable: false },
  { name: "reportGroup.dropDownValue", label: "Report Group Name", field: "reportGroup.dropDownValue", align: "left", sortable: true }
]);

const getAllReportGroupRoles = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  reportService.getAllReportGroupRoles(payload).then((resp) => {
    let lastRoleName = null;
    const Rows = resp.data
      .sort((a, b) => a.sitesRoles?.applicationRole?.name?.localeCompare(b.sitesRoles?.applicationRole?.name || ""))
      .map(row => {
        const showRoleName = row.sitesRoles?.applicationRole?.name !== "" &&
        row.sitesRoles?.applicationRole?.name !== lastRoleName;
        lastRoleName = row.sitesRoles?.applicationRole?.name;

        return {
          ...row,
          showRoleName
        };
      });
    rows.value = Rows;
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

// ----------------------------
// Get All Dropdowns
// ----------------------------

// Get all site role list for dropdown
const siteRolesList = ref([]);
const siteRolesListFilter = ref([]);
function getAllSitesRoleListForDropdown () {
  sitesService.getAllSitesRoleListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.applicationRole.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    siteRolesList.value = responseData;
    siteRolesListFilter.value = responseData;
  });
}

// Search site role for dropdown
function getAllSitesRoleListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      siteRolesList.value = siteRolesListFilter.value;
    } else {
      siteRolesList.value = siteRolesListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

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

// ----------------------------
// Add role group
// ----------------------------
const onAdd = () => {
  $q.dialog({
    component: assignGroupRole,
    componentProps: {}
  }).onOk(() => {
    getAllReportGroupRoles({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// ----------------------------
// Delete record
// ----------------------------
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.sitesRoles.applicationRole.name}, ${item.reportGroup.dropDownValue}` }, () => {
    reportService.deleteGroupRole(item.id).then(resp => {
      notifySuccess({ message: "Report group role is deleted successfully." });
      getAllReportGroupRoles({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};

// ----------------------------
// Toggle Active/Inactive Status
// ----------------------------
const toggleActiveStatus = (row) => {
  const isCurrentlyActive = row.active === true;
  const newStatus = !isCurrentlyActive; // Toggle the status
  reportService.updateReportGroupsRoleStatus(row.id, newStatus).then(() => {
    notifySuccess({ message: "Report group role status is saved successfully.." });
    row.active = newStatus;
    getAllReportGroupRoles({ pagination: pagination.value });
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
  ...mapFilterToLabel(search.value.siteRoleIds, siteRolesList, "Role Name"),
  ...mapFilterToLabel(search.value.reportGroupIds, reportGroupList, "Report Group Name")
}));

// ----------------------------
// Search records as per parameters
// ----------------------------

const onSearch = () => {
  const props = { pagination: pagination.value };
  getAllReportGroupRoles(props);
};

// ----------------------------
// Clear search
// ----------------------------
const onClear = () => {
  search.value.siteRoleIds = [];
  search.value.reportGroupIds = [];
  clearLocalStorage(localStorageKey);
  onSearch();
};

function onClearFilters (key) {
  if (key === "Role Name") {
    search.value.siteRoleIds = [];
  } else if (key === "Report Group Name") {
    search.value.reportGroupIds = [];
  }
  delete appliedFilters.value[key];
  getAllReportGroupRoles({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  case "Role Name": return search.value.siteRoleIds?.length || 0;
  case "Report Group Name": return search.value.reportGroupIds?.length || 0;
  default: return null;
  }
}
// ----------------------------
// On page rendering
// ----------------------------
onMounted(() => {
  tableRef.value.requestServerInteraction();
  getAllSitesRoleListForDropdown();
  getAllReportGroupListForDropDown("Report Group");
});

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getAllReportGroupRoles({ pagination: pagination.value });
});

</script>
