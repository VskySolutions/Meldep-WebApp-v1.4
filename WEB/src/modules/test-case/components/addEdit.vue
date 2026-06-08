<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 80vw !important;max-width: 80vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Test Case</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Test Case Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.projectId"
                    label="Project Name"
                    :readonly="readonlyProject != '' ? '' : 'readonlyProject'"
                    :options="projectNameDropdownSingleSelect.list.value"
                    :filter="projectNameDropdownSingleSelect.filter"
                    :error="v$.projectId.$error"
                    :error-message="v$.projectId.$errors[0]?.$message"
                    @update:model-value="getPlanByProjectId(model.projectId)"
                  />
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.planId"
                    label="Test Plan Name"
                    :readonly="readonlyTestPlan != '' ? '' : 'readonlyTestPlan'"
                    :options="testPlansByProjectIdForDropdownSingleSelect.list.value"
                    :filter="testPlansByProjectIdForDropdownSingleSelect.filter"
                    :disable="(model.projectId?.length || 0) === 0"
                    :error="v$.planId.$error"
                    :error-message="v$.planId.$errors[0]?.$message"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-12 col-md-12">
                  <label class="label q-mb-xs text-black">Test Case Name<span class="required">*</span></label>
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
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.areaId"
                    label="Area"
                    :required="false"
                    :options="areaForDropdownSingleSelect.list.value"
                    :filter="areaForDropdownSingleSelect.filter"
                  />
                </div>
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
                  <formSingleSelectDropdown
                    v-model="model.statusId"
                    label="Status"
                    :required="false"
                    :option-disable="disableOption"
                    :options="testCaseStatusForDropdownSingleSelect.list.value"
                    :filter="testCaseStatusForDropdownSingleSelect.filter"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div v-if="isFailStatus()" class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.employeeId"
                    label="Assigned To"
                    :required="false"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.testedBy"
                    label="Tested By"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4">
                  <formDate
                    v-model="model.testedDateStr"
                    label="Tested Date"
                    :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
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
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="form-group">
                    <label class="label q-mb-xs text-black">Steps</label>
                    <q-editor
                      v-model="model.steps"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="form-group">
                    <label class="label q-mb-xs text-black">Expected Result</label>
                    <q-editor
                      v-model="model.expectedResult"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="form-group">
                    <label class="label q-mb-xs text-black">Actual Result</label>
                    <q-editor
                      v-model="model.actualResult"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
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
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import { useAuthStore } from "stores/auth";
import { notifySuccess, notifyError, getLocalStorage } from "assets/utils";
import { isDate } from "validators/zw_validators.js";
import useVuelidate from "@vuelidate/core";
import _ from "lodash";
import { format } from "date-fns"; // Standard TimeZone Conversion

import testcaseService from "../testCase.service";

import projectModule from "src/modules/project/utils/dropdowns.js";
import testPlanModule from "src/modules/test-plan/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import testCaseModule from "src/modules/test-case/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------

const props = defineProps({ id: { type: String, default: "" }, projectIdAttr: { type: String, default: "" }, projectIdValue: { type: String, default: "" }, testPlanIdAttr: { type: String, default: "" }, testPlanIdValue: { type: String, default: "" } });
const readonlyProject = props.projectIdAttr ? "readonly" : "";
const readonlyTestPlan = props.testPlanIdAttr ? "readonly" : "";

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
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "Test Case";
const filterLocalStorage = getLocalStorage(localStorageKey);
const projectIds = filterLocalStorage ? filterLocalStorage.projectIds[0] : [];
const planIds = filterLocalStorage ? filterLocalStorage.planIds[0] : [];

// ----------------------------------------------------------------------------------------------------------------
// Define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  projectId: props.projectIdAttr !== "" ? props.projectIdAttr : (props.projectIdValue !== "" ? props.projectIdValue : (projectIds !== "" ? projectIds : null)),
  name: "",
  planId: props.testPlanIdAttr !== "" ? props.testPlanIdAttr : (planIds !== "" ? planIds : null),
  planMakerId: "",
  planReviewerId: "",
  employeeId: "",
  testedBy: user?.employeeId ? user.employeeId : "",
  description: "",
  steps: "",
  expectedResult: "",
  actualResult: "",
  testedDateStr: format(new Date(), "MM/dd/yyyy")
});

// ----------------------------------------------------------------------------------------------------------------
// Validation rules
// ----------------------------------------------------------------------------------------------------------------

const rules = {
  projectId: { required: helpers.withMessage("Project name is required", required) },
  planId: { required: helpers.withMessage("Test plan is required", required) },
  name: { required: helpers.withMessage("Name is required", required), minLength: minLength(1), maxLength: maxLength(200) },
  testedDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate)
  }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// ----------------------------------------------------------------------------------------------------------------
// Get Test case Details
// ----------------------------------------------------------------------------------------------------------------

const getTestCase = () => {
  loading.value = true;
  testcaseService.getTestCase(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.testedDateStr = resp.testedDate ? format(resp.testedDate, "MM/dd/yyyy") : "";
    testPlansByProjectIdForDropdownSingleSelect.load(resp.projectId);
  }).finally(() => {
    loading.value = false;
  });
};

// ------------------------------------------------------------------------------------
// All Dropdowns
// ------------------------------------------------------------------------------------

const { projectNameDropdownSingleSelect } = projectModule();
const { testPlansByProjectIdForDropdownSingleSelect } = testPlanModule();
const { testCaseStatusForDropdownSingleSelect } = testCaseModule();
const { activeEmployeesDropdownSingleSelect } = employeeModule();

const {
  areaForDropdownSingleSelect,
  workspaceForDropdownSingleSelect
} = projectTaskModule();

// Get all test case status List
// const statusList = ref([]);
// const statusListFilter = ref([]);
// function getAllStatusListForDropdown (typeName) {
//   commonService.getDropDown(typeName).then((resp) => {
//     const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
//     statusList.value = responseData;
//     statusListFilter.value = responseData;

//     // Set "New" status as the default if it exists
//     const newStatus = responseData.find(status => status.text.toLowerCase() === "new");
//     if (newStatus && props.id === "") {
//       model.value.statusId = newStatus.value;
//     }
//   });
// }
// // Search test case status for dropdown
// function getAllStatusListDropdownForFilter (val, update, abort) {
//   update(() => {
//     const needle = val ? val.toLowerCase() : "";
//     if (needle === "") {
//       statusList.value = statusListFilter.value;
//     } else {
//       statusList.value = statusListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
//     }
//   });
// }

function getPlanByProjectId (projectId) {
  model.value.planId = "";
  testPlansByProjectIdForDropdownSingleSelect.load(projectId);
}

function disableOption (option) {
  return option.text && option.text.toLowerCase() === "reopen";
}

function isFailStatus () {
  const selectedStatus = testCaseStatusForDropdownSingleSelect.list.value.find((status) => status.value === model.value.statusId);
  return selectedStatus?.text.toLowerCase() === "fail";
}

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    if (await v$.value.$validate()) {
      processing.value = true;
      model.value.employeeId = model.value.employeeId === "" ? null : model.value.employeeId;
      model.value.testedBy = model.value.testedBy === "" ? null : model.value.testedBy;
      testcaseService.saveTestCase(props.id, model.value).then((resp) => {
        notifySuccess({ message: "Test Case is saved successfully." });
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
    getTestCase();
  }
}, { immediate: true });

watch(() => model.value.projectId, (newValue, oldValue) => {
  if (newValue) {
    testPlansByProjectIdForDropdownSingleSelect.load(model.value.projectId);
  }
}, { immediate: true });

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  projectNameDropdownSingleSelect.load();
  activeEmployeesDropdownSingleSelect.load();
  areaForDropdownSingleSelect.load("Area");
  workspaceForDropdownSingleSelect.load("Workspace");
  await testCaseStatusForDropdownSingleSelect.load("Test Case Status");

  // Set "New" status as the default if it exists  
  const newStatus = await testCaseStatusForDropdownSingleSelect.getValueByLabel("New");
  if (newStatus && props.id === "") {
    model.value.statusId = newStatus;
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
</style>
