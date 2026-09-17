<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1300px !important; max-width: 95vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">  {{ isManageDescription ? 'Manage Description' : 'Edit Requirement' }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <q-card>
              <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
                <q-tab v-if="!isManageDescription" name="1_tab" label="Manage Description" class="q-px-lg q-mr-md" />
                <q-tab v-if="!isManageDescription"name="2_tab" label="Requirement Info" class="q-px-lg" :disable="disableTab" />
                <q-tab  v-if="!isManageDescription" name="3_tab" label="Document Reference List" class="q-px-lg" :disable="disableTab" />
              </q-tabs>
              <q-separator />
              <q-tab-panels v-model="tab" animated>
                <q-tab-panel name="1_tab">
                  <viewDescriptionTimeLineView
                    :id="props.id"
                  />
                  <div v-if="!isManageDescription" align="center" class="q-gutter-sm justify-center q-mt-sm">
                    <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                  </div>
                </q-tab-panel>
                <q-tab-panel name="2_tab">
                  <fieldset>
                    <legend>Requirement Info</legend>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12">
                        <label class="q-mb-xs text-black">Requirement<span class="required">*</span></label>
                        <div>
                          <q-input
                            v-model="model.title"
                            outlined
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            autogrow
                            :error="v$.title.$error"
                            :error-message="v$.title.$errors[0]?.$message"
                            @blur="v$.title.$touch"
                          />
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12">
                        <div class="form-group">
                          <label class="q-mb-xs text-black">Short Description
                            <q-icon
                              name="o_info"
                              size="16px"
                              class="q-ml-xs cursor-pointer text-grey-7"
                            >
                              <q-tooltip
                                anchor="top middle"
                                self="bottom middle"
                                :offset="[0, 6]"
                              >
                                <div style="max-width: 320px; white-space: normal;">
                                  Enter a description that will be displayed on the Requirement List, Week Planner, and Monthly Planner wherever the Requirement is linked.
                                </div>
                              </q-tooltip>
                            </q-icon>
                          </label>
                          <q-input v-model="model.shortDescription" outlined autogrow hint="The maximum length allowed is 200." maxlength="200" />
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                         <formSingleSelectDropdown
                          v-model="model.projectId"
                          label="Project Name"
                          :options="projectNameDropdownSingleSelect.list.value"
                          :filter="projectNameDropdownSingleSelect.filter"
                          :error="v$.projectId.$error"
                          :error-message="v$.projectId.$errors[0]?.$message"
                        />
                      </div>
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formSingleSelectDropdown
                          v-model="model.projectModuleId"
                          label="Project Module"
                          :disable="(model.projectId?.length || 0) === 0"
                          :options="projectModulesByProjectIdForDropdownSingleSelect.list.value"
                          :filter="projectModulesByProjectIdForDropdownSingleSelect.filter"
                          :error="v$.projectModuleId.$error"
                          :error-message="v$.projectModuleId.$errors[0]?.$message"
                        />
                      </div>
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formSingleSelectDropdown
                          v-model="model.requirementTypeId"
                          label="Type"
                          :required="false"
                          :options="requirementTypeDropdownSingleSelect.list.value"
                          :filter="requirementTypeDropdownSingleSelect.filter"
                        />
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                         <formSingleSelectDropdown
                          v-model="model.areaId"
                          label="Area"
                          :required="false"
                          :options="areaForDropdownSingleSelect.list.value"
                          :filter="areaForDropdownSingleSelect.filter"
                        />
                      </div>
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formSingleSelectDropdown
                          v-model="model.workspaceId"
                          label="Workspace"
                          :required="false"
                          :options="workspaceForDropdownSingleSelect.list.value"
                          :filter="workspaceForDropdownSingleSelect.filter"
                        />
                      </div>
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formSingleSelectDropdown
                          v-model="model.identifiedUserType"
                          label="Requirement Identifier"
                          :required="false"
                          :options="requirementIdentifiedUserTypeDropdownSingleSelect.list.value"
                          :filter="requirementIdentifiedUserTypeDropdownSingleSelect.filter"
                        />
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div v-if="identifiedUserTypeText === 'Employee'" class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formSingleSelectDropdown
                          v-model="model.identifiedEmployeeId"
                          label="Employee Name"
                          :required="false"
                          :options="activeEmployeesDropdownSingleSelect.list.value"
                          :filter="activeEmployeesDropdownSingleSelect.filter"
                        />
                      </div>
                      <div v-if="identifiedUserTypeText === 'Customer'" class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formSingleSelectDropdown
                          v-model="model.identifiedCustomerId"
                          label="Customer Contact Name"
                          :required="false"
                          :options="customerContactByProjectIdDropdownSingleSelect.list.value"
                          :filter="customerContactByProjectIdDropdownSingleSelect.filter"
                        />
                      </div>
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formDate
                          v-model="model.identifiedDateStr"
                          label="Requirement Identified Date"
                          :error="v$.identifiedDateStr.$error"
                          :error-message="v$.identifiedDateStr.$errors[0]?.$message"
                          :onBlur="() => v$.identifiedDateStr.$touch()"
                        />
                      </div>
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formSingleSelectDropdown
                          v-model="model.requirementOwnerId"
                          label="Requirement Owner"
                          :options="activeEmployeesDropdownSingleSelect.list.value"
                          :filter="activeEmployeesDropdownSingleSelect.filter"
                          :error="v$.requirementOwnerId.$error"
                          :error-message="v$.requirementOwnerId.$errors[0]?.$message"
                       />
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formSingleSelectDropdown
                          v-model="model.requirementEnteredBy"
                          label="Requirement Entered By"
                          :required="false"
                          :options="activeEmployeesDropdownSingleSelect.list.value"
                          :filter="activeEmployeesDropdownSingleSelect.filter"
                        />
                      </div>
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formSingleSelectDropdown
                          v-model="model.approvalStatus"
                          label="Approval Status"
                          :required="false"
                          :options="requirementApprovalStatusDropdownSingleSelect.list.value"
                          :filter="requirementApprovalStatusDropdownSingleSelect.filter"
                        />
                      </div>
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formDate
                          v-model="model.plannedStartDateStr"
                          label="Planned Start Date"
                          :required="false"
                          :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                        />
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                       <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formDate
                          v-model="model.plannedEndDateStr"
                          label="Planned End Date"
                          :required="false"
                          :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                          :dateOptions="disablePlannedDatesBeforeStartDate"
                        />
                      </div>
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formDate
                          v-model="model.actualStartDateStr"
                          label="Actual Start Date"
                          :required="false"
                          :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                        />
                      </div>
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formDate
                          v-model="model.actualEndDateStr"
                          label="Actual End Date"
                          :required="false"
                          :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                          :dateOptions="disableActualDatesBeforeStartDate"
                        />
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formSingleSelectDropdown
                          v-model="model.confirmedById"
                          label="Confirmed By"
                          class="hidden"
                          :required="false"
                          :options="activeEmployeesDropdownSingleSelect.list.value"
                          :filter="activeEmployeesDropdownSingleSelect.filter"
                        />
                      </div>
                      <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                        <formSingleSelectDropdown
                          v-model="model.approvedById"
                          label="Approved By"
                          class="hidden"
                          :required="false"
                          :options="activeEmployeesDropdownSingleSelect.list.value"
                          :filter="activeEmployeesDropdownSingleSelect.filter"
                        />
                      </div>
                    </div>
                  </fieldset>
                  <div align="center" class="q-gutter-sm justify-center q-mt-sm">
                    <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                    <q-btn v-if="tab === '2_tab'" label="Save & Next" type="submit" color="primary" class="actionBtn hidden" :loading="processing" no-caps />
                    <q-btn label="Save" type="button" color="primary" class="actionBtn" :loading="processingClose" no-caps @click="onSubmitClose()" />
                  </div>
                </q-tab-panel>
                <q-tab-panel name="3_tab">
                  <fieldset class="q-mb-lg">
                    <legend>Document Reference List</legend>
                    <div class="flex items-center justify-end q-mb-md">
                      <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAddDocumentReference" />
                    </div>
                     <q-table
                      ref="tableRef"
                      v-model:pagination="pagination"
                      bordered
                      class="no-shadow"
                      :loading="loading"
                      :rows="rows"
                      :columns="columns"
                      row-key="id"
                      separator="cell"
                      no-data-label="No data available"
                      binary-state-sort
                    >
                      <template #header="props">
                        <q-tr :props="props" class="bg-primary text-white">
                          <q-th
                            v-for="col in props.cols"
                            :key="col.name" :props="props"
                          >
                            {{ col.label }}
                            <span v-if="['filePath','fileName'].includes(col.name)" class="required">*</span>
                          </q-th>
                          <q-th auto-width class="text-center">Actions</q-th>
                        </q-tr>
                      </template>
                      <template #top-row>
                        <q-tr v-if="mode == 'addDocumentReference' && editingRow" class="row-highlight">
                          <q-td>
                            <div>
                              <q-input
                                v-model="editingRow.filePath"
                                outlined
                                stack-label
                                hide-bottom-space
                                :dense="true"
                                :error="editingRowV$.filePath.$error"
                                :error-message="editingRowV$.filePath.$errors[0]?.$message"
                                @blur="editingRowV$.filePath.$touch"
                              />
                            </div>
                          </q-td>
                          <q-td>
                            <div>
                              <q-input
                                v-model="editingRow.fileName"
                                outlined
                                stack-label
                                hide-bottom-space
                                :dense="true"
                                :error="editingRowV$.fileName.$error"
                                :error-message="editingRowV$.fileName.$errors[0]?.$message"
                                @blur="editingRowV$.fileName.$touch"
                              />
                            </div>
                          </q-td>
                          <q-td style="width: 350px;">
                            <div>
                              <q-input
                                v-model="editingRow.note"
                                outlined
                                stack-label
                                type="textarea"
                                hide-bottom-space
                                :dense="true"
                                maxlength="500"
                              />
                            </div>
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
                        <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                          <q-td class="text-left" style="width: 40%;">
                            <q-input
                              v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                              v-model="editingRow.filePath"
                              outlined
                              stack-label
                              hide-bottom-space
                              :dense="true"
                              :error="editingRowV$.filePath.$error"
                              :error-message="editingRowV$.filePath.$errors[0]?.$message"
                              @blur="editingRowV$.filePath.$touch"
                            />
                            <span
                              v-else :class="props.row.deleted ? 'text-delete' : ''"
                              style="white-space: normal; word-break: break-word;"
                            >
                              <a :href="props.row.filePath" target="_blank" class="text-bluee">
                                {{ props.row.filePath }}
                              </a>
                            </span>
                          </q-td>
                          <q-td class="text-left" style="width: 25%;">
                            <q-input
                              v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                              v-model="editingRow.fileName"
                              outlined
                              stack-label
                              hide-bottom-space
                              :dense="true"
                              :error="editingRowV$.fileName.$error"
                              :error-message="editingRowV$.fileName.$errors[0]?.$message"
                              @blur="editingRowV$.fileName.$touch"
                            />
                            <span
                              v-else :class="props.row.deleted ? 'text-delete' : ''"
                              style="white-space: normal; word-break: break-word;"
                            >
                              {{ props.row.fileName }}
                            </span>
                          </q-td>
                          <q-td class="text-left">
                            <q-input
                              v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                              v-model="editingRow.note"
                              outlined
                              stack-label
                              type="textarea"
                              hide-bottom-space
                              :dense="true"
                              maxlength="500"
                            />
                            <span
                              v-else :class="props.row.deleted ? 'text-delete' : ''"
                              style="display: block; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;"
                              v-html="props.row.note"
                            />
                          </q-td>
                          <q-td auto-width class="text-center">
                            <template v-if="mode == 'edit' && editingRow && props.row.id === activeRowId">
                              <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                                <q-tooltip>Save</q-tooltip>
                              </q-icon>
                              <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                                <q-tooltip>Cancel</q-tooltip>
                              </q-icon>
                            </template>
                            <template v-else>
                              <q-icon v-if="!props.row.deleted" name="o_delete_outline" color="red" size="xs" class="cursor-pointer" @click="onDeleteDocumentReference(props.row)">
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
                    <div align="center" class="q-gutter-sm justify-center q-mt-sm">
                      <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                      <q-btn label="Save" type="button" color="primary" class="actionBtn" :loading="processingClose" no-caps @click="onSubmitClose()" />
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
import { useDialogPluginComponent, useQuasar, uid } from "quasar";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { notifySuccess, notifyError, notifyWarning, zwConfirm } from "assets/utils";
import { ref, watch, onMounted, computed } from "vue";
import { isDate } from "validators/zw_validators.js";
import { useAuthStore } from "stores/auth";
import _ from "lodash";
import useVuelidate from "@vuelidate/core";
import { format } from "date-fns"; // Standard TimeZone Conversion

import requirementService from "../requirement.service";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// Shared Dropdowns
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";

import viewDescriptionTimeLineView from "src/modules/requirement/components/_description_timeline_view.vue";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, isManageDescription: { type: Boolean, default: false } });
let requirementId = props.id;
// ----------------------------------------------------------------------------------------------------------------
// Define emits
// ----------------------------------------------------------------------------------------------------------------

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------
const loading = ref(true);
const rows = ref([]);
const tab = ref("1_tab");
const $q = useQuasar();
const authStore = useAuthStore();
const user = authStore.user;
const processing = ref(false);
const processingClose = ref(false);
const mode = ref(null);
const activeRowId = ref(null);
const editingRow = ref(null);
const editingLogRow = ref(null);

const model = ref({
  projectId: props.projectIdAttr !== "" ? props.projectIdAttr : (props.projectIdValue !== "" ? props.projectIdValue : (projectIds !== "" ? projectIds : null)),
  projectModuleId: props.moduleIdAttr !== "" ? props.moduleIdAttr : (projectModuleIds !== "" ? projectModuleIds : null),
  title: "",
  areaId: null,
  workspaceId: null,
  notes: "",
  identifiedUserType: "",
  employeeId: "",
  identifiedEmployeeId: user?.employeeId ? user.employeeId : "",
  requirementOwnerId: user?.employeeId ? user.employeeId : "",
  confirmedById: null,
  approvedById: null,
  identifiedDateStr: format(new Date(), "MM/dd/yyyy"),
  plannedStartDateStr: "",
  plannedEndDateStr: "",
  actualStartDateStr: "",
  actualEndDateStr: "",
  requirementEnteredBy: user?.employeeId ? user.employeeId : "",
  statusId: "",
  approvalStatus: "",
  closeDateStr: format(new Date(), "MM/dd/yyyy"),
  description: "",
  shortDescription: "",
  editingStatus: 0,
  status: {
    dropDownValue: ""
  }
});

const rules = {
  projectId: { required: helpers.withMessage("Project name is required", required) },
  projectModuleId: { required: helpers.withMessage("Project module is required", required) },
  statusId: { required: helpers.withMessage("Requirement Status is required", required) },
  title: { required: helpers.withMessage("Title is required", required), minLength: minLength(1), maxLength: maxLength(200) },
  identifiedDateStr: {
    required: helpers.withMessage("Identified date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  requirementOwnerId: { required: helpers.withMessage("Requirement Owner is required", required) },
  closeDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate)
  }
};

// ----------------------------------------------------------------------------------------------------------------
// Document Reference List - Validation Rules
// ----------------------------------------------------------------------------------------------------------------

const editingRowrules = {
  filePath: { required: helpers.withMessage("File Path is required", required), minLength: minLength(1), maxLength: maxLength(200) },
  fileName: { required: helpers.withMessage("File Name is required", required) }
};


// ----------------------------------------------------------------------------------------------------------------
// Define columns for File path and Requirement Change log
// ----------------------------------------------------------------------------------------------------------------

const columns = ref([
  { name: "filePath", label: "File Path", field: "filePath", align: "left", sortable: true },
  { name: "fileName", label: "File Name", field: "fileName", align: "left", sortable: true },
  { name: "note", label: "Notes", field: "note", align: "left", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// Get Requirement Details
// ----------------------------------------------------------------------------------------------------------------

const getRequirement = () => {
  loading.value = true;
  requirementService.getRequirementDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.identifiedDateStr = resp.identifiedDate ? format(resp.identifiedDate, "MM/dd/yyyy") : "";
    model.value.plannedStartDateStr = resp.plannedStartDate ? format(resp.plannedStartDate, "MM/dd/yyyy") : "";
    model.value.plannedEndDateStr = resp.plannedEndDate ? format(resp.plannedEndDate, "MM/dd/yyyy") : "";
    model.value.actualStartDateStr = resp.actualStartDate ? format(resp.actualStartDate, "MM/dd/yyyy") : "";
    model.value.actualEndDateStr = resp.actualEndDate ? format(resp.actualEndDate, "MM/dd/yyyy") : "";
    model.value.closeDateStr = resp.closeDate ? format(resp.closeDate, "MM/dd/yyyy") : "";
    model.value.description = resp.description ? resp.description : "";
    model.value.shortDescription = resp.shortDescription ? resp.shortDescription : "";
    rows.value = resp.filePathDetails.map(item => ({
      ...item,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

// ----------------------------------------------------------------------------------------------------------------
// Custom Functions
// ----------------------------------------------------------------------------------------------------------------

function onAddDocumentReference () {
  mode.value = "addDocumentReference";
  editingRow.value = {
    filePath: "",
    fileName: "",
    note: ""
  };
  activeRowId.value = null;
}

function onCancel () {
  mode.value = null;
  editingRow.value = null;
  editingLogRow.value = null;
  activeRowId.value = null;
}

function onUndo (item) {
  item.deleted = false;
  activeRowId.value = null;
}

function onDeleteDocumentReference (item) {
  item.deleted = true;
  const rowIndex = rows.value.findIndex((row) => row.id === item.id);
  if (rowIndex !== -1) {
    rows.value.splice(rowIndex, 1, {
      ...rows.value[rowIndex],
      id: item.id,
      fileName: item.fileName,
      filePath: item.filePath,
      note: item.note,
      flag: "Delete"
    });
  }
  activeRowId.value = item.id;
}

async function onSave () {
  if (mode.value === "addDocumentReference") {
    if (!await editingRowV$.value.$validate()) {
      return;
    }
    // check duplicate row
    let isDuplicate = 0;
    rows.value.forEach((item, index) => {
      if (item.fileName.toLowerCase() === editingRow.value.fileName.toLowerCase()) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const newRow = {
        id: uid(),
        fileName: editingRow.value.fileName,
        filePath: editingRow.value.filePath,
        note: editingRow.value.note,
        flag: "New"
      };
      rows.value.unshift(newRow);
      mode.value = null;
      activeRowId.value = null;
    } else {
      notifyError({ message: "Duplicate File Name." });
    }
  }
}
// ------------------------------------------------------------------------------------
// All Dropdowns
// ------------------------------------------------------------------------------------
const { projectNameDropdownSingleSelect, customerContactByProjectIdDropdownSingleSelect } = projectModule();
const { projectModulesByProjectIdForDropdownSingleSelect } = projectModuleOfProjectModule();
const { activeEmployeesDropdownSingleSelect } = employeeModule();
// const { customerContactDropdownSingleSelect } = customerModule();
const {
  requirementIdentifiedUserTypeDropdownSingleSelect,
  requirementStatusDropdownSingleSelect,
  requirementApprovalStatusDropdownSingleSelect,
  requirementTypeDropdownSingleSelect
 } = requirementModule();

const {
  areaForDropdownSingleSelect,
  workspaceForDropdownSingleSelect
} = projectTaskModule();

// Computed property to get the identifiedUserType's text
const identifiedUserTypeText = computed(() => {
  const selectedOption = requirementIdentifiedUserTypeDropdownSingleSelect.list.value.find(
    item => item.value === model.value.identifiedUserType
  );
  return selectedOption ? selectedOption.text : null;
});

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });
const editingRowV$ = useVuelidate(editingRowrules, editingRow, { $lazy: true, $autoDirty: true });

function disablePlannedDatesBeforeStartDate (date) {
  // If no Start Date is set, allow all dates
  if (!model.value.plannedStartDateStr) {
    return true;
  }
  const start = new Date(model.value.plannedStartDateStr);
  const current = new Date(date);

  // Disable dates before the Start Date
  return current >= start;
}

function disableActualDatesBeforeStartDate (date) {
  // If no Start Date is set, allow all dates
  if (!model.value.actualStartDateStr) {
    return true;
  }
  const start = new Date(model.value.actualStartDateStr);
  const current = new Date(date);

  // Disable dates before the Start Date
  return current >= start;
}

const onSubmitClose = () => {
  onSubmit(1);
};

// Submit form
const onSubmit = async (isClose = 0) => {
  if (isClose === 1) {
    processingClose.value = true;
  } else {
    processing.value = true;
  }

  try {
    // Validate Requirement Info
    await v$.value.$touch();
    const isFormValid = await v$.value.$validate();

    if (!isFormValid) {
      notifyError({ message: "Please fill in all required fields." });
      return;
    }

    // Validate Document Reference / Change Log editing rows
    if (mode.value === "addDocumentReference") {
      const isDocumentValid = await editingRowV$.value.$validate();
      if (!isDocumentValid) {
        notifyError({
          message: "Please fill in all required fields in the third tab."
        });
        return;
      }
      notifyError({
        message: "Please save the document reference first."
      });
      return;
    }

    // Prepare Requirement data
    model.value.filePathDetailsModel = rows.value;

    // Save Requirement
    const resp = await requirementService.saveRequirement(
      requirementId,
      model.value
    );
    requirementId = resp;
    notifySuccess({
      message: "Requirement is saved successfully."
    });

    // Save & Close
    if (isClose === 1) {
      onDialogOK();
      return;
    }

    // Save & Next
    if (tab.value === "2_tab") {
      tab.value = "3_tab";
    }
  } catch (error) {
    console.error("Error in submitting requirement:", error);

    notifyError({
      message: "An error occurred while saving the requirement."
    });
  } finally {
    processing.value = false;
    processingClose.value = false;
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(() => props.id, (newValue) => {
  if (newValue) {
    getRequirement();
  } else {
    loading.value = false;
  }
}, { immediate: true });

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
      v$.value.projectModuleId.$reset();
    }
  },
  { immediate: true }
);

watch(() => model.value.projectId, async (newValue, oldValue) => {
  if (newValue === oldValue && oldValue !== undefined) return;

  const oldProjectId = Array.isArray(oldValue)
    ? oldValue[0]
    : oldValue;

  // No project selected
  if (!newValue) {
    model.value.identifiedCustomerId = null;
    customerContactByProjectIdDropdownSingleSelect.list.value = null;
    return;
  }
  // Project changed
  if (oldProjectId !== undefined && newValue !== oldProjectId) {
    model.value.identifiedCustomerId = null;
  }

  // Clear customer dropdown
  // customerContactByProjectIdDropdownSingleSelect.list.value = null;
  await customerContactByProjectIdDropdownSingleSelect.load(newValue);

}, { immediate: true });

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  projectNameDropdownSingleSelect.load();
  areaForDropdownSingleSelect.load("Area");
  workspaceForDropdownSingleSelect.load("Workspace");
  requirementTypeDropdownSingleSelect.load("Requirement Type");
  activeEmployeesDropdownSingleSelect.load();

  await requirementStatusDropdownSingleSelect.load("Requirement Status");
  await requirementIdentifiedUserTypeDropdownSingleSelect.load("Requirement Identifier");
  await requirementApprovalStatusDropdownSingleSelect.load("Approval Status");

  // Set "Employee" employeeType as the default if it exists
  const employeeType = requirementIdentifiedUserTypeDropdownSingleSelect.getValueByLabel("Employee");
  if (employeeType && props.id === "") {
    model.value.identifiedUserType = employeeType;
  }

  // Set "New" status as the default if it exists
  const newStatus = requirementStatusDropdownSingleSelect.getValueByLabel("New");
  if (newStatus && props.id === "") {
    model.value.statusId = newStatus;
  }

  // Set "Unapproved" status as the default if it exists
  const unapprovedStatus = requirementApprovalStatusDropdownSingleSelect.getValueByLabel("Unapproved");
  if (unapprovedStatus && props.id === "") {
    model.value.approvalStatus = unapprovedStatus;
  }
});

</script>

