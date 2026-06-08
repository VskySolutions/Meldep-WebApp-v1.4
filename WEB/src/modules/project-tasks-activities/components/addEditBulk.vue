<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:80vw !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ props.pageHeading ? props.pageHeading : 'Edit Bulk Activities' }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable']">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>
                <q-breadcrumbs class="text-brown text-weight-bold text-h3">
                  <template #separator>
                    <q-icon size="1.5em" name="o_chevron_right" color="primary" />
                  </template>
                  <q-breadcrumbs-el :label="'Project - ' + model.projectName" :error="v$.projectName.$error" :error-message="v$.projectName.$errors[0]?.$message" />
                  <q-breadcrumbs-el :label="'Project Module - ' + model.projectModuleName" :error="v$.projectModuleName.$error" :error-message="v$.projectModuleName.$errors[0]?.$message" />
                  <q-breadcrumbs-el class="text-primary" :label="'Task - ' + model.taskName" :error="v$.taskName.$error" :error-message="v$.taskName.$errors[0]?.$message" />
                </q-breadcrumbs>
              </legend>
              <div class="row items-end q-mb-sm q-col-gutter-x-md">
                <div v-if="!isPreviousTargetMonth" class="col-xs-12 col-sm-12 flex justify-end">
                  <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAdd" />
                </div>
              </div>
              <div class="table-wrapper-container">
                <!-- Scrollable Table -->
                <div class="scroll-container">
                  <q-table
                    ref="tableRef"
                    class="no-shadow"
                    virtual-scroll
                    bordered
                    :loading="loading"
                    :rows="TaskActivityRows"
                    :columns="TaskActivityColumns"
                    row-key="id"
                    separator="cell"
                    binary-state-sort
                    :rows-per-page-options="[20, 50, 100, 200, 500]"
                  >
                    <template #header="props">
                      <q-tr :props="props" class="bg-primary text-white">
                        <q-th
                          v-for="col in props.cols" :key="col.name"
                          :class="{
                            'text-right': col.name === 'estimateHours',
                            'text-start': col.name !== 'estimateHours'
                          }"
                        >{{ col.label }}<span v-if="['name', 'assignedToId', 'estimateHours'].includes(col.name) && !isPreviousTargetMonth" class="required">*</span>
                        </q-th>
                        <q-th v-if="!isPreviousTargetMonth" class="text-center">Actions</q-th>
                      </q-tr>
                    </template>
                    <template #body="props">
                      <q-tr :class="props.row.deleted ? 'hidden' : ''">
                        <q-td style="width: 22%;">
                          <template v-if="!isPreviousTargetMonth">
                            <formSingleSelectDropdown
                              v-model="props.row.assignedToId"
                              :options="projectCharterEmployeesWithWeeklyPlanHoursForDropdown.list.value"
                              :filter="projectCharterEmployeesWithWeeklyPlanHoursForDropdown.filter"
                              :error="rowValidations[props.rowIndex]?.value?.assignedToId.$error"
                              :error-message="rowValidations[props.rowIndex]?.value?.assignedToId.$errors[0]?.$message"
                            />
                          </template>
                          <template v-else>
                            {{ props.row.assignedTo.person.firstName + " " + props.row.assignedTo.person.lastName }}
                          </template>
                        </q-td>
                        <q-td style="width: 10%;">
                          <template v-if="!isPreviousTargetMonth">
                            <formSingleSelectDropdown
                              v-model="props.row.name"
                              :options="projectTaskActivityNameForDropdownSingleSelect.list.value"
                              :filter="projectTaskActivityNameForDropdownSingleSelect.filter"
                              :error="rowValidations[props.rowIndex]?.value?.name.$error"
                              :error-message="rowValidations[props.rowIndex]?.value?.name.$errors[0]?.$message"
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
                          </template>
                          <template v-else>
                            {{ props.row.name }}
                          </template>
                        </q-td>
                        <q-td style="white-space: normal; overflow-wrap: break-word; width: 55%;">
                          <template v-if="!isPreviousTargetMonth">
                            <q-editor
                              v-model="props.row.description"
                              :dense="$q.screen.lt.md"
                              :toolbar="toolbar"
                              :fonts="fonts"
                            />
                          </template>
                          <template v-else>
                            {{ props.row.description }}
                          </template>
                        </q-td>
                        <q-td class="text-right" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 8%;">
                          <template v-if="!isPreviousTargetMonth">
                            <q-input
                              v-model="props.row.estimateHours"
                              outlined
                              dense
                              input-class="text-right"
                              :error="rowValidations[props.rowIndex]?.value?.estimateHours.$error"
                              :error-message="rowValidations[props.rowIndex]?.value?.estimateHours.$errors[0]?.$message"
                              @blur="rowValidations[props.rowIndex]?.value?.estimateHours.$touch"
                            />
                          </template>
                          <template v-else>
                            <div class="text-right">{{ props.row.estimateHours }}</div>
                          </template>
                        </q-td>
                        <q-td v-if="!isPreviousTargetMonth" class="text-center" style="width: 5%;">
                          <q-icon name="o_delete" size="xs" class="cursor-pointer text-red" @click="deleteRow(props.rowIndex)">
                            <q-tooltip>Delete</q-tooltip>
                          </q-icon>
                        </q-td>
                      </q-tr>
                      <q-tr v-if="props.pageIndex === TaskActivityRows.length - 1">
                        <q-td colspan="3" class="text-right font-bold"><b>Total Hours:</b></q-td>
                        <q-td class="text-right"><b>{{ totalHours }}</b></q-td>
                        <q-td />
                      </q-tr>
                    </template>
                  </q-table>
                </div>
                <div align="center" class="q-gutter-sm justify-center q-mt-sm stickyFooter">
                  <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                  <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
                </div>
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
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, computed, onMounted } from "vue";
import { uid, useDialogPluginComponent, useQuasar } from "quasar";
import { notifySuccess, notifyError } from "assets/utils";

import taskService from "modules/project-tasks/projectTasks.service";
import activityService from "modules/project-tasks-activities/projectTasksActivities.service";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectTasksActivities from "src/modules/project-tasks-activities/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel, onDialogOK } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" },
  projectIdAttr: { type: String, default: "" },
  moduleIdAttr: { type: String, default: "" },
  taskIdAttr: { type: String, default: "" },
  projectName: { type: String, default: "" },
  moduleName: { type: String, default: "" },
  taskName: { type: String, default: "" },
  pageHeading: { type: String, default: "" }
});

// Common variables
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const loading = ref(true);
const processing = ref(false);
const TaskActivityRows = ref([]);
const isPreviousTargetMonth = ref(false);
const TaskId = props.taskIdAttr;
const selectedProjectId = props.projectIdAttr;

const model = ref({
  projectId: props.projectIdAttr !== "" ? props.projectIdAttr : "",
  projectModuleId: props.moduleIdAttr !== "" ? props.moduleIdAttr : "",
  projectName: props.projectName !== "" ? props.projectName : "",
  projectModuleName: props.moduleName !== "" ? props.moduleName : "",
  taskName: props.taskName
});

// Tab Task Activities
const TaskActivityColumns = ref([
  { name: "assignedToId", label: "Activity Owner", field: "assignedToId", align: "left", sortable: false },
  { name: "name", label: "Activity Name", field: "name", align: "left" },
  { name: "description", label: "Description", field: "description", align: "left" },
  { name: "estimateHours", label: "Est.Hrs", field: "estimateHours", align: "right" }
]);

// get project details on edit mode
const getProjectActivityByTaskId = (targetMonth) => {
  loading.value = true;
  activityService.getProjectActivityByTaskId(TaskId, targetMonth).then((resp) => {
    TaskActivityRows.value = resp.map(item => ({
      ...item,
      id: item.id,
      name: item.name,
      assignedToName: item.assignedTo.person.firstName + " " + item.assignedTo.person.lastName,
      assignedToId: item.assignedTo.id,
      description: item.description ? item.description : "",
      startDate: item.startDate,
      endDate: item.endDate,
      popupVisible: false,
      currentView: "Years",
      estimateHours: item.estimateHours,
      editing: false,
      flag: "Edit"
    }));
    // Calculate total hours
    totalHours.value = TaskActivityRows.value.reduce((acc, row) => {
      return acc + (parseFloat(row.estimateHours) || 0);
    }, 0);
  }).finally(() => {
    loading.value = false;
  });
};

const totalHours = computed(() => {
  const total = TaskActivityRows.value.reduce((sum, row) => {
    if (!row.deleted) {
      sum += parseFloat(row.estimateHours) || 0;
    }
    return sum;
  }, 0);

  // Round to 2 decimal places without using toFixed()
  return Math.round(total * 100) / 100;
});

const onAdd = () => {
  TaskActivityRows.value.unshift({
    id: uid(),
    name: "",
    assignedToId: "",
    popupVisible: false,
    currentView: "Years",
    estimateHours: "0",
    description: "",
    deleted: false
  });
};

const deleteRow = (index) => {
  if (TaskActivityRows.value.filter(row => row.deleted === false).length > 1) {
    TaskActivityRows.value[index].deleted = true;
  } else {
    notifyError({ message: "Please add at least one row." });
  }
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { projectCharterEmployeesWithWeeklyPlanHoursForDropdown } = projectModule();

const {
  projectTaskActivityNameForDropdownSingleSelect
} = projectTasksActivities();

// ------------------------------------------------------------------------------------
// Validation rules
// ------------------------------------------------------------------------------------
const rules = {
  projectName: { required: helpers.withMessage("Project is required", required) },
  projectModuleName: { required: helpers.withMessage("project module is required", required) },
  taskName: { required: helpers.withMessage("Task is required", required) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const rowRules = {
  name: { required: helpers.withMessage("Activity Name is required", required) },
  assignedToId: { required: helpers.withMessage("Activity Owner is required", required) },
  estimateHours: {
    required: helpers.withMessage("Est.Hrs is required", required),
    minValue: helpers.withMessage("Invalid Estimate Hours", (value) => value >= 0)
  }
};

const rowValidations = ref([]);

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    let isValid = true;
    if (!await v$.value.$validate()) { return; }
    if (TaskActivityRows.value.length === 0) {
      notifyError({ message: "Add at least one activity." });
      return;
    }
    // Initialize validations for all rows
    const nonDeletedRows = TaskActivityRows.value.filter(row => !row.deleted);
    rowValidations.value = nonDeletedRows.map((row) =>
      useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true })
    );

    // Validate each row
    for (const [index, validation] of rowValidations.value.entries()) {
      if (validation?.value) {
        await validation.value.$touch(); // Mark the row as touched
        const isRowValid = await validation.value.$validate(); // Validate the row
        if (!isRowValid) {
          isValid = false; // If any row is invalid, set isValid to false
        }
      } else {
        console.error(`Validation object for row ${index} is undefined`);
        isValid = false;
      }
    }

    if (!isValid) {
      return; // Prevent submission
    }

    if (isValid) {
      processing.value = true;
      const valueToTextMap = {};
      projectTaskActivityNameForDropdownSingleSelect.list.value.forEach(x => {
        valueToTextMap[x.value] = x.text;
      });

      const cleanedRows = TaskActivityRows.value.map(row => {
        const trimmedHours = (row.estimateHours ?? "").toString().trim();
        let parsedHours;
        if (row.deleted === true) {
          parsedHours = 0.0;
        } else {
          parsedHours = trimmedHours === "" ? null : parseFloat(trimmedHours);
        }
        return {
          ...row,
          estimateHours: parsedHours,
          name: valueToTextMap[row.name] || row.name || "",
          description: row.description ?? "",
          assignedToId: row.assignedToId ?? "",
          activityStatusId: row.activityStatusId ?? "",
          deleted: row.deleted ?? ""
        };
      });

      // Prepare the payload as a plain object
      const payload = {
        projectId: model.value.projectId,
        projectModuleId: model.value.projectModuleId,
        projectActivities: cleanedRows
      };

      // Submit the payload
      taskService.taskAssignToOwner(TaskId, payload).then((resp) => {
        notifySuccess({ message: "Task assigned successfully." });
        onDialogOK();
      });
    }
  } catch (error) {
    console.error("Error in assigning the task:", error);
    notifyError({ message: "An error occurred while while assigning the task." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => TaskId, (newValue, oldValue) => {
  if (newValue) {
    getProjectActivityByTaskId();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  projectTaskActivityNameForDropdownSingleSelect.load("Project Activities");
  projectCharterEmployeesWithWeeklyPlanHoursForDropdown.load(selectedProjectId);
});
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_tasks .q-select__dropdown-icon{
  display: none;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
.task-activities-table{
  white-space: normal !important;
}
.align-hint-right .q-field__bottom .q-field__messages {
  text-align: right;
}
.table-wrapper-container {
  display: flex;
  flex-direction: column;
  height: 100%; /* or use a fixed px/em if needed */
}
.scroll-container {
  flex: 1 1 auto;
  overflow-y: auto;
  max-height: 70vh; /* adjust based on available space */
}
</style>
