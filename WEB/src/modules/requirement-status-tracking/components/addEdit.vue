<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 90vw !important; max-width: 90vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.recordId ? "Edit" : "Add" }} Requirement Status Tracking</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogCancel">
          <q-tooltip>Close without saving</q-tooltip>
        </q-btn>
      </q-card-section>
      <q-separator />
      <q-form ref="formRef" greedy @submit.prevent.stop>
        <div class="q-pa-md cardTable">
          <q-inner-loading :showing="loading" />
          <div class="q-gutter-y-md">
            <q-banner v-if="readonly" dense rounded class="bg-grey-2 text-grey-9">
              <template #avatar><q-icon name="o_lock" /></template>
              You have view-only access to this tracking record.
            </q-banner>
            <q-banner v-if="recordLoadError" dense rounded class="bg-red-1 text-negative">
              <template #avatar><q-icon name="o_error_outline" /></template>
              The tracking record could not be loaded, so it cannot be edited. Close the form and try again.
            </q-banner>
            <q-banner v-if="serverErrors.length" dense rounded class="bg-red-1 text-negative">
              <template #avatar><q-icon name="o_error_outline" /></template>
              <div>Nothing was saved. Please fix the following and save again:</div>
              <ul class="q-my-xs q-pl-md">
                <li v-for="(message, index) in serverErrors" :key="index">{{ message }}</li>
              </ul>
            </q-banner>
            <q-card>
              <fieldset>
                <legend>Tracking Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-4 col-md-4">
                    <formSingleSelectDropdown
                      v-if="!isEdit"
                      :model-value="model.projectId"
                      label="Project Name"
                      wrapper-class=""
                      :readonly="readonly"
                      :options="projectNameDropdownSingleSelect.list.value"
                      :filter="projectNameDropdownSingleSelect.filter"
                      :error="showErrors && !!validation.header.projectId"
                      :error-message="validation.header.projectId"
                      @update:model-value="onProjectChange"
                    />
                    <div v-else>
                      <label class="label q-mb-xs text-black">Project Name<span class="required">*</span></label>
                      <q-input :model-value="model.projectName" outlined dense readonly hide-bottom-space />
                    </div>
                  </div>
                  <div class="col-12 col-sm-4 col-md-4">
                    <formSingleSelectDropdown
                      v-if="!isEdit"
                      :model-value="model.projectModuleId"
                      label="Project Module"
                      wrapper-class=""
                      :disable="!model.projectId"
                      :readonly="readonly"
                      :options="projectModulesByProjectIdForDropdownSingleSelect.list.value"
                      :filter="projectModulesByProjectIdForDropdownSingleSelect.filter"
                      :error="showErrors && !!validation.header.projectModuleId"
                      :error-message="validation.header.projectModuleId"
                      @update:model-value="onModuleChange"
                    />
                    <div v-else>
                      <label class="label q-mb-xs text-black">Project Module<span class="required">*</span></label>
                      <q-input :model-value="model.projectModuleName" outlined dense readonly hide-bottom-space />
                    </div>
                  </div>
                  <formDate
                    v-model="model.trackingDate"
                    label="Date"
                    wrapper-class="col-12 col-sm-4 col-md-4"
                    :readonly="readonly"
                    :error="showErrors && !!validation.header.trackingDate"
                    :error-message="validation.header.trackingDate"
                  />
                </div>

                <q-banner v-if="requirementLoadState === 'loading'" dense rounded class="bg-grey-2 q-mb-sm">
                  <template #avatar><q-spinner color="primary" size="sm" /></template>
                  Loading the module's requirements…
                </q-banner>
                <q-banner v-else-if="requirementLoadState === 'error'" dense rounded class="bg-red-1 text-negative q-mb-sm">
                  <template #avatar><q-icon name="o_error_outline" /></template>
                  The module's requirements could not be loaded.
                  <template #action>
                    <q-btn flat dense no-caps color="negative" label="Retry" @click="loadRequirements(!isEdit && rows.length === 0)" />
                  </template>
                </q-banner>
                <q-banner v-else-if="requirementLoadState === 'empty'" dense rounded class="bg-orange-1 text-orange-10 q-mb-sm">
                  <template #avatar><q-icon name="o_info" /></template>
                  No requirements are assigned to this project module.
                </q-banner>

                <div v-if="!readonly" class="row justify-end q-mb-sm">
                  <q-btn
                    color="primary"
                    icon="o_add"
                    label="Add New Line"
                    no-caps
                    :disable="!model.projectModuleId || requirementLoadState === 'loading'"
                    @click="onAddLine"
                  >
                    <q-tooltip v-if="!model.projectModuleId">Select a project module first</q-tooltip>
                  </q-btn>
                </div>

                <q-table
                  class="no-shadow"
                  bordered
                  flat
                  separator="cell"
                  row-key="key"
                  :rows="rows"
                  :columns="columns"
                  :pagination="{ rowsPerPage: 0 }"
                  hide-pagination
                >
                  <template #header="headerProps">
                    <q-tr :props="headerProps" class="bg-primary text-white">
                      <q-th v-for="col in headerProps.cols" :key="col.name" class="text-start" :style="col.headerStyle">
                        {{ col.label }}<span v-if="col.name === 'requirement'" class="required">*</span>
                      </q-th>
                    </q-tr>
                  </template>
                  <template #body="bodyProps">
                    <q-tr :props="bodyProps">
                      <q-td key="requirement" :props="bodyProps" style="max-width: 30%;">
                        <formSingleSelectDropdown
                          :model-value="bodyProps.row.requirementId"
                          wrapper-class=""
                          :is-clearable="false"
                          :readonly="readonly"
                          :options="rowRequirementOptions(bodyProps.row)"
                          :option-disable="opt => opt.disable"
                          :filter="rowRequirementFilter(bodyProps.row)"
                          :error="showErrors && !!rowError(bodyProps.row, 'requirementId')"
                          :error-message="rowError(bodyProps.row, 'requirementId')"
                          @update:model-value="value => onRowRequirementChange(bodyProps.row, value)"
                        />
                        <q-badge v-if="bodyProps.row.isRequirementUnavailable" color="warning" class="q-mt-xs">
                          Moved or deleted
                          <q-tooltip>This requirement was deleted or moved out of the module after the row was saved. It can be kept unchanged or removed.</q-tooltip>
                        </q-badge>
                      </q-td>
                      <q-td key="currentPercent" :props="bodyProps" style="max-width: 5%">
                        <q-input
                          v-model.number="bodyProps.row.currentPercent"
                          type="number" min="0" max="100" step="0.01"
                          outlined dense hide-bottom-space
                          aria-label="Current Completion (%)"
                          :readonly="readonly"
                          :error="showErrors && !!rowError(bodyProps.row, 'currentPercent')"
                          :error-message="rowError(bodyProps.row, 'currentPercent')"
                        />
                      </q-td>
                      <q-td key="timeSpentAsOfToday" :props="bodyProps" style="max-width: 5%">
                        <q-input
                          v-model.number="bodyProps.row.timeSpentAsOfToday"
                          type="number" min="0" step="0.25"
                          outlined dense hide-bottom-space
                          aria-label="Time Spent as of Now (Hours)"
                          :readonly="readonly"
                          :error="showErrors && !!rowError(bodyProps.row, 'timeSpentAsOfToday')"
                          :error-message="rowError(bodyProps.row, 'timeSpentAsOfToday')"
                        />
                      </q-td>
                      <q-td key="estimatedRemainingTime" :props="bodyProps" style="max-width: 5%">
                        <q-input
                          v-model.number="bodyProps.row.estimatedRemainingTime"
                          type="number" min="0" step="0.25"
                          outlined dense hide-bottom-space
                          aria-label="Estimated Remaining Time Required (Hours)"
                          :readonly="readonly"
                          :error="showErrors && !!rowError(bodyProps.row, 'estimatedRemainingTime')"
                          :error-message="rowError(bodyProps.row, 'estimatedRemainingTime')"
                        />
                      </q-td>
                      <q-td key="scope" :props="bodyProps" style="min-width: 45%;">
                        <!-- <q-input
                          v-model="bodyProps.row.scope"
                          type="textarea" autogrow
                          outlined dense hide-bottom-space
                          aria-label="Additional Scope"
                          :readonly="readonly"
                        /> -->
                        <q-editor
                          v-model="bodyProps.row.scope"
                          :dense="$q.screen.lt.md"
                          :toolbar="toolbar"
                          :fonts="fonts"
                        />
                      </q-td>
                      <q-td key="estimatedTimeForCompletion" :props="bodyProps" style="max-width: 5%">
                        <q-input
                          v-model.number="bodyProps.row.estimatedTimeForCompletion"
                          type="number" min="0" step="0.25"
                          outlined dense hide-bottom-space
                          aria-label="Estimated Time for Completion of Additional Scope (Hours)"
                          :readonly="readonly"
                          :error="showErrors && !!rowError(bodyProps.row, 'estimatedTimeForCompletion')"
                          :error-message="rowError(bodyProps.row, 'estimatedTimeForCompletion')"
                        />
                      </q-td>
                      <q-td key="action" :props="bodyProps" class="text-center">
                        <q-btn
                          v-if="!readonly"
                          flat round dense
                          icon="o_delete"
                          color="negative"
                          aria-label="Remove row"
                          @click="onRemoveRow(bodyProps.row)"
                        >
                          <q-tooltip>Remove row (saved on the next save)</q-tooltip>
                        </q-btn>
                      </q-td>
                    </q-tr>
                  </template>
                  <template #no-data>
                    <div class="full-width text-center text-grey-7 q-pa-md">
                      {{ model.projectModuleId ? "No requirement rows. Use Add New Line to add one." : "Select a project and module to load its requirements." }}
                    </div>
                  </template>
                </q-table>

                <div align="center" class="q-gutter-sm justify-center q-mt-sm">
                  <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                  <template v-if="!readonly">
                    <q-btn
                      color="primary"
                      push
                      outline
                      label="Save and Continue"
                      type="button"
                      class="actionBtn"
                      style="width: 160px !important;"
                      no-caps
                      :loading="processing && activeButton === 'continue'"
                      :disable="processing || loading || recordLoadError"
                      @click="onSubmit(false, 'continue')"
                    />
                    <q-btn
                      color="primary"
                      push
                      outline
                      label="Save and Close"
                      type="button"
                      class="actionBtn"
                      no-caps
                      :loading="processing && activeButton === 'close'"
                      :disable="processing || loading || recordLoadError"
                      @click="onSubmit(true, 'close')"
                    />
                  </template>
                </div>
              </fieldset>
            </q-card>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import { ref, computed, onMounted } from "vue";
import { notifySuccess, notifyError, notifyWarning } from "assets/utils";

import requirementStatusTrackingService from "../requirementStatusTracking.service";
import requirementService from "src/modules/requirement/requirement.service";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

import {
  createRow,
  rowsFromRequirements,
  rowsFromRecord,
  dedupeOptions,
  rowHasEnteredValues,
  findDuplicateRowIndex,
  validateDraft,
  buildPayload,
  parseServerErrors,
  fromApiDate
} from "../utils/trackingForm.js";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const props = defineProps({
  recordId: { type: String, default: "" },
  readonly: { type: Boolean, default: false },
  // Called after every successful batch save (including Save and Continue) so the list can refresh.
  onSaved: { type: Function, default: null }
});

// Common variables
const $q = useQuasar();
const loading = ref(false);
const recordLoadError = ref(false);
const processing = ref(false);
const activeButton = ref("");
const showErrors = ref(false);
const serverErrors = ref([]);
const { fonts, toolbar } = getEditorConfig($q);

const model = ref({
  recordId: props.recordId || "",
  projectId: "",
  projectName: "",
  projectModuleId: "",
  projectModuleName: "",
  trackingDate: ""
});
const rows = ref([]);
const removedRowIds = ref([]);
const isEdit = computed(() => !!model.value.recordId);

const columns = [
  { name: "requirement", label: "Requirement", field: "requirementId", align: "left" },
  { name: "currentPercent", label: "Current Completion(%)", field: "currentPercent", align: "left" },
  { name: "timeSpentAsOfToday", label: "Time Spent as of Today(Hours)", field: "timeSpentAsOfToday", align: "left" },
  { name: "estimatedRemainingTime", label: "Est. Remaining Time Required(Hours)", field: "estimatedRemainingTime", align: "left" },
  { name: "scope", label: "Additional Scope", field: "scope", align: "left" },
  { name: "estimatedTimeForCompletion", label: "Est. Time for Completion of Scope(Hours)", field: "estimatedTimeForCompletion", align: "left" },
  { name: "action", label: "Action", field: "action", align: "center" }
];

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { projectNameDropdownSingleSelect } = projectModule();
const { projectModulesByProjectIdForDropdownSingleSelect } = projectModuleOfProjectModule();

// Requirements of the selected module (all types; no Data Mapping rows needed).
const requirementOptions = ref([]);
const requirementLoadState = ref("idle"); // idle | loading | loaded | empty | error
let requirementRequestSeq = 0;
let moduleRequestSeq = 0;

const validation = computed(() => validateDraft(model.value, rows.value));
const rowError = (row, field) => validation.value.rows[row.key]?.[field] || "";

// ----------------------------------------------------------------------------------------------------------------
// Lookups
// ----------------------------------------------------------------------------------------------------------------

async function loadModules (projectId) {
  const seq = ++moduleRequestSeq;
  projectModulesByProjectIdForDropdownSingleSelect.list.value = [];
  await projectModulesByProjectIdForDropdownSingleSelect.load(false, false, projectId);
  // A newer project selection won the race; reload for it so the module list matches.
  if (seq !== moduleRequestSeq) return;
  if (model.value.projectId && model.value.projectId !== projectId) await loadModules(model.value.projectId);
}

// populateRows is only true for a new record whose context the user just chose; saved rows are never regenerated.
async function loadRequirements (populateRows) {
  const { projectId, projectModuleId } = model.value;
  const seq = ++requirementRequestSeq;
  if (!projectId || !projectModuleId) {
    requirementOptions.value = [];
    requirementLoadState.value = "idle";
    return;
  }

  requirementLoadState.value = "loading";
  try {
    const resp = await requirementService.getAllRequirementListForDropdown(projectModuleId, projectId);
    if (seq !== requirementRequestSeq) return; // stale response for an earlier selection

    const items = Array.isArray(resp) ? resp : (resp?.data || []);
    requirementOptions.value = dedupeOptions(items.map(item => ({ text: item.text, value: item.value })));
    requirementLoadState.value = requirementOptions.value.length ? "loaded" : "empty";
    if (populateRows) rows.value = rowsFromRequirements(requirementOptions.value);
  } catch (error) {
    // The http interceptor already shows the server error notification.
    if (seq === requirementRequestSeq) requirementLoadState.value = "error";
  }
}

// ----------------------------------------------------------------------------------------------------------------
// Context changes (new records only; project/module are read-only when editing a saved record)
// ----------------------------------------------------------------------------------------------------------------

function confirmDiscardRows () {
  if (!rows.value.some(rowHasEnteredValues)) return Promise.resolve(true);
  return new Promise(resolve => {
    $q.dialog({
      title: "Replace requirement rows?",
      message: "Changing the project or module reloads the requirement rows. Values entered in the current rows will be discarded.",
      cancel: { label: "Keep current rows", flat: true, noCaps: true },
      ok: { label: "Replace rows", color: "primary", noCaps: true },
      persistent: true
    }).onOk(() => resolve(true)).onCancel(() => resolve(false));
  });
}

async function onProjectChange (projectId) {
  if ((projectId || "") === model.value.projectId) return;
  if (!(await confirmDiscardRows())) return;

  model.value.projectId = projectId || "";
  model.value.projectModuleId = "";
  rows.value = [];
  requirementOptions.value = [];
  requirementRequestSeq++;
  requirementLoadState.value = "idle";
  if (projectId) await loadModules(projectId);
  else projectModulesByProjectIdForDropdownSingleSelect.list.value = [];
}

async function onModuleChange (projectModuleId) {
  if ((projectModuleId || "") === model.value.projectModuleId) return;
  if (!(await confirmDiscardRows())) return;

  model.value.projectModuleId = projectModuleId || "";
  rows.value = [];
  await loadRequirements(true);
}

// ----------------------------------------------------------------------------------------------------------------
// Rows
// ----------------------------------------------------------------------------------------------------------------

const rowFilters = ref({});

function rowRequirementFilter (row) {
  return (val, update) => update(() => { rowFilters.value[row.key] = (val || "").toLowerCase(); });
}

function rowRequirementOptions (row) {
  const usedByOthers = new Set(rows.value.filter(r => r.key !== row.key).map(r => r.requirementId));
  const options = requirementOptions.value.map(option => ({ ...option, disable: usedByOthers.has(option.value) }));
  // Keep a saved requirement that is no longer in the module so its label still renders.
  if (row.requirementId && !options.some(option => option.value === row.requirementId)) {
    options.unshift({ text: row.requirementTitle || row.requirementId, value: row.requirementId, disable: false });
  }
  const needle = rowFilters.value[row.key];
  return needle ? options.filter(option => option.value === row.requirementId || option.text?.toLowerCase().includes(needle)) : options;
}

function onRowRequirementChange (row, requirementId) {
  const duplicateIndex = findDuplicateRowIndex(rows.value, row.key, requirementId);
  if (duplicateIndex !== -1) {
    notifyWarning({ message: `This requirement is already selected in row ${duplicateIndex + 1}. Each requirement can be tracked only once per record.` });
    return;
  }
  const option = requirementOptions.value.find(o => o.value === requirementId);
  row.requirementId = requirementId || "";
  row.requirementTitle = option?.text || "";
  row.isRequirementUnavailable = false;
}

function onAddLine () {
  rows.value.push(createRow(null, true));
}

// Removal is staged: persisted rows are only deleted by the next successful batch save.
function onRemoveRow (row) {
  if (row.id) removedRowIds.value.push(row.id);
  rows.value = rows.value.filter(r => r.key !== row.key);
}

// ----------------------------------------------------------------------------------------------------------------
// Save and Continue / Save and Close: one request for the whole batch
// ----------------------------------------------------------------------------------------------------------------

function applySavedRecord (record) {
  model.value.recordId = record.recordId;
  model.value.projectName = record.projectName || model.value.projectName;
  model.value.projectModuleName = record.projectModuleName || model.value.projectModuleName;
  model.value.trackingDate = fromApiDate(record.trackingDate);
  rows.value = rowsFromRecord(record);
  removedRowIds.value = [];
  showErrors.value = false;
}

async function onSubmit (shouldClose, buttonType) {
  if (processing.value || props.readonly) return;

  showErrors.value = true;
  serverErrors.value = [];
  if (!validation.value.isValid) {
    notifyError({ message: validation.value.messages[0] || "Please fill the required fields before saving." });
    return;
  }

  processing.value = true;
  activeButton.value = buttonType;
  try {
    const payload = buildPayload(model.value, rows.value, removedRowIds.value);
    const saved = model.value.recordId
      ? await requirementStatusTrackingService.bulkSaveRequirementStatusTracking(payload)
      : await requirementStatusTrackingService.bulkAddRequirementStatusTracking(payload);

    // Keep the returned record/row ids so the next save updates this record instead of creating another.
    if (saved?.recordId) applySavedRecord(saved);

    notifySuccess({ message: "Requirement status tracking is saved successfully." });
    props.onSaved?.();
    if (shouldClose) onDialogOK();
  } catch (error) {
    // Draft is kept as-is; the interceptor already showed the error notification.
    serverErrors.value = parseServerErrors(error);
  } finally {
    processing.value = false;
    activeButton.value = "";
  }
}

// ----------------------------------------------------------------------------------------------------------------
// On page rendering: hydrate a saved record before any lookup can touch its rows
// ----------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  projectNameDropdownSingleSelect.load();
  if (!props.recordId) return;

  loading.value = true;
  try {
    const record = await requirementStatusTrackingService.getRequirementStatusTrackingById(props.recordId);
    model.value = {
      recordId: record.recordId,
      projectId: record.projectId,
      projectName: record.projectName,
      projectModuleId: record.projectModuleId,
      projectModuleName: record.projectModuleName,
      trackingDate: fromApiDate(record.trackingDate)
    };
    rows.value = rowsFromRecord(record);
    // Options only (for Add New Line and labels); saved rows are kept as loaded.
    await loadRequirements(false);
  } catch (error) {
    recordLoadError.value = true;
  } finally {
    loading.value = false;
  }
});
</script>

<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.q-editor__content img {
  max-width: 100%;
  height: auto;
  display: block;
}

</style>
