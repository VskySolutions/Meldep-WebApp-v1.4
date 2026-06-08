<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Assign" }} Assignment</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <div class="row q-col-gutter-x-md q-mb-xs">
                <formSingleSelectDropdown
                  v-model="model.templateId"
                  label="Template"
                  :options="sopTemplateDropdownSingleSelect.list.value"
                  :filter="sopTemplateDropdownSingleSelect.filter"
                  :error="v$.templateId.$error"
                  :error-message="v$.templateId.$errors[0]?.$message"
                />
                <div class="col-xxl-8 col-lg-8 col-md-8 col-sm-8 col-xs-12">
                  <div class="q-mb-xs text-black">Name<span class="required">*</span></div>
                  <q-input
                    v-model="model.name"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :error="v$.name.$error"
                    :error-message="v$.name.$errors[0]?.$message"
                    @click="v$.name.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-xs">
                <formSingleSelectDropdown
                  v-model="model.assignedToEmployeeId"
                  label="Assigned To"
                  :options="activeEmployeesDropdown.list.value"
                  :filter="activeEmployeesDropdown.filter"
                  :error="v$.assignedToEmployeeId.$error"
                  :error-message="v$.assignedToEmployeeId.$errors[0]?.$message"
                />
                <formSingleSelectDropdown
                  v-model="model.approverEmployeeId"
                  label="Approver"
                  :options="activeEmployeesDropdown.list.value"
                  :filter="activeEmployeesDropdown.filter"
                  :error="v$.approverEmployeeId.$error"
                  :error-message="v$.approverEmployeeId.$errors[0]?.$message"
                />
                <formSingleSelectDropdown
                  v-model="model.priorityId"
                  label="Priority"
                  :options="sopAssignmentPriorityDropdownSingleSelect.list.value"
                  :filter="sopAssignmentPriorityDropdownSingleSelect.filter"
                  :error="v$.priorityId.$error"
                  :error-message="v$.priorityId.$errors[0]?.$message"
                />
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12">
                  <div class="q-mb-xs text-black">Assigned Date<span class="required">*</span></div>
                  <q-input
                    v-model="model.assignedDateStr"
                    outlined
                    stack-label
                    hide-bottom-space
                    mask="##/##/####"
                    dense
                    :error="v$.assignedDateStr.$error"
                    :error-message="v$.assignedDateStr.$errors[0]?.$message"
                    @click="v$.assignedDateStr.$touch"
                  >
                    <template #append>
                      <q-icon name="o_calendar_month" class="cursor-pointer">
                        <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                          <q-date v-model="model.assignedDateStr" mask="MM/DD/YYYY" :options="disablePastDates" @update:model-value="() => $refs.qDateProxy.hide()" />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
                <div class="col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12">
                  <div class="q-mb-xs text-black">Due Date<span class="required">*</span></div>
                  <q-input
                    v-model="model.dueDateStr"
                    outlined
                    stack-label
                    hide-bottom-space
                    mask="##/##/####"
                    dense
                    :error="v$.dueDateStr.$error"
                    :error-message="v$.dueDateStr.$errors[0]?.$message"
                    @click="v$.dueDateStr.$touch"
                  >
                    <template #append>
                      <q-icon name="o_calendar_month" class="cursor-pointer">
                        <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                          <q-date v-model="model.dueDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-xs">
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
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" label="Close" type="button" class="text-grey-9 actionBtn same-size-btn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" label="Submit & Start Assignment" type="submit" class="actionBtn same-size-btn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import _ from "lodash";
import { isDate } from "validators/zw_validators.js";
import { notifySuccess, notifyError } from "assets/utils";
import { useAuthStore } from "stores/auth";
import useVuelidate from "@vuelidate/core";
import { ref, onMounted, watch } from "vue";
import { useQuasar, useDialogPluginComponent } from "quasar";
import { required, helpers } from "@vuelidate/validators";

// SOP Change :- Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import sopAssignmentModule from "src/modules/sop-assignment/utils/dropdowns.js";
import sOPTemplateModule from "src/modules/sop-template/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import sopAssignmentService from "../sopAssignment.service";
import { format } from "date-fns";

const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
// Common variables
const processing = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Define model values
const model = ref({
  id: "",
  name: "",
  templateId: "",
  assignedToEmployeeId: "",
  approverEmployeeId: "",
  priorityId: "",
  dueDateStr: "",
  description: "",
  assignedDateStr: format(new Date(), "MM/dd/yyyy")
});

function disablePastDates (date) {
  const today = new Date();
  today.setHours(0, 0, 0, 0); // remove time

  const parts = date.split("/"); // q-date format: YYYY/MM/DD
  const selectedDate = new Date(parts[0], parts[1] - 1, parts[2]);

  return selectedDate >= today; // allow today & future
}

const getSOPAssignmentByIdInDetail = () => {
  sopAssignmentService.getSOPAssignmentByIdInDetail(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.assignedDateStr = resp.assignedDate ? format(resp.assignedDate, "MM/dd/yyyy") : "";
    model.value.dueDateStr = resp.dueDate ? format(resp.dueDate, "MM/dd/yyyy") : "";
    model.value.description = resp.description ? resp.description : "";
  }).finally(() => {
  });
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { sopAssignmentPriorityDropdownSingleSelect } = sopAssignmentModule();
const { activeEmployeesDropdown } = employeeModule();
const {
  sopTemplateDropdownSingleSelect
} = sOPTemplateModule();
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Validation Rules
// --------------------------------------------------------------------------------------------------------------------------------------------------
const rules = {
  assignedDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    required: helpers.withMessage("Assigned date is required", required)
  },
  dueDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    required: helpers.withMessage("Due date is required", required)
  },
  templateId: { required: helpers.withMessage("SOP template is required", required) },
  assignedToEmployeeId: { required: helpers.withMessage("Assigned to is required", required) },
  approverEmployeeId: { required: helpers.withMessage("Approver is required", required) },
  priorityId: { required: helpers.withMessage("Priority is required", required) },
  name: { required: helpers.withMessage("Name is required", required) }
};
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Submit form
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onSubmit = async () => {
  processing.value = true;

  try {
    const isValid = await v$.value.$validate();

    if (!isValid) {
      processing.value = false;
      return;
    }

    if (model.value.assignedToEmployeeId === model.value.approverEmployeeId) {
      notifyError({ message: "Assigned To and Approver cannot be same" });
      processing.value = false;
      return;
    }

    await sopAssignmentService.createUpdateSOPAssignment(props.id, model.value);

    notifySuccess({ message: "SOP assignment is saved successfully." });

    onDialogOK();
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = false;
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getSOPAssignmentByIdInDetail();
  }
}, { immediate: true });
// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------
onMounted(async () => {
  activeEmployeesDropdown.load(user.siteId);
  sopTemplateDropdownSingleSelect.load();

  await sopAssignmentPriorityDropdownSingleSelect.load("SOP Assignment Priority");
  const mediumOption = sopAssignmentPriorityDropdownSingleSelect.list.value.find(
    opt => opt.text === "Medium"
  );

  if (mediumOption) {
    model.value.priorityId = mediumOption.value;
  }
});

</script>
<style>
.same-size-btn {
  min-width: 150px;
  height: 50px;
}
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
