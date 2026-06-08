<template>
  <q-dialog class="customDialog" ref="dialogRef" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1000px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Add Leave Credit</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
        <q-form greedy @submit.prevent.stop="onSubmit">
          <div :class="['q-pa-md cardTable', readonlyProject != '' ? 'edit_projectModule' : '']">
            <div class="q-gutter-y-md">
                <fieldset>
                  <legend>Leave Credit Info</legend>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-xs-12 col-sm-6 col-md-4 col-lg-3">
                    <label class="label q-mb-xs text-black">Year</label>
                    <div>
                      <q-input
                        v-model="model.leaveCreditsforYear"
                        outlined
                        stack-label
                        hide-bottom-space
                        dense
                        readonly
                      />
                    </div>
                    </div>
                    <div class="col-xs-12 col-sm-6 col-md-4 col-lg-4">
                      <label class="label q-mb-xs text-black">Employee<span class="required">*</span></label>
                      <div>
                        <q-select
                          v-model="model.employeeId"
                          clearable
                          use-input
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          :options="employeeList"
                          option-value="value"
                          option-label="text"
                          emit-value
                          map-options
                          @filter="getAllActiveEmployeesListDropdownForFilter"
                          :error="v$.employeeId.$error"
                          :error-message="v$.employeeId.$errors[0]?.$message"
                          @blur="v$.employeeId.$touch"
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
                    <div class="col-xs-12 col-sm-6 col-md-4 col-lg-4">
                      <label class="label q-mb-xs text-black">Leave Type<span class="required">*</span></label>
                      <q-select
                        v-model="model.leaveTypeId"
                        clearable
                        use-input
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        :options="bonusLeavesList"
                        option-value="value"
                        option-label="text"
                        emit-value
                        map-options
                        @filter="getAllLeaveTypeListDropDownForFilter"
                        :error="v$.leaveTypeId.$error"
                        :error-message="v$.leaveTypeId.$errors[0]?.$message"
                        @blur="v$.leaveTypeId.$touch"
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
                    <div class="col-xs-12 col-sm-6 col-md-3">
                      <label class="label q-mb-xs text-black">Casual Leaves<span class="required"></span></label>
                      <div>
                        <q-input
                          v-model="model.casualLeaves"
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          mask="##.##"
                          hint="00.00"
                          :error="v$.casualLeaves.$error"
                          :error-message="v$.casualLeaves.$errors[0]?.$message"
                          @blur="v$.casualLeaves.$touch"
                        />
                      </div>
                    </div>
                    <div class="col-xs-12 col-sm-6 col-md-3">
                      <label class="label q-mb-xs text-black">Sick Leaves<span class="required"></span></label>
                      <div>
                        <q-input
                          v-model="model.sickLeaves"
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          mask="##.##"
                          hint="00.00"
                          :error="v$.sickLeaves.$error"
                          :error-message="v$.sickLeaves.$errors[0]?.$message"
                          @blur="v$.sickLeaves.$touch"
                        />
                      </div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div v-if="isPaidLeave" class="col-xs-12 col-sm-6 col-md-3">
                      <q-checkbox
                        v-model="model.isDefault"
                        label="Default"
                        color="primary"
                      />
                    </div>
                    <div class="col-xs-12 col-md-6">
                      <div>
                        <div class="q-mb-xs text-black">Reason<span class="required">*</span></div>
                        <div class="form-group">
                          <q-input
                            outlined
                            v-model="computedCreditReason"
                            autogrow
                            hint="The maximum length allowed is 250."
                            :readonly="isPaidLeave"
                            :error="v$.creditReason.$error"
                            :error-message="v$.creditReason.$errors[0]?.$message"
                            @blur="v$.creditReason.$touch"
                          />
                        </div>
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
import { useDialogPluginComponent } from "quasar";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, onMounted, computed, watch } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import employeesService from "src/modules/employee/employee.service";
import commonService from "services/common.service";
import leaveService from "../leave.service";

// define props
const props = defineProps({ id: { type: String, default: "" }, projectIdAttr: { type: String, default: "" } });
const readonlyProject = props.projectIdAttr ? "readonly" : "";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Common variables
const processing = ref(false);

// Define model values
const model = ref({
  employeeId: "",
  leaveTypeId: "",
  casualLeaves: "",
  sickLeaves: "",
  creditReason: "",
  isDefault: false,
  leaveCreditsforYear: new Date().getFullYear() // Auto-select current year
});

const atLeastOneLeave = helpers.withMessage(
  "At least one Casual Leave or Sick Leave is required",
  (value, leaves) => {
    return !!(leaves.casualLeaves || leaves.sickLeaves);
  }
);

// Validation rules
const rules = {
  employeeId: { required: helpers.withMessage("Employee is required", required) },
  leaveTypeId: { required: helpers.withMessage("Leave Type is required", required) },
  casualLeaves: { required: atLeastOneLeave },
  sickLeaves: { required: atLeastOneLeave },
  // casualLeaves: { required: helpers.withMessage("Casual Leave is required", required) },
  // sickLeaves: { required: helpers.withMessage("Sick Leave is required", required) },
  creditReason: { required: helpers.withMessage("Credit Reason is required", required), minLength: minLength(1), maxLength: maxLength(250) }
};
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Get all team members (Employee) list
const employeeList = ref([]);
const employeeListArr = ref([]);
const employeeListFilter = ref([]);
function getAllActiveEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    employeeListArr.value = resp;
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id }));
    employeeList.value = responseData;
    employeeListFilter.value = responseData;
  });
}

function getAllActiveEmployeesListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = employeeListFilter.value;
    } else {
      employeeList.value = employeeListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all project Module Type List
const bonusLeavesList = ref([]);
const bonusLeavesListFilter = ref([]);
function getAllLeaveTypeListForDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    bonusLeavesList.value = responseData;
    bonusLeavesListFilter.value = responseData;
  });
}

// Search project Module Type List for dropdown
function getAllLeaveTypeListDropDownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      bonusLeavesList.value = bonusLeavesListFilter.value;
    } else {
      bonusLeavesList.value = bonusLeavesListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const isPaidLeave = computed(() => {
  const selectedLeave = bonusLeavesList.value.find(item => item.value === model.value.leaveTypeId);
  return selectedLeave?.text === "Paid";
});

const computedCreditReason = computed({
  get: () => (isPaidLeave.value ? "Yearly" : model.value.creditReason),
  set: (val) => {
    if (!isPaidLeave.value) {
      model.value.creditReason = val;
    }
  }
});

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    model.value.leaveCreditId = props.id;
    // Convert empty values to null
    model.value.casualLeaves = model.value.casualLeaves || 0;
    model.value.sickLeaves = model.value.sickLeaves || 0;
    if (await v$.value.$validate()) {
      processing.value = true;
      leaveService.saveEmployeeLeaveCredits(model.value).then((resp) => {
        notifySuccess({ message: "Leave Credit is saved successfully." });
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

watch(isPaidLeave, (newVal) => {
  if (newVal) {
    model.value.isDefault = true; // Select checkbox when "Paid" is selected
    model.value.creditReason = "Yearly"; // Set reason to "Yearly"
  } else {
    model.value.isDefault = false; // Unselect checkbox for other types
    model.value.creditReason = ""; // Allow manual input for reason
  }
});

// On page rendering
onMounted(() => {
  getAllLeaveTypeListForDropDown("Leave Type");
  getAllActiveEmployeesListForDropdown();
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
