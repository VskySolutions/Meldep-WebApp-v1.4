<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none PlannerDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Edit Project Tasks</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogClose()" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Basic Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md edit_bulk_tasks">
                <formSingleSelectDropdown
                  v-model="model.projectId"
                  label="Project"
                  :readonly="!!readonlyProject"
                  :options="projectNameDropdownSingleSelect.list.value"
                  :filter="projectNameDropdownSingleSelect.filter"
                  :wrapperClass="'col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12'"
                />
                <formSingleSelectDropdown
                  v-model="model.projectModuleId"
                  label="Project Module"
                  :readonly="!!readonlyProjectModule"
                  :options="projectModulesByProjectIdForDropdownSingleSelect.list.value"
                  :filter="projectModulesByProjectIdForDropdownSingleSelect.filter"
                  :wrapperClass="'col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12'"
                />
              </div>
            </fieldset>
            <fieldset>
              <legend>Tasks Info</legend>
              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                virtual-scroll
                bordered
                class="no-shadow"
                :loading="loading"
                :rows="rows"
                :columns="columns"
                row-key="id"
                separator="cell"
                no-data-label="No data available"
                binary-state-sort
                :rows-per-page-options="[20, 50, 100, 200, 500]"
              >
                <template #header="props">
                  <q-tr :props="props">
                    <q-th
                      v-for="col in props.cols"
                      :key="col.name"
                      :props="props"
                      >
                      {{ col.label }}
                      <span v-if="['name', 'startDateStr','endDateStr','statusId','priorityId'].includes(col.name)" class="required">*</span>
                    </q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #top-row>
                  <q-tr v-if="mode == 'add' && editingRow" class="row-highlight">
                    <q-td style="width: 400px; max-width: 400px; white-space: normal; overflow-wrap: break-word;">
                      <q-input
                        v-model="editingRow.name"
                        outlined
                        hide-bottom-space
                        :dense="true"
                        :error="editingRowV$.name.$error"
                        :error-message="editingRowV$.name.$errors[0]?.$message"
                        @blur="editingRowV$.name.$touch"
                      />
                    </q-td>
                    <q-td width="10%">
                      <formDate
                        v-model="editingRow.startDateStr"
                        :error="editingRowV$.startDateStr.$error"
                        :error-message="editingRowV$.startDateStr.$errors[0]?.$message"
                        :onBlur="() => editingRowV$.startDateStr.$touch()"
                      />
                    </q-td>
                    <q-td width="10%">
                      <formDate
                        v-model="editingRow.endDateStr"
                        :error="editingRowV$.endDateStr.$error"
                        :error-message="editingRowV$.endDateStr.$errors[0]?.$message"
                        :onBlur="() => editingRowV$.endDateStr.$touch()"
                        :dateOptions="disableBeforeStartDate"
                      />
                    </q-td>
                    <q-td class="text-left" style="width: 300px; max-width: 300px; white-space: normal; overflow-wrap: break-word;">
                      <formSingleSelectDropdown
                        v-model="editingRow.statusId"
                        :options="projectTaskStatusList"
                        :filter="getAllTaskStatusesFilter"
                        :popup-show="() => handlePopupShow(editingRow.status.dropDownValue, model.projectStatus)"
                        :error="editingRowV$.statusId.$error"
                        :error-message="editingRowV$.statusId.$errors[0]?.$message"
                        :on-blur="editingRowV$.statusId.$touch"
                      />
                    </q-td>
                    <q-td class="text-left" style="width: 120px; max-width: 120px; white-space: normal; overflow-wrap: break-word;">
                      <formSingleSelectDropdown
                        v-model="editingRow.priorityId"
                        :options="projectTaskPriorityForDropdownSingleSelect.list.value"
                        :filter="projectTaskPriorityForDropdownSingleSelect.filter"
                        :error="editingRowV$.priorityId.$error"
                        :error-message="editingRowV$.priorityId.$errors[0]?.$message"
                        :on-blur="editingRowV$.priorityId.$touch"
                      />
                    </q-td>
                    <q-td style="white-space: normal; overflow-wrap: break-word; width: 400px;">
                      <q-editor
                        v-model="editingRow.description" y:dense="$q.screen.lt.md" :toolbar="[
                          [
                            {
                              label: $q.lang.editor.align,
                              icon: $q.iconSet.editor.align,
                              fixedLabel: true,
                              list: 'only-icons',
                              options: ['left', 'center', 'right', 'justify']
                            },
                          ],
                          ['bold', 'italic', 'strike', 'underline'],
                        ]"
                        style="width: 400px;"
                      />
                    </q-td>
                    <q-td auto-width class="text-center">
                      <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                        <q-tooltip>Save</q-tooltip>
                      </q-icon>
                      <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                        <q-tooltip>Cancel</q-tooltip>
                      </q-icon>
                    </q-td>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :props="props" :class="activeBulkRowId == props.row.id ? 'highlight' : ''">
                    <q-td class="text-left" style="width: 400px; max-width: 400px; white-space: normal; overflow-wrap: break-word;">
                      <q-input
                        v-if="mode == 'edit' && editingRow && props.row.id === activeBulkRowId"
                        v-model="editingRow.name"
                        outlined
                        hide-bottom-space
                        :dense="true"
                        autogrow
                        :error="editingRowV$.name.$error"
                        :error-message="editingRowV$.name.$errors[0]?.$message"
                        @blur="editingRowV$.name.$touch"
                      />
                      <!-- <span v-else :class="props.row.deleted ? 'text-delete' : ''" style="display: inline-block; width: 100px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">{{ props.row.name }} </span> -->
                      <span
                        v-else
                        :class="props.row.deleted ? 'text-delete' : ''"
                        style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 28% !important;"
                      >
                        <p class="q-mb-none" v-html="props.row.name" />
                      </span>
                    </q-td>
                    <q-td class="text-left">
                      <formDate
                        v-if="mode == 'edit' && editingRow && props.row.id === activeBulkRowId"
                        v-model="editingRow.startDateStr"
                        :error="editingRowV$.startDateStr.$error"
                        :error-message="editingRowV$.startDateStr.$errors[0]?.$message"
                        :onBlur="() => editingRowV$.startDateStr.$touch()"
                      />
                      <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.startDateStr }} </span>
                    </q-td>
                    <q-td class="text-left">
                      <formDate
                        v-if="mode == 'edit' && editingRow && props.row.id === activeBulkRowId"
                        v-model="editingRow.endDateStr"
                        :error="editingRowV$.endDateStr.$error"
                        :error-message="editingRowV$.endDateStr.$errors[0]?.$message"
                        :onBlur="() => editingRowV$.endDateStr.$touch()"
                        :dateOptions="disableBeforeStartDate"
                      />
                      <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.endDateStr }} </span>
                    </q-td>
                    <q-td class="text-left">
                      <formSingleSelectDropdown
                        v-if="mode == 'edit' && editingRow && props.row.id === activeBulkRowId"
                        v-model="editingRow.statusId"
                        :options="projectTaskStatusList"
                        :filter="getAllTaskStatusesFilter"
                        :popup-show="() => handlePopupShow(editingRow.status.dropDownValue, model.projectStatus)"
                        :error="editingRowV$.statusId.$error"
                        :error-message="editingRowV$.statusId.$errors[0]?.$message"
                        :on-blur="editingRowV$.statusId.$touch"
                      />
                      <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getStatus(props.row.statusId) }} </span>
                    </q-td>
                    <q-td class="text-left">
                      <formSingleSelectDropdown
                        v-if="mode == 'edit' && editingRow && props.row.id === activeBulkRowId"
                        v-model="editingRow.priorityId"
                        :options="projectTaskPriorityForDropdownSingleSelect.list.value"
                        :filter="projectTaskPriorityForDropdownSingleSelect.filter"
                        :error="editingRowV$.priorityId.$error"
                        :error-message="editingRowV$.priorityId.$errors[0]?.$message"
                        :on-blur="editingRowV$.priorityId.$touch"
                      />
                      <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getPriority(props.row.priorityId) }} </span>
                    </q-td>
                    <q-td class="text-left" style="white-space: normal; overflow-wrap: break-word; width: 400px;">
                      <q-editor
                        v-if="mode == 'edit' && editingRow && props.row.id === activeBulkRowId" v-model="editingRow.description" :dense="$q.screen.lt.md" :toolbar="[
                          [
                            {
                              label: $q.lang.editor.align,
                              icon: $q.iconSet.editor.align,
                              fixedLabel: true,
                              list: 'only-icons',
                              options: ['left', 'center', 'right', 'justify']
                            },
                          ],
                          ['bold', 'italic', 'strike', 'underline'],
                        ]"
                        style="width: 400px;"
                      />
                      <span v-if="mode !== 'edit' || props.row.id !== activeBulkRowId" :class="props.row.deleted ? 'text-delete RichTextEditor' : 'normal-text RichTextEditor'" v-html="props.row.description" />
                    </q-td>
                    <q-td auto-width class="text-center">
                      <template v-if="mode == 'edit' && editingRow && props.row.id === activeBulkRowId">
                        <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                          <q-tooltip>Save</q-tooltip>
                        </q-icon>
                        <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                          <q-tooltip>Cancel</q-tooltip>
                        </q-icon>
                      </template>
                      <template v-else>
                        <q-icon name="o_edit" size="xs" class="cursor-pointer q-mr-lg" @click="onEdit(props.row)">
                          <q-tooltip>Edit</q-tooltip>
                        </q-icon>
                        <q-icon v-if="!props.row.deleted" name="o_delete_outline" color="red" size="xs" class="cursor-pointer" @click="onDelete(props.row)">
                          <q-tooltip>Delete</q-tooltip>
                        </q-icon>
                        <q-icon v-if="props.row.deleted" name="o_redo" size="xs" class="cursor-pointer" @click="onUndo(props.row)">
                          <q-tooltip>Undo</q-tooltip>
                        </q-icon>
                      </template>
                    </q-td>
                  </q-tr>
                </template>
              </q-table>
            </fieldset>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="stickyFooter">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogClose()" />
          <q-btn color="primary" push outline label="Update" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
      <!-- </q-card-section> -->
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, uid } from "quasar";
import useVuelidate from "@vuelidate/core";
import taskService from "modules/project-tasks/projectTasks.service";
import { required, helpers } from "@vuelidate/validators";
import { ref, onMounted, watch } from "vue";
import _ from "lodash";
import { notifySuccess, notifyError, zwConfirmLeave } from "assets/utils";
import commonService from "services/common.service";
import { isDate } from "validators/zw_validators.js";
import { format, parse } from "date-fns"; // Standard TimeZone Conversion

// SOP Change :- Shared Dropdowns
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// SOP Change :- Shared Dropdowns
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, projectIdAttr: { type: String, default: "" }, moduleIdAttr: { type: String, default: "" } });

const readonlyProject = props.projectIdAttr ? "readonly" : "";
const readonlyProjectModule = props.moduleIdAttr ? "readonly" : "";

// Common variables
const loading = ref(true);
const processing = ref(false);

const tableRef = ref();
const rows = ref([]);
const mode = ref(null);
const editingRow = ref(null);
const activeBulkRowId = ref(null);

const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "name", label: "Task Name", field: "activityName", align: "left", sortable: true },
  { name: "startDateStr", label: "Start Date", field: "startDateStr", align: "left", sortable: true },
  { name: "endDateStr", label: "End Date", field: "endDateStr", align: "left", sortable: true },
  { name: "statusId", label: "Status", field: "statusId", align: "left", sortable: true },
  { name: "priorityId", label: "Priority", field: "priorityId", align: "left", sortable: true },
  { name: "description", label: "Description", field: "description", align: "left", sortable: true }
]);

// Define model values
const model = ref({
  projectId: props.projectIdAttr,
  projectModuleId: props.moduleIdAttr
});

// Multiple row validation
const editingRowRules = {
  name: { required: helpers.withMessage("Task name is required", required) },
  startDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    required: helpers.withMessage("Start date is required", required)
  },
  endDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    required: helpers.withMessage("End date is required", required),
    afterStartDate: helpers.withMessage("End date must occur after the start date", (value, { startDateStr }) => {
      if (!value) return true;
      return new Date(value) >= new Date(startDateStr);
    })
  },
  statusId: { required: helpers.withMessage("Status is Required", required) },
  priorityId: { required: helpers.withMessage("Priority is Required", required) }
};
const editingRowV$ = useVuelidate(editingRowRules, editingRow, { $lazy: true, $autoDirty: true });

let isSaveDialog = false;
let isConfirmSaveDialog = false;

function onEdit (item) {
  let isContinue = 0;
  if (isConfirmSaveDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onEditConfirm(item);
    }, () => {
      isContinue = 0;
    });
  } else {
    isContinue = 1;
  }
  if (isContinue === 1) {
    onEditConfirm(item);
  }
}

function onEditConfirm (item) {
  isSaveDialog = true;
  isConfirmSaveDialog = true;
  mode.value = "edit";
  editingRow.value = _.cloneDeep(item);
  activeBulkRowId.value = item.id;
}

function disableBeforeStartDate (date) {
  // If no Start Date is set, allow all dates
  if (!editingRow.value.startDateStr) {
    return true;
  }

  const start = parse(editingRow.value.startDateStr, "MM/dd/yyyy", new Date());
  const currentDate = parse(date, "yyyy/MM/dd", new Date());

  // Disable dates before the Start Date
  return currentDate >= start;
}

// Get row status
function getStatus (value) {
  if (value) {
    return projectTaskStatusListArr.value.find((item) => item.id === value)?.dropdownValue;
  }
}

// Get row priority
function getPriority (value) {
  if (value) {
    return projectTaskPriorityForDropdownSingleSelect.list.value.find((item) => item.value === value)?.text;
  }
}

// get all task
async function getTasksByProjectModule () {
  loading.value = true;

  try {
    const resp = await taskService.getTasksByProjectModule(props.moduleIdAttr);

    rows.value = (resp || []).map(item => ({
      ...item,
      assignedToId: item.assignedToId,
      description: item.description || "",
      startDateStr: item.startDate ? format(item.startDate, "MM/dd/yyyy") : "",
      endDateStr: item.endDate ? format(item.endDate, "MM/dd/yyyy") : "",
      editing: false,
      flag: "Edit"
    }));
  } catch (error) {
    console.error("Error fetching tasks:", error);
    rows.value = [];
    notifyError({ message: "Failed to load project tasks." });
  } finally {
    // Remove loader only after full data load / error completion
    loading.value = false;
  }
}
// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { projectNameDropdownSingleSelect
} = projectModule();

const {
  projectTaskPriorityForDropdownSingleSelect
} = projectTaskModule();

const { projectModulesByProjectIdForDropdownSingleSelect } = projectModuleOfProjectModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get All Dropdowns
// --------------------------------------------------------------------------------------------------------------------------------------------------

function handlePopupShow (taskStatus, projectStatusLabel) {
  getAllTaskStatuses("Task Status", taskStatus, projectStatusLabel);
}
// Get all project task status List
const projectTaskStatusList = ref([]);
const projectTaskStatusListArr = ref([]);
const projectTaskStatusListOptions = ref([]);
function getAllTaskStatuses (typeName, taskStatusLabel = null, projectStatusLabel = null) {
  commonService.getDropDown(typeName).then((resp) => {
    projectTaskStatusListArr.value = resp;
    const lockedStatuses = ["Cancelled", "Completed", "On Hold"];
    // const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    const responseData = resp.map((item) => {
      const label = item.dropdownValue;
      let shouldDisable = false;

      if (projectStatusLabel === "New") {
        // Only allow "New", disable everything else
        shouldDisable = label !== "New";
      } else if (lockedStatuses.includes(projectStatusLabel) && !props.id) {
        shouldDisable = label === "Open";
      } else if (lockedStatuses.includes(projectStatusLabel) && taskStatusLabel === "New") {
        shouldDisable = label === "Open";
      }
      return {
        text: item.dropdownValue,
        value: item.id,
        disable: shouldDisable
      };
    });

    projectTaskStatusList.value = responseData;
    projectTaskStatusListOptions.value = responseData;
  });
}

// Search project task status List for dropdown
function getAllTaskStatusesFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectTaskStatusList.value = projectTaskStatusListOptions.value;
    } else {
      projectTaskStatusList.value = projectTaskStatusListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Save & Close
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Save row
async function onSave (editingRowId = null) {
  if (!await editingRowV$.value.$validate()) {
    return;
  }
  isConfirmSaveDialog = false;
  if (mode.value === "edit") {
    if (editingRowId != null) {
      editingRow.value.id = editingRowId;
    }
    const rowIndex = rows.value.findIndex((row) => row.id === editingRow.value.id);

    if (rowIndex !== -1) {
      rows.value.splice(rowIndex, 1, {
        ...rows.value[rowIndex],
        name: editingRow.value.name,
        startDateStr: editingRow.value.startDateStr,
        endDateStr: editingRow.value.endDateStr,
        statusId: editingRow.value.statusId,
        priorityId: editingRow.value.priorityId,
        // estimateTime: editingRow.value.estimateTime ? editingRow.value.estimateTime : "00.00",
        description: editingRow.value.description,
        flag: "Edit"
      });
      editingRow.value = null;
      mode.value = null;
      activeBulkRowId.value = null;
    }
  } else if (mode.value === "add") {
    const newRow = {
      id: uid(),
      name: editingRow.value.name,
      startDateStr: editingRow.value.startDateStr,
      endDateStr: editingRow.value.endDateStr,
      statusId: editingRow.value.statusId,
      priorityId: editingRow.value.priorityId,
      // estimateTime: editingRow.value.estimateTime ? editingRow.value.estimateTime : "00.00",
      description: editingRow.value.description,
      flag: "New"
    };

    rows.value.unshift(newRow);
    mode.value = null;
    activeBulkRowId.value = null;
  }
}

// Remove row
function onDelete (item) {
  item.deleted = true;
  isSaveDialog = true;
  const rowIndex = rows.value.findIndex((row) => row.id === item.id);
  if (rowIndex !== -1) {
    rows.value.splice(rowIndex, 1, {
      ...rows.value[rowIndex],
      id: item.id,
      name: item.name,
      startDateStr: item.startDateStr,
      endDateStr: item.endDateStr,
      description: item.description,
      flag: "Delete"
    });
  }
  // activeBulkRowId.value = item.id;
}

function onUndo (item) {
  item.deleted = false;
  activeBulkRowId.value = null;
}

function onCancel () {
  isConfirmSaveDialog = false;
  mode.value = null;
  editingRow.value = null;
  activeBulkRowId.value = null;
}

function onDialogClose () {
  if (isSaveDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onDialogCancel();
    }, () => {
    });
  } else {
    onDialogCancel();
  }
}

async function onSubmit () {
  processing.value = true;
  try {
    if (rows.value.length === 0) {
      notifyError({ message: "Add at-least one activity." });
    } else {
      if ((mode.value === "edit")) {
        return;
      }
      processing.value = true;
      // model.value.estimateTime = model.value.estimateTime ? model.value.estimateTime : "00.00";
      const payload = {
        projectId: model.value.projectId,
        projectModuleId: model.value.projectModuleId,
        projectTaskModel: rows.value
      };
      taskService.updateBulkTasks(payload).then(resp => {
        notifySuccess({ message: "Tasks are saved successfully." });
        onDialogOK();
        processing.value = false;
      });
    }
  } catch (error) {
    console.error("Error in submitting the project users:", error);
    notifyError({ message: "An error occurred while assigning users to the project." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

watch(() => model.value.projectId, async (newId) => {
  await projectNameDropdownSingleSelect.load();
  const selected = projectNameDropdownSingleSelect.list.value.find(
    (p) => p.value === newId
  );

  if (selected) {
    model.value.projectStatus = selected.data;
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  projectModulesByProjectIdForDropdownSingleSelect.load(false, false, model.value.projectId);
  getTasksByProjectModule();
  getAllTaskStatuses("Task Status");
  projectTaskPriorityForDropdownSingleSelect.load("Task Priorities");
});

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_bulk_tasks .q-select__dropdown-icon{
  display: none;
}
</style>
