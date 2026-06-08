<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:25vw !important; max-width: 25vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Multi Task-Activitiessss {{ selectedField }} Change</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <div v-if="selectedField == 'Status'">
                <div class="row items-center">
                  <div class="col-lg-3 col-md-3 col-sm-6 col-xs-6 flex items-center">
                    <label class="Cutomlabel fs-13">Status</label>
                  </div>
                  <div class="col-lg-9 col-md-9 col-sm-6 col-xs-6">
                   <formSingleSelectDropdown
                    v-model="model.activityStatusId"
                    :options="projectTaskActivityStatusDropdownSingleSelect.list.value"
                    :filter="projectTaskActivityStatusDropdownSingleSelect.filter"
                    wrapper-class="col-12"
                    :error="v$.activityStatusId.$error"
                    :error-message="v$.activityStatusId.$errors[0]?.$message"
                    />
                  </div>
                </div>
              </div>
              <div v-if="selectedField == 'Active/Inactive Status'">
                <div class="row items-center">
                  <div class="col-lg-3 col-md-3 col-sm-6 col-xs-6">
                    <label class="Cutomlabel q-mt-xs fs-13">Active/Inactive</label>
                  </div>
                  <div class="col-lg-9 col-md-9 col-sm-6 col-xs-6">
                    <q-option-group
                      v-model="model.active"
                      :options="statusList"
                      type="radio"
                      inline
                      class="q-ml-sm"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter">
          <q-btn
            color="grey-4"
            push
            outline
            label="CANCEL"
            type="button"
            class="text-grey-9 actionBtn"
            no-caps
            @click="onDialogCancel"
          />
          <q-btn
            color="primary"
            push outline
            label="SET"
            type="submit"
            class="actionBtn"
            :loading="processing"
            no-caps
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, onMounted, computed } from "vue";
import { notifySuccess, notifyError } from "assets/utils";

import projectActivitiesService from "modules/project-tasks-activities/projectTasksActivities.service";

// SOP Change :- Shared Dropdowns
import projectTasksActivities from "src/modules/project-tasks-activities/utils/dropdowns.js";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";


// ------------------------------------------------------------------------------------
// Common variables
// ------------------------------------------------------------------------------------
const processing = ref(false);

// ------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ------------------------------------------------------------------------------------
const props = defineProps({ activityIds: { type: Array, required: true }, selectedField: { type: String, default: "" }, disableOpen: { type: Boolean, default: false } });
const selectedField = props.selectedField;
const activityIds = props.activityIds;

// ------------------------------------------------------------------------------------
// Define emits
// ------------------------------------------------------------------------------------
const $emit = defineEmits(["hide", "ok"]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// ------------------------------------------------------------------------------------
// Define model values
// ------------------------------------------------------------------------------------
const model = ref({
  activityStatusId: null,
  active: null
});

// ------------------------------------------------------------------------------------
// Validation rules
// ------------------------------------------------------------------------------------
const rules = computed(() => {
  const baseRules = {};

  if (selectedField === "Status") {
    baseRules.activityStatusId = {
      required: helpers.withMessage("Status is required", required)
    };
  }
  if (selectedField === "Active/Inactive Status") {
    baseRules.active = {
      required: helpers.withMessage("Please select Active or Inactive", required)
    };
  }
  return baseRules;
});

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get All Dropdowns
// --------------------------------------------------------------------------------------------------------------------------------------------------
const { projectTaskActivityStatusDropdownSingleSelect } = projectTasksActivities();

const statusList = ref([
  { label: "Active", value: true },
  { label: "Inactive", value: false }
]);

// -------------------------------------------------------------------------------------------------------
// Submit form
// -------------------------------------------------------------------------------------------------------
async function onSubmit () {
  if (!await v$.value.$validate()) {
    return;
  }

  if (selectedField === "Status") {
    await submitStatusUpdate();
  }

  if (selectedField === "Active/Inactive Status") {
    await submitActiveInactiveUpdate();
  }
}

async function submitStatusUpdate () {
  processing.value = true;
  try {
    const payload = {
      activityIds,
      activityStatusId: model.value.activityStatusId
    };
    await projectActivitiesService.updateTaskActivityStatus(payload);
    notifySuccess({ message: "Status updated successfully." });
    $emit("ok");
    $emit("hide");
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

async function submitActiveInactiveUpdate () {
  processing.value = true;
  try {
    const payload = {
      activityIds,
      active: model.value.active
    };
    await projectActivitiesService.updateActivityActiveStatus(payload);
    notifySuccess({
      message: `Activities ${model.value.active ? "activated" : "deactivated"} successfully.`
    });
    $emit("ok");
    $emit("hide");
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------
onMounted(() => {
  projectTaskActivityStatusDropdownSingleSelect.load("Activity Status");
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
