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
        <!-- Week Calendar -->
        <formDate
          v-model="selectedWeekLabel"
          label="Select Weekend"
          mask="MM/DD/YYYY"
          :dateOptions="isSunday"
          :required="false"
          @update:model-value="onWeekSelect"
          :wrapperClass="'col-xxl-2 col-lg-2 col-md-2 col-sm-2 col-xs-2'"
        />
        <!-- Add Row -->
        <q-btn
          v-if="selectedWeekLabel"
          color="primary"
          icon="o_add"
          label="Add"
          no-caps
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
                  type="number"
                  dense
                  v-model="row.hours[i]"
                  input-class="text-right"
                  @blur="() => onHoursChange(row, i)"
                  @focus="() => storePreviousHours(row, i)"
                >
                  <!-- + icon -->
                  <q-icon
                    v-if="row.hours[i] && Number(row.hours[i]) > 0"
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
                  class="cursor-pointer q-mr-xs"
                  color="negative"
                  @click="onDeleteWeekTimesheet(row, index)"
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

      <!-- Time entry details -->
      <div v-if="selectedWeekLabel && previewList.length > 0" class="q-pa-sm time-entry-section">
        <h3 class="text-weight-bold q-pa-xs">Time Entry Details</h3>
        <div class="row q-pa-xs bg-primary text-white table-row sticky-header">
          <div class="" style="width: 10%;">Date</div>
          <div class="" style="width: 20%;">Project</div>
          <div class="" style="width: 25%;">Task</div>
          <div class="" style="width: 5%;">Time</div>
          <div class="" style="width: 40%;">Description</div>
        </div>
        <div class="scroll-area">
          <div v-for="item in previewList" :key="item.id" class="row border-bottom table-row">
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
import { ref, onMounted, onBeforeUnmount, computed } from "vue";
import { uid, useQuasar } from "quasar";
import { setLocalStorage, getLocalStorage, notifySuccess, notifyError, zwConfirmDelete, notifyWarning } from "assets/utils";
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
const { fonts, toolbar } = getEditorConfig($q);

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
        hours: weekDates.value.map(() => 0), // initialize week
        lineIds: weekDates.value.map(() => null),
        description: weekDates.value.map(() => ""),
        timesheetIds: weekDates.value.map(() => null)
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

  const lines = resp.flatMap(x => {
  return (x.timesheetLines || []).map(line => {
    return {
      ...line,
      timesheetDate: x.timesheetDate,
      timesheetId: x.id
    };
  });
});

  // map timesheet data to entryRows
  mapTimesheetToEntryRows(lines);
  // preview mapping
  mapToPreviewList(lines);
};

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

function getRowTotal (row) {
  const total = row.hours.reduce((a, b) => a + (Number(b) || 0), 0);
  return roundToTwo(total);
}

function getColumnTotal (index) {
  // return entryRows.value.reduce((sum, r) => {
  //   return sum + (Number(r.hours[index]) || 0);
  // }, 0);
  const total = entryRows.value.reduce((sum, r) => {
    return sum + (Number(r.hours[index]) || 0);
  }, 0);

  return roundToTwo(total);
}

const getGrandTotal = computed(() => {
  const total = entryRows.value.reduce((sum, r) => sum + getRowTotal(r), 0);
  return roundToTwo(total);
});

// after getting all timesheet data from api then map to preview list
function mapToPreviewList (lines) {
  previewList.value = lines.map(line => ({
    id: line.id || uid(),
    date: formatDate(line.timesheetDate || line.date),
    project: line.project?.name,
    task: line.task?.name,
    hours: line.hours,
    description: line.description
  }));
}

// Used debounce for handle minimal api calls
const debouncedSave = debounce((row, index) => {
  saveTimesheet(row, index, row.description[index]);
}, 500); // 500ms delay

// stored previous hours
function storePreviousHours (row, index) {
  if (!row._prevHours) {
    row._prevHours = {};
  }

  row._prevHours[index] = Number(row.hours[index] || 0);
}

// save data on change hours
function onHoursChange(row, index) {
  if (!row.taskId || !row.hours[index]) return;

  const raw = row.hours[index];
  const hours = Number(row.hours[index] || 0);
  const prev = row._prevHours?.[index] ?? 0;

  // ignore incomplete typing like "0."
  if (raw === "" || raw === null) return;

  // Negative value validation
  if (hours < 0 || raw.toString().startsWith("-")) {
    notifyWarning({ message: "Invalid hours format." });

    // restore previous value
    row.hours[index] = prev;
    return;
  }

  // if hours = 0 and already saved before
  if (hours === 0 && (row.lineIds[index] || row.timesheetIds[index])) {

    zwConfirmDelete(
      {
        data: `You entered 0 hours. This timesheet entry will be deleted.`
      },
      async () => {
        await deleteSingleEntry(row, index);
      },
      () => {
        // restore previous value
        row.hours[index] = prev;
      }
    );

    return;
  }

  // save
  if (hours > 0 && hours !== prev) {
    debouncedSave(row, index);
  }
}

// show preview
function rebuildPreview () {
  const list = [];

  entryRows.value.forEach(row => {
    row.hours.forEach((h, i) => {
      if (h > 0) {
        list.push({
          id: row.lineIds[i] || `${row.id}-${i}`,
          date: formatDate(weekDates.value[i].date),
          project: row.projectName,
          task: row.taskName,
          hours: h,
          description: row.description[i]
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
    row.hours[index] = 0;
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
</style>
