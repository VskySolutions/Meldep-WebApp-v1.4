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
              <q-breadcrumbs-el label="Project Instance" />
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
                      v-model="search.infraProjectIds"
                      label="Infra Project"
                      :options="projectNameDropdown.list.value"
                      :filter="projectNameDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.platformIds"
                      label="Platform"
                      :options="platformsForDropdown.list.value"
                      :filter="platformsForDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.instanceTypeIds"
                      label="Instance Type"
                      :options="instanceTypesForDropdown.list.value"
                      :filter="instanceTypesForDropdown.filter"
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
              <div>
                <q-btn
                  icon="o_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
                  @click="onAddInfraProjectInstance"
                >
                  <q-tooltip>Add Instance</q-tooltip>
                </q-btn>
              </div>
              <div v-if="hasNewInstance">
                <q-btn
                  color="primary"
                  icon="o_save"
                  no-caps
                  class="btnRounded q-ml-sm"
                  :loading="processing"
                  @click="onSubmitNewRows"
                >
                  <q-tooltip>Save Instance</q-tooltip>
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
          :rows-per-page-options="[15,30,50,100]"
          @request="getAllInfraProjectInstanceForList"
        >
          <template #loading>
            <q-inner-loading showing color="primary">
              <q-spinner-ios size="40px" class="q-mt-xl" />
            </q-inner-loading>
          </template>
          <template #header="props">
            <q-tr :props="props" class="bg-primary text-white">
              <q-th v-for="col in props.cols" :key="col.name" :props="props">
                {{ col.label }}
                <span
                  v-if="['infraProjectId', 'instanceTypeId', 'platformId', 'url'].includes(col.name)"
                  class="required"
                >*
                </span>
              </q-th>
              <q-th auto-width class="text-center">Actions</q-th>
            </q-tr>
          </template>
          <template #body="props">
            <q-tr v-if="!props.row.deleted" :class="activeRowId == props.row.id ? 'highlight' : ''">
              <q-td class="wrap-text" style="width: 22%;">
                <div v-if="!props.row.isNew && editingRowId !== props.row.id">
                  {{ props.row.infraProject?.name }}
                </div>
                <formSingleSelectDropdown
                  v-else
                  v-model="props.row.infraProjectId"
                  :options="projectNameDropdownSingleSelect.list.value"
                  :filter="projectNameDropdownSingleSelect.filter"
                  :error="getRowValidation(props.row)?.infraProjectId?.$error"
                  :error-message="getRowValidation(props.row)?.infraProjectId?.$errors[0]?.$message"
                  @update:model-value="getRowValidation(props.row)?.infraProjectId?.$touch()"
                />
              </q-td>
              <q-td class="wrap-text" style="width: 22%;">
                <div v-if="!props.row.isNew && editingRowId !== props.row.id">
                  {{ props.row.instanceType?.dropDownValue || "-" }}
                </div>
                <formSingleSelectDropdown
                  v-else
                  v-model="props.row.instanceTypeId"
                  :options="instanceTypeDropdownSingleSelect.list.value"
                  :filter="instanceTypeDropdownSingleSelect.filter"
                  :error="getRowValidation(props.row)?.instanceTypeId?.$error"
                  :error-message="getRowValidation(props.row)?.instanceTypeId?.$errors[0]?.$message"
                  @update:model-value="getRowValidation(props.row)?.instanceTypeId?.$touch()"
                />
              </q-td>
              <q-td class="wrap-text" style="width: 21%;">
                <div v-if="!props.row.isNew && editingRowId !== props.row.id">
                  {{ props.row.platform?.dropDownValue || "-" }}
                </div>
                <formSingleSelectDropdown
                  v-else
                  v-model="props.row.platformId"
                  :options="platformDropdownSingleSelect.list.value"
                  :filter="platformDropdownSingleSelect.filter"
                  :error="getRowValidation(props.row)?.platformId?.$error"
                  :error-message="getRowValidation(props.row)?.platformId?.$errors[0]?.$message"
                  @update:model-value="getRowValidation(props.row)?.platformId?.$touch()"
                />
              </q-td>
              <q-td style="width: 30%;">
                <div class="row items-center justify-between">
                  <div class="col">
                    <div v-if="!props.row.isNew && editingRowId !== props.row.id">
                      {{ props.row.url }}
                    </div>
                    <q-input
                      v-else
                      v-model="props.row.url"
                      outlined
                      stack-label
                      hide-bottom-space
                      :error="getRowValidation(props.row)?.url?.$error"
                      :error-message="getRowValidation(props.row)?.url?.$errors[0]?.$message"
                      @blur="getRowValidation(props.row)?.url?.$touch()"
                    />
                  </div>
                </div>
              </q-td>
              <q-td auto-width class="text-center actions" style="width: 5%;">
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
                    @click="onRowSubmit(props.row)"
                  >
                    <q-tooltip>Save</q-tooltip>
                  </q-icon>
                </template>
                <q-icon v-if="!props.row.isNew" name="o_person_add_alt" class="cursor-pointer q-mr-sm" size="xs" @click="onInfraProjectInstanceUserAdd(props.row.id, refreshInfraProjectInstancesList)">
                  <q-tooltip>Add User</q-tooltip>
                </q-icon>
                <q-icon v-if="!props.row.isNew" name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="onInfraProjectInstanceView(props.row.id, refreshInfraProjectInstancesList)">
                  <q-tooltip>View</q-tooltip>
                </q-icon>
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
import { required, helpers, url } from "@vuelidate/validators";
import { ref, onMounted, reactive, computed, watch } from "vue";
import { clearLocalStorage, getLocalStorage, setLocalStorage, notifyError, notifySuccess, zwConfirm } from "assets/utils";
import useVuelidate from "@vuelidate/core";

import infraProjectInstanceService from "../infraProjectInstance.service";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import infraProjectInstanceModule from "src/modules/infra-project-instance/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

import {
  initInfraProjectInstanceDialogs,
  onInfraProjectInstanceView,
  onInfraProjectInstanceUserAdd
} from "src/modules/infra-project-instance/utils/dialogs.js";

// SOP Change :- Shared Project Actions
import {
  initInfraProjectInstanceActions,
  onSubmitInfraProjectInstanceDelete
} from "src/modules/infra-project-instance/utils/actions.js";
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
const localStorageKey = "Infra Project Instance";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const infraProjectIds = filterLocalStorage ? filterLocalStorage.infraProjectIds : [];
const platformIds = filterLocalStorage ? filterLocalStorage.platformIds : [];
const instanceTypeIds = filterLocalStorage ? filterLocalStorage.instanceTypeIds : [];

// search
const search = ref({
  searchText,
  infraProjectIds,
  platformIds,
  instanceTypeIds
});

// --------------------------------------------------------------------------------------------------------------------------------------------------
// ProjectInstance List
// --------------------------------------------------------------------------------------------------------------------------------------------------
const tableRef = ref();
const rows = ref([]);
const pagination = ref({ sortBy: "CreatedOnUtc", descending: true, rowsPerPage: 15, page: 1 });
const columns = ref([
  { name: "infraProjectId", label: "Infra Project", field: "infraProjectId", align: "left", sortable: true },
  { name: "instanceTypeId", label: "Instance Type", field: "instanceTypeId", align: "left", sortable: true },
  { name: "platformId", label: "Platform", field: "platformId", align: "left", sortable: true },
  { name: "url", label: "URL", field: "url", align: "left", sortable: true }
]);

// get Infra Project Instance and map list
const getAllInfraProjectInstanceForList = async (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });

  const resp = await infraProjectInstanceService.getAllInfraProjectInstanceForList(payload);
  // map async calls and wait for all of them
  rows.value = resp.infraProjectInstancesList.map(r => ({
    ...r,
    infraProjectId: r.infraProject.id,
    instructions: r.instructions ?? "",
    instanceTypeId: r.instanceType.id,
    platformId: r.platform.id,
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

function refreshInfraProjectInstancesList () {
  getAllInfraProjectInstanceForList({ pagination: pagination.value });
}

const hasNewInstance = computed(() =>
  rows.value.some(row => row.isNew)
);

function getRowValidation (row) {
  const index = rows.value.indexOf(row);
  if (index === -1) return null;
  return rowValidations.value[index]?.value ?? null;
}

function onAddInfraProjectInstance () {
  // Check if a row is currently being edited
  if (editingRowId.value) {
    zwConfirm({
      title: "Edit in Progress",
      message: "Please finish editing the current row before adding a new Instance.",
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
      message: "You can add a maximum of 10 instances at a time. Please save the existing rows before adding more.",
      okLabel: "OK",
      cancel: false
    }, () => {
    });
    return;
  }
  const newRow = reactive({
    id: uid(),
    infraProjectId: null,
    instanceTypeId: null,
    platformId: null,
    url: "",
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
  infraProjectId: { required: helpers.withMessage("Infra project is required", required) },
  instanceTypeId: { required: helpers.withMessage("Instance type is Required", required) },
  platformId: { required: helpers.withMessage("Platform is Required", required) },
  url: {
    required: helpers.withMessage("URL is Required", required),
    url: helpers.withMessage("Invalid URL", url)
  }
};

const handleDelete = (row, index) => {
  onSubmitInfraProjectInstanceDelete(
    row,
    index,
    rows.value,
    rowValidations.value,
    refreshInfraProjectInstancesList
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

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initInfraProjectInstanceDialogs(activeRowId);
initInfraProjectInstanceActions(activeRowId);
// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const {
  projectNameDropdown,
  projectNameDropdownSingleSelect
} = projectModule();

const {
  platformsForDropdown,
  instanceTypesForDropdown,
  platformDropdownSingleSelect,
  instanceTypeDropdownSingleSelect
} = infraProjectInstanceModule();

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
      instructions
    };
    infraProjectInstanceService.addOrUpdateInstructions(id, payload)
      .then(() => {
        notifySuccess({ message: "Instruction is saved successfully." });
      });
  });
}
// =====================================================

function buildPayload (rowsToSend) {
  return {
    infraProjectInstanceLines: rowsToSend.map(row => ({
      id: row.id ?? uid(),
      infraProjectId: row.infraProjectId,
      instanceTypeId: row.instanceTypeId,
      platformId: row.platformId,
      url: row.url,
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

  await infraProjectInstanceService.addEditInfraProjectInstance(payload);

  notifySuccess({ message: successMessage });

  editingRowId.value = null;

  refreshInfraProjectInstancesList();
}

async function onRowSubmit (row) {
  await saveRows([row], "Project instance updated successfully.");
}

async function onSubmitNewRows () {
  const newRows = rows.value.filter(r => r.isNew && !r.deleted);

  if (!newRows.length) {
    notifyError({ message: "No new rows to save." });
    return;
  }

  await saveRows(newRows, "Project instance saved successfully.");
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onSearch = () => {
  refreshInfraProjectInstancesList();
};

const onClear = () => {
  search.value.searchText = "";
  search.value.infraProjectIds = [];
  search.value.platformIds = [];
  search.value.instanceTypeIds = [];
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
  ...mapFilterToLabel(search.value.infraProjectIds, projectNameDropdown.list, "Infra Project"),
  ...mapFilterToLabel(search.value.platformIds, platformsForDropdown.list, "Platform"),
  ...mapFilterToLabel(search.value.instanceTypeIds, instanceTypesForDropdown.list, "Instance Type")
}));

function onClearFilters (key) {
  if (key === "Infra Project") {
    search.value.infraProjectIds = [];
  } else if (key === "Platform") {
    search.value.platformIds = [];
  } else if (key === "Instance Type") {
    search.value.instanceTypeIds = [];
  }
  delete appliedFilters.value[key];
  refreshInfraProjectInstancesList();
}

function getFilterCount (key) {
  switch (key) {
  case "Infra Project": return search.value.infraProjectIds?.length || 0;
  case "Platform": return search.value.platformIds?.length || 0;
  case "Instance Type": return search.value.instanceTypeIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Search: When user types in search
// --------------------------------------------------------------------------------------------------------------------------------------------------
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  refreshInfraProjectInstancesList();
});

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Page Load
// --------------------------------------------------------------------------------------------------------------------------------------------------
onMounted(async () => {
  refreshInfraProjectInstancesList();
  projectNameDropdown.load();
  projectNameDropdownSingleSelect.load(false, true, true);
  instanceTypesForDropdown.load("Instance Type");
  instanceTypeDropdownSingleSelect.load("Instance Type");
  platformsForDropdown.load("Platform Name");
  platformDropdownSingleSelect.load("Platform Name");
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
