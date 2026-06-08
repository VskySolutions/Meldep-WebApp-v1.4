<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1500px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div v-if="props.requirementId || props.issueId" class="text-h2 text-white">Convert to Task</div>
        <div v-else class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Project Task</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form ref="formRef" greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable']">
          <div class="q-gutter-y-md">
            <q-card>
              <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator inline-label mobile-arrows>
                <q-tab name="1_tab" label="Tasks" class="q-px-lg q-mr-md" />
                <q-tab name="2_tab" label="Task Activities" class="q-px-lg" :disable="disableTab" />
              </q-tabs>
              <q-tab-panels v-model="tab" animated>
                <q-tab-panel name="1_tab">
                  <fieldset>
                    <legend>Task Info</legend>
                    <div v-if="props.taskNumbers.length" class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-md-12">
                        <label class="label q-mb-xs text-black text-bold">
                          Already created Task(s):
                        </label>
                        <span>{{ props.taskNumbers.map(num => `#${num}`).join(", ") }}</span>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <formSingleSelectDropdown
                        v-model="model.projectId"
                        label="Project"
                        :readonly="!!readonlyProject"
                        :class="readonlyProject !== '' ? 'edit_tasks' : ''"
                        :options="projectNameDropdownSingleSelect.list.value"
                        :filter="projectNameDropdownSingleSelect.filter"
                        :wrapperClass="'col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12'"
                        :error="v$.projectId.$error"
                        :error-message="v$.projectId.$errors[0]?.$message"
                      >
                        <template #after>
                          <q-icon
                            v-if="!readonlyProject && TaskId == ''"
                            name="o_add"
                            color="primary"
                            class="cursor-pointer q-ml-xs add-icon"
                            @click="onProjectAdd(null, refreshProjectDropdownList)"
                          >
                            <q-tooltip>Add new Project</q-tooltip>
                          </q-icon>
                        </template>
                      </formSingleSelectDropdown>
                      <formSingleSelectDropdown
                        v-model="model.projectModuleId"
                        label="Project Module"
                        :readonly="!!readonlyProjectModule"
                        :disable="!model.projectId"
                        :class="readonlyProjectModule !== '' ? 'edit_tasks' : ''"
                        :options="projectModulesByProjectIdForDropdownSingleSelect.list.value"
                        :filter="projectModulesByProjectIdForDropdownSingleSelect.filter"
                        :wrapperClass="'col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12'"
                        :error="v$.projectModuleId.$error"
                        :error-message="v$.projectModuleId.$errors[0]?.$message"
                      >
                      <template v-if="model.projectId" #after>
                        <q-icon v-if="!readonlyProjectModule && TaskId == ''"
                          name="o_add"
                          color="primary"
                          class="cursor-pointer q-ml-xs add-icon"
                          @click="onProjectModuleAdd(model?.projectId, null, refreshProjectModuleDropdownList)">
                          <q-tooltip>Add new Project Module</q-tooltip>
                        </q-icon>
                      </template>
                      </formSingleSelectDropdown>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12">
                        <label class="label q-mb-xs text-black">Task Name<span class="required">*</span></label>
                        <div>
                          <q-input
                            v-model="model.name"
                            outlined
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            :error="v$.name.$error"
                            :error-message="v$.name.$errors[0]?.$message"
                            @blur="v$.name.$touch"
                          />
                        </div>
                      </div>
                      <formSingleSelectDropdown
                        v-model="model.areaId"
                        label="Area"
                        :required="false"
                        :wrapperClass="'col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12'"
                        :options="areaForDropdownSingleSelect.list.value"
                        :filter="areaForDropdownSingleSelect.filter"
                      />
                      <formSingleSelectDropdown
                        v-model="model.workspaceId"
                        label="Workspace"
                        :required="false"
                        :wrapperClass="'col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12'"
                        :options="workspaceForDropdownSingleSelect.list.value"
                        :filter="workspaceForDropdownSingleSelect.filter"
                      />
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <formSingleSelectDropdown
                        v-model="model.actionId"
                        label="Action"
                        :required="false"
                        :wrapperClass="'col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12'"
                        :options="projectTaskActionForDropdownSingleSelect.list.value"
                        :filter="projectTaskActionForDropdownSingleSelect.filter"
                      />
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="row items-end no-wrap full-width">
                          <div class="col">
                          <formSingleSelectDropdown
                            v-model="model.assignedToId"
                            label="Task Owner"
                            :required="false"
                            :wrapperClass="'col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12'"
                            :options="projectEmployeeDropdownSingleSelect.list.value"
                            :filter="projectEmployeeDropdownSingleSelect.filter"
                          />
                          </div>
                          <div v-if="model.assignedToId && props.id" class="q-pl-sm q-pb-sm flex flex-center">
                            <q-icon
                              name="o_history"
                              class="cursor-pointer q-ml-sm"
                              size="xs"
                              @click.stop="onSiteModifiedLog(model.id, model.name, 'Task Owner')"
                            >
                              <q-tooltip>Data Change Log</q-tooltip>
                            </q-icon>
                          </div>
                        </div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <label class="label q-mb-xs text-black">Estimate Hours<span class="required">*</span></label>
                        <div>
                          <q-input
                            v-model="model.estimateTime"
                            outlined
                            dense
                            maxlength="7"
                            :rules="[validateHours]"
                            :error="v$.estimateTime.$error"
                            :error-message="v$.estimateTime.$errors[0]?.$message"
                            @blur="v$.estimateTime.$touch"
                          />
                        </div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <label class="label q-mb-xs text-black">Status<span class="required">*</span></label>
                        <div>
                          <q-select
                            v-model="model.statusId"
                            clearable
                            use-input
                            outlined
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            :options="projectTaskStatusList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            :disable="model.status?.dropDownValue?.toLowerCase() === 'close' && !!props.id"
                            :error="v$.statusId.$error"
                            :error-message="v$.statusId.$errors[0]?.$message"
                            @filter="getAllTaskStatusesfilter"
                            @popup-show="() => handlePopupShow(model.status.dropDownValue, model.projectStatus)"
                            @blur="v$.statusId.$touch"
                          >
                            <template #option="{ itemProps, opt }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center">
                                    <span>{{ opt.text }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <formSingleSelectDropdown
                        v-model="model.priorityId"
                        label="Priority"
                        :wrapperClass="'col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12'"
                        :options="projectTaskPriorityForDropdownSingleSelect.list.value"
                        :filter="projectTaskPriorityForDropdownSingleSelect.filter"
                        :error="v$.priorityId.$error"
                        :error-message="v$.priorityId.$errors[0]?.$message"
                      />
                      <formDate
                        v-model="model.startDateStr"
                        label="Start Date"
                        :error="v$.startDateStr.$error"
                        :error-message="v$.startDateStr.$errors[0]?.$message"
                        :onBlur="() => v$.startDateStr.$touch()"
                      />
                      <formDate
                        v-model="model.endDateStr"
                        label="Due Date"
                        :error="v$.endDateStr.$error"
                        :error-message="v$.endDateStr.$errors[0]?.$message"
                        :onBlur="() => v$.endDateStr.$touch()"
                        :dateOptions="disableBeforeStartDate"
                      />
                      <formSingleSelectDropdown
                        v-model="model.typeId"
                        label="Task Type"
                        :required="false"
                        :wrapperClass="'col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12'"
                        :options="projectTaskTypeForDropdownSingleSelect.list.value"
                        :filter="projectTaskTypeForDropdownSingleSelect.filter"
                      />
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-3 col-md-3 col-lg-3">
                        <div class="q-mb-xs text-black">
                          Sort Order<span class="required">*</span>
                        </div>
                        <div class="flex items-center">
                          <q-input
                            v-model="fraction"
                            outlined
                            dense
                            maxlength="5"
                            :rules="[val => validateSortOrder(val) || 'Enter a valid sort order']"
                            @input="onInput"
                          >
                            <template #prepend>
                              <span class="fs-13">{{ prefix }}.</span>
                            </template>
                          </q-input>
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-xxl-12 col-lg-12 col-md-12 col-sm-12 col-xs-12">
                        <div class="form-group relative-position">
                          <label class="label q-mb-xs text-black">Task Description</label>
                          <div class="relative-position">
                            <q-editor
                              v-model="model.description"
                              :dense="$q.screen.lt.md"
                              :toolbar="toolbar"
                              :fonts="fonts"
                              class="relative-position"
                            />
                            <q-inner-loading
                              v-if="loadingDescription"
                              showing
                              class="absolute-full"
                              color="primary"
                              size="30px"
                            />
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-lg">
                      <div class="col-12 q-mb-xs text-black">Project Task Files</div>
                      <div class="col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12">
                        <div class="form-group">
                          <multiFileUploader
                            :initial-files="model.projectTaskFiles"
                            :allowed-extensions="[
                              '.pdf','.xls','.xlsx','.doc','.docx','.jpeg','.jpg','.png','.ppt','.pptx'
                            ]"
                            :max-size-in-mb="25"
                            label="Drag files here or (+) to upload."
                            @files-selected="handleFiles"
                          />
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-lg">
                      <div v-if="model.projectTaskFiles && model.projectTaskFiles.length > 0" class="row q-gutter-md">
                        <div
                          v-for="(file, index) in model.projectTaskFiles"
                          :key="index"
                          class="col-3 position-relative file-card text-center"
                          style="max-width: 140px; min-width: 140px;"
                        >
                          <div class="file-preview square-box">
                            <template v-if="isImageFile(file)">
                              <img
                                :src="file.file.virtualPath ? file.file.virtualPath : getFilePreview(file.file)"
                                alt="File Preview"
                                class="square-content centered-image"
                              >
                            </template>
                            <template v-else>
                              <q-icon
                                :name="getFileIcon(file.file?.mimeType)"
                                class="file-icon square-content"
                                size="70px"
                              />
                            </template>
                            <div class="file-name q-mt-sm">
                              <q-btn
                                v-if="file.file?.virtualPath || file?.name"
                                class="bg-primary text-white q-pa-xs"
                                no-caps
                                @click="viewFile(file)"
                              >
                                <span class="truncate-text">
                                  {{ file.file?.name || file.name || extractFileName(file.file?.seoFilename) }}
                                </span>
                              </q-btn>
                            </div>
                          </div>
                          <!-- Remove Button -->
                          <q-btn
                            color="negative"
                            flat
                            round
                            dense
                            icon="o_close"
                            class="remove-file-icon"
                            @click="removeFile(index)"
                          />
                        </div>
                      </div>
                    </div>
                    <div align="center" class="q-gutter-sm justify-center">
                      <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                      <q-btn v-if="tab === '1_tab'" label="Save & Next" type="submit" color="primary" class="actionBtn" :loading="processing" no-caps />
                      <q-btn label="Save & Close" type="button" color="primary" class="actionBtn" :loading="processingClose" no-caps @click="onSubmitClose()" />
                    </div>
                  </fieldset>
                </q-tab-panel>
                <q-tab-panel name="2_tab">
                  <fieldset>
                    <legend>Task Activity Info</legend>
                    <div>
                      <div class="row items-end q-mb-sm q-col-gutter-x-md">
                        <div class="col-xs-12 col-sm-12 flex justify-end">
                          <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAdd" />
                        </div>
                      </div>
                      <q-table
                        ref="tableRef"
                        class="no-shadow"
                        virtual-scroll bordered
                        :loading="loading"
                        :rows="TaskActivitiesRows"
                        :columns="TaskActivitiesColumns"
                        row-key="id" separator="cell"
                        binary-state-sort
                        :rows-per-page-options="[20, 50, 100, 200, 500]"
                      >
                        <template #header="props">
                          <q-tr :props="props" class="bg-primary text-white">
                            <q-th v-for="col in props.cols" :key="col.name" class="text-start">{{ col.label }}<span v-if="['name', 'assignedToId', 'estimateHours'].includes(col.name)" class="required">*</span>
                            </q-th>
                            <q-th class="text-center">Actions</q-th>
                          </q-tr>
                        </template>
                        <template #body="props">
                          <q-tr :class="props.row.deleted ? 'hidden' : ''">
                            <q-td style="width: 10%;">
                              <formSingleSelectDropdown
                                v-model="props.row.assignedToId"
                                :options="projectCharterEmployeesWithWeeklyPlanHoursForDropdown.list.value"
                                :filter="projectCharterEmployeesWithWeeklyPlanHoursForDropdown.filter"
                                :error="rowValidations[props.rowIndex]?.value?.assignedToId.$error"
                                :error-message="rowValidations[props.rowIndex]?.value?.assignedToId.$errors[0]?.$message"
                              />
                            </q-td>
                            <q-td style="width: 10%;">
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
                            </q-td>
                            <q-td style="white-space: normal; overflow-wrap: break-word; width: 40%;">
                              <q-editor
                              v-model="props.row.description"
                              :dense="$q.screen.lt.md"
                              :toolbar="rowToolbar"
                              :fonts="fonts"
                              class="relative-position"
                              />
                            </q-td>
                            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 8%;">
                              <q-input
                                v-model="props.row.estimateHours"
                                outlined
                                dense
                                :error="rowValidations[props.rowIndex]?.value?.estimateHours.$error"
                                :error-message="rowValidations[props.rowIndex]?.value?.estimateHours.$errors[0]?.$message"
                                @blur="rowValidations[props.rowIndex]?.value?.estimateHours.$touch"
                              />
                            </q-td>
                            <q-td class="text-center" style="width: 5%;">
                              <q-icon name="o_delete" size="xs" class="cursor-pointer text-red" @click="deleteRow(props.rowIndex)">
                                <q-tooltip>Delete</q-tooltip>
                              </q-icon>
                            </q-td>
                          </q-tr>
                          <q-tr v-if="props.pageIndex === TaskActivitiesRows.length - 1">
                            <q-td colspan="3" class="text-right font-bold"><b>Total Hours:</b></q-td>
                            <q-td class="text-right"><b>{{ totalHours }}</b></q-td>
                            <q-td />
                          </q-tr>
                        </template>
                      </q-table>
                    </div>
                    <div align="center" class="q-gutter-sm justify-center q-mt-sm">
                      <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                      <q-btn v-if="tab === '1_tab'" label="Save & Next" type="submit" color="primary" class="actionBtn" :loading="processing" no-caps />
                      <q-btn label="Save & Close" type="button" color="primary" class="actionBtn" :loading="processing" no-caps @click="onSubmitClose()" />
                    </div>
                  </fieldset>
                </q-tab-panel>
              </q-tab-panels>
            </q-card>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { uid, useDialogPluginComponent, useQuasar } from "quasar";
import { ref, watch, computed, onMounted, toRaw } from "vue";
import { isDate } from "validators/zw_validators.js";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { notifySuccess, getLocalStorage, notifyError, notifyWarning, zwConfirm } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import _ from "lodash";
import useFilters from "composables/useFilters";
import { format, parse } from "date-fns"; // Standard TimeZone Conversion

import projectModuleService from "modules/project-modules/projectModules.service";
import taskService from "modules/project-tasks/projectTasks.service";
import commonService from "services/common.service";
import requirementService from "src/modules/requirement/requirement.service";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";
// Shared Inputs
import multiFileUploader from "src/components/form-inputs/_multiFileUpload.vue";

// SOP Change :- Shared Dropdowns
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import projectTasksActivities from "src/modules/project-tasks-activities/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Project Dialogs
import { onProjectAdd } from "src/modules/project/utils/dialogs.js";

// Shared Project Module Dialogs
import {
  onProjectModuleAdd
} from "src/modules/project-modules/utils/dialogs.js";

// SOP Change :- Shared Site Dialogs
import {
  onSiteModifiedLog
} from "src/modules/sites/utils/dialogs.js";

const $q = useQuasar();
const { fonts, toolbar, rowToolbar } = getEditorConfig($q);
const tab = ref("1_tab");
// Common variables
const { toDate } = useFilters();
const { toPrice } = useFilters();
const loading = ref(true);
const processing = ref(false);
const processingClose = ref(false);
const rows = ref([]);
const TaskActivitiesRows = ref([]);
const rowValidations = ref([]);
const loadingDescription = ref(false);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();
const props = defineProps({
  id: { type: String, default: "" },
  projectIdAttr: { type: String, default: "" },
  moduleIdAttr: { type: String, default: "" },
  issueProjectId: { type: String, default: "" },
  issueModuleId: { type: String, default: "" },
  issuePriorityId: { type: String, default: "" },
  name: { type: String, default: "" },
  description: { type: String, default: "" },
  projectName: { type: String, default: "" },
  moduleName: { type: String, default: "" },
  startDate: { type: String, default: "" },
  endDate: { type: String, default: "" },
  issueId: { type: String, default: "" },
  requirementId: { type: String, default: "" },
  isIssueConverted: { type: Boolean, default: false },
  isRequirementConverted: { type: Boolean, default: false },
  taskNumbers: { type: Array, default: () => [] }
});

let TaskId = props.id;
let disableTab = true;
if (TaskId) {
  disableTab = false;
}

const readonlyProject = props.projectIdAttr || props.issueProjectId ? "readonly" : "";
const readonlyProjectModule = props.moduleIdAttr ? "readonly" : "";

// Define model values
const model = ref({
  projectId: props.projectIdAttr !== "" ? props.projectIdAttr : (props.issueProjectId && props.issueProjectId !== null ? props.issueProjectId : null),
  projectModuleId: props.moduleIdAttr !== "" ? props.moduleIdAttr : (props.issueModuleId && props.issueModuleId !== null ? props.issueModuleId : null),
  name: props.name !== "" ? props.name : "",
  startDateStr: props.startDate ? props.startDate : format(new Date(), "MM/dd/yyyy"),
  endDateStr: props.endDate,
  description: props.description ? props.description : "",
  projectActivities: [],
  projectName: props.projectName,
  projectModuleName: props.moduleName,
  isIssueConverted: props.isIssueConverted,
  isRequirementConverted: props.isRequirementConverted,
  issueId: props.issueId,
  requirementId: props.requirementId,
  status: {
    dropDownValue: ""
  }
});

// Tab Task Activities
const TaskActivitiesColumns = ref([
  { name: "assignedToId", label: "Activity Owner", field: "assignedToId", align: "left", sortable: false },
  { name: "name", label: "Activity Name", field: "name", align: "left" },
  { name: "description", label: "Description", field: "description", align: "left" },
  { name: "estimateHours", label: "Est.Hrs", field: "estimateHours", align: "left" }
]);

// ==================================================================================
// sort order
// ==================================================================================
const prefix = ref("0");
const fraction = ref("0");
let existingModuleId = null;
const getNextSortOrderOfProjectModuleAndTask = (moduleId) => {
  loading.value = true;
  projectModuleService
    .getNextSortOrderOfProjectModuleAndTask(null, moduleId)
    .then((resp) => {
      // resp contains { nextSortOrderOfProjectModule, nextSortOrderOfProjectTask }

      // module sort order (prefix)
      prefix.value = String(resp.selectedModuleSortOrder || 0);
      if (!props.id || existingModuleId !== moduleId) {
      // task sort order (fraction part)
        const formatted = Number(resp.nextSortOrderOfProjectTask).toFixed(3);
        if (formatted) {
        // take decimal part if exists
          fraction.value = String(formatted).split(".")[1] || "1";
        } else {
          fraction.value = "1"; // start fresh
        }
      } else {
        const currentModuleSortOrderFormatted = Number(resp.currentModuleSortOrder).toFixed(3);
        if (currentModuleSortOrderFormatted) {
        // take decimal part if exists
          fraction.value = String(currentModuleSortOrderFormatted).split(".")[1] || "1";
        } else {
          fraction.value = "1"; // start fresh
        }
      }

      // 3. final combined value
      model.value.sortOrder = `${prefix.value}.${fraction.value}`;
    })
    .finally(() => {
      loading.value = false;
    });
};

function getSortOrderByModuleId (moduleId) {
  model.value.sortOrder = "";
  getNextSortOrderOfProjectModuleAndTask(moduleId);
}

const isDescriptionEmpty = (html) => {
  if (!html || html.trim() === "") return true;
  const hasImage = /<img[\s\S]*?>/i.test(html);
  if (hasImage) return false;
  let plainText = html.replace(/<[^>]*>/g, "");
  plainText = plainText.replace(/&nbsp;/g, " ");

  return plainText.trim() === "";
};

const getRequirementDescriptionById = async (id) => {
  const resp = await requirementService.getRequirementDescriptionById(id);
  const description = resp?.description || "";
  if (isDescriptionEmpty(description)) {
    return;
  }
  loadingDescription.value = true;
  setTimeout(() => {
    model.value.description = description;
    loadingDescription.value = false;
  }, 1500);
};

let oldStatus = null;
// get project details on edit mode
const getProjectTask = () => {
  loading.value = true;
  taskService.getProjectTaskDetails(TaskId).then((resp) => {
    model.value = _.cloneDeep(resp);
    oldStatus = model.value.status.dropDownValue;
    // const formatted = Number(resp.sortOrder).toFixed(3);
    // fraction.value = formatted;
    model.value.estimateTime = toPrice(resp.estimateTime);
    model.value.startDateStr = resp.startDate ? toDate(resp.startDate) : "";
    model.value.endDateStr = resp.endDate ? toDate(resp.endDate) : "";
    model.value.description = resp.description ? resp.description : "";
    model.value.projectTaskFiles = resp.projectTaskFilesList || [];
    existingModuleId = resp.projectModuleId;
    getSortOrderByModuleId(existingModuleId);
    // model.value.targetMonthStr = toMonthYear(resp.projectActivities?.[0]?.targetMonth);
    model.value.projectStatus = resp.project.projectStatus.dropDownValue;
    const formatted = Number(resp.sortOrder).toFixed(3);
    if (formatted) {
      const [intPart, fracPart] = String(formatted).split(".");
      prefix.value = intPart || "0";
      fraction.value = fracPart || "1";
    } else {
      prefix.value = "0";
      fraction.value = "1";
    }
    rows.value = resp.projectActivities.map(item => ({
      ...item,
      name: item.name,
      assignedToName: item.assignedTo.person.firstName + " " + item.assignedTo.person.lastName,
      startDate: item.startDate,
      endDate: item.endDate,
      // targetMonthStr: toMonthYear(item.targetMonth),
      popupVisible: false,
      currentView: "Years",
      estimateHours: item.estimateHours,
      editing: false,
      flag: "Edit"
    }));
    TaskActivitiesRows.value = resp.projectActivities.map(item => ({
      ...item,
      id: item.id,
      name: item.name,
      assignedToName: item.assignedTo.person.firstName + " " + item.assignedTo.person.lastName,
      assignedToId: item.assignedTo.id,
      // activityStatusId: item.activityStatus.id,
      description: item.description ? item.description : "",
      startDate: item.startDate,
      endDate: item.endDate,
      // targetMonthStr: toMonthYear(item.targetMonth),
      popupVisible: false,
      currentView: "Years",
      estimateHours: item.estimateHours,
      editing: false,
      flag: "Edit"
    }));
    // Calculate total hours
    totalHours.value = TaskActivitiesRows.value.reduce((acc, row) => {
      return acc + (parseFloat(row.estimateHours) || 0);
    }, 0);
  }).finally(() => {
    loading.value = false;
  });
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const refreshProjectDropdownList = () => {
  projectNameDropdownSingleSelect.load();
};

const refreshProjectModuleDropdownList = () => {
  projectModulesByProjectIdForDropdownSingleSelect.load(false, false, model.value.projectId);
};

function handlePopupShow (taskStatus, projectStatusLabel) {
  getAllTaskStatuses("Task Status", taskStatus, projectStatusLabel);
}

const disableBeforeStartDate = (date) => {
  if (!model.value.startDateStr) {
    return true;
  }

  // Convert MM/dd/yyyy string to Date
  const start = parse(model.value.startDateStr, "MM/dd/yyyy", new Date());
  const currentDate = parse(date, "yyyy/MM/dd", new Date());

  // Disable dates before the Start Date
  return currentDate >= start;
};

const onAdd = () => {
  TaskActivitiesRows.value.unshift({
    id: uid(),
    name: "",
    assignedToId: "",
    activityStatusId: "",
    startDate: "",
    endDate: "",
    popupVisible: false,
    currentView: "Years",
    estimateHours: "0",
    description: "",
    deleted: false
  });
};

const deleteRow = (index) => {
  if (TaskActivitiesRows.value.filter(row => row.deleted === false).length > 1) {
    TaskActivitiesRows.value[index].deleted = true;
  } else {
    notifyError({ message: "Please add at least one row." });
  }
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { projectNameDropdownSingleSelect,
  projectEmployeeDropdownSingleSelect,
  projectCharterEmployeesWithWeeklyPlanHoursForDropdown
} = projectModule();

const {
  projectTaskActivityNameForDropdownSingleSelect
} = projectTasksActivities();

const { projectModulesByProjectIdForDropdownSingleSelect } = projectModuleOfProjectModule();
const {
  areaForDropdownSingleSelect,
  workspaceForDropdownSingleSelect,
  projectTaskActionForDropdownSingleSelect,
  projectTaskPriorityForDropdownSingleSelect,
  projectTaskTypeForDropdownSingleSelect
} = projectTaskModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// All Dropdowns
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get all project task status List
const projectTaskStatusList = ref([]);
const projectTaskStatusListFilter = ref([]);
function getAllTaskStatuses (typeName, taskStatusLabel = null, projectStatusLabel = null) {
  commonService.getDropDown(typeName).then((resp) => {
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
    projectTaskStatusListFilter.value = responseData;

    // Set "New" status as the default if it exists
    const newStatus = responseData.find(status => status.text.toLowerCase() === "new");
    if (newStatus && TaskId === "") {
      model.value.statusId = newStatus.value;
    }
  });
}

function getAllTaskStatusesfilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectTaskStatusList.value = projectTaskStatusListFilter.value;
    } else {
      projectTaskStatusList.value = projectTaskStatusListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// -------------------------------------------------------------------------------------------------------
// Upload Image
// -------------------------------------------------------------------------------------------------------
function handleFiles (files) {
  model.value.projectTaskFiles = files;
}

function getFilePreview (file) {
  return file && file instanceof File ? URL.createObjectURL(file) : "";
}

function isImageFile (file) {
  if (file.file instanceof File) {
    return file.file.type.startsWith("image/");
  } else if (file.file && file.file.mimeType) {
    return file.file.mimeType.startsWith("image/");
  }
  return false;
}

function viewFile (file) {
  let fileUrl; // Declare fileUrl before using it

  if (file?.file?.virtualPath) {
    fileUrl = new URL(file.file.virtualPath).href; // For uploaded files
  } else if (file?.file instanceof File) {
    fileUrl = URL.createObjectURL(file.file); // For newly added files
  } else if (file instanceof File) {
    fileUrl = URL.createObjectURL(file); // Direct File object case
  }

  // const fileUrl = new URL(file, baseURL).href;
  const fileExtension = fileUrl.split(".").pop().toLowerCase();
  const supportedFormats = ["pdf", "docx", "xlsx", "pptx"];
  const imageFormats = ["jpg", "jpeg", "png", "gif", "svg"];

  let viewerUrl = fileUrl;

  // Use Google Docs Viewer for Documents
  if (supportedFormats.includes(fileExtension)) {
    viewerUrl = `https://docs.google.com/gview?url=${encodeURIComponent(fileUrl)}&embedded=true`;
  }

  // Open new window
  const newWindow = window.open("", "_blank");

  // Check if the popup is blocked
  if (!newWindow) {
    alert("Popup blocked! Please allow popups for this site.");
    return;
  }

  // Delay to avoid null reference issues
  setTimeout(() => {
    newWindow.document.write(`
<html>
<head>
<title>${fileUrl.split("/").pop()}</title>
<style>
              * { margin: 0; padding: 0; box-sizing: border-box; }
              body, html { width: 100vw; height: 100vh; display: flex; align-items: center; justify-content: center; background-color: #f4f4f4; overflow: hidden; }
              .top-right {
                position: fixed;
                top: 10px;
                right: 10px;
                background: #007bff;
                color: white;
                padding: 10px 15px;
                border-radius: 5px;
                font-size: 16px;
                text-decoration: none;
                z-index: 10;
              }
              .top-right:hover {
                background: #0056b3;
              }

              iframe, img {
                width: 100%;
                height: 100%;
                border: none;
                display: block;
                object-fit: contain; /* Ensures images fit properly */
              }

              /* Responsive Fixes */
              @media (max-width: 768px) {
                .top-right {
                  top: 5px;
                  right: 5px;
                  padding: 8px 12px;
                  font-size: 14px;
                }
              }
</style>
</head>
<body>
<a class="top-right" href="${fileUrl}" download>Download</a>
            ${
  imageFormats.includes(fileExtension)
    ? `<img src="${fileUrl}" alt="Image Preview">` // Show image directly
    : `<iframe src="${viewerUrl}"></iframe>` // Show document using iframe
}
</body>
</html>
        `);
  }, 100);
}

function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
}

function getFileIcon (mimeType) {
  const mimeToIconMap = {
    "application/pdf": "o_picture_as_pdf",
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": "o_insert_chart",
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document": "o_description",
    "application/vnd.openxmlformats-officedocument.presentationml.presentation": "o_slideshow", // PPTX MIME type
    "application/vnd.ms-powerpoint": "o_slideshow", // PPT MIME type
    "application/zip": "o_folder_zip",
    "text/plain": "o_article",
    "image/png": "o_image",
    "image/jpeg": "o_image",
    "image/gif": "o_image",
    default: "o_insert_drive_file"
  };

  return mimeToIconMap[mimeType] || mimeToIconMap.default;
}

function removeFile (index) {
  const file = model.value.projectTaskFiles[index];

  if (file && file.name && file.type) {
    URL.revokeObjectURL(file); // Clean up object URL
  }

  if (file && file.file && file.file.virtualPath) {
    file.flag = "remove";
    model.value.projectTaskFiles.splice(index, 1);
  } else {
    // For new files, just remove them from the list
    model.value.projectTaskFiles.splice(index, 1);
  }
}
// ==================================================================================
// Validation rules
// ==================================================================================
const rules = {
  projectId: { required: helpers.withMessage("Project is required", required) },
  projectModuleId: { required: helpers.withMessage("Project module is required", required) },
  name: { required: helpers.withMessage("Task name is required", required), minLength: minLength(1), maxLength: maxLength(300) },
  statusId: { required: helpers.withMessage("Status is required", required) },
  sortOrder: { required: helpers.withMessage("Sort Order is required", required) },
  priorityId: { required: helpers.withMessage("Priority is required", required) },
  startDateStr: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  endDateStr: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate),
    afterStartDate: helpers.withMessage("End date must occur after the start date", (value, { startDateStr }) => {
      return new Date(value) >= new Date(startDateStr);
    })
  },
  estimateTime: { required: helpers.withMessage("Estimate Hours is required", required) }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

function validateHours (value) {
  const strValue = (value ?? "").toString().trim();
  const regex = /^\d+(?:\.\d{1,2})?$/;
  if (!strValue) return "Estimate Hours is required";
  if (!regex.test(strValue)) return "Invalid hours format";
  if (Number(strValue) <= 0) return "Hours must be greater than 0";
  return true;
}

const totalHours = computed(() => {
  const total = TaskActivitiesRows.value.reduce((sum, row) => {
    if (!row.deleted) {
      sum += parseFloat(row.estimateHours) || 0;
    }
    return sum;
  }, 0);

  // Round to 2 decimal places without using toFixed()
  return Math.round(total * 100) / 100;
});

function validateSortOrder (value) {
  if (typeof value !== "string" && typeof value !== "number") return false;
  const str = String(value).trim();

  // Must be digits only
  if (!/^\d+$/.test(str)) {
    return false;
  }

  // Convert to number and check > 0
  return Number(str) > 0;
}

const rowRules = {
  name: { required: helpers.withMessage("Activity Name is required", required) },
  assignedToId: { required: helpers.withMessage("Activity Owner is required", required) },
  estimateHours: {
    required: helpers.withMessage("Estimate Hours is required", required),
    minValue: helpers.withMessage("Invalid Estimate Hours", (value) => value >= 0)
  }
};
// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Save & Next or Save & Close
// --------------------------------------------------------------------------------------------------------------------------------------------------
const formRef = ref(null);

const onSubmitClose = async () => {
  const valid = await formRef.value.validate();
  if (!valid) {
    // Quasar will show the error message automatically from :rules
    return;
  }
  onSubmit(1);
};

const getActivityText = (value) => {
  return projectTaskActivityNameForDropdownSingleSelect.list.value.find(
    x => x.value === value
  )?.text || '';
};

const onSubmit = async (isClose = 0) => {
  if (isClose === 1) {
    processingClose.value = true;
    processing.value = false;
  } else {
    processing.value = true;
  }
  try {
    let isValid = true;
    if (tab.value === "2_tab") {
      const nonDeletedRows = TaskActivitiesRows.value.filter(row => !row.deleted);
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
    }

    // validation for target month
    await v$.value.$touch();
    const isFormValid = await v$.value.$validate();

    if (!isFormValid) {
      notifyError({ message: "Please fill in all required fields." });
      return;
    }

    if (!isValid) {
      notifyError({ message: "Please fill in all required fields in the second tab." });
      return; // Prevent submission
    }
    // if (!await v$.value.$validate()) {
    //   notifyError({ message: "Please fill in all required fields in the first tab." });
    // }

    if (isFormValid && isValid) {
      if (tab.value === "2_tab" && TaskActivitiesRows.value.length === 0) {
        notifyError({ message: "Add at least one activity in project task." });
        return;
      }
      // processing.value = true;
      if (isClose === 1) {
        processingClose.value = true;
      } else {
        processing.value = true;
      }
      // Status confirmation
      if (props.id) {
        const selected = projectTaskStatusList.value.find(item => item.value === model.value.statusId);
        if (oldStatus.toLowerCase() !== "close" && selected?.text.toLowerCase() === "close") {
          const resp = await taskService.checkTaskCanBeDeleted(props.id);
          const canClose = resp?.canDelete;
          if (!canClose) {
            // Warning confirmation
            zwConfirm({
              title: "Active Activities Found",
              message: "This task has active activities. You cannot close it.",
              okLabel: "OK",
              cancel: false
            }, () => {
            });
            return;
          }
        }
      }
      const formData = new FormData();
      formData.append("estimateTime", model.value.estimateTime ? model.value.estimateTime : "00.00");
      formData.append("projectId", model.value.projectId);
      formData.append("projectModuleId", model.value.projectModuleId);
      formData.append("areaId", model.value.areaId ? model.value.areaId : "");
      formData.append("workspaceId", model.value.workspaceId ? model.value.workspaceId : "");
      formData.append("actionId", model.value.actionId ? model.value.actionId : "");
      formData.append("name", model.value.name);
      formData.append("startDateStr", model.value.startDateStr);
      formData.append("endDateStr", model.value.endDateStr);
      formData.append("statusId", model.value.statusId);
      formData.append("priorityId", model.value.priorityId);
      formData.append("typeId", model.value.typeId);
      formData.append("sortOrder", model.value.sortOrder = `${prefix.value}.${fraction.value}`);
      formData.append("assignedToId", model.value.assignedToId != null ? model.value.assignedToId : "");
      formData.append("description", model.value.description);
      formData.append("issueId", model.value.issueId);
      formData.append("requirementId", model.value.requirementId);
      formData.append("isIssueConverted", model.value.isIssueConverted);
      formData.append("isRequirementConverted", model.value.isRequirementConverted);

      // model.value.projectActivities = TaskActivitiesRows.value.filter(row => !row.deleted);
      const isValidRow = (emp) => {
        if (emp.deleted && emp.flag !== "Edit") return false;
        if (!emp) return false;

        return true;
      };

      model.value.projectActivities = TaskActivitiesRows.value
        .filter(isValidRow)
        .map(emp => ({ ...emp }));
      // .filter(row => !row.deleted && row.name?.trim() !== "")
      // .map((activity) => ({
      //   ...activity
      //   // targetMonthStr: model.value.targetMonthStr // override with global value
      // }));

      // model.value.projectActivities = TaskActivitiesRows.value;
      model.value.projectActivities.forEach((activity, index) => {
        formData.append(`projectActivities[${index}].id`, activity.id ?? uid());
        formData.append(`projectActivities[${index}].name`, getActivityText(activity.name) || activity.name || "");
        formData.append(`projectActivities[${index}].assignedToId`, activity.assignedToId ? activity.assignedToId : "");
        formData.append(`projectActivities[${index}].activityStatusId`, activity.activityStatusId);
        formData.append(`projectActivities[${index}].description`, activity.description);
        formData.append(`projectActivities[${index}].estimateHours`, activity.estimateHours);
        formData.append(`projectActivities[${index}].deleted`, activity.deleted);
      });
      toRaw(model.value.projectTaskFiles || []).forEach((file) => {
        if (file.file && file.file.virtualPath) {
        // For existing files, append metadata instead of the file itself
          formData.append("ExistingFiles", JSON.stringify({
            id: file.id,
            virtualPath: file.file.virtualPath
          }));
        } else {
        // For new files, append as raw file objects (IFormFile)
          formData.append("projectTaskFiles", file);
        }
      });
      // Check if sort order already exists
      const checkResp = await taskService.displayWarningForSortOrder(
        model.value.projectId,
        `${prefix.value}.${fraction.value}`,
        model.value.projectModuleId,
        TaskId
      );

      if (checkResp.warning != null) {
      // Show backend warning
        notifyWarning({ message: checkResp.warning });
      }
      // Submit the payload
      taskService.saveProjectTask(TaskId, formData).then((resp) => {
        notifySuccess({ message: "Project task is saved successfully." });
        TaskId = resp.id;
        disableTab = false;
        if (isClose === 1) {
          onDialogOK();
        } else {
          const currentTab = tab.value;
          switch (currentTab) {
          case "1_tab":
            tab.value = "2_tab";
            break;
          default:
            break;
          }
        }
      });
    }
  } catch (error) {
    console.error("Error in submitting the project task:", error);
    notifyError({ message: "An error occurred while saving the project task." });
  } finally {
    if (isClose === 1) {
      processingClose.value = true;
      processing.value = false;
    } else {
      processing.value = true;
    }
    setTimeout(() => {
      processing.value = false;
      processingClose.value = false;
    }, 1500);
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => TaskId, (newValue, oldValue) => {
  if (newValue) {
    getProjectTask();
  }
}, { immediate: true });

const normalize = (val) => {
  if (Array.isArray(val)) {
    return val.length > 0 ? val[0] : null;
  }
  return val ?? null;
};

watch(
  () => model.value.projectId,
  async (newId, oldId) => {
    const newVal = normalize(newId);
    const oldVal = normalize(oldId);

    const isValid = newVal != null && newVal !== "";

    // Clear module only when actual value changes
    if (oldId !== undefined && oldId !== null && newVal !== oldVal) {
      model.value.projectModuleId = null;
    }

    // Always load dropdown
    await projectNameDropdownSingleSelect.load();

    if (isValid) {
      const selected = projectNameDropdownSingleSelect.list.value.find(
        (p) => p.value === newVal
      );

      if (selected) {
        model.value.projectStatus = selected.data;
      }

      projectModulesByProjectIdForDropdownSingleSelect.load(false, false, newVal);
      projectCharterEmployeesWithWeeklyPlanHoursForDropdown.load(newVal);
      projectEmployeeDropdownSingleSelect.load(newVal);
    } else {
      // Optional cleanup
      projectModulesByProjectIdForDropdownSingleSelect.list.value = [];
    }
  },
  { immediate: true }
);

watch([prefix, fraction], () => {
  model.value.sortOrder = `${prefix.value}.${fraction.value}`;
});

watch(() => model.value.projectModuleId, (newValue, oldValue) => {
  if (newValue) {
    getSortOrderByModuleId(newValue);
  }
}, { immediate: true });

watch(
  () => projectTaskPriorityForDropdownSingleSelect.list.value,
  (newList) => {
    if (!newList.length) return;

    const lowPriority = newList.find(
      p => p.text.toLowerCase() === "low"
    );

    if (lowPriority && !props.id && !model.value.priorityId) {
      model.value.priorityId = lowPriority.value;
    }
  },
  { immediate: true }
);

watch(tab, (newVal, oldVal) => {
  if (newVal !== oldVal) {
    projectCharterEmployeesWithWeeklyPlanHoursForDropdown.reset();
    projectTaskActivityNameForDropdownSingleSelect.reset?.();
  }
});

onMounted(() => {
  getAllTaskStatuses("Task Status");
  projectTaskPriorityForDropdownSingleSelect.load("Task Priorities");
  projectTaskTypeForDropdownSingleSelect.load("Task Type");
  projectTaskActivityNameForDropdownSingleSelect.load("Project Activities");
  areaForDropdownSingleSelect.load("Area");
  workspaceForDropdownSingleSelect.load("Workspace");
  projectTaskActionForDropdownSingleSelect.load("Action");
  if (props.isRequirementConverted) {
    getRequirementDescriptionById(props.requirementId);
  }
  if (Array.isArray(model.value.projectId)) {
    model.value.projectId = model.value.projectId[0] ?? null;
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
.edit_tasks .q-select__dropdown-icon{
  display: none;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
.wrap-text {
  max-width: 250px;
}
</style>
