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
              <q-breadcrumbs-el label="Infrastructure" />
              <q-breadcrumbs-el label="FTP" />
            </q-breadcrumbs>
          </div>
          <div class="col-xxl-4 col-xl-4 col-lg-4 col-md-5 col-sm-6 col-xs-12">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }}
                <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" />
                <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-xxl-6 col-xl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12">
            <div class="row items-center justify-end no-wrap">
              <div class="search-container position-relative">
                <searchFilterBar
                  v-model="searchText"
                  :loading="searchLoader"
                  :applied-filters="appliedFilters"
                  @toggle-filter="showFilter = !showFilter"
                />
                <!-- Dropdown Content -->
                <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                  <q-card class="q-pa-sm">
                    <multiSelectDropdown
                      v-model="search.infraServiceIds"
                      label="Infra Services"
                      :options="infraAccountServicesForDropdown.list.value"
                      :filter="infraAccountServicesForDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.protocolTypeIds"
                      label="Protocol Type"
                      :options="protocolTypesForDropdown.list.value"
                      :filter="protocolTypesForDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.encryptionTypeIds"
                      label="Encryption Type"
                      :options="encryptionTypesForDropdown.list.value"
                      :filter="encryptionTypesForDropdown.filter"
                    />
                    <div class="row items-center q-mb-sm">
                      <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                        <label class="Cutomlabel q-mt-sm fs-13">Name</label>
                      </div>
                      <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                        <q-input v-model="search.name" class="q-mx-sm w-100 h-auto" fill-input :dense="true" />
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
              <div>
                <q-btn
                  icon="o_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
                  @click="onAddInfraFTP"
                >
                  <q-tooltip>Add FTP</q-tooltip>
                </q-btn>
              </div>
              <div v-if="hasNewFtp">
                <q-btn
                  color="primary"
                  icon="o_save"
                  no-caps
                  class="btnRounded q-ml-sm"
                  :loading="processing"
                  @click="onSubmitNewRows"
                >
                  <q-tooltip>Save FTP</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="table-scroll-container">
        <q-table
          ref="tableRef"
          v-model:pagination="pagination"
          :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable'"
          :loading="loading"
          :rows="rows"
          :columns="columns"
          row-key="id"
          separator="cell"
          no-data-label="No data available"
          binary-state-sort
          :rows-per-page-options="[20, 50, 100, 200, 500]"
          @request="getAllInfraFTPForList"
        >
          <template #loading>
            <q-inner-loading showing color="primary">
              <q-spinner-ios size="40px" class="q-mt-xl" />
            </q-inner-loading>
          </template>
          <template #header="props">
            <q-tr :props="props" class="bg-primary text-white">
              <q-th v-for="col in props.cols" :key="col.name" :props="props">
                {{ col.label }}<span
                  v-if="['protocolTypeId', 'encryptionTypeId', 'name', 'host', 'port'].includes(col.name)"
                  class="required"
                >*</span>
              </q-th>
              <q-th auto-width class="text-center">Actions</q-th>
            </q-tr>
          </template>
          <template #body="props">
            <q-tr v-if="!props.row.deleted" :class="activeRowId == props.row.id ? 'highlight' : ''">
              <q-td class="wrap-text" style="width: 15%;">
                <div v-if="!props.row.isNew && editingRowId !== props.row.id">
                  {{ props.row.infraService?.name }}
                </div>
                <formSingleSelectDropdown
                  v-else
                  v-model="props.row.infraServiceId"
                  :options="infraAccountServiceForDropdownSingleSelect.list.value"
                  :filter="infraAccountServiceForDropdownSingleSelect.filter"
                  :required="false"
                />
              </q-td>
              <q-td class="wrap-text" style="width: 14%;">
                <div v-if="!props.row.isNew && editingRowId !== props.row.id">
                  {{ props.row.protocolType?.dropDownValue || "-" }}
                </div>
                <formSingleSelectDropdown
                  v-else
                  v-model="props.row.protocolTypeId"
                  :options="protocolTypeForDropdownSingleSelect.list.value"
                  :filter="protocolTypeForDropdownSingleSelect.filter"
                  :error="getRowValidation(props.row)?.protocolTypeId?.$error"
                  :error-message="getRowValidation(props.row)?.protocolTypeId?.$errors[0]?.$message"
                  @update:model-value="getRowValidation(props.row)?.protocolTypeId?.$touch()"
                />
              </q-td>
              <q-td class="wrap-text" style="width: 15%;">
                <div v-if="!props.row.isNew && editingRowId !== props.row.id">
                  {{ props.row.encryptionType?.dropDownValue || "-" }}
                </div>
                <formSingleSelectDropdown
                  v-else
                  v-model="props.row.encryptionTypeId"
                  :options="encryptionTypeForDropdownSingleSelect.list.value"
                  :filter="encryptionTypeForDropdownSingleSelect.filter"
                  :error="getRowValidation(props.row)?.encryptionTypeId?.$error"
                  :error-message="getRowValidation(props.row)?.encryptionTypeId?.$errors[0]?.$message"
                  @update:model-value="getRowValidation(props.row)?.encryptionTypeId?.$touch()"
                />
              </q-td>
              <q-td style="width: 25%;">
                <div class="row items-center">
                  <div class="col">
                    <div v-if="!props.row.isNew && editingRowId !== props.row.id">
                      {{ props.row.name }}
                    </div>
                    <q-input
                      v-else
                      v-model="props.row.name"
                      outlined
                      stack-label
                      hide-bottom-space
                      :error="getRowValidation(props.row)?.name?.$error"
                      :error-message="getRowValidation(props.row)?.name?.$errors[0]?.$message"
                      @blur="getRowValidation(props.row)?.name?.$touch()"
                    />
                  </div>
                </div>
              </q-td>
              <q-td style="width: 8%;">
                <div class="row items-center">
                  <div class="col">
                    <div v-if="!props.row.isNew && editingRowId !== props.row.id">
                      {{ props.row.host }}
                    </div>
                    <q-input
                      v-else
                      v-model="props.row.host"
                      outlined
                      stack-label
                      hide-bottom-space
                      :error="getRowValidation(props.row)?.host?.$error"
                      :error-message="getRowValidation(props.row)?.host?.$errors[0]?.$message"
                      @blur="getRowValidation(props.row)?.host?.$touch()"
                    />
                  </div>
                </div>
              </q-td>
              <q-td style="width: 8%;">
                <div class="row items-center">
                  <div class="col">
                    <div v-if="!props.row.isNew && editingRowId !== props.row.id">
                      {{ props.row.port }}
                    </div>
                    <q-input
                      v-else
                      v-model="props.row.port"
                      outlined
                      stack-label
                      hide-bottom-space
                      :error="getRowValidation(props.row)?.port?.$error"
                      :error-message="getRowValidation(props.row)?.port?.$errors[0]?.$message"
                      @blur="getRowValidation(props.row)?.port?.$touch()"
                    />
                  </div>
                </div>
              </q-td>
              <q-td style="width: 10%;">
                <div class="row items-center q-gutter-xs">
                  <q-chip
                    v-for="(item, index) in (props.row.infraFTPsProjectInstanceMapping || []).slice(0, 2)"
                    :key="index"
                    dense
                    class="bg-primary text-white"
                  >
                    {{ item.infraProjectInstance.platform.dropDownValue }} - {{ item.infraProjectInstance.url }}
                  </q-chip>
                  <q-chip
                    v-if="props.row.infraFTPsProjectInstanceMapping?.length > 2"
                    dense
                    clickable
                    class="bg-grey-4 text-black"
                    @click="onFTPView(props.row.id, true, refreshFTPList)"
                  >
                    +{{ props.row.infraFTPsProjectInstanceMapping.length - 2 }} more...
                  </q-chip>
                </div>
              </q-td>
              <q-td auto-width class="text-center actions" style="width: 5%;">
                <template v-if="editingRowId === props.row.id && !props.row.isNew">
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
                    @click="onFTPSubmit(props.row)"
                  >
                    <q-tooltip>Save</q-tooltip>
                  </q-icon>
                </template>
                <q-icon v-if="!props.row.isNew" name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="onFTPView(props.row.id)">
                  <q-tooltip>View</q-tooltip>
                </q-icon>
                <q-icon v-if="!props.row.isNew" name="o_add_task" class="cursor-pointer q-mr-sm" size="xs" @click="onFTPView(props.row.id, true, refreshFTPList)">
                  <q-tooltip>Assign Project Instance</q-tooltip>
                </q-icon>
                <!-- <q-icon
                  v-if="!props.row.isNew && editingRowId !== props.row.id"
                  name="o_edit"
                  class="cursor-pointer q-mr-sm"
                  size="xs"
                  @click="onEdit(props.row)"
                >
                  <q-tooltip>Edit</q-tooltip>
                </q-icon>
                <q-icon
                  v-else-if="!props.row.isNew"
                  :loading="processing"
                  name="o_save"
                  class="cursor-pointer q-mr-sm"
                  size="xs"
                  @click="onRowSubmit(props.row)"
                >
                  <q-tooltip>Save</q-tooltip>
                </q-icon> -->
                <q-icon
                  v-if="!props.row.isNew && editingRowId !== props.row.id"
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
                  class="cursor-pointer q-mr-xs"
                  @click="activeRowId = props.row.id"
                >
                  <q-tooltip>Add Instructions</q-tooltip>
                  <q-popup-edit
                    v-model="props.row.instructions"
                    anchor="center middle"
                    self="center middle"
                    buttons
                    persistent
                    label-set="Save"
                    label-cancel="Cancel"
                    class="instruction-popup"
                    @save="val => { handleInstructionSave(props.row, val); activeRowId = null }"
                    @hide="activeRowId = null"
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
                  :on-save-api="handleWalletDetailSave"
                  @success="refreshFTPList"
                />
                <q-icon
                  name="o_delete_outline"
                  size="xs"
                  class="cursor-pointer"
                  color="negative"
                  @click="handleDelete(props.row, props.rowIndex)"
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
import { uid, useQuasar } from "quasar";
import { required, numeric, helpers } from "@vuelidate/validators";
import { ref, onMounted, reactive, computed, watch } from "vue";
import { clearLocalStorage, getLocalStorage, setLocalStorage, notifyError, notifySuccess, zwConfirm } from "assets/utils";
import useVuelidate from "@vuelidate/core";

import infraFTPService from "../infraFTP.service";
import WalletPopup from "modules/infra-account/components/_walletPopup.vue";

// Shared Dropdowns
import infraAccountModule from "src/modules/infra-account/utils/dropdowns.js";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import infraAccountServiceModule from "src/modules/infra-account-services/utils/dropdowns.js";
import infraFTPModule from "src/modules/infra-ftp/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

import {
  initFTPDialogs,
  onFTPView,
} from "src/modules/infra-ftp/utils/dialogs.js";

// SOP Change :- Shared FTP Actions
import {
  initInfraFTPActions,
  onSubmitInfraFTPDelete
} from "src/modules/infra-ftp/utils/actions.js";

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Variable Declarations
// --------------------------------------------------------------------------------------------------------------------------------------------------
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const loading = ref(true);
const processing = ref(false);
const rowValidations = ref([]);
const showFilter = ref(false);
const searchLoader = ref(false);
const activeRowId = ref(null);
const editingRowId = ref(null);
const editingRow = ref(null);

// local storage
const localStorageKey = "Infra FTP";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const infraServiceIds = filterLocalStorage ? filterLocalStorage.infraServiceIds : [];
const protocolTypeIds = filterLocalStorage ? filterLocalStorage.protocolTypeIds : [];
const encryptionTypeIds = filterLocalStorage ? filterLocalStorage.encryptionTypeIds : [];
const name = filterLocalStorage ? filterLocalStorage.name : "";

// search
const search = ref({
  searchText,
  infraServiceIds,
  protocolTypeIds,
  encryptionTypeIds,
  name
});

// --------------------------------------------------------------------------------------------------------------------------------------------------
// FTP List
// --------------------------------------------------------------------------------------------------------------------------------------------------
const tableRef = ref();
const rows = ref([]);
const pagination = ref({ sortBy: "CreatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "infraServiceId", label: "Infra Account Service", field: "infraServiceId", align: "left", sortable: true },
  { name: "protocolTypeId", label: "Protocol Type", field: "protocolTypeId", align: "left", sortable: true },
  { name: "encryptionTypeId", label: "Encryption Type", field: "encryptionTypeId", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "host", label: "Host", field: "host", align: "left", sortable: true },
  { name: "port", label: "Port", field: "port", align: "left", sortable: true },
  { name: "infraFTPsProjectInstanceMapping", label: "Project Instance", field: "infraFTPsProjectInstanceMapping", align: "left", sortable: false }
]);

// get name Logs and map list
const getAllInfraFTPForList = async (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });

  const resp = await infraFTPService.getAllInfraFTPForList(payload);
  // map async calls and wait for all of them
  rows.value = resp.infraFTPsList.map(r => ({
    ...r,
    instructions: r.instructions ?? "",
    infraServiceId: r.infraService.id,
    protocolTypeId: r.protocolType.id,
    encryptionTypeId: r.encryptionType.id,
    walletTypeId: r.walletType.id,
    isNew: false,
    editFields: []
  }));
  rowValidations.value = rows.value.map(row =>
    useVuelidate(editingRowRules, row)
  );
  pagination.value.page = page;
  pagination.value.rowsPerPage = rowsPerPage;
  pagination.value.sortBy = sortBy;
  pagination.value.descending = descending;
  pagination.value.rowsNumber = resp.total;
  loading.value = false;
  searchLoader.value = false;
};

const hasNewFtp = computed(() =>
  rows.value.some(row => row.isNew)
);

function getRowValidation (row) {
  const index = rows.value.indexOf(row);
  if (index === -1) return null;
  return rowValidations.value[index]?.value ?? null;
}

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initFTPDialogs(activeRowId);
initInfraFTPActions(activeRowId);

// add rows
function onAddInfraFTP () {
  // Check if a row is currently being edited
  if (editingRowId.value) {
    zwConfirm({
      title: "Edit in Progress",
      message: "Please finish editing the current row before adding a new FTP.",
      okLabel: "OK",
      cancel: false
    }, () => {
    });
    return;
  }
  const newRowsCount = rows.value.filter(r => r.isNew && !r.deleted).length;

  if (newRowsCount >= 10) {
    zwConfirm({
      title: "Limit Reached",
      message: "You can add a maximum of 10 FTPs at a time. Please save the existing rows before adding more.",
      okLabel: "OK",
      cancel: false
    }, () => {
    });
    return;
  }
  const newRow = reactive({
    id: uid(),
    infraServiceId: null,
    protocolTypeId: null,
    encryptionTypeId: null,
    walletTypeId: null,
    walletNumber: "",
    name: "",
    host: "",
    port: "",
    instructions: "",
    isNew: true,
    deleted: false
  });

  rows.value.unshift(newRow);

  const v$ = useVuelidate(editingRowRules, newRow, {
    $lazy: true,
    $autoDirty: true
  });

  rowValidations.value.unshift(v$);
}

function onEdit (row) {
  const hasUnsavedRows = rows.value.some(r => r.isNew && !r.deleted);

  if (hasUnsavedRows) {
    zwConfirm({
      title: "Unsaved Rows",
      message: "Please save the newly added rows before editing existing records.",
      okLabel: "OK",
      cancel: false
    }, () => {
    });
    return;
  }

  if (editingRowId.value && editingRowId.value !== row.id) {
    zwConfirm({
      title: "Edit in Progress",
      message: "Please finish editing the current row before continuing.",
      okLabel: "OK",
      cancel: false
    }, () => {
    });
    return;
  }

  editingRowId.value = row.id;
  editingRow.value = { ...row };
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

// validation rules for divisions
const editingRowRules = {
  // infraServiceId: { required: helpers.withMessage("Infra service is required", required) },
  protocolTypeId: { required: helpers.withMessage("Protocol type is Required", required) },
  encryptionTypeId: { required: helpers.withMessage("Encryption type is Required", required) },
  name: { required: helpers.withMessage("Name is Required", required) },
  host: { required: helpers.withMessage("Host is Required", required) },
  port: {
    required: helpers.withMessage("Port is Required", required),
    numeric: helpers.withMessage("Port must contain numbers only", numeric)
  }
};

const handleDelete = (row, index) => {
  onSubmitInfraFTPDelete(
    row,
    index,
    rows.value,
    rowValidations.value,
    refreshFTPList
  );
};

async function validateRows (rowsToValidate) {
  let hasError = false;

  for (const row of rowsToValidate) {
    const v$ = getRowValidation(row);
    if (!v$) continue;

    const isValid = await v$.$validate();
    if (!isValid) hasError = true;
  }

  return !hasError;
}

function buildPayload (rowsToSend) {
  return {
    infraFTPLines: rowsToSend.map(row => ({
      id: row.id ?? uid(),
      infraServiceId: row.infraServiceId,
      protocolTypeId: row.protocolTypeId,
      encryptionTypeId: row.encryptionTypeId,
      walletTypeId: row.walletTypeId,
      walletNumber: row.walletNumber,
      name: row.name,
      host: row.host,
      port: row.port,
      instructions: row.instructions,
      isNew: row.isNew,
      deleted: row.deleted
    }))
  };
}

async function saveRows (rowsToSend, successMessage) {
  const isValid = await validateRows(rowsToSend);

  if (!isValid) {
    notifyError({ message: "Please fill the mandatory fields." });
    return;
  }

  const payload = buildPayload(rowsToSend);

  await infraFTPService.addEditInfraFTP(payload);

  notifySuccess({ message: successMessage });

  editingRowId.value = null;

  refreshFTPList();
}

// =====================================================
// Save Wallet Details
// =====================================================
function handleWalletDetailSave (row, payload) {
  row.walletTypeId = payload.walletTypeId;
  row.walletNumber = payload.walletNumber;

  if (!row.isNew) {
    return saveWalletDetails(row, payload).then(() => true);
  }

  return Promise.resolve(false);
}

function saveWalletDetails (row, payload) {
  return infraFTPService
    .addOrUpdateInstructions(row.id, payload)
    .then(() => {
      notifySuccess({ message: "Wallet details is saved successfully." });
    });
}

function refreshFTPList () {
  getAllInfraFTPForList({ pagination: pagination.value });
}
// =====================================================
// Save Instructions
// =====================================================
function handleInstructionSave (row, instructions) {
  row.instructions = instructions;

  if (!row.isNew) {
    onSaveInstructions(row.id, instructions);
  }
}

function onSaveInstructions (id, instructions) {
  setTimeout(function () {
    const payload = {
      instructions,
      IsInstruction: true
    };

    infraFTPService.addOrUpdateInstructions(id, payload)
      .then(() => {
        notifySuccess({ message: "Instruction is saved successfully." });
      });
  });
}
// =====================================================
async function onFTPSubmit (row) {
  await saveRows([row], "FTP saved successfully.");
}

async function onSubmitNewRows () {
  const newRows = rows.value.filter(r => r.isNew && !r.deleted);

  if (!newRows.length) {
    notifyError({ message: "No new rows to save." });
    return;
  }

  await saveRows(newRows, "New FTP rows saved successfully.");
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onSearch = () => {
 refreshFTPList();
};

const onClear = () => {
  search.value.searchText = "";
  search.value.infraServiceIds = [];
  search.value.protocolTypeIds = [];
  search.value.encryptionTypeIds = [];
  search.value.name = "";
  clearLocalStorage(localStorageKey);
  onSearch();
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

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.infraServiceIds, infraAccountServicesForDropdown.list, "Infra Services"),
  ...mapFilterToLabel(search.value.protocolTypeIds, protocolTypesForDropdown.list, "Protocol Type"),
  ...mapFilterToLabel(search.value.encryptionTypeIds, encryptionTypesForDropdown.list, "Encryption Type"),
  ...(search.value.name ? { Name: search.value.name } : {})
}));

function onClearFilters (key) {
  if (key === "Infra Services") {
    search.value.infraServiceIds = [];
  } else if (key === "Protocol Type") {
    search.value.protocolTypeIds = [];
  } else if (key === "Encryption Type") {
    search.value.encryptionTypeIds = [];
  } else if (key === "Name") {
    search.value.name = "";
  }
  delete appliedFilters.value[key];
  refreshFTPList();
}

function getFilterCount (key) {
  switch (key) {
  case "Infra Services": return search.value.infraServiceIds?.length || 0;
  case "Protocol Type": return search.value.protocolTypeIds?.length || 0;
  case "Encryption Type": return search.value.encryptionTypeIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { infraWalletTypeDropdownSingleSelect } = infraAccountModule();

const {
  infraAccountServicesForDropdown,
  infraAccountServiceForDropdownSingleSelect
} = infraAccountServiceModule();

const {
  protocolTypesForDropdown,
  encryptionTypesForDropdown,
  protocolTypeForDropdownSingleSelect,
  encryptionTypeForDropdownSingleSelect
} = infraFTPModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Search: When user types in search
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  refreshFTPList();
});

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Page Load
// --------------------------------------------------------------------------------------------------------------------------------------------------
onMounted(async () => {
  refreshFTPList();
  protocolTypeForDropdownSingleSelect.load("Protocol Type");
  infraAccountServicesForDropdown.load();
  infraAccountServiceForDropdownSingleSelect.load();
  protocolTypesForDropdown.load("Protocol Type");
  encryptionTypesForDropdown.load("Encryption Type");
  encryptionTypeForDropdownSingleSelect.load("Encryption Type");
  infraWalletTypeDropdownSingleSelect.load("Wallet Type");
});

</script>
<style>
.table-scroll-container {
  overflow-x: auto;
  overflow-y: hidden;
  width: 100%;
  max-width: 100%;
}
.Custom-DataTable {
  min-width: max-content;
}
.cursor-pointer {
  cursor: pointer;
}
.cursor-pointer:hover {
  background-color: #f0f7ff;
  border-radius: 4px;
}
.hover-white:hover {
  color: #fff !important;
}
</style>
