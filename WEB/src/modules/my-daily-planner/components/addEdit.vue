<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none PlannerDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" :class="isForwarded == 'isForwarded' ? 'hidden' : ''">{{ id ? "Edit" : "Add" }} Daily Plan</div>
        <div class="text-h2 text-white" :class="isForwarded == 'isForwarded' ? '' : 'hidden'">{{ isForwarded == "isForwarded" ? "Forward To Timesheet" : "" }}</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogClose()" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <!-- <q-card class="card-header with-tools headerBasic"> -->
            <fieldset>
              <legend>{{ isForwarded == "isForwarded" ? "Daily Plan Info" : "Daily Plan Info" }}</legend>
              <div style="">
                <div class="flex items-center justify-between q-my-md">
                  <div class="col-3">
                    <div>
                      <formDate
                        v-model="model.dailyPlannerDateStr"
                        label="Planner Date"
                        :dateOptions="(date) => getTimesheetAllowedDateRange(date, props.isForwarded, true, 1)"
                        :error="v$.dailyPlannerDateStr.$error"
                        :error-message="v$.dailyPlannerDateStr.$errors[0]?.$message"
                        :onBlur="() => v$.dailyPlannerDateStr.$touch()"
                      />
                    </div>
                  </div>
                  <div>
                    <q-btn color="primary" icon="o_add" label="Add Daily Plan" no-caps @click="onAddDailyPlanner" />
                  </div>
                </div>
                <q-table
                  ref="tableRef"
                  v-model:pagination="pagination"
                  virtual-scroll
                  bordered
                  class="no-shadow"
                  :loading="loading"
                  :rows="dailyPlanRows"
                  :columns="columns"
                  row-key="id"
                  separator="cell"
                  :rows-per-page-options="[5, 15, 20, 30, 50, 0]"
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}<span v-if="['projectId', 'projectModuleId', 'projectTaskId','projectActivityId','hours'].includes(col.name)" class="required">*</span></q-th>
                      <q-th auto-width class="text-center">Actions</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :class="props.row.deleted ? 'hidden edit_tasks' : ''">
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
                            @filter="(val, update, abort) => getAllProjectListForFilter(val, update, abort)"
                            @update:model-value="getModuleByProjectId(props.row.projectId, props.row, props.rowIndex)"
                            @blur="rowValidations[props.rowIndex]?.value?.projectId.$touch"
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
                      <q-td style="width: 300px; max-width: 300px; white-space: normal;overflow-wrap: break-word;">
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
                                      <div class="RichTextEditor" v-html="props.row.activityNameDescription" />
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
                          :dense="$q.screen.lt.md"
                          :toolbar="toolbar"
                          :fonts="fonts"
                          class="relative-position"
                          style="width: 400px;"
                        />
                      </q-td>
                      <q-td style="width: 200px; max-width: 200px;">
                        <q-input
                          v-model.trim="props.row.hours"
                          outlined
                          hide-bottom-space
                          maxlength="5"
                          hint="hh.mm"
                          :dense="true"
                          :error="rowValidations[props.rowIndex]?.value?.hours.$error"
                          :error-message="rowValidations[props.rowIndex]?.value?.hours.$errors[0]?.$message"
                          :rules="[validateHours]"
                          @blur="rowValidations[props.rowIndex]?.value?.hours.$touch"
                        />
                      </q-td>
                      <q-td class="text-center" style="width: 5%;">
                        <q-icon name="o_delete_outline" size="xs" class="cursor-pointer" color="negative" @click="onDeleteDailyPlanner(props.rowIndex)">
                          <q-tooltip>Delete</q-tooltip>
                        </q-icon>
                      </q-td>
                    </q-tr>
                  </template>
                  <template #bottom>
                    <q-tr auto-width class="flex justify-end q-mr-xl" style="width:93%">
                      <q-td class="text-right"><b>Total :</b></q-td>
                      <q-td><b>{{ totalHours }}</b></q-td>
                    </q-tr>
                  </template>
                </q-table>
              </div>
            </fieldset>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogClose" />
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
            :label="isForwarded === 'isForwarded' ? 'Forward To Timesheet' : 'Save and Close'"
            type="button"
            :class="isForwarded === 'isForwarded' ? 'sendActionBtn' : 'actionBtn'"
            :loading="processing && activeButton === 'close'"
            :disable="processing"
            no-caps @click="onSubmit(true, 'close')"
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>
<script setup>
// Import libraries
import { useQuasar, useDialogPluginComponent, uid } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import { notifySuccess, notifyError, zwConfirmLeave, notifyWarning } from "assets/utils";
import _ from "lodash";
import { useAuthStore } from "stores/auth";
import { format } from "date-fns"; // Standard TimeZone Conversion

import projectService from "modules/project/projects.service";
import projectActivitiesService from "modules/project-tasks-activities/projectTasksActivities.service";
import dailyplannerService from "../myDailyPlanner.service";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// SOP Change :- Shared Dropdowns
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import projectTasksActivities from "src/modules/project-tasks-activities/utils/dropdowns.js";

// Common variables
const $q = useQuasar();
const $emit = defineEmits(["hide", "ok"]);
const loading = ref(true);
const processing = ref(false);
const activeButton = ref(""); // 'continue' or 'close'=
const authStore = useAuthStore();
const user = authStore.user;
const employeeId = user.employeeId;

const props = defineProps({ activityIds: { type: Array, required: true }, id: { type: String, default: "" }, isForwarded: { type: String, default: "" }, isMyTaskActivity: { type: Boolean, default: false } });
const rowIndex = ref(0);
let DailyPlannerId = props.id;

// Table variables
const tableRef = ref();
const dailyPlanRows = ref([]);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 100, page: 1 });
const columns = ref([
  { name: "projectId", label: "Project", field: "projectId", align: "left", sortable: false },
  { name: "projectModuleId", label: "Module", field: "projectModuleId", align: "left", sortable: false },
  { name: "projectTaskId", label: "Task", field: "projectTaskId", align: "left", sortable: false },
  { name: "projectActivityId", label: "Activity (Est. Hrs)", field: "projectActivityId", align: "left", sortable: false },
  { name: "description", label: "Activity Details", field: "description", align: "left", sortable: false },
  { name: "hours", label: "Hours", field: "hours", align: "left", sortable: false }
]);

// Define emits
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();
const model = ref({
  dailyPlannerDateStr: format(new Date(), "MM/dd/yyyy")
});

// Validation rules
const rules = {
  dailyPlannerDateStr: { required: helpers.withMessage("Daily planner Date is required", required) }
};
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const rowValidations = ref([]);
const editingRowrules = {
  projectId: { required: helpers.withMessage("Project is required", required) },
  projectModuleId: { required: helpers.withMessage("Project module is required", required) },
  projectTaskId: { required: helpers.withMessage("Task is required", required) },
  projectActivityId: { required: helpers.withMessage("Project activity is required", required) },
  hours: {
    required: helpers.withMessage("Hours is required", required),
    min: helpers.withMessage("Please check hours", (value) => parseFloat(value?.toString().trim()) >= 0.1),
    validFormat: helpers.withMessage("Invalid hours format", (value) => {
      const regex = /^(?:\d{1,2}(?:\.\d{1,2})?)$/;
      return regex.test(value?.toString().trim());
    })
  }
};

// validation for hrs
function validateHours (value) {
  // Ensure value is treated as a string
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


// Get all project list for dropdown
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

const getDailyPlanner = async () => {
  loading.value = true;
  try {
    const resp = await dailyplannerService.getDailyPlannerDetails(DailyPlannerId);
    model.value = _.cloneDeep(resp);
    model.value.dailyPlannerDateStr = resp.dailyPlannerDate
      ? format(resp.dailyPlannerDate, "MM/dd/yyyy")
      : "";

    const rows = [];
    for (const [index, lines] of resp.dailyPlannerLines.entries()) {
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
        lines.projectTask.id
      );
      rows.push({
        ...lines,
        editing: false,
        projectId: lines.project.id,
        projectModuleId: lines.projectModule.id,
        projectTaskId: lines.projectTask.id,
        projectActivityId: lines.projectActivity.id,
        hours: lines.hours,
        description: lines.description,
        rowIndex: rowIndex,
        moduleName: lines.projectModule.name,
        taskName: lines.projectTask.name,
        projectName: lines.projectName,
        dailyPlannerId: model.value.id,
        isMyTaskActivity: props.isMyTaskActivity,
        flag: "Edit"
      });
    }
    dailyPlanRows.value = rows;
  } catch (error) {
    console.error("Error loading Daily Planner:", error);
  } finally {
    loading.value = false;
  }
};

const getProjectTasksActivitiesDetailsByIds = () => {
  loading.value = true;
  projectActivitiesService.getProjectTasksActivitiesDetailsByIds(props.activityIds).then((resp) => {
    dailyPlanRows.value = resp.map((taskActivity, index) => {
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
        taskId: taskActivity.id,
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

function getModuleByProjectId(projectId, row, rowIndex) {
  row.projectModuleId = "";
  row.projectTaskId = "";
  row.projectActivityId = "";

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

function getProjectTaskActivityByTaskId (projectId, moduleId, taskId, row, rowIndex) {
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
  isForwarded,
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

  // date range based on forwarded flag
  if (isForwarded === "isForwarded") {
    return (
      thisWeek &&
      selectedDate >= startDate &&
      selectedDate <= today
    )
  }

  // not forwarded allow current and future dates only
  return selectedDate >= today
}

async function onAddDailyPlanner () {
  dailyPlanRows.value.push({
    id: uid(),
    projectId: "",
    projectModuleId: "",
    projectTaskId: "",
    projectActivityId: "",
    hours: "",
    description: "",
    deleted: false,
    isMyTaskActivity: false,
    rowIndex: rowIndex.value += 1,
  });
}

const onDeleteDailyPlanner = (index) => {
  if (dailyPlanRows.value.filter(row => row.deleted === false).length > 1) {
    dailyPlanRows.value[index].deleted = true;
  } else {
    notifyError({ message: "Please add at least one row." });
  }
};

// let isSaveDialog = false;
function onDialogClose () {
  zwConfirmLeave({ data: "" }, () => {
    onDialogCancel();
  }, () => {
  });
}

const totalHours = computed(() => {
  const total = dailyPlanRows.value.reduce((sum, row) => {
    if (!row.deleted) {
      sum += parseFloat(row.hours) || 0;
    }
    return sum;
  }, 0);

  // Round to 2 decimal places without using toFixed()
  return Math.round(total * 100) / 100;
});

async function onSubmit (shouldClose, buttonType) {
  activeButton.value = buttonType;
  // processing.value = true;
  try {
    if (processing.value) {
      notifyWarning({ message: "Double click not allowed. Please wait..." });
      return; // stop further submit
    }
    let isValid = true;
    const nonDeletedRows = dailyPlanRows.value.filter(row => !row.deleted);
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

    if (!isValid || !(await v$.value.$validate())) return;

    if (await v$.value.$validate() && isValid) {
      processing.value = true;
      const cleanedRows = dailyPlanRows.value.map(row => {
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
        isForwordedToTimesheet: props.isForwarded === "isForwarded",
        dailyPlannerDate: model.value.dailyPlannerDateStr,
        dailyPlannerLineModel: cleanedRows
      };

      const resp = await dailyplannerService.saveDailyPlanner(DailyPlannerId, payload);
      DailyPlannerId = resp.dailyPlannerId;

      // resp.dailyPlannerLineModel.forEach((row, index) => {
      //   row.rowCounter = index;
      // });

      // dailyPlanRows.value = resp.dailyPlannerLineModel;
      // Preserve or reset label condition based on button type
      dailyPlanRows.value = resp.dailyPlannerLineModel.map(savedRow => {
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

      notifySuccess({ message: props.isForwarded === "isForwarded" ? "Forwarded to timesheet successfully." : "Planner saved successfully." });
      dailyPlanRows.value.forEach((row, index) => {
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
    console.error("Error in submitting the planner:", error);
    notifyError({ message: "An error occurred while saving the planner." });
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
  if (!DailyPlannerId && props.activityIds) {
    dailyPlanRows.value.push({
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
});

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getDailyPlanner();
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
.global-loader {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(255, 255, 255, 0.8);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  z-index: 9999;
}
.loader-text {
  margin-top: 10px;
  color: #333;
  font-weight: 500;
}
</style>
