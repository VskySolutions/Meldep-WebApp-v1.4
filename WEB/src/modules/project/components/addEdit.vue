<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div v-if="!isCharter" class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Project</div>
        <div v-else class="text-h2 text-white">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <q-card>
              <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
                <q-tab v-if="!isCharter" name="1_tab" label="Project Info." class="q-px-lg q-mr-md" />
                <q-tab name="2_tab" label="Project Charter" class="q-px-lg" :disable="disableTab" />
              </q-tabs>
              <q-separator />
              <q-tab-panels v-model="tab" animated>
                <q-tab-panel v-if="!isCharter" name="1_tab">
                  <fieldset>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-xxl-12 col-lg-12 col-md-12">
                        <div class="q-mb-xs text-black">Project Name<span class="required">*</span></div>
                        <q-input
                          v-model="model.name"
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          maxlength="128"
                          :error="v$.name.$error"
                          :error-message="v$.name.$errors[0]?.$message"
                          @click="v$.name.$touch"
                        />
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                    <formSingleSelectDropdown
                      v-model="model.projectCategoryId"
                      label="Project Category"
                      :required="false"
                      :options="projectCategoryDropdownSingleSelect.list.value"
                      :filter="projectCategoryDropdownSingleSelect.filter"
                      @update:model-value="getSubCategoriesByCategoryId(model.projectCategoryId)"
                    />
                      <formSingleSelectDropdown
                        v-model="model.projectSubcategoryId"
                        label="Project Subcategory"
                        :required="false"
                        :options="projectSubCategoryDropdownSingleSelect.list.value"
                        :filter="projectSubCategoryDropdownSingleSelect.filter"
                        @update:model-value="getDescriptionBySubCategoryId(model.projectCategoryId, model.projectSubcategoryId)"
                      />
                      <div class="col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12">
                        <div class="q-mb-xs text-black"><label>Project Subcategory Description</label></div>
                        <div class="form-group" disabled>
                          <q-input
                            v-model="model.subCtgDescription"
                            outlined
                            autogrow
                            readonly
                          />
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <formSingleSelectDropdown
                        v-model="model.customerId"
                        label="Customer"
                        required
                        :options="customerDropdownSingleSelect.list.value"
                        :filter="customerDropdownSingleSelect.filter"
                        :error="v$.customerId.$error"
                        :error-message="v$.customerId.$errors[0]?.$message"
                        @update:model-value="getContactByCustomerId(model.customerId)"
                      />
                      <formSingleSelectDropdown
                        v-model="model.companyContactId"
                        label="Contact"
                        required
                        :options="companyContactDropdownSingleSelect.list.value"
                        :filter="companyContactDropdownSingleSelect.filter"
                        :error="v$.companyContactId.$error"
                        :error-message="v$.companyContactId.$errors[0]?.$message"
                      />
                      <div class="col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12" :class="{ edit_project: !props.id }">
                        <div class="q-mb-xs text-black">Project Status<span class="required">*</span></div>
                        <q-select
                          v-model="model.projectStatusId"
                          clearable use-input
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          :options="projectStatusList"
                          option-value="value"
                          option-label="text"
                          emit-value
                          map-options
                          :readonly="!props.id"
                          :error="v$.projectStatusId.$error"
                          :error-message="v$.projectStatusId.$errors[0]?.$message"
                          @filter="getProjectStatusListFilter"
                          @popup-show="() => handlePopupShow(model.projectStatus.dropDownValue)"
                          @blur="v$.projectStatusId.$touch"
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
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <formDate
                        v-model="model.startDateStr"
                        label="Start Date"
                        :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                        :error="v$.startDateStr.$error"
                        :error-message="v$.startDateStr.$errors[0]?.$message"
                        :onBlur="() => v$.startDateStr.$touch()"
                      />
                      <formDate
                        v-model="model.goLiveDateStr"
                        label="Due Date"
                        :required="false"
                        :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                        :error="v$.goLiveDateStr.$error"
                        :error-message="v$.goLiveDateStr.$errors[0]?.$message"
                        :onBlur="() => v$.goLiveDateStr.$touch()"
                        :dateOptions="disableProjectDatesBeforeStartDate"
                      />
                      <formSingleSelectDropdown
                        v-model="model.projectPriorityId"
                        label="Project Priority"
                        required
                        :options="projectPriorityDropdownSingleSelect.list.value"
                        :filter="projectPriorityDropdownSingleSelect.filter"
                        :error="v$.projectPriorityId.$error"
                        :error-message="v$.projectPriorityId.$errors[0]?.$message"
                      />
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <formSingleSelectDropdown
                        v-model="model.projectTypeId"
                        label="Project Type"
                        required
                        :options="projectTypeDropdownSingleSelect.list.value"
                        :filter="projectTypeDropdownSingleSelect.filter"
                        :error="v$.projectTypeId.$error"
                        :error-message="v$.projectTypeId.$errors[0]?.$message"
                      />
                      <formSingleSelectDropdown
                        v-model="model.planApproverId"
                        label="Project Approver"
                        required
                        :options="projectApproverDropdownSingleSelect.list.value"
                        :filter="projectApproverDropdownSingleSelect.filter"
                        :error="v$.planApproverId.$error"
                        :error-message="v$.planApproverId.$errors[0]?.$message"
                      />
                      <div class="col-12 col-sm-2 col-md-2 col-lg-2">
                        <div class="q-mb-xs q-mt-md text-black">Select Status</div>
                        <q-checkbox
                          v-model="model.active"
                          label="Active"
                          :dense="true"
                        />
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-lg">
                      <div class="col-12 col-sm-12 col-md-12 col-lg-12">
                        <div class="q-mb-xs text-black"><label>Description</label></div>
                        <div class="form-group">
                          <q-editor
                            v-model="model.description"
                            :dense="$q.screen.lt.md"
                            :toolbar="toolbar"
                            :fonts="fonts"
                          />
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-lg">
                      <div class="col-12 q-mb-xs text-black">Project Files</div>
                      <!-- File Uploader -->
                      <div class="col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12">
                        <multiFileUploader
                          :key="model.projectFiles?.length"
                          :initialFiles="model.projectFiles"
                          :allowedExtensions="[
                            '.pdf','.xls','.xlsx','.doc','.docx','.jpeg','.jpg','.png','.ppt','.pptx'
                          ]"
                          :maxSizeInMb="25"
                          label="Drag files here or (+) to upload."
                          @files-selected="handleFiles"
                          @files-valid="isFilesValid = $event"
                        />
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-lg">
                      <!-- Display Files in Square Boxes with File Name Below -->
                      <div v-if="model.projectFiles && model.projectFiles.length > 0" class="row q-gutter-md">
                        <div
                          v-for="(file, index) in model.projectFiles"
                          :key="index"
                          class="col-3 position-relative file-card text-center"
                          style="max-width: 140px; min-width: 140px;"
                        >
                          <div class="file-preview square-box">
                            <template v-if="isImageFile(file)">
                              <img :src="file.file.virtualPath ? file.file.virtualPath : getFilePreview(file.file)" alt="File Preview" class="square-content centered-image">
                            </template>
                            <template v-else>
                              <q-icon :name="getFileIcon(file.file?.mimeType)" class="file-icon square-content" size="70px" />
                            </template>
                            <div class="file-name q-mt-sm">
                              <q-btn v-if="file.file?.virtualPath || file?.name" class="bg-primary text-white q-pa-xs" no-caps @click="viewFile(file)">
                                <span class="truncate-text">
                                  {{ file.file?.name || file.name || extractFileName(file.file?.seoFilename) }}
                                </span>
                              </q-btn>
                            </div>
                          </div>
                          <!-- Remove Button -->
                          <q-btn color="negative" flat round dense icon="o_close" class="remove-file-icon" @click="removeFile(index)" />
                        </div>
                      </div>
                    </div>
                    <div align="center" class="q-gutter-sm justify-center">
                      <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                      <q-btn v-if="tab === '1_tab'" label="Save & Next" type="submit" color="primary" class="actionBtn" :loading="processing" :disable="processing" no-caps />
                      <q-btn label="Save & Close" type="button" color="primary" class="actionBtn" :loading="processingClose" :disable="processingClose" no-caps @click="onSubmitClose()" />
                    </div>
                  </fieldset>
                </q-tab-panel>
                <q-tab-panel name="2_tab">
                  <fieldset class="q-mb-lg">
                    <div>
                      <div class="flex items-center justify-end q-mb-md">
                        <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAddProjectCharter" />
                      </div>
                      <q-table
                        ref="tableRef"
                        v-model:pagination="pagination"
                        bordered
                        class="no-shadow"
                        virtual-scroll
                        :loading="loading"
                        :rows="rows"
                        :columns="columns"
                        row-key="id"
                        separator="cell"
                        binary-state-sort
                        :rows-per-page-options="[20, 50, 100, 200, 500]"
                      >
                        <template #header="props">
                          <q-tr :props="props" class="bg-primary text-white">
                            <q-th
                              v-for="col in props.cols"
                              :key="col.name"
                              :props="props"
                            >
                              {{ col.label }}
                              <span v-if="['employeeId', 'employeeDesignationId', 'productivityFactor'].includes(col.name)" class="required">*</span>
                            </q-th>
                            <q-th auto-width class="text-center">Actions</q-th>
                          </q-tr>
                        </template>
                        <template #body="props">
                          <q-tr :class="props.row.deleted ? 'hidden' : ''">
                            <q-td width="40%">
                              <formSingleSelectDropdown
                                v-model="props.row.employeeId"
                                :options="activeEmployeesDropdownSingleSelect.list.value"
                                :filter="activeEmployeesDropdownSingleSelect.filter"
                                :error="rowValidations[props.rowIndex]?.value?.employeeId.$error"
                                :errorMessage="rowValidations[props.rowIndex]?.value?.employeeId.$errors[0]?.$message"
                              />
                            </q-td>
                            <q-td width="40%">
                              <formSingleSelectDropdown
                                v-model="props.row.employeeDesignationId"
                                :options="employeeDesignationDropdownSingleSelect.list.value"
                                :filter="employeeDesignationDropdownSingleSelect.filter"
                                :error="rowValidations[props.rowIndex]?.value?.employeeDesignationId.$error"
                                :error-message="rowValidations[props.rowIndex]?.value?.employeeDesignationId.$errors[0]?.$message"
                              />
                            </q-td>
                            <q-td style="width: 10%; white-space: normal; overflow-wrap: break-word;">
                              <q-input
                                v-model="props.row.productivityFactor"
                                outlined
                                stack-label
                                hide-bottom-space
                                dense
                                :rules="[validateFactor]"
                                maxlength="5"
                                :input-class="'text-right'"
                                :error="rowValidations[props.rowIndex]?.value?.productivityFactor.$error"
                                :error-message="rowValidations[props.rowIndex]?.value?.productivityFactor.$errors[0]?.$message"
                              />
                            </q-td>
                            <q-td class="text-center" style="width: 10%;">
                              <q-icon name="o_delete" size="xs" class="cursor-pointer text-red" @click="onDeleteProjectCharter(props.row)">
                                <q-tooltip>Delete</q-tooltip>
                              </q-icon>
                            </q-td>
                          </q-tr>
                        </template>
                      </q-table>
                    </div>
                    <div align="center" class="q-gutter-sm justify-center q-mt-sm">
                      <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                      <q-btn v-if="tab === '1_tab'" label="Save & Next" type="submit" color="primary" class="actionBtn" :loading="processing" :disable="processing" no-caps />
                      <q-btn label="Save & Close" type="button" color="primary" class="actionBtn" :loading="processing" :disable="processing" no-caps @click="onSubmitClose()" />
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
import _ from "lodash";
import { useAuthStore } from "stores/auth";
import { isDate } from "validators/zw_validators.js";
import { zwConfirmDelete, notifySuccess, notifyError, notifyWarning } from "assets/utils";
import { ref, watch, onMounted, toRaw } from "vue";
import { useQuasar, useDialogPluginComponent, uid } from "quasar";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import useVuelidate from "@vuelidate/core";
import { format } from "date-fns"; // Standard TimeZone Conversion

import commonService from "services/common.service";
import projectService from "modules/project/projects.service";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";
import companyContactsModule from "src/modules/company-contacts/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// Shared Inputs
import multiFileUploader from "src/components/form-inputs/_multiFileUpload.vue";
// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const authStore = useAuthStore();
const user = authStore.user;
// const baseURL = process.env.API_BASE_URL;
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();
defineEmits([...useDialogPluginComponent.emits]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, isCharter: { type: Boolean, default: false } });

// Common variables
const tab = ref(props.isCharter ? "2_tab" : "1_tab");
const loading = ref(true);
const processing = ref(false);
const processingClose = ref(false);
const isFilesValid = ref(true);

const rows = ref([]);
const rowCounter = ref(0);
const pagination = ref({ sortBy: "", descending: false, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "employeeId", label: "Employee Name", field: "employeeId", align: "left", sortable: true },
  { name: "employeeDesignationId", label: "Role", field: "employeeDesignationId", align: "left", sortable: true },
  { name: "productivityFactor", label: "Productivity Factor", field: "productivityFactor", align: "right", sortable: true }
]);

// Define model values
const model = ref({
  id: "",
  name: "",
  projectStatusId: "",
  startDateStr: format(new Date(), "MM/dd/yyyy"),
  goLiveDateStr: "",
  description: "",
  active: true,
  employeeId: null,
  projectFiles: [],
  projectFileFlag: "edit"
});

// Project Info - Validation Rules
const rules = {
  name: { required: helpers.withMessage("Project name is required", required), minLength: minLength(1), maxLength: maxLength(100) },
  startDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    required: helpers.withMessage("Start date is required", required)
  },
  goLiveDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    afterStartDate: helpers.withMessage("End date must occur after the start date", (value, { startDateStr }) => {
      if (!value) return true;
      return new Date(value) >= new Date(startDateStr);
    })
  },
  projectStatusId: { required: helpers.withMessage("Project status is required", required) },
  projectPriorityId: { required: helpers.withMessage("Project priority is required", required) },
  projectTypeId: { required: helpers.withMessage("Project type is required", required) },
  planApproverId: { required: helpers.withMessage("Project Approver is required", required) },
  customerId: { required: helpers.withMessage("Customer status is required", required) },
  companyContactId: { required: helpers.withMessage("Contact is required", required) }
};

// Project Charter - Multiple row validation
const rowValidations = ref([]);
function isValidProductivityFactor (value) {
  if (typeof value !== "string" && typeof value !== "number") return false;
  const str = String(value).trim();

  // Reject if contains anything other than digits or one dot
  if (!/^\d*\.?\d+$/.test(str)) return false;

  const num = parseFloat(str);
  return !isNaN(num) && num >= 0 && num <= 1;
}
function validateFactor (value) {
  if (value != null && value !== undefined && String(value).trim() !== "") {
    return isValidProductivityFactor(value) || "Productivity Factor must be a number between 0 and 1";
  }
}

const betweenZeroAndOne = helpers.withMessage(
  "Productivity Factor must be a number between 0 and 1",
  isValidProductivityFactor
);

const rowRules = {
  employeeId: { required: helpers.withMessage("Employee name is required", required) },
  employeeDesignationId: { required: helpers.withMessage("Role is required", required) },
  productivityFactor: { required: helpers.withMessage("Productivity Factor is required", required), betweenZeroAndOne }
};

let projectId = props.id;
let disableTab = true;
if (projectId) {
  disableTab = false;
}

const getProject = (projectId) => {
  loading.value = true;
  projectService.getProject(projectId).then((resp) => {
    model.value = _.cloneDeep(resp);
    console.log("resp", resp);
    companyContactDropdownSingleSelect.load(resp.customerId);
    projectSubCategoryDropdownSingleSelect.load(resp.projectCategoryId);
    activeEmployeesDropdownSingleSelect.load(user.siteId);
    setTimeout(() => {
      getDescriptionBySubCategoryId(resp.projectCategoryId, resp.projectSubcategoryId);
    }, 200);
    model.value.startDateStr = resp.startDate ? format(resp.startDate, "MM/dd/yyyy") : "";
    model.value.description = resp.description ? resp.description : "";
    model.value.goLiveDateStr = resp.goLiveDate ? format(resp.goLiveDate, "MM/dd/yyyy") : "";
    model.value.projectFiles = resp.projectFileList || [];
    let counter = rowCounter.value;
    rows.value = resp.projectEmployeeMappings.map(item => {
      const row = {
        ...item,
        rowCounter: ++counter,
        productivityFactor: item.productivityFactor,
        editing: false,
        flag: "Edit"
      };
      return row;
    });
    rowCounter.value = counter;
  }).finally(() => {
    loading.value = false;
  });
};

function handlePopupShow (statusLabel) {
  getAllProjectStatusDropdown("Project Status", statusLabel);
}

function getContactByCustomerId (customerId) {
  model.value.companyContactId = "";
    companyContactDropdownSingleSelect.load(customerId);
}

function getSubCategoriesByCategoryId (projectCategoryId) {
  model.value.projectSubcategoryId = "";
  model.value.subCtgDescription = "";
    projectSubCategoryDropdownSingleSelect.load(projectCategoryId);
}

async function getDescriptionBySubCategoryId (projectCategoryId, projectSubcategoryId) {
  if (!projectSubCategoryDropdownSingleSelect.list.value.length) {
    await projectSubCategoryDropdownSingleSelect.load(projectCategoryId);
  }
  const found = projectSubCategoryDropdownSingleSelect.list.value.find(item => item.value === projectSubcategoryId);
  model.value.subCtgDescription = found?.data || "";
}

function syncRowValidations () {
  rowValidations.value = rows.value.map(row =>
    !row.deleted ? useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true }) : null
  );
}

function onAddProjectCharter () {
  const currentCounter = ++rowCounter.value;
  rows.value.unshift({
    id: uid(),
    employeeId: "",
    employeeDesignationId: "",
    productivityFactor: "",
    deleted: false,
    rowCounter: currentCounter
  });
  syncRowValidations();
}

const onDeleteProjectCharter = (row) => {
  zwConfirmDelete(
    { data: "This Employee's project access will be removed as well." },
    () => {
      const activeRows = rows.value.filter(r => !r.deleted);

      if (activeRows.length > 1) {
        row.deleted = true;
      } else {
        notifyError({ message: "Please add at least one row." });
      }
    }
  );
};

function disableProjectDatesBeforeStartDate (date) {
  // If no Start Date is set, allow all dates
  if (!model.value.startDateStr) {
    return true;
  }
  const start = new Date(model.value.startDateStr);
  const current = new Date(date);

  // Disable dates before the Start Date
  return current >= start;
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Save & Next or Save & Close
// --------------------------------------------------------------------------------------------------------------------------------------------------

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const onSubmitClose = () => {
  onSubmit(1);
};

const onSubmit = async (isClose = 0) => {
  if (isClose === 1) {
    processingClose.value = true;
    processing.value = false;
  } else {
    processing.value = true;
  }
  try {
    const formData = new FormData();
    let isValid = true;
    rowValidations.value = rows.value.map(row =>
      !row.deleted ? useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true }) : null
    );

    for (let i = 0; i < rowValidations.value.length; i++) {
      const validation = rowValidations.value[i];
      const row = rows.value[i];

      if (!row.deleted && validation?.value) {
        await validation.value.$touch();
        const isRowValid = await validation.value.$validate();
        if (!isRowValid) isValid = false;
      }
    }

    if (!isFilesValid.value) {
      notifyWarning({ message: "Please upload valid files" });
      return;
    }

    if ((await v$.value.$validate() && isValid) || (props.isCharter && isValid)) {
      if (tab.value === "2_tab" && rows.value.length === 0) {
        notifyError({ message: "Add at least one employee in project." });
        return;
      }
      if (isClose === 1) {
        processingClose.value = true;
      } else {
        processing.value = true;
      }
      if (!props.isCharter) {
        // Append other fields
        formData.append("companyContactId", model.value.companyContactId);
        formData.append("projectStatusId", model.value.projectStatusId);
        formData.append("projectCategoryId", model.value.projectCategoryId ? model.value.projectCategoryId : "");
        formData.append("projectSubcategoryId", model.value.projectSubcategoryId ? model.value.projectSubcategoryId : "");
        formData.append("startDateStr", model.value.startDateStr);
        formData.append("goLiveDateStr", model.value.goLiveDateStr);
        formData.append("projectPriorityId", model.value.projectPriorityId);
        formData.append("projectTypeId", model.value.projectTypeId);
        formData.append("planApproverId", model.value.planApproverId);
        formData.append("active", model.value.active);
        formData.append("isTemplate", model.value.isTemplate ?? false);
        formData.append("description", model.value.description);

        toRaw(model.value.projectFiles || []).forEach((file) => {
          if (file.file && file.file.virtualPath) {
            // For existing files, append metadata instead of the file itself
            formData.append("ExistingFiles", JSON.stringify({
              id: file.id,
              virtualPath: file.file.virtualPath
            }));
          } else {
            // For new files, append as raw file objects (IFormFile)
            formData.append("ProjectFiles", file);
          }
        });

        // Also pass the projectFileFlag for general status tracking
        formData.append("projectFileFlag", model.value.projectFileFlag || "no_change");
      }
      const isValidRow = (emp) => {
        if (emp.deleted && emp.id === null) return false;
        if (!emp) return false;

        return true;
      };

      model.value.projectEmployeeMappings = rows.value
        .filter(isValidRow)
        .map(emp => ({ ...emp }));

      formData.append("name", model.value.name);
      formData.append("customerId", model.value.customerId);
      formData.append("isCharter", props.isCharter);
      formData.append("tab", tab.value);
      model.value.projectEmployeeMappings.forEach((emp, index) => {
        const trimmedProductivityFactor = (emp.productivityFactor ?? "").toString().trim();
        const parsedProductivityFactor =
      emp.deleted === true ? 0 : trimmedProductivityFactor === "" ? null : parseFloat(trimmedProductivityFactor);

        formData.append(`projectEmployeeMappings[${index}].id`, emp.id ?? uid());
        formData.append(`projectEmployeeMappings[${index}].employeeId`, emp.employeeId ?? "");
        formData.append(`projectEmployeeMappings[${index}].employeeDesignationId`, emp.employeeDesignationId ?? "");
        formData.append(`projectEmployeeMappings[${index}].productivityFactor`, parsedProductivityFactor);
        formData.append(`projectEmployeeMappings[${index}].deleted`, emp.deleted ?? false);
      });

      projectService.saveProject(projectId, formData).then((resp) => {
        notifySuccess({ message: "Project is saved successfully." });
        if (tab.value === "2_tab" && rows.value.length === 0) {
          notifyError({ message: "Add at least one role in project charter." });
          return;
        }
        projectId = resp.id;
        disableTab = false;
        getProject(projectId);
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
    console.error("Error in submitting the project:", error);
    notifyError({ message: "An error occurred while saving the project." });
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

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get All Dropdowns
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get all project status list
const projectStatusList = ref([]);
const projectStatusListOptions = ref([]);

function getAllProjectStatusDropdown (typeName, currentStatusLabel = null) {
  commonService.getDropDown(typeName).then((resp) => {
    const allowedTransitions = {
      New: ["Open", "On Hold", "Cancelled"],
      Open: ["On Hold", "Cancelled"],
      "In progress": ["On Hold", "Completed"],
      "On Hold": ["Open", "Cancelled", "Completed"]
    };

    const responseData = resp.map((item) => {
      const label = item.dropdownValue;
      const isAllowed = allowedTransitions[currentStatusLabel]?.includes(label);
      const shouldDisable = currentStatusLabel === "Cancelled" || currentStatusLabel === "Completed"
        ? true
        : currentStatusLabel
          ? !isAllowed
          : false;

      return {
        text: item.dropdownValue,
        value: item.id,
        disable: shouldDisable
      };
    });

    const newActivityStatus = responseData.find(status => status.text.toLowerCase() === "new");
    if (newActivityStatus && props.id === "") {
      model.value.projectStatusId = newActivityStatus.value;
    }

    projectStatusListOptions.value = responseData;
    projectStatusList.value = responseData;
  });
}

function getProjectStatusListFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectStatusList.value = projectStatusListOptions.value;
    } else {
      projectStatusList.value = projectStatusListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// -------------------------------------------------------------------------------------------------------
// Upload Image
// -------------------------------------------------------------------------------------------------------
function handleFiles (files) {
  model.value.projectFiles = files;
  model.value.projectFileFlag = "edit";
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
        ${imageFormats.includes(fileExtension) ? `<img src="${fileUrl}" alt="Image Preview">` : `<iframe src="${viewerUrl}"></iframe>`}
      </body>
    </html>
  `);
  }, 100);
}

function getFileIcon (mimeType) {
  const mimeToIconMap = {
    "application/pdf": "o_picture_as_pdf",
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": "o_insert_chart",
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document": "o_description",
    "application/vnd.openxmlformats-officedocument.presentationml.presentation": "o_slideshow",
    "application/vnd.ms-powerpoint": "o_slideshow",
    "application/zip": "o_folder_zip",
    "text/plain": "o_article",
    "image/png": "o_image",
    "image/jpeg": "o_image",
    "image/gif": "o_image",
    default: "o_insert_drive_file"
  };

  return mimeToIconMap[mimeType] || mimeToIconMap.default;
}

function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
}

function removeFile (index) {
  const file = model.value.projectFiles[index];

  if (file && file.name && file.type) {
    URL.revokeObjectURL(file); // Clean up object URL
  }

  if (file && file.file && file.file.virtualPath) {
    // If it's an existing file, mark it as "remove" instead of deleting from array
    file.flag = "remove";
    model.value.projectFiles.splice(index, 1);
  } else {
    // For new files, just remove them from the list
    model.value.projectFiles.splice(index, 1);
  }

  isFilesValid.value = true;
  if (model.value.projectFiles.length === 0) {
    model.value.projectFileFlag = "remove";
  }
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectCategoryDropdownSingleSelect,
  projectSubCategoryDropdownSingleSelect,
  projectTypeDropdownSingleSelect,
  projectPriorityDropdownSingleSelect,
  projectApproverDropdownSingleSelect
} = projectModule();

const { activeEmployeesDropdownSingleSelect,
  employeeDesignationDropdownSingleSelect
} = employeeModule();

const { customerDropdownSingleSelect } = customerModule();
const { companyContactDropdownSingleSelect } = companyContactsModule();

watch(tab, (newVal, oldVal) => {
  if (newVal !== oldVal) {
  projectApproverDropdownSingleSelect.reset();
  activeEmployeesDropdownSingleSelect.reset();
  employeeDesignationDropdownSingleSelect.reset();
  }
});
// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------

onMounted(() => {
  projectApproverDropdownSingleSelect.load(user.siteId);
  employeeDesignationDropdownSingleSelect.load("Employee Designation");
  customerDropdownSingleSelect.load();
  getAllProjectStatusDropdown("Project Status");
  projectPriorityDropdownSingleSelect.load("Project Priorities");
  projectTypeDropdownSingleSelect.load("Project Type");
  projectCategoryDropdownSingleSelect.load("ProjectCategory");
});

watch(() => projectId, (newValue, oldValue) => {
  if (newValue) {
    getProject(projectId);
  }
}, { immediate: true });

</script>
