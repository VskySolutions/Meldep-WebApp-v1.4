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
              <q-breadcrumbs-el label="Infrastructure" />
              <q-breadcrumbs-el label="Account" clickable to="/infra-account" />
              <q-breadcrumbs-el label="Services" />
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
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <multiSelectDropdown
                        v-model="search.itemTypeIds"
                        label="Item Type"
                        :options="itemTypesForDropdown.list.value"
                        :filter="itemTypesForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.infraAccountIds"
                        label="Account"
                        :options="infraAccountsForDropdown.list.value"
                        :filter="infraAccountsForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.ownerShipTypeIds"
                        label="Ownership Type"
                        :options="ownershipTypesForDropdown.list.value"
                        :filter="ownershipTypesForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.paymentTermIds"
                        label="Payment Term"
                        :options="paymentTermsForDropdown.list.value"
                        :filter="paymentTermsForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.projectIds"
                        label="Project Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
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
              <!-- </div> -->
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Add Bulk" no-caps class="text-primary btnRounded" @click="onInfraAccountServicesAddBulk(refreshInfraAccountServicesList)">
                  <q-tooltip>Add Infra Account Services</q-tooltip>
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
        @request="getAllInfraAccountServicesForList"
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
            <q-td style="width:5%;">
              <div v-if="editingRowId !== props.row.id">
                <span
                  class="hoverable-cell"
                  @click="onInfraAccountView(props.row.infraAccount.id)"
                >
                  {{ props.row.infraAccount.customerId + "(" + props.row.infraAccount.provider.dropDownValue + ")" }}
                </span>
              </div>
              <formSingleSelectDropdown
                v-else
                v-model="props.row.infraAccountId"
                :options="infraAccountDropdownSingleSelect.list.value"
                :filter="infraAccountDropdownSingleSelect.filter"
                :error="v$.infraAccountId.$error"
                :error-message="v$.infraAccountId.$errors[0]?.$message"
                @update:model-value="getInfraAccountServicesByInfraAccountId"
              />
            </q-td>
            <q-td style="width:5%;">
              <div v-if="editingRowId !== props.row.id">
                {{ props.row.itemType.dropDownValue }}
              </div>
              <formSingleSelectDropdown
                v-else
                v-model="props.row.itemTypeId"
                :options="itemTypeDropdownSingleSelect.list.value"
                :filter="itemTypeDropdownSingleSelect.filter"
                :error="v$.itemTypeId.$error"
                :error-message="v$.itemTypeId.$errors[0]?.$message"
              />
            </q-td>
            <q-td style="width: 5%;">
              <div v-if="editingRowId !== props.row.id">
                {{ props.row.ownerShipType.dropDownValue }}
              </div>
              <formSingleSelectDropdown
                v-else
                v-model="props.row.ownerShipTypeId"
                :options="ownershipTypeDropdownSingleSelect.list.value"
                :filter="ownershipTypeDropdownSingleSelect.filter"
                :error="v$.ownerShipTypeId.$error"
                :error-message="v$.ownerShipTypeId.$errors[0]?.$message"
              />
            </q-td>
            <q-td style="width:20%;">
              <div v-if="editingRowId !== props.row.id">
                {{ props.row.name }}
              </div>
              <q-input
                v-else
                v-model="props.row.name"
                outlined
                stack-label
                hide-bottom-space
                :error="v$.name.$error"
                :error-message="v$.name.$errors[0]?.$message"
                @blur="v$.name.$touch"
                style="min-width:250px"
              />
            </q-td>
            <q-td style="width: 20%;">
              <div v-if="editingRowId !== props.row.id" class="ellipsis-cell">
                {{ props.row.url }}
              </div>
              <q-input
                v-else
                v-model="props.row.url"
                outlined
                stack-label
                hide-bottom-space
                style="min-width:250px"
              />
            </q-td>
            <q-td style="width: 8%;min-width:150px">
              <div v-if="editingRowId !== props.row.id">
                {{ props.row.startDate }}
              </div>
              <formDate
                v-else
                v-model="props.row.startDateStr"
                :error="v$.startDateStr.$error"
                :error-message="v$.startDateStr.$errors[0]?.$message"
                :onBlur="() => v$.startDateStr.$touch()"
              />
            </q-td>
            <q-td style="width: 5%;">
              <div v-if="editingRowId !== props.row.id">
                {{ props.row.paymentTerm.dropDownValue }}
              </div>
              <formSingleSelectDropdown
                v-else
                v-model="props.row.paymentTermId"
                :options="paymentTermDropdownSingleSelect.list.value"
                :filter="paymentTermDropdownSingleSelect.filter"
                :error="v$.paymentTermId.$error"
                :error-message="v$.paymentTermId.$errors[0]?.$message"
              />
            </q-td>
            <q-td style="width: 6%;" align="right">
              <div v-if="editingRowId !== props.row.id">
                ${{ props.row.priceInDollar }}
              </div>
              <q-input
                v-else
                v-model="props.row.priceInDollar"
                outlined
                stack-label
                hide-bottom-space
                prefix="$"
                input-class="text-right"
                inputmode="decimal"
                class="break-error"
                :error="v$.priceInDollar.$error"
                :error-message="v$.priceInDollar.$errors[0]?.$message"
                @blur="v$.priceInDollar.$touch"
              />
            </q-td>
             <q-td style="width: 5%;" align="right">
              <div>
                ${{ props.row.ytd }}
              </div>
            </q-td>
            <q-td style="width: 10%;">
              <div class="row items-center q-gutter-xs">
                <q-chip
                  v-for="(item, index) in (props.row.infraProjectServices || []).slice(0, 2)"
                  :key="index"
                  dense
                  class="bg-primary text-white"
                >
                  {{ item.project.name }}
                </q-chip>
                <q-chip
                  v-if="props.row.infraProjectServices?.length > 2"
                  dense
                  clickable
                  class="bg-grey-4 text-black"
                  @click="onInfraAccountServicesView(props.row.id, true, refreshInfraAccountServicesList)"
                >
                  +{{ props.row.infraProjectServices.length - 2 }} more...
                </q-chip>
              </div>
            </q-td>
            <q-td style="width: 6%;">
              <div v-if="editingRowId !== props.row.id">
                {{ props.row.infraAccountService.name }}
              </div>
              <formSingleSelectDropdown
                v-else
                v-model="props.row.infraAccountServiceId"
                :required="false"
                :disable="!props.row.infraAccountId"
                :options="infraAccountServiceForDropdownSingleSelect.list.value"
                :filter="infraAccountServiceForDropdownSingleSelect.filter"
              />
            </q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <template v-if="editingRowId === props.row.id">
                <q-icon
                  name="o_cancel"
                  class="cursor-pointer q-mr-sm"
                  size="xs"
                  color="negative"
                  @click="onCancel(props.row)"
                >
                  <q-tooltip>Cancel</q-tooltip>
                </q-icon>
                <q-icon
                  :loading="processing"
                  name="o_save"
                  class="cursor-pointer q-mr-sm hover-white"
                  size="xs"
                  color="primary"
                  @click="onSave(props.row)"
                >
                  <q-tooltip>Save</q-tooltip>
                </q-icon>
              </template>
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="onInfraAccountServicesView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon name="o_add_task" class="cursor-pointer q-mr-sm" size="xs" @click="onInfraAccountServicesView(props.row.id, true, refreshInfraAccountServicesList)">
                <q-tooltip>Assign Project</q-tooltip>
              </q-icon>
              <q-icon
                v-if="editingRowId !== props.row.id"
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="onEdit(props.row)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon
                name="o_note_alt"
                size="xs"
                class="cursor-pointer q-mr-sm"
                @click="() => {
                  activeRowId = props.row.id;
                  activeActionType = 'instruction';
                }"
              >
                <q-tooltip>Add Instructions</q-tooltip>
                <q-popup-edit
                  v-model="props.row.instructions"
                  anchor="top middle"
                  self="bottom middle"
                  buttons
                  persistent
                  label-set="Save"
                  label-cancel="Cancel"
                  class="instruction-popup"
                  @save="val => onSaveInstructions(props.row.id, val)"
                >
                  <template #default="scope">
                    <div class="popup-container q-pa-sm">
                      <q-btn
                        icon="o_close"
                        flat
                        round
                        dense
                        size="sm"
                        class="absolute-top-right"
                        @click="scope.cancel"
                      />
                      <div class="text-subtitle2 q-mb-xs">Instructions</div>
                      <div class="editor-wrapper">
                        <q-editor
                          v-model="scope.value"
                          :dense="$q.screen.lt.md"
                          :toolbar="toolbar"
                          :fonts="fonts"
                          class="fixed-editor"
                        />
                      </div>
                    </div>
                  </template>
                </q-popup-edit>
              </q-icon>
              <WalletPopup
                :row="props.row"
                :wallet-options="infraWalletTypeDropdownSingleSelect.list.value"
                :on-save-api="saveWalletDetails"
              />
              <q-icon
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                size="xs"
                @click="onSubmitInfraAccountServiceDelete(props.row, refreshInfraAccountServicesList)"
              >
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
          <q-separator />
        </template>
        <template #bottom-row>
          <q-tr v-if="rows.length" class="bg-grey-2 text-black">
            <q-td colspan="7" class="text-right text-weight-bold">
              Total Price:
            </q-td>
            <q-td class="text-right text-weight-bold">
              ${{ totalPrice.toFixed(2) }}
            </q-td>
            <q-td />
            <q-td />
            <q-td />
            <q-td />
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
import { useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { isDate } from "validators/zw_validators.js";
import { zwConfirm, notifySuccess, getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

import infraAccountsServicesService from "modules/infra-account-services/infraAccountServices.service";
import WalletPopup from "modules/infra-account/components/_walletPopup.vue";

// Shared Dropdowns
import infraAccountModule from "src/modules/infra-account/utils/dropdowns.js";
import infraAccountServiceModule from "src/modules/infra-account-services/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

import {
  initInfraAccountServicesDialogs,
  onInfraAccountServicesView,
  onInfraAccountServicesAddBulk
} from "src/modules/infra-account-services/utils/dialogs.js";

import {
  initInfraAccountDialogs,
  onInfraAccountView
} from "src/modules/infra-account/utils/dialogs.js";

// SOP Change :- Shared Project Actions
import {
  initInfraAccountServiceActions,
  onSubmitInfraAccountServiceDelete
} from "src/modules/infra-account-services/utils/actions.js";

// Common variables
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);

const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const editingRowId = ref(null);
const editingRow = ref(null);
const processing = ref(false);
const activeActionType = ref(null);

// local storage values
const localStorageKey = "Infra Account Services";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const itemTypeIds = filterLocalStorage ? filterLocalStorage.itemTypeIds : [];
const projectIds = filterLocalStorage ? filterLocalStorage.projectIds : [];
const infraAccountIds = filterLocalStorage ? filterLocalStorage.infraAccountIds : [];
const ownerShipTypeIds = filterLocalStorage ? filterLocalStorage.ownerShipTypeIds : [];
const paymentTermIds = filterLocalStorage ? filterLocalStorage.paymentTermIds : [];
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

const highlightProjectId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightProjectId);
const highlightedId = computed(() => {
  return activeRowId.value;
});

// Search variables
const search = ref({
  searchText,
  itemTypeIds,
  projectIds,
  infraAccountIds,
  ownerShipTypeIds,
  paymentTermIds
});

// Table variables
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "infraAccount.name", label: "Account", field: "infraAccount.name", align: "left", sortable: true },
  { name: "itemType.dropDownValue", label: "Item Type", field: "itemType.dropDownValue", align: "left", sortable: true },
  { name: "ownerShipType.dropDownValue", label: "Ownership Type", field: "ownerShipType.dropDownValue", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "url", label: "URL", field: "url", align: "left", sortable: true },
  { name: "startDate", label: "Start Date", field: "startDate", align: "left", sortable: true },
  { name: "paymentTerm.dropDownValue", label: "Payment Term", field: "paymentTerm.dropDownValue", align: "left", sortable: true },
  { name: "priceInDollar", label: "Price (Dollar)", field: "priceInDollar", align: "right", sortable: true },
  { name: "ytd", label: "Year To Date", field: "ytd", align: "right", sortable: true },
  { name: "infraProjectServices", label: "Projects", field: "infraProjectServices", align: "left", sortable: false },
  { name: "infraAccountServiceId", label: "Infra Account Service", field: "infraAccountServiceId", align: "left", sortable: true }
]);

const handleDocumentClick = (event) => {
  const highlightElement = document.querySelector(".highlight");
  // Check if clicked inside the highlighted row or icons
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;
    const storedData = getLocalStorage(localStorageKey) || {};
    setLocalStorage(localStorageKey, { ...storedData, activeRowId: null });
  }
};

// Get/Map Infra Account list to table
const getAllInfraAccountServicesForList = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
  infraAccountsServicesService.getAllInfraAccountServicesForList(payload).then((resp) => {
    rows.value = resp?.infraAccountServicesList?.map(service => ({
      ...service,
      id: service.id,
      instructions: service.instructions || "",
      infraAccountServiceId: service.infraAccountService.id || "",
      startDateStr: service.startDate,
      infraAccountId: service.infraAccount.id,
      itemTypeId: service.itemType.id,
      ownerShipTypeId: service.ownerShipType.id,
      paymentTermId: service.paymentTerm.id,
      walletTypeId: service.walletType.id,
      isEditing: false
    })) ?? [];
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

function refreshInfraAccountServicesList () {
  getAllInfraAccountServicesForList({ pagination: pagination.value });
}
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const totalPrice = computed(() => {
  return rows.value.reduce((sum, row) => {
    const price = parseFloat(row.priceInDollar) || 0;
    return sum + price;
  }, 0);
});

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getAllInfraAccountServicesForList(propps);
};

// Clear search
const onClear = () => {
  search.value.projectIds = [];
  search.value.itemTypeIds = [];
  search.value.infraAccountIds = [];
  search.value.ownerShipTypeIds = [];
  search.value.paymentTermIds = [];
  clearLocalStorage(localStorageKey);
  onSearch();
};

function onEdit(row) {
  // prevent switching edit rows
  if (editingRowId.value && editingRowId.value !== row.id) {
    zwConfirm({
      title: "Edit in Progress",
      message: "Please finish editing the current row before continuing.",
      okLabel: "OK",
      cancel: false
    }, () => {});
    return;
  }

  editingRowId.value = row.id;
  editingRow.value = { ...row };

  infraAccountServiceForDropdownSingleSelect.load(row.infraAccountId);
  itemTypeDropdownSingleSelect.load("Account Item Type");
  infraAccountDropdownSingleSelect.load();
  ownershipTypeDropdownSingleSelect.load("Ownership Type");
  paymentTermDropdownSingleSelect.load("Payment Term");
}

function onCancel(row) {
  const index = rows.value.findIndex(r => r.id === row.id);

  if (index !== -1 && editingRow.value) {
    // revert changes
    rows.value[index] = { ...editingRow.value };
  }

  editingRowId.value = null;
  editingRow.value = null;
}

function getInfraAccountServicesByInfraAccountId(infraAccountId) {
  const row = rows.value.find(r => r.id === editingRowId.value);

  if (row) {
    row.infraAccountServiceId = "";
  }

  infraAccountServiceForDropdownSingleSelect.load(infraAccountId);
}

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initInfraAccountServicesDialogs(activeRowId);
initInfraAccountDialogs(activeRowId);
initInfraAccountServiceActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { projectNameDropdown } = projectModule();
const {
  infraWalletTypeDropdownSingleSelect,
  itemTypesForDropdown,
  infraAccountsForDropdown,
  ownershipTypesForDropdown,
  paymentTermsForDropdown,
  itemTypeDropdownSingleSelect,
  infraAccountDropdownSingleSelect,
  ownershipTypeDropdownSingleSelect,
  paymentTermDropdownSingleSelect
} = infraAccountModule();
const { infraAccountServiceForDropdownSingleSelect } = infraAccountServiceModule();

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
  ...mapFilterToLabel(search.value.itemTypeIds, itemTypesForDropdown.list, "Item Type"),
  ...mapFilterToLabel(search.value.infraAccountIds, infraAccountsForDropdown.list, "Account"),
  ...mapFilterToLabel(search.value.ownerShipTypeIds, ownershipTypesForDropdown.list, "OwnerShip Type"),
  ...mapFilterToLabel(search.value.paymentTermIds, paymentTermsForDropdown.list, "Payment Term"),
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name")
}));

function getFilterCount (key) {
  switch (key) {
  case "Item Type": return search.value.itemTypeIds?.length || 0;
  case "Account": return search.value.infraAccountIds?.length || 0;
  case "OwnerShip Type": return search.value.ownerShipTypeIds?.length || 0;
  case "Payment Term": return search.value.paymentTermIds?.length || 0;
  case "Project Name": return search.value.projectIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Item Type") {
    search.value.itemTypeIds = [];
  } else if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Account") {
    search.value.infraAccountIds = [];
  } else if (key === "OwnerShip Type") {
    search.value.ownerShipTypeIds = [];
  } else if (key === "Payment Term") {
    search.value.paymentTermIds = [];
  }
  delete appliedFilters.value[key];
  getAllInfraAccountServicesForList({ pagination: pagination.value });
}
// Validate rules
const decimalNumber = helpers.regex(/^\d+(\.\d{1,2})?$/);
const rules = {
  infraAccountId: { required: helpers.withMessage("Account is required", required) },
  itemTypeId: { required: helpers.withMessage("Item type is required", required) },
  ownerShipTypeId: { required: helpers.withMessage("OwnerShip type is required", required) },
  paymentTermId: { required: helpers.withMessage("Payment term is required", required) },
  startDateStr: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  name: { required: helpers.withMessage("Name is required", required) },
  priceInDollar: {
    required: helpers.withMessage("Price is required", required),
    decimalNumber: helpers.withMessage(
      "Enter valid amount (max 2 decimal places)",
      decimalNumber
    )
  }
};
const currentRow = computed(() =>
  rows.value.find(r => r.id === editingRowId.value) || {}
);
const v$ = useVuelidate(rules, currentRow, { $lazy: true, $autoDirty: true });

// =================================================================================
// wallet details
// =================================================================================
function saveWalletDetails (row, payload) {
  return infraAccountsServicesService
    .addOrUpdateInstructions(row.id, payload)
    .then(() => {
      notifySuccess({ message: "Wallet details is saved successfully." });
    });
}

function onSaveInstructions (id, instructions) {
  setTimeout(function () {
    const payload = {
      instructions,
      isInstruction: true
    };

    infraAccountsServicesService
      .addOrUpdateInstructions(id, payload)
      .then(() => {
        notifySuccess({ message: "Instruction is saved successfully." });
      });
  });
}
// =================================================================================

async function onSave (row) {
  v$.value.$touch();

  if (v$.value.$invalid) {
    return;
  }

  try {
    const payload = {
      infraAccountId: row.infraAccountId,
      infraAccountServiceId: row.infraAccountServiceId,
      itemTypeId: row.itemTypeId,
      ownerShipTypeId: row.ownerShipTypeId,
      name: row.name,
      url: row.url,
      startDateStr: row.startDateStr,
      paymentTermId: row.paymentTermId,
      priceInDollar: row.priceInDollar,
      walletTypeId: row.walletTypeId,
      walletNumber: row.walletNumber
    };

    await infraAccountsServicesService.saveInfraAccountServices(row.id, payload);

    notifySuccess({
      message: "Infra Account Service updated successfully."
    });

    editingRowId.value = null;

    getAllInfraAccountServicesForList({ pagination: pagination.value });
  } catch (error) {
    console.error(error);
  }
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getAllInfraAccountServicesForList({ pagination: pagination.value });
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

onMounted(() => {
  infraAccountsForDropdown.load();
  itemTypesForDropdown.load("Account Item Type");
  ownershipTypesForDropdown.load("Ownership Type");
  paymentTermsForDropdown.load("Payment Term");

  projectNameDropdown.load(false, true, true);
  infraWalletTypeDropdownSingleSelect.load("Wallet Type");
  infraAccountServiceForDropdownSingleSelect.load();
  if (tableRef.value) {
    tableRef.value.requestServerInteraction();
  }

  if (!activeRowId.value && highlightProjectId) {
    activeRowId.value = highlightProjectId;
  }

  document.addEventListener("click", handleDocumentClick);
});

</script>
<style>
.ellipsis-cell {
  max-width: 260px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.hover-white:hover {
  color: #fff !important;
}
</style>
