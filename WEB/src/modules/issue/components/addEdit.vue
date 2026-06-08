<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw !important;max-width: 60vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Issue</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Issue Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.projectId"
                    label="Project Name"
                    :options="projectNameDropdownSingleSelect.list.value"
                    :filter="projectNameDropdownSingleSelect.filter"
                    :error="v$.projectId.$error"
                    :error-message="v$.projectId.$errors[0]?.$message"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.projectModuleId"
                    label="Project Module"
                    :disable="!model.projectId"
                    :options="projectModulesByProjectIdForDropdownSingleSelect.list.value"
                    :filter="projectModulesByProjectIdForDropdownSingleSelect.filter"
                    :error="v$.projectModuleId.$error"
                    :error-message="v$.projectModuleId.$errors[0]?.$message"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.areaId"
                    label="Area"
                    :required="false"
                    :options="areaForDropdownSingleSelect.list.value"
                    :filter="areaForDropdownSingleSelect.filter"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.workspaceId"
                    label="Workspace"
                    :required="false"
                    :options="workspaceForDropdownSingleSelect.list.value"
                    :filter="workspaceForDropdownSingleSelect.filter"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4">
                  <label class="label q-mb-xs text-black">Issue Title<span class="required">*</span></label>
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
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.priorityId"
                    label="Issue Priority"
                    :options="issuePriorityDropdownSingleSelect.list.value"
                    :filter="issuePriorityDropdownSingleSelect.filter"
                    :error="v$.priorityId.$error"
                    :error-message="v$.priorityId.$errors[0]?.$message"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.statusId"
                    label="Status"
                    :options="issueStatusDropdownSingleSelect.list.value"
                    :filter="issueStatusDropdownSingleSelect.filter"
                    :error="v$.statusId.$error"
                    :error-message="v$.statusId.$errors[0]?.$message"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.typeId"
                    label="Issue Type"
                    :options="issueTypeForDropdownSingleSelect.list.value"
                    :filter="issueTypeForDropdownSingleSelect.filter"
                    :error="v$.typeId.$error"
                    :error-message="v$.typeId.$errors[0]?.$message"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.reportedById"
                    label="Reported By"
                    :required="false"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.employeeId"
                    label="Assigned To"
                    :required="false"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4 q-mb-md">
                  <formDate
                    v-model="model.dueDateStr"
                    label="Due Date"
                    :required="false"
                    :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                    :error="v$.dueDateStr.$error"
                    :error-message="v$.dueDateStr.$errors[0]?.$message"
                    :onBlur="() => v$.dueDateStr.$touch()"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="form-group">
                    <label class="label q-mb-xs text-black">Description</label>
                    <q-editor
                      v-model="model.description"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                      class="q-pa-sm"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import { useAuthStore } from "stores/auth";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import { notifySuccess, notifyError, getLocalStorage } from "assets/utils";
import { isDate } from "validators/zw_validators.js";
import _ from "lodash";
import useVuelidate from "@vuelidate/core";

import issueService from "../issue.service";

// Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import issueModule from "src/modules/issue/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------

const props = defineProps({ id: { type: String, default: "" }, projectIdAttr: { type: String, default: "" }, projectIdValue: { type: String, default: "" }, moduleIdAttr: { type: String, default: "" }, taskProjectId: { type: String, default: "" }, taskModuleId: { type: String, default: "" }, taskPriorityId: { type: String, default: "" }, taskName: { type: String, default: "" } });

// ----------------------------------------------------------------------------------------------------------------
// Define emits
// ----------------------------------------------------------------------------------------------------------------

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const loading = ref(true);
const processing = ref(false);
const $q = useQuasar();
const authStore = useAuthStore();
const user = authStore.user;
const { fonts, toolbar } = getEditorConfig($q);

// ----------------------------------------------------------------------------------------------------------------
// Local Storage
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "Issue";
const filterLocalStorage = getLocalStorage(localStorageKey);
const projectIds = filterLocalStorage ? filterLocalStorage.projectIds[0] : [];
const projectModuleIds = filterLocalStorage ? filterLocalStorage.projectModuleIds[0] : [];

// ----------------------------------------------------------------------------------------------------------------
// Define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  projectId: props.projectIdAttr !== "" ? props.projectIdAttr : (props.taskProjectId && props.taskProjectId !== null ? props.taskProjectId : (projectIds !== "" ? projectIds : null)),
  name: props.taskName !== "" ? props.taskName : "",
  priorityId: "",
  statusId: "",
  typeId: "",
  projectModuleId: props.moduleIdAttr !== "" ? props.moduleIdAttr : (props.taskModuleId && props.taskModuleId !== null ? props.taskModuleId : (projectModuleIds !== "" ? projectModuleIds : null)),
  reportedById: user?.employeeId ? user.employeeId : "",
  employeeId: "",
  dueDateStr: "",
  description: "",
  areaId: null,
  workspaceId: null
});

// ----------------------------------------------------------------------------------------------------------------
// Validation rules
// ----------------------------------------------------------------------------------------------------------------

const rules = {
  projectId: { required: helpers.withMessage("Project is required", required) },
  projectModuleId: { required: helpers.withMessage("Project module is required", required) },
  priorityId: { required: helpers.withMessage("Priority is required", required) },
  statusId: { required: helpers.withMessage("Status is required", required) },
  typeId: { required: helpers.withMessage("Type is required", required) },
  name: { required: helpers.withMessage("Issue title is required", required), minLength: minLength(1), maxLength: maxLength(200) },
  dueDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate)
  }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// ----------------------------------------------------------------------------------------------------------------
// Get Issue Details
// ----------------------------------------------------------------------------------------------------------------

const getIssue = () => {
  loading.value = true;
  issueService.getIssue(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.dueDateStr = model.value.dueDate;
  }).finally(() => {
    loading.value = false;
  });
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------

const { projectNameDropdownSingleSelect } = projectModule();
const { projectModulesByProjectIdForDropdownSingleSelect } = projectModuleOfProjectModule();
const { activeEmployeesDropdownSingleSelect } = employeeModule();

const {
  areaForDropdownSingleSelect,
  workspaceForDropdownSingleSelect
} = projectTaskModule();

const {
  issuePriorityDropdownSingleSelect,
  issueStatusDropdownSingleSelect,
  issueTypeForDropdownSingleSelect
} = issueModule();

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    if (await v$.value.$validate()) {
      processing.value = true;
      issueService.saveIssue(props.id, model.value).then((resp) => {
        notifySuccess({ message: "Issue is saved successfully." });
        onDialogOK();
      });
    }
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getIssue();
  }
}, { immediate: true });

watch(() => model.value.projectId, (newValue, oldValue) => {
  if (newValue) {
    projectModulesByProjectIdForDropdownSingleSelect.load(false, false, model.value.projectId);
  }
}, { immediate: true });

// On page rendering
onMounted(async () => {
  projectNameDropdownSingleSelect.load();
  areaForDropdownSingleSelect.load("Area");
  workspaceForDropdownSingleSelect.load("Workspace");
  activeEmployeesDropdownSingleSelect.load();
  
  await issueTypeForDropdownSingleSelect.load("Issue Type");
  await issuePriorityDropdownSingleSelect.load("Issue Priority");
  await issueStatusDropdownSingleSelect.load("Issue Status");

  // Set "high" status as the default if it exists
  const highPriority = await issuePriorityDropdownSingleSelect.getValueByLabel("High");
  if (highPriority && props.id === "") {
    model.value.priorityId = highPriority;
  }
  
  // Set "New" status as the default if it exists  
  const newStatus = await issueStatusDropdownSingleSelect.getValueByLabel("New");
  if (newStatus && props.id === "") {
    model.value.statusId = newStatus;
  }

  // Set "bug" type as the default if it exists
  const bugType = await issueTypeForDropdownSingleSelect.getValueByLabel("Bug");
  if (bugType && props.id === "") {
    model.value.typeId = bugType;
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
.q-editor__content img {
  max-width: 100%;
  height: auto;
  display: block;
}

</style>
