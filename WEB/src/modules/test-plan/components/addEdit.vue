<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Test Plan</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Test Plan Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <formSingleSelectDropdown
                    v-model="model.projectId"
                    label="Project Name"
                    :options="projectNameDropdownSingleSelect.list.value"
                    :filter="projectNameDropdownSingleSelect.filter"
                    :error="v$.projectId.$error"
                    :error-message="v$.projectId.$errors[0]?.$message"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <label class="label q-mb-xs text-black">Name<span class="required">*</span></label>
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
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.areaId"
                    label="Area"
                    :required="false"
                    :options="areaForDropdownSingleSelect.list.value"
                    :filter="areaForDropdownSingleSelect.filter"
                  />
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.workspaceId"
                    label="Workspace"
                    :required="false"
                    :options="workspaceForDropdownSingleSelect.list.value"
                    :filter="workspaceForDropdownSingleSelect.filter"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                  <formSingleSelectDropdown
                    v-model="model.planMakerId"
                    label="Plan Maker"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                    :error="v$.planMakerId.$error"
                    :error-message="v$.planMakerId.$errors[0]?.$message"
                  />
                </div>
                <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                  <formSingleSelectDropdown
                    v-model="model.planReviewerId"
                    label="Plan Reviewer"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                    :error="v$.planReviewerId.$error"
                    :error-message="v$.planReviewerId.$errors[0]?.$message"
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
            </fieldset>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
      <!-- </q-card-section> -->
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { ref, watch, onMounted } from "vue";
import _ from "lodash";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { notifySuccess, notifyError, getLocalStorage } from "assets/utils";

import testplanService from "../testPlan.service";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import projectModule from "src/modules/project/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// ----------------------------------------------------------------------------------------------------------------
// Define emits
// ----------------------------------------------------------------------------------------------------------------

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------

const props = defineProps({ id: { type: String, default: "" }, projectIdAttr: { type: String, default: "" }, projectIdValue: { type: String, default: "" } });

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const loading = ref(true);
const processing = ref(false);
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "Test Plan";
const filterLocalStorage = getLocalStorage(localStorageKey);
const projectIds = filterLocalStorage ? filterLocalStorage.projectIds[0] : [];

// ----------------------------------------------------------------------------------------------------------------
// Define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  // projectId: "",
  projectId: props.projectIdAttr !== "" ? props.projectIdAttr : (props.projectIdValue !== "" ? props.projectIdValue : (projectIds !== "" ? projectIds : null)),
  name: "",
  planMakerId: "",
  planReviewerId: "",
  description: ""
});

// ----------------------------------------------------------------------------------------------------------------
// Validation rules
// ----------------------------------------------------------------------------------------------------------------

const rules = {
  projectId: { required: helpers.withMessage("Project is required", required) },
  planMakerId: { required: helpers.withMessage("Plan Maker is required", required) },
  planReviewerId: { required: helpers.withMessage("Plan Reviewer is required", required) },
  name: { required: helpers.withMessage("Name is required", required), minLength: minLength(1), maxLength: maxLength(200) }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// ----------------------------------------------------------------------------------------------------------------
// Get Test Plan Details
// ----------------------------------------------------------------------------------------------------------------

const getTestPlan = () => {
  loading.value = true;
  testplanService.getTestPlan(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------

const { projectNameDropdownSingleSelect
} = projectModule();

const {
  areaForDropdownSingleSelect,
  workspaceForDropdownSingleSelect
} = projectTaskModule();

const { activeEmployeesDropdownSingleSelect } = employeeModule();

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    if (await v$.value.$validate()) {
      processing.value = true;
      testplanService.saveTestPlan(props.id, model.value).then((resp) => {
        notifySuccess({ message: "Test Plan is saved successfully." });
        onDialogOK();
      }).finally(() => {
        processing.value = false;
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

// --------------------------------------------------------------------------------------------------------------------------------------------------
// watches a data property with the same name i.e. immediate effect
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getTestPlan();
  }
}, { immediate: true });

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  projectNameDropdownSingleSelect.load();
  areaForDropdownSingleSelect.load("Area");
  workspaceForDropdownSingleSelect.load("Workspace");
  activeEmployeesDropdownSingleSelect.load();
});

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
