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
              <q-breadcrumbs-el label="My Work" />
              <q-breadcrumbs-el label="Add Weekly Timesheets" />
            </q-breadcrumbs>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="row items-center justify-between q-pa-sm">
        <div class="row items-center q-gutter-md">
          <formDate
            v-model="selectedWeekLabel"
            label="Select Weekend"
            mask="MM/DD/YYYY"
            :dateOptions="isSunday"
            :required="false"
            @update:model-value="onWeekSelect"
            :wrapperClass="'col-auto'"
          />
          <div
            v-if="entryRows.length && selectedWeekStatus?.dropDownValue"
            class="col-auto"
          >
            <div class="label text-black">
              Timesheet Status
            </div>
            <q-badge
              rounded
              class="text-h6 q-px-sm q-py-sm"
              :style="{
                color: selectedWeekStatus?.color,
                background: selectedWeekStatus?.bgColor
              }"
            >
              {{ selectedWeekStatus?.dropDownValue }}
            </q-badge>
          </div>
        </div>

        <!-- Right Side -->
        <q-btn
          v-if="selectedWeekLabel"
          color="primary"
          icon="o_add"
          label="Add"
          no-caps
          :disable="entryRows.length && isActionDisabled(entryRows[0], 'button')"
          @click="onAdd"
        />
      </div>
      <!-- Entry Rows -->
      <div class="q-pa-sm">
        <div class="timesheet-wrapper">
          <!-- Header -->
          <div class="row q-pa-sm bg-primary text-white sticky-header">
            <div class="col-xxl-2 col-lg-2 col-md-2 col-sm-3 col-xs-12">Project</div>
            <div class="col-xxl-2 col-lg-2 col-md-2 col-sm-3 col-xs-12">Task</div>
            <div v-for="day in weekDates" :key="day.date" class="col text-center">{{ day.label }}</div>
            <div class="col-1 text-center">Total</div>
            <div class="col-auto">Action</div>
          </div>

          <div class="timesheet-body">
            <div
              v-if="!entryRows.length"
              class="row q-pa-sm"
            >
              <div class="text-grey-6 text-h6 text-left">
                No data available.
              </div>
            </div>
            <div
              v-for="(row, index) in entryRows"
              :key="row.id"
              class="row items-center q-mb-xs q-mt-xs entry-row"
            >
              <!-- Project -->
              <div class="col-xxl-2 col-lg-2 col-md-2 col-sm-3 col-xs-12 q-pa-xs">
                {{ row.projectName }}
              </div>
              <!-- Task -->
               <div class="col-xxl-2 col-lg-2 col-md-2 col-sm-3 col-xs-12">
                  <formSingleSelectDropdown
                    v-model="row.taskId"
                    :options="projectTasksWithProjectForDropdown.list.value"
                    :filter="projectTasksWithProjectForDropdown.filter"
                    :disable="isActionDisabled(row, 'task')"
                    @update:model-value="(val) => onTaskSelect(val, row)"
                  />
               </div>
              <!-- Days -->
              <div
                v-for="(day, i) in weekDates"
                :key="i"
                class="col text-center q-ml-sm"
              >
                <q-input
                  v-model="row.hours[i]"
                  input-class="text-right"
                  maxlength="5"
                  mask="##:##"
                  dense
                  :readonly="isActionDisabled(row, 'hours', i)"
                   @blur="() => {
                    formatHoursValue(row, i);
                    onHoursChange(row, i);
                  }"
                  :rules="[validateHours]"
                  @focus="() => storePreviousHours(row, i)"
                >
                <template #hint>
                  <span
                    v-if="row.hours[i] && validateHours(row.hours[i]) === true && row.hours[i] !== '00:00'""
                    class="text-caption text-primary"
                  >
                    {{ getHoursMinutesText(row.hours[i]) }}
                  </span>

                  <span v-else>
                    hh:mm
                  </span>
                </template>
                  <!-- + icon -->
                  <q-icon
                    v-if="
                      row.hours[i] &&
                      row.hours[i] !== '00:00' &&
                      validateHours(row.hours[i]) === true &&
                      !isActionDisabled(row, 'description', i)
                    "
                    name="o_add_circle"
                    size="xs"
                    class="cursor-pointer q-mr-xs"
                  >
                    <q-tooltip>Add description</q-tooltip>
                    <q-popup-edit
                      v-model="row.description[i]"
                      anchor="center middle"
                      self="center middle"
                      buttons
                      persistent
                      label-set="Save"
                      label-cancel="Cancel"
                      class="instruction-popup"
                      @save="val => saveTimesheet(row, i, val)"
                    >
                      <template #default="scope">
                        <div class="popup-container q-pa-sm">
                          <!-- Close button -->
                          <q-btn
                            icon="o_close"
                            flat
                            round
                            dense
                            size="sm"
                            class="absolute-top-right"
                            @click="scope.cancel"
                          />
                          <!-- Title -->
                          <div class="text-subtitle2 q-mb-xs">Description</div>
                          <!-- Editor -->
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
                </q-input>
              </div>
              <!-- Total -->
              <div class="col-1 text-center">
                {{ getRowTotal(row) }}
              </div>
              <div class="col-auto">
                <q-icon
                  name="o_delete_outline"
                  size="xs"
                  :class="{
                    'cursor-pointer': !isActionDisabled(row, 'delete'),
                    'disabled': isActionDisabled(row, 'delete')
                  }"
                  color="negative"
                  @click="!isActionDisabled(row, 'delete') && onDeleteWeekTimesheet(row, index)"
                >
                  <q-tooltip>Delete Weekly Timesheet</q-tooltip>
                </q-icon>
              </div>
            </div>
          </div>
          <!-- TOTAL ROW -->
          <div v-if="selectedWeekLabel && entryRows.length > 0" class="row text-right text-weight-bold bg-purple-2 q-mt-xs q-pa-sm sticky-footer">
            <div class="col-xxl-2 col-lg-2 col-md-2 col-sm-3 col-xs-12"></div>
            <div class="col-xxl-2 col-lg-2 col-md-2 col-sm-3 col-xs-12">Total Hours:</div>
            <div
              v-for="(day, i) in weekDates"
              :key="i"
              class="col"
            >
              {{ getColumnTotal(i) }}
            </div>
            <div class="col-1 text-center">
              {{ getGrandTotal }}
            </div>
            <div class="col-auto"></div>
          </div>
        </div>
      </div>
      <div class="flex justify-center">
      <q-btn
        v-if="entryRows.length > 0"
        label="Submit For Approval"
        no-caps
        color="primary"
        class="actionBtn"
        :loading="processing"
        :disable="processing || isActionDisabled(entryRows[0], 'button')"
        @click="submitForApproval"
      />
    </div>

      <!-- Time entry details -->
      <div v-if="selectedWeekLabel && previewList.length > 0" class="q-pa-sm time-entry-section" >
        <h3 class="text-weight-bold q-pa-xs">Time Entry Details</h3>
        <div class="row q-pa-xs bg-primary text-white table-row sticky-header">
          <div class="" style="width: 10%;">Date</div>
          <div class="" style="width: 20%;">Project</div>
          <div class="" style="width: 25%;">Task</div>
          <div class="" style="width: 5%;">Time</div>
          <div class="" style="width: 40%;">Description</div>
        </div>
        <div class="scroll-area">
          <div v-for="item in previewList" :key="item.id" class="row border-bottom table-row" :class="{'bg-light-red': item.timesheetStatus?.dropDownValue === 'Declined' && item.isApproved === false }">
            <q-tooltip
              v-if="item.timesheetStatus?.dropDownValue === 'Declined' && item.isApproved === false"
            >
              This entry was declined. Please correct it and resubmit for approval.
            </q-tooltip>
            <div class="text-left" style="width: 10%;">{{ item.date }}</div>
            <div class="text-left" style="width: 20%;">{{ item.project }}</div>
            <div class="text-left" style="width: 25%;">{{ item.task }}</div>
            <div class="text-right" style="width: 5%;">{{ item.hours }}</div>
            <div class="text-black RichTextEditor text-left" style="width: 40%;">
              <p v-html="item.description" />
            </div>
          </div>
        </div>
      </div>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, onBeforeUnmount, computed, watch } from "vue";
import { uid, useQuasar } from "quasar";
import { setLocalStorage, getLocalStorage, notifySuccess, notifyError, zwConfirmDelete, notifyWarning, zwConfirm } from "assets/utils";
import { debounce } from "lodash";

import timesheetService from "modules/timesheet/timesheet.service";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// Shared Dropdowns
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const previewList = ref([]);
const selectedWeekLabel = ref("");
const weekDates = ref([]);
const entryRows = ref([]);
const $q = useQuasar();
const processing = ref(false);
const { fonts, toolbar } = getEditorConfig($q);
const selectedWeekStatus = ref(null);
// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "Weekly Timesheet";
const filterLocalStorage = getLocalStorage(localStorageKey);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Timesheet
// ----------------------------------------------------------------------------------------------------------------

function mapTimesheetToEntryRows (data) {
  const grouped = {};
  data.forEach(item => {
    // const key = item.projectTaskId;
    const key = item.projectTaskId + "_" + item.id;
    // create row if not exists
    if (!grouped[key]) {
      grouped[key] = {
        id: item.id,
        taskId: item.projectTaskId,
        taskName: item.task?.name,
        projectId: item.projectId,
        projectName: item.project?.name,
        projectModuleId: item.projectModule?.id,
        timesheetStatus: item.timesheetStatus ?? null,
        hours: weekDates.value.map(() => "00:00"),
        lineIds: weekDates.value.map(() => null),
        description: weekDates.value.map(() => ""),
        timesheetIds: weekDates.value.map(() => null),
        isApproved: weekDates.value.map(() => false)
      };
    }
    const itemDate = formatDate(item.timesheetDate);
    // find correct day index
    const index = weekDates.value.findIndex(
      d => d.date === itemDate
    );

    if (index !== -1) {
      grouped[key].hours[index] = item.hours;
      grouped[key].lineIds[index] = item.id; // store per day id
      grouped[key].description[index] = item.description; // store per day description
      grouped[key].timesheetIds[index] = item.timesheetId; // store per day id
      grouped[key].isApproved[index] = item.isApproved;
    }
  });
  entryRows.value = Object.values(grouped);
}

// Get all Timesheet data and map
const getTimesheet = async () => {
  const payload = {
    fromDate: weekDates.value[0]?.date,
    toDate: weekDates.value[weekDates.value.length - 1]?.date
  };

  const resp = await timesheetService.getAllTimesheetByWeek(payload);
  selectedWeekStatus.value = resp.length > 0 ? resp[0].timesheetStatus : null;
  const lines = resp.flatMap(x => {
  return (x.timesheetLines || []).map(line => {
    return {
      ...line,
      timesheetDate: x.timesheetDate,
      timesheetStatus: x.timesheetStatus,
      timesheetId: x.id,
    };
  });
});

  // map timesheet data to entryRows
  mapTimesheetToEntryRows(lines);
  // preview mapping
  mapToPreviewList(lines);
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

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const highlightProjectId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightProjectId);

const handleDocumentClick = (event) => {
  const highlightElement = document.querySelector(".highlight");
  // Check if clicked inside the highlighted row or icons
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;
    const storedData = getLocalStorage(localStorageKey) || {};
    setLocalStorage(localStorageKey, { ...storedData, activeRowId: null });
  }
};

// add new row
function onAdd () {
  entryRows.value.unshift({
    id: uid(),
    taskId: null,
    taskName: "",
    projectId: null,
    projectName: "",
    projectModuleId: null,
    hours: weekDates.value.map(() => 0),
    lineIds: weekDates.value.map(() => null),
    description: weekDates.value.map(() => ""),
    timesheetIds: weekDates.value.map(() => null),
    showDescIndex: null,
    deleted: false
  });
}

// Week select (Sunday)
function onWeekSelect (val) {
  selectedWeekLabel.value = val;

  const start = new Date(val);
  start.setDate(start.getDate() - 6);

  const end = new Date(val);

  weekDates.value = [];

  for (let i = 0; i < 7; i++) {
    const d = new Date(start);
    d.setDate(start.getDate() + i);

    weekDates.value.push({
      date: formatDate(d),
      label: `${d.toLocaleDateString("en-US", { weekday: "short" })} ${String(d.getDate()).padStart(2, "0")}`
    });
  }

  getTimesheet(start, end);
}

// Only allow Sundays and today/past
const isSunday = (dateStr) => {
  const day = new Date(dateStr);
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  // Get current week's Sunday
  const currentSunday = new Date(today);
  currentSunday.setDate(today.getDate() + (7 - today.getDay()));
  currentSunday.setHours(0, 0, 0, 0);
  return (
    day.getDay() === 0 && // only Sunday
    day <= currentSunday // only past + today
  );
};

const validateWeeklyTimesheetHours = () => {
  // Validate only Monday to Friday (indexes 0-4)
  for (let i = 0; i < 5; i++) {
    const totalHours = getColumnTotal(i);

    if (Number(totalHours) <= 0) {
      notifyError({
        message: 'Please enter hours for all timesheet dates from Monday to Friday before submitting for approval.'
      });
      return false;
    }
  }

  return true;
};

function validateHours(value) {
  const strValue = String(value ?? "").trim();

  if (!strValue) return true;
  if (/^\d{1,2}:?$/.test(strValue)) {
    return true;
  }

  const match = strValue.match(/^(\d{1,2}):(\d{1,2})$/);

  if (!match) return "Invalid hours format.";

  return Number(match[2]) > 59
    ? "Minutes can't exceed 59."
    : true;
}

function formatHoursValue(row, index) {
  if (!row.hours[index]) return;

  let value = row.hours[index].trim();
  if (!value.includes(":")) {
    row.hours[index] = `${value.padStart(2, "0")}:00`;
    return;
  }

  const parts = value.split(":");
  if (parts.length !== 2) return;
  let [hours, minutes] = parts;
  if (!minutes) {
    minutes = "00";
  }
  else if (minutes.length === 1) {
    minutes += "0";
  }

  row.hours[index] = `${hours.padStart(2, "0")}:${minutes}`;
}

function getCurrentWeekEndSunday () {
  const today = new Date();

  const day = today.getDay(); // 0 = Sunday
  const sunday = new Date(today);

  // move forward to upcoming Sunday
  sunday.setDate(today.getDate() + (7 - day));

  return formatDate(sunday);
}

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// let timesheetId = null;
function formatDate (date) {
  const d = new Date(date);

  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");

  return `${month}/${day}/${year}`; // safe local date
}

function onTaskSelect (taskId, row) {
  const selected = projectTasksWithProjectForDropdown.list.value.find(
    x => x.value === taskId
  );

  if (!selected) return;

  const parts = selected.text.split("/");
  row.projectModuleName = parts[0];
  row.taskName = parts[1];
  row.projectTaskNumber = parts[2];
  row.projectName = parts[3];

  row.hours.forEach((h, i) => {
    if (h > 0) {
      debouncedSave(row, i);
    }
  });
}

function roundToTwo (num) {
  return Math.round((num + Number.EPSILON) * 100) / 100;
}

function calculateHoursTotal(hours) {
  const totalMinutes = hours.reduce((total, value) => {
    if (!value) return total;

    const time = value.toString().trim();

    if (time.includes(":")) {
      const [hours, minutes] = time.split(":").map(Number);
      return total + (hours || 0) * 60 + (minutes || 0);
    }

    return total + (Number(time) || 0) * 60;
  }, 0);

  return `${String(Math.floor(totalMinutes / 60)).padStart(2, "0")}:${String(
    totalMinutes % 60
  ).padStart(2, "0")}`;
}

function getRowTotal(row) {
  return calculateHoursTotal(row.hours);
}

function getColumnTotal(index) {
  return calculateHoursTotal(
    entryRows.value.map(row => row.hours[index])
  );
}

const getGrandTotal = computed(() => {
  return calculateHoursTotal(
    entryRows.value.flatMap(row => row.hours)
  );
});

const isActionDisabled = (row, action, dayIndex = null) => {
  const status = selectedWeekStatus.value?.dropDownValue ?? selectedWeekStatus.value;

  // Lock everything for these statuses
  const isLocked =
    ["Approved", "Submitted", "Resubmitted"].includes(status);

  // Add & Submit buttons
  if (action === "button") {
    return isLocked;
  }

  // Task dropdown & Delete icon
  if (["task", "delete"].includes(action)) {
    return (
      isLocked ||
      row.isApproved?.some(
        (approved, index) => approved && Number(row.hours?.[index]) > 0
      )
    );
  }

  // Hours input & Description icon
  if (["hours", "description"].includes(action)) {
    if (isLocked) {
      return true;
    }

    // Lock only the current approved line
    return (
      row.isApproved?.[dayIndex] === true &&
      Number(row.hours?.[dayIndex]) > 0
    );
  }

  return false;
};

// after getting all timesheet data from api then map to preview list
function mapToPreviewList (lines) {
  previewList.value = lines.map(line => ({
    id: line.id || uid(),
    date: formatDate(line.timesheetDate || line.date),
    project: line.project?.name,
    task: line.task?.name,
    hours: line.hours,
    description: line.description,
    timesheetStatus: line.timesheetStatus,
    isApproved: line.isApproved
  }));
}

// Used debounce for handle minimal api calls
const debouncedSave = debounce((row, index) => {
  saveTimesheet(row, index, row.description[index]);
}, 500); // 500ms delay

// stored previous hours
function storePreviousHours(row, index) {
  if (!row._prevHours) {
    row._prevHours = {};
  }

  row._prevHours[index] = (row.hours[index] ?? "").trim();
}
// save data on change hours
// function onHoursChange(row, index) {
//   debugger;
//   if (!row.taskId || !row.hours[index]) return;

//   const raw = row.hours[index];
//   const hours = Number(row.hours[index] || 0);
//   const prev = row._prevHours?.[index] ?? 0;

//   // ignore incomplete typing like "0."
//   if (raw === "" || raw === null) return;

//   // Negative value validation
//   if (hours < 0 || raw.toString().startsWith("-")) {
//     notifyWarning({ message: "Invalid hours format." });

//     // restore previous value
//     row.hours[index] = prev;
//     return;
//   }

//   // if hours = 0 and already saved before
//   if (hours === 0 && (row.lineIds[index] || row.timesheetIds[index])) {

//     zwConfirmDelete(
//       {
//         data: `You entered 0 hours. This timesheet entry will be deleted.`
//       },
//       async () => {
//         await deleteSingleEntry(row, index);
//       },
//       () => {
//         // restore previous value
//         row.hours[index] = prev;
//       }
//     );

//     return;
//   }

//   // save
//   if (hours > 0 && hours !== prev) {
//     debouncedSave(row, index);
//   }
// }

function onHoursChange(row, index) {
  if (!row.taskId || !row.hours[index]) return;

  const currentHours = row.hours[index]?.trim();
  const previousHours = row._prevHours?.[index] ?? "";

  // Ignore empty input
  if (currentHours === "" || currentHours == null) return;

  if (validateHours(currentHours) !== true) {
    return;
  }

  // Delete if user enters 00:00 for an existing entry
  if (currentHours === "00:00" && (row.lineIds[index] || row.timesheetIds[index])) {
    zwConfirmDelete(
      {
        data: "You entered 00:00 hours. This timesheet entry will be deleted."
      },
      async () => {
        await deleteSingleEntry(row, index);
      },
      () => {
        row.hours[index] = previousHours;
      }
    );

    return;
  }

  // Save only if the value changed
  if (currentHours !== previousHours) {
    debouncedSave(row, index);
  }
}

// show preview
function rebuildPreview () {
  const list = [];

  entryRows.value.forEach(row => {
    row.hours.forEach((h, i) => {
      if (h > "00:00") {
        list.push({
          id: row.lineIds[i] || `${row.id}-${i}`,
          date: formatDate(weekDates.value[i].date),
          project: row.projectName,
          task: row.taskName,
          hours: h,
          description: row.description[i],
          timesheetStatus: row.timesheetStatus,
          isApproved: row.isApproved?.[i] ?? false
        });
      }
    });
  });

  previewList.value = list.reverse();
}

// delete whole week timesheet
const onDeleteWeekTimesheet = async (row, index) => {
  activeRowId.value = row.id;
  const message = row.taskName
  ? `You are deleting the full week timesheet for task "${row.taskName}".`
  : null;

  zwConfirmDelete(
    {
      data: message
    },
    async () => {
      try {
        // collect all timesheetIds (remove nulls)
        // const ids = row.timesheetIds.filter(id => id);

        // collect all timesheetLinesIds (remove nulls)
         const ids = row.lineIds.filter(id => id);

        // call delete API for each day
        await timesheetService.deleteWeeklyTimesheets(ids)
        notifySuccess({ message: "Weekly timesheet deleted successfully." });

        // remove row from UI
        entryRows.value.splice(index, 1);

        // rebuild preview
        rebuildPreview();

      } catch (err) {
        console.error(err);
        notifyError({ message: "Error deleting timesheet." });
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

// delete single timesheet
async function deleteSingleEntry (row, index) {
  try {
    const timesheetId = row.timesheetIds[index];
    const lineId = row.lineIds[index];

    // If nothing saved yet → just clear UI
    if (!timesheetId && !lineId) {
      row.hours[index] = 0;
      row.description[index] = "";
      rebuildPreview();
      return;
    }

    // Call delete API (you can reuse existing one)
    await timesheetService.deleteWeeklyTimesheetById(timesheetId);

    // clear UI
    row.hours[index] = "00:00";
    row.description[index] = "";
    row.lineIds[index] = null;
    row.timesheetIds[index] = null;

    notifySuccess({ message: "Timesheet entry deleted successfully." });

    rebuildPreview();
  } catch (err) {
    console.error(err);
    notifyError({ message: "Error deleting entry." });
  }
}

// save timesheet Data
async function saveTimesheet (row, dayIndex, description) {
  try {
    const timesheetId = row.timesheetIds?.[dayIndex] || null;
    const payload = {
      timesheetDate: formatDate(weekDates.value[dayIndex].date),
      timesheetLineModel: [
        {
          id: row.lineIds?.[dayIndex] || null,
          projectId: row.projectId,
          projectTaskId: row.taskId,
          projectModuleId: row.projectModuleId,
          hours: row.hours[dayIndex],
          description: description,
          date: formatDate(weekDates.value[dayIndex].date)
        }
      ]
    };

    const resp = await timesheetService.saveTimesheet(timesheetId, payload);

    row.timesheetIds[dayIndex] = resp.timesheetId;

    // extract lineId from array
    const newLineId = resp?.timesheetLineModel?.[0]?.id;
    if (newLineId) {
      row.lineIds[dayIndex] = newLineId;
    }
    // store description per day
    row.description[dayIndex] = description;

    notifySuccess({ message: "Timesheet saved successfully." });
    rebuildPreview();

    row.showDescIndex = null;
  } catch (error) {
    console.error("Error in submitting the timesheet:", error);
    notifyError({ message: "An error occurred while saving the timesheet." });
  } finally {
    setTimeout(() => {
    }, 1500);
  }
}

const submitForApproval = async () => {
  if (!validateWeeklyTimesheetHours ()) {
    return;
  }

  zwConfirm(
  {
    title: "Confirmation",
    message: "Are you sure you want to submit this weekly timesheet for approval?",
    okLabel: "OK",
    cancelLabel: "Cancel",
    cancel: true
  },
  async () => {
    processing.value = true;
    try {
      const payload = {
        projectNames: [...new Set(entryRows.value.map(row => row.projectName))],
        timesheetDate: selectedWeekLabel.value,
        approvalStatus: selectedWeekStatus.value?.dropDownValue === "Declined" ? "Resubmitted" : "Submitted"
      };

      await timesheetService.sendWeeklyTimesheetNotification(payload);

      notifySuccess({
        message: "Timesheet has been successfully sent to the approver for approval."
      });
      await getTimesheet();
    } finally {
      setTimeout(() => {
        processing.value = false;
      }, 1500);
    }
  }
);
};

// const checkWeekCompleted = async () => {
//   const isWeekCompleted =
//     weekDates.value.length === 7 &&
//     weekDates.value.every((_, index) => getColumnTotal(index) > 0);

//   if (!isWeekCompleted) {
//     return;
//   }

//   const payload = {
//     projectNames: [...new Set(entryRows.value.map(row => row.projectName))],
//     fromDate: formatDate(weekDates.value[0].date),
//     toDate: formatDate(weekDates.value[6].date)
//   };

//   await timesheetService.sendWeeklyTimesheetNotification(payload);
// };
// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const { projectTasksWithProjectForDropdown } = projectTaskModule();

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  projectTasksWithProjectForDropdown.load();
  // getAllProjectTaskWithProjectListForDropdown();

  if (!activeRowId.value && highlightProjectId) {
    activeRowId.value = highlightProjectId;
  }

  document.addEventListener("click", handleDocumentClick);

  const currentSunday = getCurrentWeekEndSunday();

  selectedWeekLabel.value = currentSunday;
  onWeekSelect(currentSunday);

  // console.log("projectTasksWithProjectForDropdown", projectTasksWithProjectForDropdown.list);
});

</script>

<style scoped>
.table-row > div {
  border-right: 1px solid #ccc;
  padding: 6px 8px;
}
.table-row > div:last-child {
  border-right: none;
}
.border-bottom {
  border-bottom: 1px solid #e0e0e0;
}

/* scrollbar for Timesheet Entry preview details section*/
.time-entry-section {
  display: flex;
  flex-direction: column;
}

.sticky-header {
  z-index: 10;
  padding-right: 12px;
}

.scroll-area {
  max-height: 300px;
  overflow-y: auto;
  border: 1px solid #e0e0e0;
}

.table-row {
  display: flex;
  align-items: center;
  padding: 8px 4px;
}

.border-bottom {
  border-bottom: 1px solid #eeeeee;
}

.scroll-area::-webkit-scrollbar {
  width: 6px;
}
.scroll-area::-webkit-scrollbar-thumb {
  background: #ccc;
  border-radius: 10px;
}

/* scrollbar for fill Timesheet data section*/
.timesheet-wrapper {
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  overflow: hidden;
}

.timesheet-body {
  max-height: 400px;
  overflow-y: auto;
  overflow-x: hidden;
}

.sticky-footer {
  border-top: 2px solid #ce93d8;
  position: relative;
  z-index: 10;
}

.sticky-header {
  position: relative;
  z-index: 10;
}

.entry-row {
  border-bottom: 1px solid #f0f0f0;
}

.timesheet-body::-webkit-scrollbar {
  width: 6px;
}
.timesheet-body::-webkit-scrollbar-thumb {
  background-color: #bdbdbd;
  border-radius: 10px;
}
.bg-light-red {
  background-color: #ed9e9e !important;
}
</style>
