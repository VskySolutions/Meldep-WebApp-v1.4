<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent @hide="onDialogHide">
    <q-card style="width: 400px; max-width: 90vw;">
      <q-card-section class="card-header row items-center q-pa-sm">
        <div class="text-h2">{{ detailId ? "Edit" : "Add" }} Work From Home Details</div>
        <q-btn v-close-popup icon="o_close" class="q-ml-auto close" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="submitWorkFromHome">
        <q-card-section class="card-body scroll">
          <div class="col-12 q-mb-sm">
            <label class="label q-mb-xs text-black">Date<span class="required">*</span></label>
            <q-input
              v-model="workFromHomeModel.date"
              outlined
              stack-label
              hide-bottom-space
              mask="##/##/####"
              disable
              :error="workFromHomeModelV$.date.$error"
              :error-message="workFromHomeModelV$.date.$errors[0]?.$message"
              @blur="workFromHomeModelV$.date.$touch"
            >
              <template #append>
                <q-icon name="o_calendar_month" class="cursor-pointer">
                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                    <q-date
                      v-model="workFromHomeModel.date"
                      mask="MM/DD/YYYY"
                      :options="enableCurrentDate"
                      @update:model-value="() => $refs.qDateProxy.hide()"
                    />
                  </q-popup-proxy>
                </q-icon>
              </template>
            </q-input>
          </div>
          <div class="col-12 q-mb-sm">
            <label class="label text-black">Duration <span class="required">*</span></label>
            <q-select
              v-model="selectedWFHDurationId"
              use-input
              fill-input
              outlined
              hide-bottom-space
              hide-selected
              input-debounce="0"
              option-value="id"
              option-label="text"
              map-options
              maxlength="3"
              new-value-mode="add-unique"
              dense
              placeholder="Enter minutes or select from dropdown"
              class="q-mb-sm"
              :options="breakTimeList"
              :error="workFromHomeModelV$.timeInMinutes.$error"
              :error-message="workFromHomeModelV$.timeInMinutes.$errors[0]?.$message"
              @blur="workFromHomeModelV$.timeInMinutes.$touch"
              @update:model-value="onDurationChange"
              @new-value="val => workFromHomeModel.timeInMinutes = val"
              @input-value="onManualInput"
            />
            <label
              v-if="!isNaN(Number(workFromHomeModel.timeInMinutes))
                && Number(workFromHomeModel.timeInMinutes) > 0"
              class="label text-black"
            >
              Hours:
              <span class="text-primary">
                {{ formatMinutesToHours(workFromHomeModel.timeInMinutes) }}
              </span>
            </label>

          </div>
          <div class="col-12">
            <label class="label q-mb-xs text-black">Permission By (Permission taken from / informed to the following person, and acknowledgement received)<span class="required">*</span></label>
            <q-select
              v-model="workFromHomeModel.approverById"
              clearable
              use-input
              stack-label
              outlined
              hide-bottom-space
              :dense="true"
              :options="employeeApproverList"
              option-value="value"
              option-label="text"
              emit-value
              map-options
              :disable="isEditMode"
              :error="workFromHomeModelV$.approverById.$error"
              :error-message="workFromHomeModelV$.approverById.$errors[0]?.$message"
              @blur="workFromHomeModelV$.approverById.$touch"
              @filter="getDefaultLeaveApproverNameForFilter"
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
        </q-card-section>
        <q-separator />
        <q-card-actions align="right">
          <q-btn v-close-popup flat dense bordered color="grey-8" label="Close" @click="onDialogCancel" />
          <!-- <q-btn label="Save" type="submit" flat bordered color="primary" :loading="processing" /> -->
          <q-btn
            label="Save"
            flat
            bordered
            color="primary"
            type="button"
            :loading="processing"
            @click="openTooltip"
          >
            <q-tooltip
              v-model="showTooltip"
              persistent
              no-parent-event
              anchor="top middle"
              self="bottom middle"
            >
              <div class="q-pa-md column q-gutter-md" style="max-width: 400px">
                <div class="text-body2">
                  <div class="text-red-10 text-weight-bold">
                    <b>Important: Please review the WFH policy and obtain approval before submitting the request.</b>
                  </div><br>
                  <b>When to Use</b><br>
                  When working remotely with prior approval from the Project Lead.<br><br>

                  <b>How to Use</b>
                  <ol>
                    <li>Click <b>WFH</b> on the dashboard.</li>
                    <li>Enter the following details:
                      <ol type="a">
                        <li><b>Date:</b> Current date is auto-selected.</li>
                        <li><b>Duration:</b> Select the applicable option (1st Half / 2nd Half / Full Day).</li>
                        <li><b>Permission By:</b> Select the authority who approved your WFH request.</li>
                      </ol>
                    </li>
                    <li>Submit the WFH entry.</li>
                  </ol>

                  <b>Mandatory Conditions</b>
                  <ul>
                    <li>WFH approval must be obtained in advance.</li>
                    <li><b>Multiple-Day WFH: Email confirmation with valid reason is required. Medical documents must be attached if applicable.</b></li>
                    <li><b>Video call attendance is mandatory</b> during working hours and meetings.</li>
                    <li>Employees must remain available during working hours, attend meetings, and ensure timely task completion</li>
                  </ul>

                  <b>Do Not Use</b>
                  <ul>
                    <li>Without prior approval.</li>
                    <li>Without availability during working hours.</li>
                  </ul>
                </div>
                <div class="row items-center q-gutter-sm">
                  <q-btn
                    outline
                    dense
                    label="Close"
                    @click.stop="closeTooltip"
                  />
                  <q-space />
                  <q-btn
                    unelevated
                    color="primary"
                    label="Continue"
                    @click.stop="onContinue"
                  />
                </div>
              </div>
            </q-tooltip>
          </q-btn>
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>
<script setup>
import { ref, onMounted, watch } from "vue";
import useVuelidate from "@vuelidate/core";
import useFilters from "composables/useFilters";
import { required, helpers } from "@vuelidate/validators";
import { notifySuccess, notifyError } from "assets/utils";
import { isDate } from "validators/zw_validators.js";
import { useDialogPluginComponent } from "quasar";

import movementRegisterService from "src/modules/movementRegister/movementRegister.service";
import commonService from "services/common.service";
import employeesService from "src/modules/employee/employee.service";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, detailId: { type: String, default: "" } });

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const { toDate } = useFilters();
const loading = ref(true);
const processing = ref(false);
const currentDate = new Date();
const selectedWFHDurationId = ref(null);
const isEditMode = !!props.detailId;

const workFromHomeModel = ref({
  date: toDate(currentDate),
  timeInMinutes: "",
  wFHDurationId: "",
  approverById: ""
});

const workFromHomeModelRules = {
  date: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  timeInMinutes: {
    required: helpers.withMessage("Duration is Required", required),
    maxValue: helpers.withMessage(
      "Maximum allowed value is 480 minutes",
      value => {
        return Number(value) <= 480;
      }
    )
  },
  approverById: { required: helpers.withMessage("Permission By is required", required) }
};

const workFromHomeModelV$ = useVuelidate(workFromHomeModelRules, workFromHomeModel);

const breakTimeList = ref([]);
const breakTimeListFilter = ref([]);
function getAllBreakTimesListForDropdown (buttonType, typeName) {
  commonService.getDropdownByButton(buttonType, typeName).then((resp) => {
    const responseData = resp.map((item) => ({ id: item.id, text: item.dropDownText, value: Number(item.dropDownValue) }));
    breakTimeList.value = responseData;
    breakTimeListFilter.value = responseData;
    // Set "Full Day" as the default selected value
    if (!isEditMode) {
      const fullDayItem = responseData.find(x => x.text === "Full Day");
      if (fullDayItem) {
        workFromHomeModel.value.timeInMinutes = Number(fullDayItem.value);
        workFromHomeModel.value.wFHDurationId = fullDayItem.id;
        selectedWFHDurationId.value = fullDayItem.id;
      }
    }
  });
}

const breakExplanationList = ref([]);
const breakExplanationListFilter = ref([]);
function getAllBreakExplanationListForDropdown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.dropdownValue })).sort((a, b) => a.sortOrder - b.sortOrder).map(({ sortOrder, ...rest }) => rest);
    breakExplanationList.value = responseData;
    breakExplanationListFilter.value = responseData;
  });
}

const employeeApproverList = ref([]);
const employeeApproverFilter = ref([]);
function getDefaultLeaveApproverNameForDropdown () {
  employeesService.getDefaultLeaveApproverNameForDropdown().then((resp) => {
    const responseData = resp.employees.map(item => ({
      text: item.person.fullName,
      value: item.id
    }));
    employeeApproverList.value = responseData;
    employeeApproverFilter.value = responseData;
    if (!isEditMode) {
      workFromHomeModel.value.approverById = resp.leaveApproverId;
    }
  });
}
function getDefaultLeaveApproverNameForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeApproverList.value = employeeApproverFilter.value;
    } else {
      employeeApproverList.value = employeeApproverFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// get Movement Register details
const getMovementRegisterDetails = () => {
  loading.value = true;
  movementRegisterService
    .getMovementRegisterDetails(props.id, props.detailId)
    .then((resp) => {
      const movementRegisterDetail = resp.movementRegisterDetails?.[0];
      if (!movementRegisterDetail) return;
      workFromHomeModel.value = {
        date: resp.date,
        approverById: movementRegisterDetail.approverById
      };
      if (movementRegisterDetail.wfhDurationId) {
        workFromHomeModel.value.timeInMinutes = Number(movementRegisterDetail.timeInMinutes);
        selectedWFHDurationId.value = movementRegisterDetail.wfhDurationId;
      } else {
        workFromHomeModel.value.timeInMinutes = movementRegisterDetail.timeInMinutes;
        workFromHomeModel.value.wFHDurationId = null;
        selectedWFHDurationId.value = movementRegisterDetail.timeInMinutes;
      }
    })
    .finally(() => {
      loading.value = false;
    });
};

// ===========================================================
// Tooltip
// ===========================================================
const showTooltip = ref(false);

function openTooltip () {
  processing.value = true;
  showTooltip.value = true;
}

function closeTooltip () {
  showTooltip.value = false;
  processing.value = false;
}

async function onContinue () {
  showTooltip.value = false;
  processing.value = true;
  await submitWorkFromHome();
  processing.value = false;
}
// =============================================================

async function submitWorkFromHome () {
  processing.value = true;
  try {
    const isValid = await workFromHomeModelV$.value.$validate();
    if (!isValid) {
      processing.value = false;
      return;
    }
    const workFromHomePayload = {
      date: workFromHomeModel.value.date,
      timeInMinutes: workFromHomeModel.value.timeInMinutes,
      wFHDurationId: workFromHomeModel.value.wFHDurationId || null,
      approverById: workFromHomeModel.value.approverById,
      message: "Working From Home",
      Type: "Work From Home"
    };
    await movementRegisterService.saveMovementRegister(props.detailId, workFromHomePayload);
    notifySuccess({ message: "Work From Home is saved successfully." });
    onDialogOK();
  } catch (error) {
    console.error("Work From Home submit error:", error);
  } finally {
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

const formatMinutesToHours = (minutes) => {
  if (!minutes || isNaN(minutes)) return "";
  const hrs = Math.floor(minutes / 60);
  const mins = minutes % 60;
  const hrText = hrs > 0 ? `${hrs} hr${hrs > 1 ? "s" : ""}` : "";
  const minText = mins > 0 ? `${mins} min` : "";
  return [hrText, minText].filter(Boolean).join(" ");
};

function onDurationChange (val) {
  const selected = breakTimeList.value.find(i => i.id === val.id);
  if (selected) {
    // Dropdown selected
    workFromHomeModel.value.timeInMinutes = Number(selected.value);
    workFromHomeModel.value.wFHDurationId = selected.id;
  } else {
    // User typed number
    workFromHomeModel.value.timeInMinutes = Number(val);
    workFromHomeModel.value.wFHDurationId = null;
  }

  workFromHomeModelV$.value.timeInMinutes.$touch();
}

function onManualInput (val) {
  // User typed manually
  if (/^\d+$/.test(val)) {
    workFromHomeModel.value.timeInMinutes = Number(val);
    workFromHomeModel.value.wFHDurationId = null;
    selectedWFHDurationId.value = val;
  }
}

function enableCurrentDate (date) {
  return date.replaceAll("/", "-") === new Date().toISOString().slice(0, 10);
}

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getMovementRegisterDetails();
  }
}, { immediate: true });

onMounted(() => {
  getAllBreakExplanationListForDropdown("Break Explanation");
  getAllBreakTimesListForDropdown("Work From Home", "Break Time");
  getDefaultLeaveApproverNameForDropdown();
});
</script>
