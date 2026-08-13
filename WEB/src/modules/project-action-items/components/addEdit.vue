<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 80vw !important;max-width: 80vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Project Action Item</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Project Action Item Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.projectId"
                    label="Project Name"
                    :options="projectNameDropdownSingleSelect.list.value"
                    :filter="projectNameDropdownSingleSelect.filter"
                    :error="v$.projectId.$error"
                    :error-message="v$.projectId.$errors[0]?.$message"
                  />
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.requirementId"
                    label="Requirement"
                    :disable="!model.projectId"
                    :required="true"
                    :options="requirementByProjectModuleIdForDropdownSingleSelect.list.value"
                    :filter="requirementByProjectModuleIdForDropdownSingleSelect.filter"
                    :error="v$.requirementId.$error"
                    :error-message="v$.requirementId.$errors[0]?.$message"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-12 col-md-12">
                  <label class="label q-mb-xs text-black">Title<span class="required">*</span></label>
                  <q-input
                    v-model="model.title"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :error="v$.title.$error"
                    :error-message="v$.title.$errors[0]?.$message"
                    @blur="v$.title.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.customerId"
                    label="Customer"
                    :required="false"
                    :options="customerDropdownSingleSelect.list.value"
                    :filter="customerDropdownSingleSelect.filter"
                  />
                </div>
                <div class="col-12 col-sm-6 col-md-6">                  
                  <formSingleSelectDropdown
                    v-model="model.employeeId"
                    label="Employee"
                    :required="false"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-4 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.priorityId"
                    label="Priority"
                    :required="false"
                    :option-disable="disableOption"
                    :options="projectActionItemPrioritySingleSelect.list.value"
                    :filter="projectActionItemPrioritySingleSelect.filter"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4">
                  <formDate
                    v-model="model.dueDate"
                    :required="false"
                    label="Due Date"
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
import { ref, watch, onMounted, computed } from "vue";
import { useAuthStore } from "stores/auth";
import { notifySuccess, notifyError } from "assets/utils";
import { isDate } from "validators/zw_validators.js";
import useVuelidate from "@vuelidate/core";
import _ from "lodash";
import { format } from "date-fns"; // Standard TimeZone Conversion

import projectActionItemsService from "modules/project-action-items/projectActionItems.service";

import projectModule from "src/modules/project/utils/dropdowns.js";
import projectActionItemModule from "src/modules/project-action-items/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// Shared DataTable Features
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------

const props = defineProps({
  id: { type: String, default: "" },
  projectIdAttr: { type: String, default: "" },
  projectIdValue: { type: String, default: "" }
});

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
const isInitializing = ref(false);
const $q = useQuasar();
const authStore = useAuthStore();
const user = authStore.user;
const { fonts, toolbar } = getEditorConfig($q);

const currentSiteId = computed(() => user?.siteId || null);
// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const { getTableState } = useSiteTableState({
  storageKey: "project-Action-Items-Index",
  siteId: currentSiteId
});

const searchStorage = getTableState();

let selectedProjectId = null;

// ----------------------------------------------------------------------------------------------------------------
// Define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  id: "",
   projectId:
    props.projectIdAttr ||
    props.projectIdValue ||
    selectedProjectId ||
    null,
  requirementId: "",
  title: "",
  description: "",
  customerId: "",
  employeeId: "",
  priorityId: "",
  dueDate: format(new Date(), "MM/dd/yyyy")
});

// ----------------------------------------------------------------------------------------------------------------
// Validation rules
// ----------------------------------------------------------------------------------------------------------------

const rules = {
  projectId: { required: helpers.withMessage("Project name is required", required) },
  title: { required: helpers.withMessage("Title is required", required), minLength: minLength(1), maxLength: maxLength(500) },
  requirementId: { required: helpers.withMessage("Requirement is required", required) },
  dueDate: {
    isDate: helpers.withMessage("Date is invalid", isDate)
  }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// ----------------------------------------------------------------------------------------------------------------
// Get Project Action Items
// ----------------------------------------------------------------------------------------------------------------

const getProjectActionItemDetailsById = async () => {
  loading.value = true;
  isInitializing.value = true;
  try {    
    const resp = await projectActionItemsService.getProjectActionItemDetailsById(props.id);
    model.value = _.cloneDeep(resp);
    await requirementByProjectModuleIdForDropdownSingleSelect.load("", model.value.projectId);
    model.id = props.id ? props.id : "";
    model.value.dueDate = resp.dueDate ? format(resp.dueDate, "MM/dd/yyyy") : "";
  } finally {
    loading.value = false;
    isInitializing.value = false;
  }
};

// ------------------------------------------------------------------------------------
// All Dropdowns
// ------------------------------------------------------------------------------------

const { projectNameDropdownSingleSelect } = projectModule();
const { projectActionItemPrioritySingleSelect } = projectActionItemModule();
const { requirementByProjectModuleIdForDropdownSingleSelect } = requirementModule();
const { customerDropdownSingleSelect } = customerModule();
const { activeEmployeesDropdownSingleSelect } = employeeModule();

function disableOption (option) {
  return option.text && option.text.toLowerCase() === "reopen";
}

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    if (await v$.value.$validate()) {
      processing.value = true;
      projectActionItemsService.saveProjectActionItems(model.value).then((resp) => {
        notifySuccess({ message: "Project action items is saved successfully." });
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
    getProjectActionItemDetailsById();
  }
}, { immediate: true });

watch(
  () => model.value.projectId,
  async (newValue) => {
    if (!isInitializing.value) {
      model.value.requirementId = "";
    }
    if (!newValue) return;

    await requirementByProjectModuleIdForDropdownSingleSelect.load("", newValue);
  }, { immediate: true }
);

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  await projectActionItemPrioritySingleSelect.load("Project Action Item Priority");
  requirementByProjectModuleIdForDropdownSingleSelect.load();  
  customerDropdownSingleSelect.load();
  activeEmployeesDropdownSingleSelect.load();

  // selected values
  await projectNameDropdownSingleSelect.load();
  const projectIds = searchStorage?.search?.projectIds || [];
  if (projectIds.length) {
    selectedProjectId =
      projectIds.find(id =>
        projectNameDropdownSingleSelect.list.value.some(x => x.value === id)
      ) || null;

    model.value.projectId = selectedProjectId;
  }
  
  // Set "Medium" Priority as the default if it exists
  const mediumPriority = await projectActionItemPrioritySingleSelect.getValueByLabel("Medium");
  if (mediumPriority && props.id === "") {
    model.value.priorityId = mediumPriority;
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
