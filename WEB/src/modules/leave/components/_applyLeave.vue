<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1000px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Apply" }} Leave</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Leave Info</legend>
              <div class="row items-center bg-light-blue-3 text-black q-pa-sm q-gutter-sm q-mb-md">
                <div class="col-auto flex items-center q-mr-lg">
                  <label class="label"><span class="font-weight-medium">Year: </span>
                    {{ selectedYear }}
                  </label>
                </div>
                <div class="col-auto flex items-center q-mr-lg">
                  <label class="label"><span class="font-weight-medium">Total Leaves: </span>
                    {{ empModel.totalLeaves }}
                  </label>
                </div>
                <div class="col-auto flex items-center  q-mr-lg">
                  <label class="label"><span class="font-weight-medium">Casual Leaves: </span>
                    {{ empModel.casualLeaves }}
                  </label>
                </div>
                <div class="col-auto flex items-center  q-mr-lg">
                  <label class="label"><span class="font-weight-medium">Sick Leaves: </span>
                    {{ empModel.sickLeaves }}
                  </label>
                </div>
                <div class="col-auto flex items-center  q-mr-lg">
                  <label class="label"><span class="font-weight-medium">Leave Balance: </span>
                    {{ empModel.leaveBalance }}
                  </label>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-6 col-lg-4">
                  <label class="label q-mb-xs text-black">Employee Name<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.employeeName"
                      outlined
                      stack-label
                      hide-bottom-space
                      readonly
                      :dense="true"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-6 col-lg-4">
                  <label class="label q-mb-xs text-black">Leave Type<span class="required">*</span></label>
                  <q-select
                    v-model="model.leaveCategoryId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="leaveCategoryList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.leaveCategoryId.$error"
                    :error-message="v$.leaveCategoryId.$errors[0]?.$message"
                    @filter="getAllLeaveTypeListDropdownForFilter"
                    @blur="v$.leaveCategoryId.$touch"
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
                <div class="col-12 col-md-6 col-lg-3">
                  <label class="label q-mb-xs text-black">Start Date of Leave<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.fromDateStr"
                      outlined
                      stack-label
                      hide-bottom-space
                      mask="##/##/####"
                      dense
                      :error="v$.fromDateStr.$error"
                      :error-message="v$.fromDateStr.$errors[0]?.$message"
                      @blur="v$.fromDateStr.$touch"
                    >
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="fromDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date
                              v-model="model.fromDateStr"
                              mask="MM/DD/YYYY"
                              :options="(date) => dateOptions(date)"
                              :events="allEvents"
                              :event-color="eventColors"
                              @navigation="onNavigate"
                              @update:model-value="() => { $refs.fromDateProxy.hide(); calculateTotalLeaveDays(); calculateHalfDayLeave(); checkLeaveStatus(model.fromDateStr, model.toDateStr); }"
                            />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
                <div class="col-12 col-md-6 col-lg-3">
                  <q-checkbox v-model="model.isHalfDay" label="Half Day" :dense="true" class="q-mt-md q-mb-sm" @click="calculateHalfDayLeave" />
                  <div v-if="model.isHalfDay" class="col-lg-3 q-gutter-sm">
                    <q-radio v-model="model.halfDay" dense :val="true" label="1st Half" />
                    <q-radio v-model="model.halfDay" dense :val="false" label="2nd Half" />
                  </div>
                </div>
                <div class="col-12 col-md-6 col-lg-3">
                  <label class="label q-mb-xs text-black">End Date of Leave<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.toDateStr"
                      outlined
                      stack-label
                      hide-bottom-space
                      mask="##/##/####"
                      dense
                      :disable="model.isHalfDay"
                      :error="v$.toDateStr.$error"
                      :error-message="v$.toDateStr.$errors[0]?.$message"
                      @blur="v$.toDateStr.$touch"
                    >
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="toDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date
                              v-model="model.toDateStr"
                              :options="disableBeforeRoleStartDate"
                              mask="MM/DD/YYYY"
                              :events="allEvents"
                              :event-color="eventColors"
                              :default-year-month="getDefaultMonth(model.fromDateStr)"
                              @update:model-value="() => { $refs.toDateProxy.hide(); calculateTotalLeaveDays(); checkLeaveStatus(model.fromDateStr, model.toDateStr); }"
                            />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
                <div class="col-12 col-md-6 col-lg-3">
                  <label class="label q-mb-xs text-black">Total Leave Days<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.noofLeaves"
                      outlined
                      stack-label
                      hide-bottom-space
                      readonly
                      :dense="true"
                      :error="v$.noofLeaves.$error"
                      :error-message="v$.noofLeaves.$errors[0]?.$message"
                      @blur="v$.noofLeaves.$touch"
                    />
                  </div>
                </div>
              </div>
              <q-banner
                v-if="model.toDateStr && !model.isHalfDay && leaveStatus.isSandwich"
                class="bg-orange text-black"
                style="min-height: 20px;"
              >
                This leave is considered a sandwich leave as per the policy. {{ leaveStatus.totalDays }} day(s) will be deducted from your leave balance.
              </q-banner>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-6">
                  <div>
                    <div class="q-mb-xs text-black">Reason<span class="required">*</span></div>
                    <div class="form-group">
                      <q-input
                        v-model="model.reason"
                        outlined
                        autogrow
                        hint="The maximum length allowed is 500."
                        :error="v$.reason.$error"
                        :error-message="v$.reason.$errors[0]?.$message"
                        @blur="v$.reason.$touch"
                      />
                    </div>
                  </div>
                </div>
                <div class="col-12 col-md-6">
                  <div class="q-mb-xs q-mt-sm text-black">Proof Of Medical</div>
                  <div>
                    <!-- <div v-if="!model.fileId" class="col">
                      <div class="form-group">
                        <q-uploader
                          ref="documentUploaderRef"
                          color="white"
                          text-color="dark"
                          with-credentials
                          hide-upload-btn
                          style="min-height: 128px; width: 100%"
                          field-name="proofofmedical"
                          flat bordered label="Drag file here or (+) to upload. (file)"
                          @uploaded="onUploaded"
                          @added="onFileAdded"
                          @removed="onFileRemoved"
                        />
                      </div>
                    </div>
                    <div v-if="model.fileId" class="row justify-center">
                      <img :src="model.virtualPath" alt="" style="width: 30%">
                    </div> -->
                    <!-- <div v-if="model.fileId" class="row justify-center q-mt-sm">
                          <q-btn color="negative" label="Remove" outline no-caps @click="clearImage" />
                        </div> -->
                    <!-- <div v-if="errorMessage" class="text-negative q-mt-sm">
                      {{ errorMessage }}
                    </div> -->

                  <singleFileUploader
                    :allowedExtensions="[
                      '.pdf','.doc','.docx','.jpeg','.jpg','.png'
                    ]"
                    :maxSizeInMb="25"
                    label="Drag file here or (+) to upload. (File)"
                    @file-selected="handleFile"
                    @file-valid="isFileValid = $event"
                  />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" :disable="processing" no-caps />
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
import { notifySuccess, notifyError, notifyWarning } from "assets/utils";
import { useAuthStore } from "stores/auth";
import { isDate } from "validators/zw_validators.js";
import leaveService from "modules/leave/leave.service";
import commonService from "services/common.service";

// Shared Inputs
import singleFileUploader from "src/components/form-inputs/_singleFileUpload.vue";

const processing = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const year = ref(new Date().getFullYear());
const previousYear = ref(year.value);
const isFileValid = ref(true);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const props = defineProps({ id: { type: String, default: "" } });

// Define model values
const leaveModel = ref({});
const empModel = ref({});
const errorMessage = ref("");
const leaveCreditFound = ref({});

// Define model values
const model = ref({
  fromDateStr: "",
  toDateStr: "",
  isHalfDay: false,
  halfDay: true,
  leaveCategoryId: "",
  virtualPath: "",
  employeeName: user.firstName + " " + user.lastName
});

const leaveStatus = ref({
  isPreviousHoliday: false,
  isNextHoliday: false,
  isSandwich: false,
  totalDays: 0
});
const allEvents = computed(() => [
  ...empModel.value.officeLeaves, // office leave dates
  ...empModel.value.employeeLeaves // employee leave dates
]);
const eventColors = (date) => {
  if (empModel.value.employeeLeaves?.includes(date)) {
    return "green-6"; // Employee Leave Color
  }
};

const getEmployeeLeaveBalance = (year) => {
  const selectedYear = year || 0;
  leaveService.getLeaveBalanceDeatilsByEmployeeId(null, selectedYear).then((resp) => {
    empModel.value = resp;
  }).finally(() => {
  });
};

const getDefaultMonth = (fromDateStr) => {
  if (!fromDateStr) return undefined;
  const [month, , year] = fromDateStr.split("/"); // skip day
  return `${year}/${month}`;
};

const disableBeforeRoleStartDate = (date) => {
  const current = new Date(date);
  const yr = current.getFullYear();

  if (model.value.fromDateStr) {
    const from = new Date(model.value.fromDateStr);
    const fromYear = from.getFullYear();

    // If year does not match → disable
    if (yr !== fromYear) return false;
  }

  // Disable all Sundays
  if (current.getDay() === 0) return false;

  // Disable dates before Role Start Date
  if (model.value.fromDateStr) {
    const start = new Date(model.value.fromDateStr);
    if (current < start) return false; // disable
  }

  // Disable dates that exist in officeLeaves
  if (empModel.value.officeLeaves && empModel.value.officeLeaves.length > 0) {
    if (empModel.value.officeLeaves.includes(date)) return false; // disable
  }
  // If leave credit for this year is not found → disable full calendar
  if (!leaveCreditFound.value[yr]) return false;

  return true; // enable date
};

const selectedYear = computed(() => {
  if (!model.value.fromDateStr) { return new Date().getFullYear(); }

  const date = new Date(model.value.fromDateStr);
  if (isNaN(date)) return ""; // invalid date format
  if (date.getFullYear() !== selectedYear.value) { getEmployeeLeaveBalance(date.getFullYear()); }

  return date.getFullYear();
});

const checkLeaveStatus = (fromDate, toDate) => {
  if (!fromDate && !toDate) return;
  if (!model.value.isHalfDay) {
    leaveService.isSandwichLeave(fromDate, toDate)
      .then(resp => {
        leaveStatus.value.isSandwich = resp?.sandwichLeaveData[0]?.isSandwich;
        if (leaveStatus.value.isSandwich) {
          model.value.fromDateStr = resp.sandwichLeaveData[0].seriesStart;
          model.value.toDateStr = resp.sandwichLeaveData[0].seriesEnd;
          leaveStatus.value.totalDays = resp.sandwichLeaveData[0].totalDays;
          model.value.noofLeaves = resp.sandwichLeaveData[0].totalDays;
        }
      });
  }
};

function calculateHalfDayLeave () {
  if (model.value.isHalfDay) {
    model.value.toDateStr = model.value.fromDateStr;
    model.value.noofLeaves = 0.5;
  } else {
    model.value.toDateStr = "";
    model.value.noofLeaves = 0;
  }
}

function calculateTotalLeaveDays () {
  const fromDate = new Date(model.value.fromDateStr);
  const toDate = new Date(model.value.toDateStr);

  if (model.value.isHalfDay) {
    model.value.toDateStr = model.value.fromDateStr;
    model.value.noofLeaves = 0.5;
  }

  if (!isNaN(fromDate) && !isNaN(toDate) && fromDate <= toDate) {
    const timeDifference = toDate.getTime() - fromDate.getTime();
    const daysDifference = Math.ceil(timeDifference / (1000 * 3600 * 24)) + 1; // Including the start date
    model.value.noofLeaves = daysDifference;
  }
}

// -------------------------------------------------------------------------------------------------------
// Drop Downs
// -------------------------------------------------------------------------------------------------------
// const dateOptions = (date) => {
//   // return !empModel.value.officeLeaves.includes(date);
//   const current = new Date(date);

//   // Disable all Sundays
//   if (current.getDay() === 0) return false;

//   // Disable dates in officeLeaves
//   if (empModel.value.officeLeaves && empModel.value.officeLeaves.includes(date)) return false;

//   return true; // enable date
// };
const dateOptions = (date) => {
  const current = new Date(date);

  // Disable all Sundays
  if (current.getDay() === 0) return false;

  // Disable dates in officeLeaves
  if (empModel.value.officeLeaves?.includes(date)) return false;
  // if (empModel.value.employeeLeaves?.includes(date)) return false;

  const yr = current.getFullYear();
  // If leave credit for this year is not found → disable full calendar
  if (!leaveCreditFound.value[yr]) return false;

  return true;
};

const onNavigate = (view) => {
  const newYear = view.year;

  // Avoid API calls on month change
  if (newYear === previousYear.value) return;

  year.value = newYear;
  previousYear.value = newYear;

  loadLeaveCredit(newYear);
};

const loadLeaveCredit = async (yr) => {
  const resp = await leaveService.getLeaveCreditByEmployeeId(
    user.employeeId,
    yr
  );

  // Check correct property
  leaveCreditFound.value[yr] = resp?.data?.length > 0;
};

// Get all project Module Type List
const leaveCategoryList = ref([]);
const leaveCategoryListFilter = ref([]);
function getAllLeaveTypeListForDropdown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    leaveCategoryList.value = responseData;
    leaveCategoryListFilter.value = responseData;
  });
}

// Search leave Type List for dropdown
function getAllLeaveTypeListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      leaveCategoryList.value = leaveCategoryListFilter.value;
    } else {
      leaveCategoryList.value = leaveCategoryListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// -------------------------------------------------------------------------------------------------------
// Upload Image
// -------------------------------------------------------------------------------------------------------
// const documentUploaderRef = ref(null);

// function onFileAdded (files) {
//   if (files[0]) {
//     const allowedTypes = ["image/jpeg", "image/png", "application/pdf"];
//     const file = files[0];
//     // Check if the file type is valid
//     if (allowedTypes.includes(file.type)) {
//       // Valid file type, update the model
//       model.value.filePic = file;
//       model.value.fileChangeFlag = "edit";
//       // Reset any error message (if needed)
//       errorMessage.value = "";
//     } else {
//       // Invalid file type, show an error message
//       errorMessage.value = "Please select a valid image (jpg, png) or PDF file.";
//       // Optionally clear the file input or reset the model to prevent invalid file submission
//       model.value.filePic = null;
//       model.value.fileChangeFlag = null;
//     }
//   }
// }

// function onUploaded (info) {
//   notifySuccess({ message: "File Uploaded successfully." });
//   documentUploaderRef.value.reset();
//   errorMessage.value = "";
// }

// function onFileRemoved (file) {
//   // Reset error message and model state
//   errorMessage.value = ""; // Clear the error message
//   model.value.filePic = null; // Reset the file model
//   model.value.fileChangeFlag = null; // Reset change flag
// }

function handleFile (file) {
  model.value.filePic = file;

  if (file) {
    model.value.fileChangeFlag = "edit";
  } else {
    model.value.filePic = null;
    model.value.fileId = null;
    model.value.fileChangeFlag = "remove";
  }
}
// -------------------------------------------------------------------------------------------------------

// Validation rules
const rules = {
  reason: { required: helpers.withMessage("Reason is required", required) },
  leaveCategoryId: { required: helpers.withMessage("Leave type is required", required) },
  noofLeaves: { required: helpers.withMessage("The Total Leave Days field is required.", required) },
  fromDateStr: {
    required: helpers.withMessage("Leave Start Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  toDateStr: {
    required: helpers.withMessage("Leave End Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate),
    afterStartDate: helpers.withMessage("End date must occur after or be equal to the start date.", (value, startDate) => {
      return new Date(value) >= new Date(startDate.fromDateStr);
    })
  }
};
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    model.value.employeeId = leaveModel.value.employeeId;
    model.value.leaveCreditId = props.id;
    model.value.isSandwich = leaveStatus?.value?.isSandwich;
    model.value.totalDeduction = leaveStatus?.value?.totalDays;
    model.value.year = selectedYear.value;
    if (!isFileValid.value) {
      notifyWarning({ message: "Please upload a valid file" });
      return;
    }
    if (await v$.value.$validate() && errorMessage.value === "") {
      processing.value = true;
      if (model.value.noofLeaves === 0) {
        notifyError({ message: "Total leave days are 0, so you cannot apply for this leave." });
        return;
      }

      if (empModel.value <= 0 || empModel.value < model.value.noofLeaves) {
        notifyError({ message: "You cannot apply for leave because the balance is low." });
        return; // Prevent submission if leave balance is 0
      }
      await leaveService.saveEmployeeLeave(model.value);
      notifySuccess({ message: "Leave is saved successfully." });
      onDialogOK();
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

// watch(() => year.value, (newYear) => {
//   loadLeaveCredit(newYear);
// });

// On page rendering
onMounted(() => {
  getEmployeeLeaveBalance();
  getAllLeaveTypeListForDropdown("Leave Category");
  loadLeaveCredit(year.value);
});
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
/* Disable clicking on dates that have a green underline */
.q-date__calendar-item:has(.q-date__event.bg-green-6) {
  pointer-events: none;
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
