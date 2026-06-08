<template>
  <q-dialog ref="dialogRef" class="customDialog PlannerDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Timesheet</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogClose()" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Timesheet Info</legend>
              <div class="flex items-center justify-between q-my-md">
                <div class="col-3">
                  <div>
                    <formDate
                        v-model="model.timesheetDateStr"
                        label="Timesheet Date"
                        :dateOptions="(date) => getTimesheetAllowedDateRange(date, true, 1)"
                        :error="v$.timesheetDateStr.$error"
                        :error-message="v$.timesheetDateStr.$errors[0]?.$message"
                        :onBlur="() => v$.timesheetDateStr.$touch()"
                      />
                  </div>
                </div>
                <div>
                  <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAddTimesheet" />
                </div>
              </div>
              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                virtual-scroll
                bordered
                class="no-shadow"
                :loading="loading"
                :rows="timesheetRows"
                :columns="columns"
                row-key="id"
                separator="cell"
                binary-state-sort
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}<span v-if="['projectId', 'projectModuleId', 'projectTaskId','projectActivityId','hours'].includes(col.name)" class="required">*</span></q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :class="props.row.deleted ? 'hidden edit_plan' : 'edit_plan'">
                    <q-td style="width: 300px; max-width: 300px; white-space: normal; overflow-wrap: break-word;">
                      <div v-if="props.row.isMyTaskActivity">
                        <q-field outlined dense stack-label readonly>
                          <template #control>
                            <div class="row no-wrap items-center">
                              <div class="q-ml-sm">
                                {{ props.row.projectName }}
                              </div>
                            </div>
                          </template>
                        </q-field>
                      </div>
                      <div v-else>
                        <q-select
                          v-model="props.row.projectId"
                          clearable
                          use-input
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          :options="projectList"
                          option-value="value"
                          option-label="text"
                          emit-value
                          map-options
                          :error="rowValidations[props.rowIndex]?.value?.projectId.$error"
                          :error-message="rowValidations[props.rowIndex]?.value?.projectId.$errors[0]?.$message"
                          @blur="rowValidations[props.rowIndex]?.value?.projectId.$touch"
                          @filter="(val, update, abort) => getAllProjectListForFilter(val, update, abort)"
                          @update:model-value="getModuleByProjectId(props.row.projectId, props.row, props.rowIndex)"
                        >
                          <template #option="{ itemProps, opt }">
                            <q-item v-bind="itemProps">
                              <q-item-section>
                                <div class="row q-col-gutter-x-md items-center" style="white-space: normal; overflow-wrap: break-word; max-width: 100%;">
                                  <span>{{ opt.text }}</span>
                                </div>
                              </q-item-section>
                            </q-item>
                          </template>
                        </q-select>
                      </div>
                    </q-td>
                    <q-td style="width: 300px; max-width: 300px; white-space: normal; overflow-wrap: break-word;">
                      <div v-if="props.row.isMyTaskActivity">
                        <q-field outlined dense stack-label readonly>
                          <template #control>
                            <div class="row no-wrap items-center">
                              <div class="q-ml-sm">
                                {{ props.row.moduleName }}
                              </div>
                            </div>
                          </template>
                        </q-field>
                      </div>
                      <div v-else>
                         <formSingleSelectDropdown
                            v-model="props.row.projectModuleId"
                            :options="projectModulesByProjectIdForDropdownSingleSelectWithRowIndex.getByIndex(props.rowIndex, props.row.projectId)"
                            :filter="projectModulesByProjectIdForDropdownSingleSelectWithRowIndex.filter(props.rowIndex)"
                            :error="rowValidations[props.rowIndex]?.value?.projectModuleId.$error"
                            :errorMessage="rowValidations[props.rowIndex]?.value?.projectModuleId.$errors[0]?.$message"
                            @update:model-value="getTaskByModuleId(props.row.projectId, props.row.projectModuleId, props.row, props.rowIndex)"
                          />
                      </div>
                    </q-td>
                    <q-td style="width: 300px; max-width: 300px; white-space: normal; overflow-wrap: break-word;">
                      <div class="row no-wrap items-center justify-between">
                        <div
                          :class="{
                            'col-11': props.row.projectTaskId,
                            'col-12': !props.row.projectTaskId
                          }"
                        >
                          <div v-if="props.row.isMyTaskActivity">
                            <q-field outlined dense stack-label readonly>
                              <template #control>
                                <div class="row no-wrap items-center">
                                  <div class="q-ml-sm">
                                    {{ props.row.taskName }}
                                  </div>
                                </div>
                              </template>
                            </q-field>
                          </div>
                          <div v-else>
                            <formSingleSelectDropdown
                              v-model="props.row.projectTaskId"
                              :options="projectTasksByProjectIdAndModuleIdForDropdownSingleSelectWithRowIndex.getByIndex(props.rowIndex, props.row.projectModuleId)"
                              :filter="projectTasksByProjectIdAndModuleIdForDropdownSingleSelectWithRowIndex.filter(props.rowIndex)"
                              :error="rowValidations[props.rowIndex]?.value?.projectTaskId.$error"
                              :errorMessage="rowValidations[props.rowIndex]?.value?.projectTaskId.$errors[0]?.$message"
                              @update:model-value="getProjectTaskActivityByTaskId(props.row.projectId, props.row.projectModuleId, props.row.projectTaskId, props.row, props.rowIndex)"
                            />
                          </div>
                        </div>
                        <div v-if="props.row.projectTaskId" class="col-1">
                          <q-icon
                            name="o_copy"
                            class="cursor-pointer q-ml-xs"
                            size="xs"
                            clickable
                            @click="onTaskDescription(props.row.projectTaskId)"
                          >
                            <q-tooltip>Task Description</q-tooltip>
                          </q-icon>
                        </div>
                      </div>
                    </q-td>
                    <q-td style="width: 450px; max-width: 450px; white-space: normal; overflow-wrap: break-word;">
                      <div v-if="props.row.isMyTaskActivity">
                        <q-field outlined dense stack-label readonly>
                          <template #control>
                            <div class="row no-wrap items-center">
                              <div class="q-ml-sm">
                                {{ props.row.projectActivityName }}
                                <q-icon
                                  v-if="props.row.activityNameDescription"
                                  name="o_info"
                                  size="20px"
                                >
                                  <q-tooltip v-if="props.row.activityNameDescription" class="text-wrap break-words" max-width="300px">
                                    <div v-html="props.row.activityNameDescription" />
                                  </q-tooltip>
                                </q-icon>
                              </div>
                            </div>
                          </template>
                        </q-field>
                      </div>
                      <div v-else>
                        <formSingleSelectDropdown
                          v-model="props.row.projectActivityId"
                          :options="projectTaskActivityNameForDropdownSingleSelectWithRowIndex.getByIndex(props.rowIndex, props.row.projectTaskId)"
                          :filter="projectTaskActivityNameForDropdownSingleSelectWithRowIndex.filter(props.rowIndex)"
                          :error="rowValidations[props.rowIndex]?.value?.projectActivityId.$error"
                          :errorMessage="rowValidations[props.rowIndex]?.value?.projectActivityId.$errors[0]?.$message"
                        >
                          <template #option="{ itemProps, opt }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center">
                                    <span>{{ opt.text }}</span>
                                      <q-icon
                                        v-if="opt.data"
                                        name="o_info"
                                        size="17px"
                                        class="q-ml-xs"
                                      >
                                        <q-tooltip class="text-wrap break-words" max-width="300px">
                                          <div v-html="opt.data" />
                                        </q-tooltip>
                                      </q-icon>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                        </formSingleSelectDropdown>
                      </div>
                    </q-td>
                    <q-td style="white-space: normal; overflow-wrap: break-word;">
                      <q-editor
                        v-model="props.row.description"
                        y:dense="$q.screen.lt.md"
                        :toolbar="toolbar"
                        :fonts="fonts"
                        class="relative-position"
                        style="width: 400px;"
                      />
                    </q-td>
                    <q-td style="width: 200px; max-width: 200px;">
                      <q-input
                        v-model="props.row.hours"
                        outlined
                        stack-label
                        hide-bottom-space
                        dense
                        maxlength="5"
                        hint="hh.mm"
                        :rules="[validateHours]"
                        :error="rowValidations[props.rowIndex]?.value?.hours.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.hours.$errors[0]?.$message"
                      />
                    </q-td>
                    <q-td auto-width class="text-center">
                      <q-td auto-width class="text-center" style="width: 5%;">
                        <q-icon name="o_delete_outline" size="xs" class="cursor-pointer" color="negative" @click="onDeleteTimesheet(props.rowIndex)">
                          <q-tooltip>Delete</q-tooltip>
                        </q-icon>
                      </q-td>
                    </q-td>
                  </q-tr>
                </template>
                <template #bottom>
                  <q-tr auto-width class="flex justify-end q-mr-xl" style="width:93%">
                    <q-td><b>Total :</b></q-td>
                    <q-td class="text-right"><b>{{ totalHours }}</b></q-td>
                  </q-tr>
                </template>
              </q-table>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="q-gutter-sm justify-center">
          <q-btn
            color="grey-4"
            push
            outline
            label="Close"
            type="button"
            class="text-grey-9 actionBtn"
            no-caps
            @click="onDialogClose()"
          />
          <!-- Save and Continue -->
          <q-btn
            color="primary"
            push
            outline
            label="Save and Continue"
            type="button"
            class="actionBtn"
            :loading="processing && activeButton === 'continue'"
            :disable="processing"
            no-caps
            style="width: 160px !important;" @click="onSubmit(false, 'continue')"
          />
          <!-- Save and Close -->
          <q-btn
            color="primary"
            push
            outline
            label="Save and Close"
            type="button"
            class="actionBtn"
            :loading="processing && activeButton === 'close'"
            :disable="processing"
            no-caps
            @click="onSubmit(true, 'close')"
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar, uid } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, computed, onMounted } from "vue";
import _ from "lodash";
import { notifySuccess, notifyError, zwConfirmLeave, notifyWarning } from "assets/utils";
import { useAuthStore } from "stores/auth";
import { format } from "date-fns"; // Standard TimeZone Conversion

import timesheetService from "../timesheet.service";
import projectService from "modules/project/projects.service";
import projectActivitiesService from "modules/project-tasks-activities/projectTasksActivities.service";
import taskDescription from "modules/timesheet/components/_taskDescription.vue";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// SOP Change :- Shared Dropdowns
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import projectTasksActivities from "src/modules/project-tasks-activities/utils/dropdowns.js";

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------
const props = defineProps({ activityIds: { type: Array, required: true }, timesheetLineIds: { type: Array, required: true }, id: { type: String, default: "" }, isMyTaskActivity: { type: Boolean, default: false } });

// ----------------------------------------------------------------------------------------------------------------
// Define emits
// ----------------------------------------------------------------------------------------------------------------
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------
const $q = useQuasar();
const $emit = defineEmits(["hide", "ok"]);
const loading = ref(true);
const processing = ref(false);
const rowValidations = ref([]);
const activeButton = ref(""); // 'continue' or 'close'
const rowIndex = ref(0);
const authStore = useAuthStore();
const user = authStore.user;
const employeeId = user.employeeId;
let TimesheetId = props.id;

// ----------------------------------------------------------------------------------------------------------------
// Define model values
// ----------------------------------------------------------------------------------------------------------------
const model = ref({
  timesheetDateStr: format(new Date(), "MM/dd/yyyy")
});

// ----------------------------------------------------------------------------------------------------------------
// Table variables
// ----------------------------------------------------------------------------------------------------------------
const tableRef = ref();
const timesheetRows = ref([]);
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 100, page: 1 });
const columns = ref([
  { name: "projectId", label: "Project", field: "projectId", align: "left", sortable: false },
  { name: "projectModuleId", label: "Module", field: "projectModuleId", align: "left", sortable: false },
  { name: "projectTaskId", label: "Task", field: "projectTaskId", align: "left", sortable: false },
  { name: "projectActivityId", label: "Activity (Est. Hrs)", field: "projectActivityId", align: "left", sortable: false },
  { name: "description", label: "Activity Details", field: "description", align: "left", sortable: false },
  { name: "hours", label: "Hours", field: "hours", align: "left", sortable: false }
]);

// ----------------------------------------------------------------------------------------------------------------
// Validation rules
// ----------------------------------------------------------------------------------------------------------------
const rules = {
  timesheetDateStr: { required: helpers.withMessage("Timesheet Date is required", required) }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const totalHours = computed(() => {
  const total = timesheetRows.value.reduce((sum, row) => {
    if (!row.deleted) {
      sum += parseFloat(row.hours) || 0;
    }
    return sum;
  }, 0);

  // Round to 2 decimal places without using toFixed()
  return Math.round(total * 100) / 100;
});

const editingRowrules = {
  projectId: { required: helpers.withMessage("Project is Required", required) },
  projectModuleId: { required: helpers.withMessage("Project module is Required", required) },
  projectTaskId: { required: helpers.withMessage("Project task is Required", required) },
  projectActivityId: { required: helpers.withMessage("Project activity is Required", required) },
  hours: {
    required: helpers.withMessage("Hours is Required", required),
    min: helpers.withMessage("Please check hours", (value) => parseFloat(value?.toString().trim()) >= 0.1),
    validFormat: helpers.withMessage("Invalid hours format", (value) => {
      const regex = /^(?:\d{1,2}(?:\.\d{1,2})?)$/;
      return regex.test(value?.toString().trim());
    })
  }
};

function validateHours (value) {
  const strValue = (value ?? "").toString().trim();
  const regex = /^(?:\d{1,2}(?:\.\d{1,2})?)$/;
  if (!strValue || (regex.test(strValue) && strValue.length <= 5)) {
    return true; // Valid input
  }
  return "Invalid hours format.";
}

// ------------------------------------------------------------------------------------
// Get All Dropdowns
// ------------------------------------------------------------------------------------
const { projectTaskActivityNameForDropdownSingleSelectWithRowIndex } = projectTasksActivities();
const { projectModulesByProjectIdForDropdownSingleSelectWithRowIndex } = projectModuleOfProjectModule();
const { projectTasksByProjectIdAndModuleIdForDropdownSingleSelectWithRowIndex } = projectTaskModule();

const projectList = ref([]);
const projectListForFilter = ref([]);
function getAllProjectListForDropdown () {
  const statuses = ["Open", "New", "In progress"];
  projectService.getAllProjectListForDropdown(statuses).then((resp) => {
    const responseData = resp.map((item) => ({
      text: `${item.name} (${item.totalModuleCount})`,
      value: item.id
    }));
    projectList.value = responseData;
    projectListForFilter.value = responseData;
  });
}
// Search project for dropdown
function getAllProjectListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectList.value = projectListForFilter.value;
    } else {
      projectList.value = projectListForFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}


// ----------------------------------------------------------------------------------------------------------------
// Get Timesheet Details
// ----------------------------------------------------------------------------------------------------------------
const getTimesheet = async () => {
  loading.value = true;
  try {
    const resp = await timesheetService.getTimesheetDetails(TimesheetId);
    model.value = _.cloneDeep(resp);
    model.value.timesheetDateStr = resp.timesheetDate
      ? format(resp.timesheetDate, "MM/dd/yyyy")
      : "";
    const rows = [];
    for (const [index, lines] of resp.timesheetLines.entries()) {
      const rowIndex = index;
      projectModulesByProjectIdForDropdownSingleSelectWithRowIndex.load(
        rowIndex,
        false,
        true,
        lines.project.id
      );
      projectTasksByProjectIdAndModuleIdForDropdownSingleSelectWithRowIndex.load(
        rowIndex,
        lines.project.id,
        lines.projectModule.id,
        employeeId
      );
      projectTaskActivityNameForDropdownSingleSelectWithRowIndex.load(
        rowIndex,
        lines.project.id,
        lines.projectModule.id,
        lines.task.id
      );
      rows.push({
        ...lines,
        editing: false,
        projectId: lines.project?.id ?? null,
        projectModuleId: lines.projectModule?.id ?? null,
        projectTaskId: lines.task?.id ?? null,
        projectActivityId: lines.projectActivity?.id ?? null,
        hours: lines.hours,
        description: lines.description,
        rowIndex: rowIndex,
        moduleName: lines.projectModule?.name ?? "",
        taskName: lines.task?.name ?? "",
        projectName: lines.project?.name ?? "",
        timesheetId: model.value.id,
        isMyTaskActivity: props.isMyTaskActivity,
        flag: "Edit"
      });
    }
    timesheetRows.value = rows;
    // Calculate total hours
    totalHours.value = rows.reduce((acc, row) => {
      return acc + (parseFloat(row.hours) || 0);
    }, 0);
  } catch (error) {
    console.error("Error loading Timesheet:", error);
  } finally {
    loading.value = false;
  }
};

const getProjectTasksActivitiesDetailsByIds = () => {
  loading.value = true;
  projectActivitiesService.getProjectTasksActivitiesDetailsByIds(props.activityIds).then((resp) => {
    timesheetRows.value = resp.map((taskActivity, index) => {
      rowIndex.value += 1;
      projectModulesByProjectIdForDropdownSingleSelectWithRowIndex.load(
        rowIndex.value,
        false,
        true,
        taskActivity.project.id
      );
      projectTasksByProjectIdAndModuleIdForDropdownSingleSelectWithRowIndex.load(
        rowIndex.value,
        taskActivity.project.id,
        taskActivity.projectModule.id,
        employeeId
      );
      projectTaskActivityNameForDropdownSingleSelectWithRowIndex.load(
        rowIndex.value,
        taskActivity.project.id,
        taskActivity.projectModule.id,
        taskActivity.task.id
      );
      return {
        ...taskActivity,
        id: uid(),
        editing: false,
        projectId: taskActivity.project.id,
        projectModuleId: taskActivity.projectModule.id,
        projectName: taskActivity.project.name,
        projectTaskId: taskActivity.task.id,
        projectActivityId: taskActivity.id,
        description: taskActivity.description ? taskActivity.description : "",
        taskName: taskActivity.task.name,
        moduleName: taskActivity.projectModule.name,
        projectActivityName: taskActivity.name,
        activityNameDescription: taskActivity.activityNameDescription,
        rowIndex: rowIndex.value,
        isMyTaskActivity: true,
        flag: "Edit"
      };
    });
  }).finally(() => {
    loading.value = false;
  });
};

const getTimesheetLinesDetailsByIds = () => {
  loading.value = true;
  timesheetService.getTimesheetLinesDetailsByIds(props.timesheetLineIds)
    .then((resp) => {
      rowIndex.value = 0;
      timesheetRows.value = resp.map((timesheetLine) => {
      rowIndex.value += 1;
        //  Load dropdowns
      projectModulesByProjectIdForDropdownSingleSelectWithRowIndex.load(
        rowIndex.value,
        false,
        true,
        timesheetLine.project.id
      );
      projectTasksByProjectIdAndModuleIdForDropdownSingleSelectWithRowIndex.load(
        rowIndex.value,
        timesheetLine.project.id,
        timesheetLine.projectModule.id,
        employeeId
      );
      projectTaskActivityNameForDropdownSingleSelectWithRowIndex.load(
        rowIndex.value,
        timesheetLine.project.id,
        timesheetLine.projectModule.id,
        timesheetLine.task.id
      );
      return {
        ...timesheetLine,
          id: uid(),
        editing: false,
        projectId: timesheetLine.project.id,
        projectModuleId: timesheetLine.projectModule.id,
        projectTaskId: timesheetLine.task.id,
        projectActivityId: timesheetLine.projectActivity.id,
        projectName: timesheetLine.project.name,
        moduleName: timesheetLine.projectModule.name,
        taskName: timesheetLine.task.name,
        projectActivityName: timesheetLine.projectActivity.name,
        description: timesheetLine.description || "",
        hours: timesheetLine.hours || 0,
        activityNameDescription: timesheetLine.activityNameDescription,
        rowIndex: rowIndex.value,
        isMyTaskActivity: true,
        flag: "Edit"
      };
    });
  }).finally(() => {
    loading.value = false;
  });
};

function getModuleByProjectId(projectId, row, rowIndex) {
  row.projectModuleId = null;
  row.projectTaskId = null;
  row.projectActivityId = null;

  if (projectId) {
    projectModulesByProjectIdForDropdownSingleSelectWithRowIndex.load(
    rowIndex,
    false,
    true,
    projectId,
  );
  } else {
    projectModulesByProjectIdForDropdownSingleSelectWithRowIndex.clearByIndex(rowIndex);
    projectTasksByProjectIdAndModuleIdForDropdownSingleSelectWithRowIndex.clearByIndex(rowIndex);
    projectTaskActivityNameForDropdownSingleSelectWithRowIndex.clearByIndex(rowIndex);
  }
}

function getTaskByModuleId (projectId, moduleId, row, rowIndex) {
  row.projectTaskId = "";
  row.projectActivityId = "";
  if (moduleId) {
     projectTasksByProjectIdAndModuleIdForDropdownSingleSelectWithRowIndex.load(
      rowIndex,
      projectId,
      moduleId,
      employeeId
    );
  } else {
    projectTasksByProjectIdAndModuleIdForDropdownSingleSelectWithRowIndex.clearByIndex(rowIndex);
    projectTaskActivityNameForDropdownSingleSelectWithRowIndex.clearByIndex(rowIndex);
  }
}

function getProjectTaskActivityByTaskId(projectId, moduleId, taskId, row, rowIndex) {
  row.projectActivityId = "";
  if (taskId) {
    projectTaskActivityNameForDropdownSingleSelectWithRowIndex.load(
      rowIndex,
      projectId,
      moduleId,
      taskId
    );
  }
}

function getTimesheetAllowedDateRange(
  date,
  thisWeek,
  lastNumberOfWeeks
) {
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const selectedDate = new Date(date)
  selectedDate.setHours(0, 0, 0, 0)

  const startDate = new Date(today)

  startDate.setDate(
    startDate.getDate() -
    startDate.getDay() -
    (7 * lastNumberOfWeeks)
  )

  startDate.setHours(0, 0, 0, 0)

  return (
    thisWeek &&
    selectedDate >= startDate &&
    selectedDate <= today
  )
}

async function onAddTimesheet () {
  timesheetRows.value.push({
    id: uid(),
    projectId: "",
    projectModuleId: "",
    projectTaskId: "",
    projectActivityId: "",
    hours: "",
    description: "",
    deleted: false,
    isMyTaskActivity: false,
    rowIndex: rowIndex.value += 1
  });
}

const onDeleteTimesheet = (index) => {
  if (timesheetRows.value.filter(row => row.deleted === false).length > 1) {
    timesheetRows.value[index].deleted = true;
  } else {
    notifyError({ message: "Please add at least one row." });
  }
};


// View status log popup
const onTaskDescription = (id) => {
  // activeRowId.value = id;
  $q.dialog({
    component: taskDescription,
    componentProps: { id }
  }).onOk(() => {
    getTimesheet({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    // activeRowId.value = id;
  });
};

function onDialogClose () {
  zwConfirmLeave({ data: "" }, () => {
    onDialogCancel();
  }, () => {
  });
}

// ------------------------------------------------------------------------------------
// Submit form
// ------------------------------------------------------------------------------------
async function onSubmit (shouldClose, buttonType) {
  if (processing.value) {
    notifyWarning({ message: "Double click not allowed. Please wait..." });
    return; // stop further submit
  }
  activeButton.value = buttonType;
  // processing.value = true;
  try {
    let isValid = true;
    const nonDeletedRows = timesheetRows.value.filter(row => !row.deleted);
    rowValidations.value = nonDeletedRows.map((row) =>
      useVuelidate(editingRowrules, row, { $lazy: true, $autoDirty: true })
    );

    // Validate each row
    for (const [index, validation] of rowValidations.value.entries()) {
      if (validation?.value) {
        await validation.value.$touch();
        const isRowValid = await validation.value.$validate();
        if (!isRowValid) {
          isValid = false;
        }
      } else {
        console.error(`Validation object for row ${index} is undefined`);
        isValid = false;
      }
    }

    if (!isValid) {
      return; // Prevent submission
    }
    if (await v$.value.$validate() && isValid) {
      processing.value = true;
      const cleanedRows = timesheetRows.value.map(row => {
        const trimmedHours = (row.hours ?? "").toString().trim();
        let parsedHours;
        if (row.deleted === true) {
          parsedHours = 0.0;
        } else {
          parsedHours = trimmedHours === "" ? null : parseFloat(trimmedHours);
        }
        return {
          ...row,
          hours: parsedHours,
          isMyTaskActivity: row.isMyTaskActivity ?? false,
          projectName: row.projectName ?? "",
          moduleName: row.moduleName ?? "",
          taskName: row.taskName ?? "",
          projectActivityName: row.projectActivityName ?? ""
        };
      });
      const payload = {
        timesheetDate: model.value.timesheetDateStr,
        // timesheetLineModel: timesheetRows.value
        timesheetLineModel: cleanedRows
      };
      const resp = await timesheetService.saveTimesheet(TimesheetId, payload);
      TimesheetId = resp.timesheetId;
      timesheetRows.value = resp.timesheetLineModel.map(savedRow => {
        const originalRow = cleanedRows.find(r => r.id === savedRow.id);
        const wasLabel = originalRow?.isMyTaskActivity ?? false;
        return {
          ...savedRow,
          rowIndex: originalRow?.rowIndex ?? 0,
          isMyTaskActivity: buttonType === "continue" ? wasLabel : false,
          projectName: originalRow?.projectName ?? "",
          moduleName: originalRow?.moduleName ?? "",
          taskName: originalRow?.taskName ?? "",
          projectActivityName: originalRow?.projectActivityName ?? ""
        };
      });

      notifySuccess({ message: "Timesheet saved successfully." });
      timesheetRows.value.forEach((row, index) => {
        if (row.projectId) {
          projectModulesByProjectIdForDropdownSingleSelectWithRowIndex.load(index, false, true, row.projectId);
          if (row.projectModuleId) {
            projectTasksByProjectIdAndModuleIdForDropdownSingleSelectWithRowIndex.load(index, row.projectId, row.projectModuleId, employeeId);
            if (row.projectTaskId) {
              projectTaskActivityNameForDropdownSingleSelectWithRowIndex.load(index, row.projectId, row.projectModuleId, row.projectTaskId);
            }
          }
        }
      });
      $emit("ok");
      if (shouldClose) {
        $emit("hide");
      }
    }
  } catch (error) {
    console.error("Error in submitting the timesheet:", error);
    notifyError({ message: "An error occurred while saving the timesheet." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
      activeButton.value = "";
    }, 1500);
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

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------
onMounted(() => {
  getAllProjectListForDropdown();
  if (!TimesheetId && props.activityIds && props.timesheetLineIds) {
    timesheetRows.value.push({
      id: uid(),
      projectId: "",
      projectModuleId: "",
      projectTaskId: "",
      projectActivityId: "",
      hours: "",
      description: "",
      deleted: false,
      isMyTaskActivity: true,
      rowIndex: rowIndex.value += 1
    });
  }
  if (props.activityIds) {
    getProjectTasksActivitiesDetailsByIds();
  }
 if (props.timesheetLineIds && props.timesheetLineIds.length) {
  getTimesheetLinesDetailsByIds();
}
});

console.log(props.activityIds);
watch(() => TimesheetId, (newValue, oldValue) => {
  if (newValue) {
    getTimesheet();
  } else {
    loading.value = false;
  }
}, { immediate: true });
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_projectModule .q-select__dropdown-icon{
  display: none;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
</style>
