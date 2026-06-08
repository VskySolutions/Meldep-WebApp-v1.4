<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-sm-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Infrastructure" />
              <q-breadcrumbs-el label="Accounts" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-sm-5">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-sm-5">
            <div class="row items-center justify-end no-wrap">
              <div class="search-container position-relative">
                <searchFilterBar
                  v-model="search.searchText"
                  :loading="searchLoader"
                  :applied-filters="appliedFilters"
                  @toggle-filter="showFilter = !showFilter"
                />
                <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                  <q-card class="q-pa-sm">
                    <multiSelectDropdown
                      v-model="search.providerIds"
                      label="Provider Name"
                      :options="providerTypesForDropdown.list.value"
                      :filter="providerTypesForDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.infraAccountIds"
                      label="Customer Id"
                      :options="infraAccountsForDropdown.list.value"
                      :filter="infraAccountsForDropdown.filter"
                    />
                    <div class="row items-center q-mb-sm">
                      <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                        <label class="Cutomlabel q-mt-sm fs-13">CC Last 4 Digits</label>
                      </div>
                      <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                        <q-input v-model="search.ccLast4Digits" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
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
              <manageDropdownOptions
                v-model="showManageDropdownOptions"
                :manage-drop-down-types="manageDropDownTypes"
                :selected-field="selectedField"
              />
              <div>
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_playlist_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="showManageDropdownOptions = !showManageDropdownOptions"
                >
                  <q-tooltip>Manage Dropdowns</q-tooltip>
                </q-btn>
                <q-btn icon="o_add" outline label="Add Infra Account" no-caps class="text-primary q-ml-sm btnRounded" @click="onInfraAccountAdd(refreshInfraAccountList)" />
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
        @request="getAllInfraAccount"
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
            <q-td style="width: 11%;">{{ props.row.provider.dropDownValue }}</q-td>
            <q-td style="width: 31%;">{{ props.row.name }}</q-td>
            <q-td style="width: 11%;">{{ props.row.customerId }}</q-td>
            <q-td style="width: 32%;">{{ props.row.url }}</q-td>
            <q-td class="text-right" style="width: 5%;">${{ props.row.totalServicesCost }}</q-td>
            <q-td style="width: 5%;" align="right">{{ props.row.ccLast4Digits }}</q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="onInfraAccountView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon name="o_edit" class="cursor-pointer q-mr-sm" size="xs" @click="onInfraAccountEdit(props.row.id, refreshInfraAccountList)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onDelete(props.row)">
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
          <q-separator />
        </template>
        <template #bottom-row>
          <q-tr v-if="rows.length" class="bg-grey-2 text-black">
            <q-td colspan="4" class="text-right text-weight-bold">
              Total Price:
            </q-td>
            <q-td class="text-right text-weight-bold">
              ${{ totalPrice.toFixed(2) }}
            </q-td>
            <q-td />
            <q-td />
          </q-tr>
        <q-separator></q-separator>
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed, onBeforeUnmount } from "vue";
import { useAuthStore } from "stores/auth";
import { zwConfirmDelete, zwConfirm, notifySuccess } from "assets/utils";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import manageDropdownOptions from "src/components/dataTable/_manageDropdownOptions.vue";

import infraAccountsService from "modules/infra-account/infraAccount.service";
import manageDropdownsService from "modules/dropdown/dropdown.service";
import infraAccountModule from "src/modules/infra-account/utils/dropdowns.js";
import {
  initInfraAccountDialogs,
  onInfraAccountView,
  onInfraAccountAdd,
  onInfraAccountEdit
} from "src/modules/infra-account/utils/dialogs.js";

// SOP Change :- Shared Scripts DataTable Features
import useSiteTableState from "composables/datatable/useSiteTableState.js";

// -----------------------------------------------------------------------------
// User Role
// -----------------------------------------------------------------------------
const authStore = useAuthStore();
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = computed(() =>
  authStore.user?.roles?.some(role => adminRoles.includes(role))
    ? "admin"
    : ""
);

const siteId = computed(() => authStore.user?.siteId);

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const showManageDropdownOptions = ref(false);
const manageDropDownTypes = ref([]);

const highlightedId = computed(() => activeRowId.value);

// Table variables
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "provider.dropDownValue", label: "Provider Name", field: "provider.dropDownValue", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "customerId", label: "Customer Id", field: "customerId", align: "left", sortable: true },
  { name: "url", label: "URL", field: "url", align: "left", sortable: true },
  { name: "totalServicesCost", label: "Total Services Cost", field: "totalServicesCost", align: "right", sortable: true },
  { name: "ccLast4Digits", label: "CC Last 4 Digits", field: "ccLast4Digits", align: "right", sortable: true }
]);

const defaultSearch = {
  searchText: "",
  providerIds: [],
  infraAccountIds: [],
  ccLast4Digits: ""
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
  saveDataTableState
} = useSiteTableState({
  storageKey: "infra-Accounts-Index",
  siteId,

  defaultSearch,
  defaultPagination
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

// Get/Map Infra Account list to table
const getAllInfraAccount = async ({ pagination: pageData }) => {
  try {
    loading.value = true;

    const { page, rowsPerPage, sortBy, descending } = pageData;

    const payload = {
      page,
      pageSize: rowsPerPage,
      sortBy,
      descending,
      ...search.value
    };

    const resp = await infraAccountsService.getAllInfraAccount(payload);

    rows.value = resp?.infraAccountsList || [];

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
      activeRowId: activeRowId.value
    });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

function getDropdownTypeByModuleName (moduleName) {
  manageDropdownsService.getDropdownTypeByModuleName(moduleName).then((resp) => {
    manageDropDownTypes.value = resp;
  });
}

// Search records as per parameters
const onSearch = () => {
  refreshInfraAccountList();
};

const getFilterValue = (data) => JSON.parse(JSON.stringify(data));

const onClear = () => {
  search.value = getFilterValue(defaultSearch);
  pagination.value = getFilterValue(defaultPagination);
  activeRowId.value = null;

  saveDataTableState({
    search: {
      ...defaultSearch
    }
  });
  onSearch();
};

const totalPrice = computed(() => {
  return rows.value.reduce((sum, row) => {
    const price = parseFloat(row.totalServicesCost) || 0;
    return sum + price;
  }, 0);
});

const onDelete = async (item) => {
  try {
    activeRowId.value = item.id;
    const resp = await infraAccountsService.checkAccountCanBeDeleted(item.id);
    const canDelete = resp?.canDelete;
    if (canDelete) {
      // Normal delete confirmation
      zwConfirmDelete({ data: `${item.name}, ${item.customerId}` }, () => {
        infraAccountsService.deleteInfraAccount(item.id).then(resp => {
          notifySuccess({ message: "Infra Account is deleted successfully." });
          refreshInfraAccountList();
          infraAccountsForDropdown.load();
        });
      }, () => {
        activeRowId.value = null;
      });
    } else {
      // Warning confirmation
      zwConfirm({
        title: "Active Services Found",
        message: "This account has active services. You cannot delete it.",
        data: `${item.customerId}`,
        okLabel: "OK",
        cancel: false
      }, () => {
      });
    }
  } catch (error) {
    console.error("Error checking module:", error);
  } finally {
    activeRowId.value = null;
  }
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initInfraAccountDialogs(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { providerTypesForDropdown, infraAccountsForDropdown } = infraAccountModule();

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const refreshInfraAccountList = () => {
  getAllInfraAccount({ pagination: pagination.value });
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
  ...mapFilterToLabel(search.value.providerIds, providerTypesForDropdown.list, "Provider Name"),
  ...mapFilterToLabel(search.value.infraAccountIds, infraAccountsForDropdown.list, "Customer Id"),
  ...(search.value.ccLast4Digits ? { "CC Last 4 Digits": search.value.ccLast4Digits } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Provider Name": return search.value.providerIds?.length || 0;
  case "Customer Id": return search.value.infraAccountIds?.length || 0;
  default: return null;
  }
}

const filterMap = {
  "Provider Name": "providerIds",
  "Customer Id": "infraAccountIds",
  "CC Last 4 Digits": "ccLast4Digits"
};

const onClearFilters = (key) => {
  const field = filterMap[key];

  if (!field) return;

  search.value[field] = Array.isArray(search.value[field]) ? [] : "";

  onSearch();
};

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(
  () => search.value.searchText,
  () => {
    searchLoader.value = true;
    refreshInfraAccountList();
  }
);

onMounted(() => {
  providerTypesForDropdown.load("Account Provider Type");
  infraAccountsForDropdown.load();

  getDropdownTypeByModuleName("Infra-Account");

  document.addEventListener("click", handleDocumentClick);

  refreshInfraAccountList();
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

</script>
