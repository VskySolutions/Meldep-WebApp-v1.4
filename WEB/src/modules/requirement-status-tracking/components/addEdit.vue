<template>
  <q-dialog ref="dialogRef" class="customDialog PlannerDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ props.requirementId ? "Edit" : "Add" }} Requirement Status Tracking</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogCancel">
          <q-tooltip>Close</q-tooltip>
        </q-btn>
      </q-card-section>
      <q-separator />
      <q-form ref="formRef" greedy @submit.prevent.stop>
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Tracking Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.projectId"
                    label="Project Name"
                    wrapper-class=""
                    :readonly="!!readonlyProject"
                    :options="projectNameDropdownSingleSelect.list.value"
                    :filter="projectNameDropdownSingleSelect.filter"
                    :error="showErrors && !!validation.header.projectId"
                    :error-message="validation.header.projectId"
                    @update:model-value="onProjectChange"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.projectModuleId"
                    label="Project Module"
                    wrapper-class=""
                    :disable="(model.projectId?.length || 0) === 0"
                    :readonly="!!readonlyProjectModule"
                    :options="projectModulesByProjectIdForDropdownSingleSelect.list.value"
                    :filter="projectModulesByProjectIdForDropdownSingleSelect.filter"
                    :error="showErrors && !!validation.header.projectModuleId"
                    :error-message="validation.header.projectModuleId"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4">
                  <div class="row items-center no-wrap">
                    <div class="col">
                      <formSingleSelectDropdown
                        v-model="model.requirementId"
                        label="Requirement"
                        :disable="(model.projectModuleId?.length || 0) === 0"
                        :readonly="!!readonlyRequirement"
                        :required="true"
                        :options="requirementsByProjectModuleIdForDropdown.list.value"
                        :filter="requirementsByProjectModuleIdForDropdown.filter"
                      />
                    </div>
                    <div class="col-auto q-ml-sm q-mt-md">
                      <q-btn
                        v-if="model.requirementId"
                        flat
                        round
                        dense
                        color="primary"
                        icon="o_visibility"
                        size="sm"
                        @click="onRequirementView(model.requirementId)"
                      >
                        <q-tooltip>View Requirement</q-tooltip>
                      </q-btn>
                    </div>
                  </div>
                </div>
              </div>
              <div class="row justify-end q-mb-sm">
                <q-btn
                  color="primary"
                  icon="o_add"
                  label="Add"
                  no-caps
                  class="hidden"
                  :disable="!model.projectModuleId"
                  @click="onAddLine"
                >
                  <q-tooltip v-if="!model.projectModuleId">Select a project module first</q-tooltip>
                </q-btn>
              </div>

              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                virtual-scroll
                bordered
                row-key="key"
                class="no-shadow"
                :loading="loading"
                :rows="rows"
                :columns="columns"
                separator="cell"
                binary-state-sort
              >
                <template #header="headerProps">
                  <q-tr :props="headerProps" class="bg-primary text-white">
                    <q-th v-for="col in headerProps.cols" :key="col.name" class="text-start" :style="col.headerStyle">
                      {{ col.label }}<span v-if="col.name === 'trackingDate'" class="required">*</span>
                      <q-icon v-if="col.tooltip" name="o_info" size="xs" class="q-mx-xs">
                        <q-tooltip class="text-caption">
                          {{ col.tooltip }}
                        </q-tooltip>
                      </q-icon>
                    </q-th>
                  </q-tr>
                </template>
                <template #body="bodyProps">
                  <q-tr :props="bodyProps">
                    <q-td key="trackingDate" :props="bodyProps" style="width: 150px; max-width: 150px;">
                      <formDate
                        v-model="bodyProps.row.trackingDate"
                        wrapper-class="col-12 col-sm-4 col-md-4"
                        :date-options="getDateOptions(bodyProps.row)"
                        :error="showErrors && !!rowError(bodyProps.row, 'trackingDate')"
                        :error-message="rowError(bodyProps.row, 'trackingDate')"
                        @update:model-value="getRequirementTimeSpentByRequirementIdAndDate(bodyProps.row)"
                      />
                    </q-td>
                    <q-td key="currentPercent" style="width: 90px; max-width: 90px;">
                      <q-input
                        v-model.number="bodyProps.row.currentPercent"
                        type="text"
                        inputmode="decimal"
                        outlined dense hide-bottom-space
                        aria-label="Current Completion (%)"
                        :error="showErrors && !!rowError(bodyProps.row, 'currentPercent')"
                        :error-message="rowError(bodyProps.row, 'currentPercent')"
                      />
                    </q-td>
                    <q-td key="timeSpentAsOfToday" :props="bodyProps" style="width: 100px; max-width: 100px;">
                      <q-input
                        v-model="bodyProps.row.timeSpentAsOfToday"
                        aria-label="Time Spent as of Now (Hours)"
                        outlined
                        dense
                        hide-bottom-space
                        maxlength="5"
                        mask="##:##"
                        @blur="formatHoursValue(bodyProps.row, 'timeSpentAsOfToday')"
                        :rules="[validateHours]"
                        :error="showErrors && !!rowError(bodyProps.row, 'timeSpentAsOfToday')"
                        :error-message="rowError(bodyProps.row, 'timeSpentAsOfToday')"
                      >
                        <template #hint>
                          <span
                            v-if="bodyProps.row.timeSpentAsOfToday && validateHours(bodyProps.row.timeSpentAsOfToday) === true && bodyProps.row.timeSpentAsOfToday !== '00:00'"
                            class="text-caption text-primary"
                          >
                            {{ getHoursMinutesText(bodyProps.row.timeSpentAsOfToday) }}
                          </span>
                          <span v-else>
                            hh:mm
                          </span>
                        </template>
                      </q-input>
                    </q-td>
                    <q-td key="estimatedRemainingTime" :props="bodyProps" style="width: 100px; max-width: 100px;">
                      <q-input
                        v-model="bodyProps.row.estimatedRemainingTime"
                        aria-label="Estimated Remaining Time Required (Hours)"
                        outlined
                        dense
                        hide-bottom-space
                        maxlength="5"
                        mask="##:##"
                        @blur="formatHoursValue(bodyProps.row, 'estimatedRemainingTime')"
                        :rules="[validateHours]"
                        :error="showErrors && !!rowError(bodyProps.row, 'estimatedRemainingTime')"
                        :error-message="rowError(bodyProps.row, 'estimatedRemainingTime')"
                      >
                       <template #hint>
                          <span
                            v-if="bodyProps.row.estimatedRemainingTime && validateHours(bodyProps.row.estimatedRemainingTime) === true && bodyProps.row.estimatedRemainingTime !== '00:00'"
                            class="text-caption text-primary"
                          >
                            {{ getHoursMinutesText(bodyProps.row.estimatedRemainingTime) }}
                          </span>
                          <span v-else>
                            hh:mm
                          </span>
                        </template>
                      </q-input>
                    </q-td>
                    <q-td key="scope" :props="bodyProps" style="width: 350px; max-width: 350px;">
                      <q-editor
                        v-model="bodyProps.row.scope"
                        :dense="$q.screen.lt.md"
                        :toolbar="toolbar"
                        :fonts="fonts"
                      />
                    </q-td>
                     <q-td key="roadblock" :props="bodyProps" style="width: 350px; max-width: 350px;">
                      <q-editor
                        v-model="bodyProps.row.roadblock"
                        :dense="$q.screen.lt.md"
                        :toolbar="toolbar"
                        :fonts="fonts"
                      />
                    </q-td>
                    <q-td key="estimatedTimeForCompletion" :props="bodyProps" style="width: 100px; max-width: 100px;">
                      <q-input
                        v-model="bodyProps.row.estimatedTimeForCompletion"
                        aria-label="Estimated Time for Completion of Additional Scope (Hours)"
                        outlined
                        dense
                        hide-bottom-space
                        maxlength="5"
                        mask="##:##"
                        @blur="formatHoursValue(bodyProps.row, 'estimatedTimeForCompletion')"
                        :rules="[validateHours]"
                        :error="showErrors && !!rowError(bodyProps.row, 'estimatedTimeForCompletion')"
                        :error-message="rowError(bodyProps.row, 'estimatedTimeForCompletion')"
                      >
                        <template #hint>
                          <span
                            v-if="bodyProps.row.estimatedTimeForCompletion && validateHours(bodyProps.row.estimatedTimeForCompletion) === true && bodyProps.row.estimatedTimeForCompletion !== '00:00'"
                            class="text-caption text-primary"
                            >
                              {{ getHoursMinutesText(bodyProps.row.estimatedTimeForCompletion) }}
                          </span>
                          <span v-else>
                            hh:mm
                          </span>
                        </template>
                      </q-input>
                    </q-td>
                    <q-td key="action" :props="bodyProps" class="text-center" style="width: 50px; max-width: 50px;">
                      <q-btn
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

              <div align="center" class="stickyFooter q-gutter-sm justify-center q-mt-sm">
                <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
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
                    :disable="processing"
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
                    :disable="processing"
                    @click="onSubmit(true, 'close')"
                  />
              </div>
            </fieldset>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import { ref, computed, onMounted, watch } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import { isDate } from "validators/zw_validators.js";
import { format } from "date-fns";

import requirementStatusTrackingService from "../requirementStatusTracking.service";

import {
  createRow,
  rowsFromRecord,
  validateDraft,
  buildPayload,
  parseServerErrors,
} from "../utils/trackingForm.js";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";

import {
  initRequirementDialogs,
  onRequirementView
} from "src/modules/requirement/utils/dialogs.js";

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const props = defineProps({
  projectId: { type: String, default: "" },
  projectModuleId: { type: String, default: "" },
  requirementId: { type: String, default: "" },
  // Called after every successful batch save (including Save and Continue) so the list can refresh.
  onSaved: { type: Function, default: null }
});

const readonlyProject = props.projectId ? "readonly" : "";
const readonlyProjectModule = props.projectModuleId ? "readonly" : "";
const readonlyRequirement= props.requirementId ? "readonly" : "";

// Common variables
const $q = useQuasar();
const loading = ref(false);
const recordLoadError = ref(false);
const processing = ref(false);
const activeButton = ref("");
const showErrors = ref(false);
const activeRowId = ref(null);
const serverErrors = ref([]);
// const { fonts, toolbar } = getEditorConfig($q);

const model = ref({
  recordId: "",
  projectId: props.projectId || "",
  projectModuleId: props.projectModuleId || "",
  projectModuleName: "",
  requirementId: props.requirementId || "",
});

const rows = ref([]);
const removedRowIds = ref([]);
const pagination = ref({ sortBy: "", descending: false, rowsPerPage: 20, page: 1 });
const columns = [
  { name: "trackingDate", label: "Tracking Date", field: "trackingDate", align: "left" },
  { name: "currentPercent", label: "Current Completion(%)", field: "currentPercent", align: "left" },
  { name: "timeSpentAsOfToday", label: "Time Spent (Hrs)", field: "timeSpentAsOfToday", align: "left", tooltip: "Time Spent as of Today(Hours)" },
  { name: "estimatedRemainingTime", label: "Est. Remaining (Hrs)", field: "estimatedRemainingTime", align: "left", tooltip: "Estimated Remaining Time Required (Hours)" },
  { name: "scope", label: "Current Status", field: "scope", align: "left" },
  { name: "roadblock", label: "Roadblock", field: "roadblock", align: "left" },
  { name: "estimatedTimeForCompletion", label: "Est. Completion Time (Hrs)", field: "estimatedTimeForCompletion", align: "left", tooltip: "Estimated Time for Completion of Additional Scope (Hours)"},
  { name: "action", label: "Action", field: "action", align: "center" }
];

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initRequirementDialogs(activeRowId);

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { projectNameDropdownSingleSelect } = projectModule();
const { requirementsByProjectModuleIdForDropdown } = requirementModule();
const { projectModulesByProjectIdForDropdownSingleSelect } = projectModuleOfProjectModule();

const validation = computed(() => validateDraft(model.value, rows.value));
const rowError = (row, field) => validation.value.rows[row.key]?.[field] || "";

const getRequirementStatusTrackingByRequirementId = async (requirementId) => {
  try {
    loading.value = true;
    recordLoadError.value = false;

    const resp =
      await requirementStatusTrackingService.getRequirementStatusTrackingByRequirementId(
        requirementId
      );

    const trackingRecords =
      resp?.requirementStatusTrackingRecordsList || [];

    if (trackingRecords.length > 0) {
      const record = trackingRecords[0];

      // Populate project and module details
      model.value = {
        ...model.value,
        recordId: record.id || "",
        projectId: record.projectId || "",
        projectModuleId: record.projectModuleId || "",
        requirementId: record.requirementId || ""
      };

      rows.value = trackingRecords.map(item => ({
        key: item.id ? item.id : newRowKey(),
        id: item.id || null,
        trackingId: item.id || null,
        trackingDate: item.trackingDate,
        currentPercent: item.currentPercent ?? null,
        timeSpentAsOfToday: item.timeSpentAsOfToday ?? null,
        estimatedRemainingTime: item.estimatedRemainingTime ?? null,
        scope: item.scope ?? "",
        roadblock: item.roadblock ?? "",
        estimatedTimeForCompletion: item.estimatedTimeForCompletion ?? null,
        isRequirementUnavailable: false
      }));
      showErrors.value = false;
      const today = format(new Date(), "MM/dd/yyyy");
      rows.value.unshift({
        ...createRow(),
        trackingDate: rows.value.some(row => row.trackingDate === today)
          ? ""
          : today
      });
    }
  } catch (error) {
    console.error("Error loading requirement status tracking:", error);
    recordLoadError.value = true;
  } finally {
    loading.value = false;
  }
};

const getRequirementTimeSpentByRequirementIdAndDate = (row) => {
  if (
    !row.trackingDate ||
    !model.value.requirementId ||
    !isDate(row.trackingDate)
  ) return;

    // Prevent duplicate API calls for the same tracking date.
  if (row.lastRequestedTrackingDate === row.trackingDate) return;

  row.lastRequestedTrackingDate = row.trackingDate;

  requirementStatusTrackingService
    .getRequirementTimeSpentByRequirementIdAndDate(
      model.value.requirementId,
      row.trackingDate
    )
    .then(resp => {
    if (resp == null || Number(resp) === 0) {
      row.timeSpentAsOfToday = "";
      return;
    }

    const totalMinutes = Math.round(Number(resp) * 60);
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    row.timeSpentAsOfToday =
      `${String(hours).padStart(2, "0")}:${String(minutes).padStart(2, "0")}`;
  });
};

function getHoursMinutesText(value) {
  if (!value) return "";

  value = value.trim();
  if (/^\d{1,2}:?$/.test(value)) {
    const hrs = parseInt(value, 10);
    return hrs > 0 ? `${hrs} hr${hrs > 1 ? "s" : ""}` : "";
  }

  const match = value.match(/^(\d{1,2}):(\d{1,2})$/);

  if (!match) return "";

  const hrs = parseInt(match[1], 10);
  let mins = parseInt(match[2], 10);

  if (match[2].length === 1) {
    mins *= 10;
  }

  if (hrs > 99 || mins > 59 || (hrs === 0 && mins === 0)) {
    return "";
  }

  const hrText = hrs > 0 ? `${hrs} hr${hrs > 1 ? "s" : ""}` : "";
  const minText = mins > 0 ? `${mins} min` : "";

  return [hrText, minText].filter(Boolean).join(" ");
}

function formatHoursValue(row, field) {
  if (!row[field]) return;

  const value = String(row[field]).trim();
  const [hours, minutes = "00"] = value.split(":");

  row[field] = `${hours.padStart(2, "0")}:${minutes.padEnd(2, "0")}`;
}

function validateHours(value) {
  const strValue = String(value ?? "").trim();

  if (!strValue) return true;
  if (/^\d{1,2}:?$/.test(strValue)) {
    return true;
  }

  const match = strValue.match(/^(\d{1,2}):(\d{1,2})$/);

  if (!match) return "Invalid hours format.";

  const hours = Number(match[1]);
  const minutes = Number(match[2]);

  if (minutes > 59) return "Minutes can't exceed 59.";

  return true;
}

function getDateOptions(currentRow) {
  const existingDates = rows.value
    .filter(row => row.key !== currentRow.key && row.trackingDate)
    .map(row => {
      // Convert MM/DD/YYYY to YYYY/MM/DD for QDate.
      const [month, day, year] = row.trackingDate.split("/");
      return `${year}/${month}/${day}`;
    });

  return date => !existingDates.includes(date);
}

// Removal is staged: persisted rows are only deleted by the next successful batch save.
function onRemoveRow(row) {
  if (row.id && !row.deleted) {
    removedRowIds.value.push(row.id);
  }
  rows.value = rows.value.filter(r => r.key !== row.key);
}

// ----------------------------------------------------------------------------------------------------------------
// Save and Continue / Save and Close: one request for the whole batch
// ----------------------------------------------------------------------------------------------------------------

function applySavedRecord (record) {
  model.value.recordId = record.recordId;
  rows.value = rowsFromRecord(record);
  removedRowIds.value = [];
  showErrors.value = false;
}

async function onSubmit (shouldClose, buttonType) {
  if (processing.value) return;

  showErrors.value = true;
  serverErrors.value = [];
  if (!validation.value.isValid) {
    // notifyError({ message: validation.value.messages[0] || "Please fill the required fields before saving." });
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
    removedRowIds.value = [];
    notifySuccess({ message: "Requirement status tracking is saved successfully." });
    await getRequirementStatusTrackingByRequirementId(model.value.requirementId);
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

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Rich Editor Tools: Fonts and Toolbar
// --------------------------------------------------------------------------------------------------------------------------------------------------
const fonts = {
  arial: "Arial",
  arial_black: "Arial Black",
  comic_sans: "Comic Sans MS",
  courier_new: "Courier New",
  impact: "Impact",
  lucida_grande: "Lucida Grande",
  times_new_roman: "Times New Roman",
  verdana: "Verdana"
};

const toolbar = [
  [
    {
      label: $q.lang.editor.align,
      icon: $q.iconSet.editor.align,
      fixedLabel: true,
      list: "only-icons",
      options: ["left", "center", "right", "justify"]
    }
  ],
   ['bold', 'italic', 'strike', 'underline'],
   ['unordered', 'ordered']
];


// ----------------------------------------------------------------------------------------------------------------
// On page rendering: hydrate a saved record before any lookup can touch its rows
// ----------------------------------------------------------------------------------------------------------------


watch(() => props.requirementId, (newValue, oldValue) => {
  if (newValue) {
    getRequirementStatusTrackingByRequirementId(props.requirementId);
  }
}, { immediate: true });

watch(
  () => model.value.requirementId,
  (newId, oldId) => {
    if (newId && newId !== oldId) {
      getRequirementStatusTrackingByRequirementId(newId);
    } else if (!newId) {
      rows.value = [];
    }
  }
);

watch(
  () => model.value.requirementId,
  (newId) => {
    activeRowId.value = newId || null;
  },
  { immediate: true }
);

watch(
  () => model.value.projectId,
  (projectId) => {
    if ((projectId?.length || 0) > 0) {
      projectModulesByProjectIdForDropdownSingleSelect.load(
        false,
        false,
        projectId
      );
    } else {
      model.value.projectModuleId = null;
      projectModulesByProjectIdForDropdownSingleSelect.list.value = [];
      requirementsByProjectModuleIdForDropdown.list.value = [];
    }
  },
  { immediate: true }
);

watch(
  () => model.value.projectModuleId,
  (projectModuleId) => {
    if ((projectModuleId?.length || 0) > 0 && (model.value.projectId?.length || 0) > 0) {
      requirementsByProjectModuleIdForDropdown.load(
        projectModuleId,
        model.value.projectId
      );
    } else {
      model.value.requirementId = "";
      requirementsByProjectModuleIdForDropdown.list.value = [];
    }
  },
  { immediate: true }
);


onMounted(async () => {
  projectNameDropdownSingleSelect.load();
  rows.value.unshift(createRow());
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
