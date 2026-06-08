<template>
  <q-page padding>
    <q-card class="q-pa-sm">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="CRM" />
              <q-breadcrumbs-el label="Customers" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs">
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
                  <!-- Dropdown Content -->
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <multiSelectDropdown
                        v-model="search.customerTypeIds"
                        label="Customer Type"
                        :options="customerTypesDropdown.list.value"
                        :filter="customerTypesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.customerIds"
                        label="Customer"
                        :options="customerNameDropdown.list.value"
                        :filter="customerNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.parentCustomerIds"
                        label="Parent Customer"
                        :options="parentCustomerDropdown.list.value"
                        :filter="parentCustomerDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Email Address</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.emailAddress"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="email"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Phone No</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.phoneNumber"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="text"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.employeeIds"
                        label="Primary Employee Person"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
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
              <q-menu v-model="showManageDropdownOptions" anchor="bottom right" self="top right" no-parent-event style="width: 320px;">
                <q-card class="q-pa-sm">
                  <div class="text-subtitle2 q-mt-sm q-mb-sm">Manage Dropdown Options</div>
                  <q-separator />
                  <q-list style="min-width: 200px">
                    <q-item
                      v-for="opt in dropdownTypes"
                      :key="opt.id"
                      clickable
                      :active="selectedField === opt.id"
                      active-class="bg-primary text-white"
                      @click="$router.push({ path: '/manage-dropdowns', state: { id: opt.id, groupName: opt.groupName, moduleName: opt.moduleName } })"
                    >
                      <q-item-section>{{ opt.type }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-card>
              </q-menu>
              <div class="q-ml-xs">
                <q-btn
                  icon="o_add"
                  outline
                  label="Add Customer"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onCustomerAdd(refreshCustomerNameDropdown, refreshCustomerList)"
                />
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_playlist_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-sm" @click="showManageDropdownOptions = !showManageDropdownOptions"
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
        flat
        :loading="loading"
        :rows="rows"
        :columns="columns"
        row-key="id"
        separator="cell"
        no-data-label="No data available"
        :filter="filter"
        binary-state-sort
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        @request="getCustomers"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th auto-width class="text-center hidden" />
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="highlightedId == props.row.id ? 'highlight' : ''">
            <q-td auto-width class="text-center hidden">
              <q-icon :name="isExpanded(props.row.id) ? '-' : '+'" class="cursor-pointer custom-plus-minus-icon" @click="toggleExpand(props.row.id)">
                <q-tooltip>{{ isExpanded(props.row.id) ? 'Collapse' : 'Expand' }}</q-tooltip>
              </q-icon>
            </q-td>
            <q-td style="width: 20%; white-space: normal;" class="hoverable-cell">
              <div class="row no-wrap items-center justify-between">
                <span style="flex: 1; word-break: break-word; white-space: normal;" @click="onCustomerView(props.row.id)">
                  {{ props.row.name }}
                </span>
                <q-icon
                  name="o_radio_button_checked" size="xs"
                  class="cursor-pointer q-ml-sm"
                  @click="setActiveRowIdInLocalStorage(props.row.id); $router.push({ path: 'customer/customer-center', state: {customerId: props.row.id, companyId: props.row.companyId } })"
                >
                  <q-tooltip>Customer Center</q-tooltip>
                </q-icon>
              </div>
            </q-td>
            <q-td>
              {{ props.row.parentCustomerName }}
            </q-td>
            <q-td>
              {{ props.row.emailAddress }}
            </q-td>
            <q-td>
              {{ props.row.phoneNumber }}
            </q-td>
            <q-td>
              {{ props.row.customerType }}
            </q-td>
            <q-td
              class="common-q-td"
              :class="{ 'hoverable-cell' : activeEdit.rowId === props.row.id }"
              @click="activeEdit = { rowId: props.row.id, field: 'assignedToName' }"
              style="width: 5%;"
            >
            <div class="row items-center justify-between">
              <div class="col">
                <quickEditSingleSelect
                  field="assignedToName"
                  :row-id="props.row.id"
                  :value="props.row.assignedToId"
                  :display-value="getNameFromId(props.row.assignedToId)"
                  :editable="activeEdit.rowId === props.row.id"
                  :options="activeEmployeesDropdown.list.value"
                  :active-edit="activeEdit"
                  :show-history="false"
                  @cancel="activeEdit = { rowId: null, field: null }"
                  @submit="({ rowId, value }) => onSubmitCustomerAdvocate(rowId, value, refreshCustomerList)"
                />
              </div>
              <div v-if="props.row.assignedToId" class="col-auto">
                <q-icon
                  name="o_history"
                  class="cursor-pointer q-ml-sm"
                  size="xs"
                  @click.stop="onSiteModifiedLog(props.row.id, props.row.name, 'Customer Advocate')"
                >
                  <q-tooltip>Data Change Log</q-tooltip>
                </q-icon>
              </div>
            </div>
            </q-td>
            <q-td class="" style="width: 5%;">
              {{ props.row.assignedDate }}
            </q-td>
            <q-td auto-width class="text-center actions">
              <q-icon
                name="o_visibility"
                class="cursor-pointer q-mr-sm"
                @click="onCustomerView(props.row.id)"
              >
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                @click="onCustomerEdit(props.row.id, props.row.customerTypeId, props.row.personId, props.row.companyId, refreshCustomerList)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <a
                style="position: relative;"
                class="q-icon notranslate cursor-pointer q-ml-sm q-mr-md"
                @click="onNoteAdd(props.row.id, 'customer', props.row.id, props.row.name, props.row.name, refreshCustomerList)"
              >
                <q-tooltip anchor="bottom middle" self="top middle">
                  Note
                </q-tooltip>
                <q-icon name="o_assignment" />
                <q-badge
                  v-if="props.row.customerNoteCount > 0"
                  style="position: absolute; right: -16px; top: -15px;"
                  color="green"
                  text-color="white"
                  :label="props.row.customerNoteCount"
                />
              </a>
              <q-icon
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                @click="onSubmitCustomerDelete(props.row.id, props.row.name, refreshCustomerList)"
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

import { ref, onMounted, computed, watch, onBeforeUnmount } from "vue";
import { useAuthStore } from "stores/auth";

import customerService from "modules/customer/customer.service";
import manageDropdownsService from "modules/dropdown/dropdown.service";

// Shared DataTable Views
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// Shared Dropdowns
import customerModule from "src/modules/customer/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Customer Dialogs
import {
  initCustomerDialogs,
  onCustomerView,
  onCustomerAdd,
  onCustomerEdit
} from "src/modules/customer/utils/dialogs.js";

// SOP Change :- Shared Site Dialogs
import {
  initSiteDialogs,
  onSiteModifiedLog
} from "src/modules/sites/utils/dialogs.js";

// Shared Notes Dialogs
import {
  initCommonDialogs,
  onNoteAdd
} from "src/modules/common/utils/dialogs.js";

// Shared Customer Actions
import {
  initCustomerActions,
  onSubmitCustomerAdvocate,
  onSubmitCustomerDelete
} from "src/modules/customer/utils/actions.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const authStore = useAuthStore();
const loading = ref(true);
const rows = ref([]);
const filter = ref("");
const expandedRows = ref([]);
const searchLoader = ref(false);
const showFilter = ref(false);
const activeEdit = ref({ rowId: null, field: null });
const dropdownTypes = ref([]);
const showManageDropdownOptions = ref(false);
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const columns = ref([
  { name: "name", label: "Customer Name", field: "name", align: "left", sortable: true },
  { name: "parentCustomerName", label: "Parent Customer", field: "parentCustomerName", align: "left", sortable: true },
  { name: "emailAddress", label: "Email", field: "emailAddress", align: "left", sortable: true },
  { name: "phoneNumber", label: "Phone No", field: "phoneNumber", align: "left", sortable: true },
  { name: "customerType", label: "Customer Type", field: "CustomerType", align: "left", sortable: true },
  { name: "assignedToId", label: "Customer Advocate", field: "assignedToId", align: "left", sortable: true },
  { name: "assignedDate", label: "Advocate Assigned Date", field: "assignedDate", align: "center", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const highlightedId = computed(() => {
  return activeRowId.value;
});
const siteId = computed(() => authStore.user?.siteId);

const {
  search,
  pagination,
  activeRowId,
  saveDataTableState
} = useSiteTableState({
  storageKey: "customer-Index",
  siteId,

  defaultSearch: {
    searchText: "",
    customerIds: [],
    employeeIds: [],
    customerTypeIds: [],
    phoneNumber: "",
    emailAddress: "",
    parentCustomerIds: []
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

function getNameFromId (value) {
  if (value) {
    const found = activeEmployeesDropdown.list.value.find(item => item.value === value);
    return found?.text || "";
  }
}

function getDropdownTypeByModuleName (moduleName) {
  manageDropdownsService.getDropdownTypeByModuleName(moduleName).then((resp) => {
    dropdownTypes.value = resp;
  });
}

function setActiveRowIdInLocalStorage(id) {
  activeRowId.value = id;

  saveDataTableState({
    activeRowId: id
  });
}

const handleDocumentClick = (event) => {
  if (event.target.closest(".q-dialog")) {
    return;
  }

  const highlightElement = document.querySelector(".highlight");

  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;

    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: null
    });
  }
};

// Get all project status list
const toggleExpand = (rowId) => {
  if (expandedRows.value.includes(rowId)) {
    expandedRows.value = expandedRows.value.filter(id => id !== rowId);
  } else {
    expandedRows.value.push(rowId);
  }
};

const isExpanded = (rowId) => {
  return expandedRows.value.includes(rowId);
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Customers
// ----------------------------------------------------------------------------------------------------------------

const getCustomers = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  saveDataTableState({
    search: search.value,
    pagination: props.pagination,
    activeRowId: activeRowId.value
  });
  customerService.getCustomers(payload).then((resp) => {
    rows.value = resp.data || [];
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
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

const refreshCustomerList = () => {
  getCustomers({ pagination: pagination.value });
};

const refreshCustomerNameDropdown = () => {
  customerNameDropdown.load();
};

const onAdvanceSearch = () => {
  refreshCustomerList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.customerIds = [];
  search.value.employeeIds = [];
  search.value.customerTypeIds = [];
  search.value.parentCustomerIds = [];
  search.value.emailAddress = "";
  search.value.phoneNumber = "";

  saveDataTableState({
    search: search.value
  });
  onAdvanceSearch();
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------

const {
  customerTypesDropdown,
  customerNameDropdown,
  parentCustomerDropdown
} = customerModule();

const { activeEmployeesDropdown } = employeeModule();

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------

initCustomerDialogs(activeRowId);
initCommonDialogs(activeRowId);
initSiteDialogs(activeRowId);
initCustomerActions(activeRowId);

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
  ...mapFilterToLabel(search.value.customerTypeIds, customerTypesDropdown.list, "Customer Type"),
  ...mapFilterToLabel(search.value.customerIds, customerNameDropdown.list, "Customer"),
  ...mapFilterToLabel(search.value.parentCustomerIds, parentCustomerDropdown.list, "Parent Customer"),
  ...(search.value.emailAddress ? { "Email Address": search.value.emailAddress } : {}),
  ...(search.value.phoneNumber ? { "Phone No": search.value.phoneNumber } : {}),
  ...mapFilterToLabel(search.value.employeeIds, activeEmployeesDropdown.list, "Primary Employee Person")
}));

function onClearFilters (key) {
  if (key === "Customer Type") {
    search.value.customerTypeIds = [];
  } else if (key === "Customer") {
    search.value.customerIds = [];
  } else if (key === "Parent Customer") {
    search.value.parentCustomerIds = [];
  } else if (key === "Email Address") {
    search.value.emailAddress = "";
  } else if (key === "Phone No") {
    search.value.phoneNumber = "";
  } else if (key === "Primary Employee Person") {
    search.value.employeeIds = [];
  }
  delete appliedFilters.value[key];
  refreshCustomerList();
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshCustomerList();
});

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

onMounted(() => {
  tableRef.value.requestServerInteraction();
  customerTypesDropdown.load("Customer Type");
  activeEmployeesDropdown.load(user.siteId);
  customerNameDropdown.load();
  parentCustomerDropdown.load();
  activeEmployeesDropdown.load();
  getDropdownTypeByModuleName("CRM");
  document.addEventListener("click", handleDocumentClick);
});
</script>
